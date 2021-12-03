using DaggerfallConnect.Arena2;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Wenzil.Console;

namespace LocationLoader
{
    public static class LocationConsole
    {
        public static void RegisterCommands()
        {
#if UNITY_EDITOR
            ConsoleCommandsDatabase.RegisterCommand("LLPruneInvalidInstances", "Tests location instances for validity and removes invalid ones from their package (only CSV supported)"
                , "LLPruneInvalidInstances [--region=<id>] [--type=<type>] [--mod=<modname>]", PruneInvalidInstances);
#endif
        }

#if UNITY_EDITOR
        static string PruneInvalidInstances(string[] Args)
        {
            int? regionId = null;
            int? type = null;
            string modName = null;

            bool parsingQuotedArg = false;
            StringBuilder quotedString = null;

            StringBuilder modNameBuilder = null;

            foreach(string Arg in Args)
            {
                if(parsingQuotedArg)
                {
                    string ArgValue = Arg;
                    if(Arg.EndsWith("\""))
                    {
                        parsingQuotedArg = false;
                        ArgValue = Arg.Substring(0, Arg.Length - 1);
                    }

                    quotedString.Append(" ").Append(ArgValue);
                }
                else if(Arg.StartsWith("--region="))
                {
                    string regionIdStr = Arg.Replace("--region=", "");
                    if (!int.TryParse(regionIdStr, out int regionIdValue))
                    {
                        return $"Unknown region id '{regionIdStr}'";
                    }
                    regionId = regionIdValue;
                }
                else if(Arg.StartsWith("--type="))
                {
                    string typeStr = Arg.Replace("--type=", "");
                    if (!int.TryParse(typeStr, out int typeValue))
                    {
                        return $"Unknown type '{typeStr}'";
                    }
                    type = typeValue;
                }
                else if(Arg.StartsWith("--mod="))
                {
                    string value = Arg.Replace("--mod=", "");
                    if(value.StartsWith("\""))
                    {
                        quotedString = modNameBuilder = new StringBuilder(value.Substring(1));
                        parsingQuotedArg = true;
                    }
                    else
                    {
                        modName = value;
                    }
                }
                else
                {
                    return $"Unknown argument '{Arg}'";
                }
            }

            if(modNameBuilder != null && modNameBuilder.Length > 0)
            {
                modName = modNameBuilder.ToString();
            }

            Dictionary<string, LocationPrefab> prefabCache = new Dictionary<string, LocationPrefab>();

            Mod basicRoads = ModManager.Instance.GetMod("BasicRoads");
            bool roadsEnabled = basicRoads != null && basicRoads.Enabled;

            bool BaseLocationPasses(LocationInstance instance, Func<string, LocationPrefab> loadPrefab)
            {
                if (type.HasValue && instance.type != type.Value)
                {
                    return true;
                }

                LocationPrefab prefab;
                if (!prefabCache.TryGetValue(instance.prefab, out prefab))
                {
                    prefab = loadPrefab(instance.prefab);
                    prefabCache.Add(instance.prefab, prefab);
                }

                int halfWidth = (prefab.width + 1) / 2;
                int halfHeight = (prefab.height + 1) / 2;

                // Get all world locations it overlaps
                // Type 0 and type 2 instances only fit within their own map pixel, but type 1 can go out of bounds
                List<Tuple<Vector2Int, Rect>> overlappingCoordinates = new List<Tuple<Vector2Int, Rect>>();
                if(type == 1)
                {
                    int xOffsetMin = (int)Math.Floor(instance.terrainX - halfWidth / (float)LocationLoader.TERRAIN_SIZE);
                    int yOffsetMin = (int)Math.Floor(instance.terrainY - halfHeight / (float)LocationLoader.TERRAIN_SIZE);
                    int xOffsetMax = (instance.terrainX + halfWidth) / LocationLoader.TERRAIN_SIZE;
                    int yOffsetMax = (instance.terrainY + halfHeight) / LocationLoader.TERRAIN_SIZE;

                    // Check for instance overflow from the bounds of the world
                    if (instance.worldX + xOffsetMax > MapsFile.MaxMapPixelX)
                        return false;

                    if (instance.worldX + xOffsetMin > MapsFile.MinMapPixelX)
                        return false;

                    if (instance.worldY - yOffsetMax < MapsFile.MinMapPixelY)
                        return false;

                    if (instance.worldY - yOffsetMin > MapsFile.MaxMapPixelY)
                        return false;

                    // Find all overlapping coordinates and their overlap rectangle
                    for(int xOffset = xOffsetMin; xOffset <= xOffsetMax; ++xOffset)
                    {
                        for(int yOffset = yOffsetMin; yOffset <= yOffsetMax; ++yOffset)
                        {
                            int xMin = Math.Max(instance.terrainX - halfWidth - xOffset * LocationLoader.TERRAIN_SIZE, 0);
                            int xMax = Math.Min(instance.terrainX + halfWidth - xOffset * LocationLoader.TERRAIN_SIZE, 128);
                            int yMin = Math.Max(instance.terrainY - halfHeight - yOffset * LocationLoader.TERRAIN_SIZE, 0);
                            int yMax = Math.Min(instance.terrainY + halfHeight - yOffset * LocationLoader.TERRAIN_SIZE, 128);

                            overlappingCoordinates.Add(
                                new Tuple<Vector2Int, Rect>(
                                    new Vector2Int(instance.worldX + xOffset, instance.worldY - yOffset),
                                    new Rect(xMin, yMin, xMax - xMin, yMax - yMin))
                                );
                        }
                    }
                }
                else
                {
                    overlappingCoordinates.Add(
                        new Tuple<Vector2Int, Rect>(new Vector2Int(instance.worldX, instance.worldY),
                        new Rect(instance.terrainX - halfWidth, instance.terrainY - halfHeight, halfWidth * 2, halfHeight * 2))
                    );
                }

                
                foreach (var (coordinate, _) in overlappingCoordinates)
                {
                    // Instance is on existing location
                    if (DaggerfallUnity.Instance.ContentReader.HasLocation(coordinate.x, coordinate.y))
                        return false;

                    // Instance is on the ocean
                    if (DaggerfallUnity.Instance.ContentReader.MapFileReader.GetClimateIndex(coordinate.x, coordinate.y) == (int)MapsFile.Climates.Ocean)
                        return false;
                }
                
                // Instance is out of bounds
                if (type != 1)
                {
                    if (instance.terrainX + halfWidth > LocationLoader.TERRAIN_SIZE
                        || instance.terrainY + halfHeight > LocationLoader.TERRAIN_SIZE
                        || instance.terrainX - halfWidth < 0
                        || instance.terrainY - halfHeight < 0
                        )
                    {
                        return false;
                    }
                }

                // Instance is on road
                if (roadsEnabled)
                {
                    foreach (var (coordinate, rectangle) in overlappingCoordinates)
                    {
                        byte pathsDataPoint = 0;
                        Vector2Int coords = new Vector2Int(coordinate.x, coordinate.y);
                        ModManager.Instance.SendModMessage("BasicRoads", "getPathsPoint", coords,
                            (string message, object data) => { pathsDataPoint = (byte)data; }
                            );

                        if (pathsDataPoint != 0)
                        {
                            if (LocationHelper.OverlapsRoad(rectangle, pathsDataPoint))
                                return false;
                        }
                    }
                }

                return true;
            }

            bool ModLocationPasses(LocationInstance instance, Mod mod)
            {
                return BaseLocationPasses(instance, name => LocationHelper.LoadLocationPrefab(mod, name));
            }

            void PruneModFile(Mod mod, string modFolder, string fileRelativePath)
            {
                string modFilename = Path.GetFileName(fileRelativePath);

                TextAsset asset = mod.GetAsset<TextAsset>(modFilename);
                TextReader assetReader = new StringReader(asset.text);

                int line = 1;
                string header = assetReader.ReadLine();
                string[] fields = header.Split(',');

                string fullAssetPath = Path.Combine(modFolder, "Locations", fileRelativePath);

                using (StreamWriter streamWriter = new StreamWriter(fullAssetPath, append: false))
                {
                    streamWriter.WriteLine(header);

                    while (assetReader.Peek() > 0)
                    {
                        ++line;
                        string instanceLine = assetReader.ReadLine();

                        string context = $"mod={mod.ModInfo.ModTitle}, file={modFilename}, line={line}";
                        LocationInstance instance = LocationHelper.LoadSingleLocationInstanceCsv(instanceLine, fields, context);
                        if (instance == null)
                        {
                            Debug.LogWarning($"({context}) Instance could not be parsed. Removing");
                            continue;
                        }

                        if (ModLocationPasses(instance, mod))
                        {
                            streamWriter.WriteLine(instanceLine);
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(modName))
            {
                return $"Loose files not yet supported. Specify a mod with --mod=<mod name>";
            }
            else
            {
                Mod mod = ModManager.Instance.GetMod(modName);
                if(mod == null)
                    return $"Mod '{modName}' not found";

                if (!mod.IsVirtual)
                    return $"Mod '{modName} must be virtual (ie: have the sources in the editor)";

                if (mod.ModInfo.Files.Count == 0)
                    return $"Mod '{modName} has no files in mod info (see .dfmod.json)";


                string dummyFilePath = mod.ModInfo.Files[0];
                string modFolderPrefix = dummyFilePath.Substring(17);
                modFolderPrefix = dummyFilePath.Substring(0, 17 + modFolderPrefix.IndexOf('/'));
                string modFolder = Path.Combine(Application.dataPath, modFolderPrefix.Substring(7));

                string locationsFolder = modFolderPrefix + "/Locations/";
                string locationPrefabsFolder = modFolderPrefix + "/Locations/LocationPrefab";

                if (regionId.HasValue)
                {
                    string regionName = DaggerfallUnity.Instance.ContentReader.MapFileReader.GetRegionName(regionId.Value);

                    string regionIdFolder = locationsFolder + regionId.Value;
                    string regionNameFolder = locationsFolder + regionName;

                    foreach (string fileRelativePath in mod.ModInfo.Files
                            .Where(file => (file.StartsWith(regionIdFolder, StringComparison.InvariantCultureIgnoreCase) || file.StartsWith(regionNameFolder, StringComparison.InvariantCultureIgnoreCase))
                                && file.EndsWith(".csv", StringComparison.InvariantCultureIgnoreCase))
                            .Select(file => file.Substring(locationsFolder.Length)))
                    {
                        try
                        {
                            PruneModFile(mod, modFolder, fileRelativePath);
                        }
                        catch(Exception e)
                        {
                            Debug.LogError(e.Message);
                        }
                    }
                }
                else
                {
                    foreach (string fileRelativePath in mod.ModInfo.Files
                    .Where(file => (file.StartsWith(locationsFolder, StringComparison.InvariantCultureIgnoreCase) && !file.StartsWith(locationPrefabsFolder, StringComparison.InvariantCultureIgnoreCase))
                        && file.EndsWith(".csv", StringComparison.InvariantCultureIgnoreCase))
                    .Select(file => file.Substring(locationsFolder.Length)))
                    {
                        try
                        {
                            PruneModFile(mod, modFolder, fileRelativePath);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e.Message);
                        }
                    }
                }
            }

            return "Success";
        }
#endif
    }
}
