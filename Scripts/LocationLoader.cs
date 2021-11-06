using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Utility.AssetInjection;
using DaggerfallConnect;

namespace DaggerfallWorkshop.Loc
{
    public class LocationLoader : MonoBehaviour
    {
        HashSet<int> visitedRegions = new HashSet<int>();
        Dictionary<Vector2Int, List<LocationInstance>> worldPixelInstances = new Dictionary<Vector2Int, List<LocationInstance>>();
        Dictionary<int, Dictionary<string, Mod>> modRegionFiles = new Dictionary<int, Dictionary<string, Mod>>();

        Dictionary<string, Mod> modLocationPrefabs = null;
        Dictionary<string, LocationPrefab> prefabInfos = new Dictionary<string, LocationPrefab>();
        Dictionary<string, GameObject> prefabTemplates = new Dictionary<string, GameObject>();

        const float TERRAIN_SIZE = 128;
        const float TERRAINPIXELSIZE = 819.2f;
        const float TERRAIN_SIZE_MULTI = TERRAINPIXELSIZE / TERRAIN_SIZE;

        void Awake()
        {
            DaggerfallTerrain.OnPromoteTerrainData += AddLocation;
        }

        private void Start()
        {
        }

        void CacheLocationPrefabs()
        {
            if (modLocationPrefabs != null)
                return;

            modLocationPrefabs = new Dictionary<string, Mod>();

            foreach (Mod mod in ModManager.Instance.Mods)
            {
                if (!mod.Enabled)
                    continue;

                if (mod.AssetBundle && mod.AssetBundle.GetAllAssetNames().Length > 0)
                {
                    string dummyFilePath = mod.AssetBundle.GetAllAssetNames()[0];
                    string modFolderPrefix = dummyFilePath.Substring(17);
                    modFolderPrefix = dummyFilePath.Substring(0, 17 + modFolderPrefix.IndexOf('/'));

                    string prefabFolder = modFolderPrefix + "/locations/locationprefab/";

                    foreach (string filename in mod.AssetBundle.GetAllAssetNames()
                        .Where(file => file.StartsWith(prefabFolder, System.StringComparison.InvariantCultureIgnoreCase)
                        && (file.EndsWith(".txt", System.StringComparison.InvariantCultureIgnoreCase)))
                        .Select(file => Path.GetFileName(file).ToLower()))
                    {
                        modLocationPrefabs[filename] = mod;
                    }
                }
#if UNITY_EDITOR
                else if (mod.IsVirtual && mod.ModInfo.Files.Count > 0)
                {
                    string dummyFilePath = mod.ModInfo.Files[0];
                    string modFolderPrefix = dummyFilePath.Substring(17);
                    modFolderPrefix = dummyFilePath.Substring(0, 17 + modFolderPrefix.IndexOf('/'));

                    string prefabFolder = modFolderPrefix + "/Locations/LocationPrefab/";

                    foreach (string filename in mod.ModInfo.Files
                        .Where(file => file.StartsWith(prefabFolder, System.StringComparison.InvariantCultureIgnoreCase)
                        && (file.EndsWith(".txt", System.StringComparison.InvariantCultureIgnoreCase)))
                        .Select(file => Path.GetFileName(file).ToLower()))
                    {
                        modLocationPrefabs[filename] = mod;
                    }
                }
#endif                
            }

            string looseLocationFolder = Path.Combine(Application.dataPath, LocationHelper.locationInstanceFolder);
            string looseLocationPrefabFolder = Path.Combine(looseLocationFolder, "LocationPrefab");
            bool hasLooseFiles = Directory.Exists(looseLocationFolder) && Directory.Exists(looseLocationPrefabFolder);
            if(hasLooseFiles)
            {
                foreach(string filename in Directory.GetFiles(looseLocationFolder)
                    .Where(file => file.EndsWith(".txt", System.StringComparison.InvariantCultureIgnoreCase))
                    .Select(file => Path.GetFileName(file).ToLower()))
                {
                    modLocationPrefabs[filename] = null;
                }
            }
        }

        LocationPrefab GetPrefabInfo(string prefabName)
        {
            CacheLocationPrefabs();

            string assetName = prefabName + ".txt";

            LocationPrefab prefabInfo;
            if (!prefabInfos.TryGetValue(assetName, out prefabInfo))
            {
                Mod mod;
                if (!modLocationPrefabs.TryGetValue(assetName.ToLower(), out mod))
                {
                    Debug.LogWarning($"Can't find location prefab '{prefabName}'");
                    return null;
                }

                prefabInfo = mod != null
                    ? LocationHelper.LoadLocationPrefab(mod, assetName)
                    : LocationHelper.LoadLocationPrefab(Application.dataPath + LocationHelper.locationPrefabFolder + assetName);

                if (prefabInfo == null)
                {
                    Debug.LogWarning($"Location prefab '{prefabName}' could not be parsed");
                    return null;
                }

                prefabInfos.Add(assetName, prefabInfo);
            }

            return prefabInfo;
        }

        void InstantiatePrefab(string prefabName, LocationPrefab locationPrefab, LocationInstance loc, DaggerfallTerrain daggerTerrain)
        {
            float terrainHeightMax = DaggerfallUnity.Instance.TerrainSampler.MaxTerrainHeight * GameManager.Instance.StreamingWorld.TerrainScale;

            Vector3 terrainOffset = new Vector3(loc.terrainX * TERRAIN_SIZE_MULTI, daggerTerrain.MapData.averageHeight * terrainHeightMax, loc.terrainY * TERRAIN_SIZE_MULTI);

            GameObject prefabObject;
            if (!prefabTemplates.TryGetValue(prefabName, out prefabObject))
            {
                prefabObject = new GameObject($"{prefabName}_Template");
                prefabObject.SetActive(false);
                Transform templateTransform = prefabObject.GetComponent<Transform>();
                templateTransform.parent = transform; // Put them under this mod for Hierarchy organization

                ModelCombiner combiner = new ModelCombiner();

                foreach (LocationPrefab.LocationObject obj in locationPrefab.obj)
                {
                    if (!LocationHelper.ValidateValue(obj.type, obj.name))
                        continue;

                    GameObject go = LocationHelper.LoadObject(
                        obj.type,
                        obj.name,
                        templateTransform,
                        obj.pos,
                        obj.rot,
                        obj.scale,
                        loc.locationID,
                        obj.objectID,
                        combiner
                        );

                    if (go != null)
                    {
                        if (go.GetComponent<DaggerfallBillboard>())
                        {
                            float tempY = go.transform.position.y;
                            go.GetComponent<DaggerfallBillboard>().AlignToBase();
                            go.transform.position = new Vector3(go.transform.position.x, tempY + ((go.transform.position.y - tempY) * go.transform.localScale.y), go.transform.position.z);
                        }

                        if (!go.GetComponent<DaggerfallLoot>())
                            go.isStatic = true;
                    }
                }

                if (combiner.VertexCount > 0)
                {
                    combiner.Apply();
                    GameObjectHelper.CreateCombinedMeshGameObject(combiner, $"{prefabName}_CombinedModels", templateTransform, makeStatic: true);
                }

                prefabTemplates.Add(prefabName, prefabObject);
            }

            GameObject instance = Instantiate(prefabObject, new Vector3(), Quaternion.identity, daggerTerrain.gameObject.transform);
            instance.transform.localPosition = terrainOffset;
            instance.transform.localRotation = loc.rot;
            instance.name = prefabName;
            instance.SetActive(true);
        }

        void CacheRegionInstances(int regionIndex)
        {
            CacheRegionFileNames(regionIndex);

            if (visitedRegions.Contains(regionIndex))
                return;

            Dictionary<string, Mod> regionFiles = modRegionFiles[regionIndex];
            foreach(var kvp in regionFiles)
            {
                string filename = kvp.Key;
                Mod mod = kvp.Value;

                if (mod == null)
                {
                    string looseLocationRegionFolder = Path.Combine(Application.dataPath, LocationHelper.locationInstanceFolder, regionIndex.ToString());
                    string looseFileLocation = Path.Combine(looseLocationRegionFolder, filename);

                    foreach(LocationInstance instance in LocationHelper.LoadLocationInstance(looseFileLocation))
                    {
                        Vector2Int location = new Vector2Int(instance.worldX, instance.worldY);
                        List<LocationInstance> instances;
                        if(!worldPixelInstances.TryGetValue(location, out instances))
                        {
                            instances = new List<LocationInstance>();
                            worldPixelInstances.Add(location, instances);
                        }

                        instances.Add(instance);
                    }
                }
                else
                {
                    foreach (LocationInstance instance in LocationHelper.LoadLocationInstance(mod, filename))
                    {
                        Vector2Int location = new Vector2Int(instance.worldX, instance.worldY);
                        List<LocationInstance> instances;
                        if (!worldPixelInstances.TryGetValue(location, out instances))
                        {
                            instances = new List<LocationInstance>();
                            worldPixelInstances.Add(location, instances);
                        }

                        instances.Add(instance);
                    }
                }
            }

            visitedRegions.Add(regionIndex);
        }

        void CacheRegionFileNames(int regionIndex)
        {
            if (!modRegionFiles.ContainsKey(regionIndex))
            {
                Dictionary<string, Mod> regionFiles = new Dictionary<string, Mod>();
                modRegionFiles.Add(regionIndex, regionFiles);

                string regionName = DaggerfallUnity.Instance.ContentReader.MapFileReader.GetRegionName(regionIndex);

                foreach (Mod mod in ModManager.Instance.Mods)
                {
                    if (!mod.Enabled)
                        continue;

                    if (mod.AssetBundle && mod.AssetBundle.GetAllAssetNames().Length > 0)
                    {
                        string dummyFilePath = mod.AssetBundle.GetAllAssetNames()[0];
                        string modFolderPrefix = dummyFilePath.Substring(17);
                        modFolderPrefix = dummyFilePath.Substring(0, 17 + modFolderPrefix.IndexOf('/'));

                        string regionIndexFolder = modFolderPrefix + "/locations/" + regionIndex.ToString();                        
                        string regionNameFolder = modFolderPrefix + "/locations/" + regionName;


                        foreach (string filename in mod.AssetBundle.GetAllAssetNames()
                            .Where(file => (file.StartsWith(regionIndexFolder, System.StringComparison.InvariantCultureIgnoreCase) || file.StartsWith(regionNameFolder, System.StringComparison.InvariantCultureIgnoreCase))
                                && (file.EndsWith(".txt", System.StringComparison.InvariantCultureIgnoreCase) || file.EndsWith(".csv", System.StringComparison.InvariantCultureIgnoreCase)))
                            .Select(file => Path.GetFileName(file).ToLower()))
                        {
                            regionFiles[filename] = mod;
                        }
                    }
#if UNITY_EDITOR
                    else if (mod.IsVirtual && mod.ModInfo.Files.Count > 0)
                    {
                        string dummyFilePath = mod.ModInfo.Files[0];
                        string modFolderPrefix = dummyFilePath.Substring(17);
                        modFolderPrefix = dummyFilePath.Substring(0, 17 + modFolderPrefix.IndexOf('/'));

                        string regionIndexFolder = modFolderPrefix + "/Locations/" + regionIndex.ToString();
                        string regionNameFolder = modFolderPrefix + "/Locations/" + regionName;

                        foreach (string filename in mod.ModInfo.Files
                            .Where(file => (file.StartsWith(regionIndexFolder, System.StringComparison.InvariantCultureIgnoreCase) || file.StartsWith(regionNameFolder, System.StringComparison.InvariantCultureIgnoreCase))
                                && (file.EndsWith(".txt", System.StringComparison.InvariantCultureIgnoreCase) || file.EndsWith(".csv", System.StringComparison.InvariantCultureIgnoreCase)))
                            .Select(file => Path.GetFileName(file).ToLower()))
                        {
                            regionFiles[filename] = mod;
                        }
                    }
#endif                   
                }

                string looseLocationFolder = Path.Combine(Application.dataPath, LocationHelper.locationInstanceFolder);
                string looseLocationRegionIndexFolder = Path.Combine(looseLocationFolder, regionIndex.ToString());
                string looseLocationRegionNameFolder = Path.Combine(looseLocationFolder, regionName);
                if(Directory.Exists(looseLocationFolder) )
                {
                    if (Directory.Exists(looseLocationRegionIndexFolder))
                    {
                        foreach (string filename in Directory.GetFiles(looseLocationRegionIndexFolder)
                            .Where(file => file.EndsWith(".txt", System.StringComparison.InvariantCultureIgnoreCase) || file.EndsWith(".csv", System.StringComparison.InvariantCultureIgnoreCase))
                            .Select(file => Path.GetFileName(file).ToLower()))
                        {
                            regionFiles[filename] = null;
                        }
                    }

                    if (Directory.Exists(looseLocationRegionNameFolder))
                    {
                        foreach (string filename in Directory.GetFiles(looseLocationRegionNameFolder)
                            .Where(file => file.EndsWith(".txt", System.StringComparison.InvariantCultureIgnoreCase) || file.EndsWith(".csv", System.StringComparison.InvariantCultureIgnoreCase))
                            .Select(file => Path.GetFileName(file).ToLower()))
                        {
                            regionFiles[filename] = null;
                        }
                    }
                }
            }
        }

        void AddLocation(DaggerfallTerrain daggerTerrain, TerrainData terrainData)
        {
            var regionIndex = GetRegionIndex(daggerTerrain);
            if(regionIndex == -1)
            {
                return;
            }
            CacheRegionInstances(regionIndex);

            //Destroy old locations by going through all the child objects, but 
            //don't delete the billboard batch (The surrounding vegettion)
            foreach (Transform child in daggerTerrain.gameObject.transform)
            {
                if (!child.GetComponent<DaggerfallBillboardBatch>())
                    Destroy(child.gameObject);
            }

            Vector2Int worldLocation = new Vector2Int(daggerTerrain.MapPixelX, daggerTerrain.MapPixelY);
            List<LocationInstance> locationInstances;
            if (!worldPixelInstances.TryGetValue(worldLocation, out locationInstances))
                return;

            foreach (LocationInstance loc in locationInstances)
            {
                if (daggerTerrain.MapData.hasLocation)
                {
                    if (loc.type == 0 || loc.type == 2)
                    {
                        if(loc.type == 0)
                            Debug.LogWarning("Location Already Present " + daggerTerrain.MapPixelX + " : " + daggerTerrain.MapPixelY);
                        continue;
                    }
                }

                if ((daggerTerrain.MapData.mapRegionIndex == 31 ||
                    daggerTerrain.MapData.mapRegionIndex == 3 ||
                    daggerTerrain.MapData.mapRegionIndex == 29 ||
                    daggerTerrain.MapData.mapRegionIndex == 28 ||
                    daggerTerrain.MapData.mapRegionIndex == 30) && daggerTerrain.MapData.worldHeight <= 2)
                {
                    if (loc.type == 0 || loc.type == 2)
                    {
                        if(loc.type == 0)
                            Debug.LogWarning("Location is in Ocean " + daggerTerrain.MapPixelX + " : " + daggerTerrain.MapPixelY);
                        continue;
                    }
                }

                if (loc.type == 0 || loc.type == 2)
                {
                    // Check if Basic Roads detects a road there
                    byte pathsDataPoint = 0;
                    Vector2Int coords = new Vector2Int(loc.worldX, loc.worldY);
                    ModManager.Instance.SendModMessage("BasicRoads", "getPathsPoint", coords,
                        (string message, object data) => { pathsDataPoint = (byte)data; }
                        );

                    if (pathsDataPoint != 0)
                        continue;
                }

                LocationPrefab locationPrefab = GetPrefabInfo(loc.prefab);
                if (locationPrefab == null)
                    continue;

                if ((loc.terrainX + locationPrefab.height > 128 || loc.terrainY + locationPrefab.width > 128))
                {
                    Debug.LogWarning("Invalid Location at " + daggerTerrain.MapPixelX + " : " + daggerTerrain.MapPixelY + " : The locationpreset exist outside the terrain");
                    continue;
                }

                if ((loc.terrainX + locationPrefab.height > 127 || loc.terrainY + locationPrefab.width > 127))
                {
                    Debug.LogWarning("Invalid Location at " + daggerTerrain.MapPixelX + " : " + daggerTerrain.MapPixelY + " : The locationpreset must be 1 pixel away (both X and Y) from the terrainBorder");
                    continue;
                }

                //Smooth the terrain
                if (loc.type == 0 || loc.type == 2)
                {
                    daggerTerrain.MapData.locationRect = new Rect(loc.terrainX, loc.terrainY, locationPrefab.width, locationPrefab.height);

                    int count = 0;
                    float tmpAverageHeight = 0;

                    for (int x = loc.terrainX; x <= loc.terrainX + locationPrefab.width; x++)
                    {
                        for (int y = loc.terrainY; y <= loc.terrainY + locationPrefab.height; y++)
                        {
                            tmpAverageHeight += daggerTerrain.MapData.heightmapSamples[y, x];
                            count++;
                        }
                    }

                    daggerTerrain.MapData.averageHeight = tmpAverageHeight /= count;
                    
                    for (int x = 1; x < 127; x++)
                        for (int y = 1; y < 127; y++)
                            daggerTerrain.MapData.heightmapSamples[y, x] = Mathf.Lerp(daggerTerrain.MapData.heightmapSamples[y, x], daggerTerrain.MapData.averageHeight, 1 / (GetDistanceFromRect(daggerTerrain.MapData.locationRect, new Vector2(x, y)) + 1));
                    
                    terrainData.SetHeights(0, 0, daggerTerrain.MapData.heightmapSamples);
                }

                InstantiatePrefab(loc.prefab, locationPrefab, loc, daggerTerrain);
            }
        }

        private float GetDistanceFromRect(Rect rect, Vector2 point)
        {
            float squared_dist = 0.0f;

            if (point.x > rect.xMax)
                squared_dist += (point.x - rect.xMax) * (point.x - rect.xMax);
            else if (point.x < rect.xMin)
                squared_dist += (rect.xMin - point.x) * (rect.xMin - point.x);

            if (point.y > rect.yMax)
                squared_dist += (point.y - rect.yMax) * (point.y - rect.yMax);
            else if (point.y < rect.yMin)
                squared_dist += (rect.yMin - point.y) * (rect.yMin - point.y);

            return Mathf.Sqrt(squared_dist);
        }

        int GetRegionIndex(DaggerfallTerrain daggerfallTerrain)
        {
            if (daggerfallTerrain.MapData.mapRegionIndex != -1)
                return daggerfallTerrain.MapData.mapRegionIndex;

            int region = daggerfallTerrain.MapData.worldPolitic & 0x7F;
            // Region 64 is an "all water" terrain tile, according to UESP
            if(region < 0 || region >= DaggerfallUnity.Instance.ContentReader.MapFileReader.RegionCount || region == 64)
            {
                if(region != 64)
                    Debug.LogWarning($"Invalid region found at map location [{daggerfallTerrain.MapPixelX}, {daggerfallTerrain.MapPixelY}]");
                return -1;
            }

            return region;
        }
    }
}
