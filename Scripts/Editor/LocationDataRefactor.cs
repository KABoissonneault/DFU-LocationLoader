using DaggerfallWorkshop.Game.Utility.ModSupport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LocationLoader
{
    internal class LocationDataRefactor : EditorWindow
    {
        string workingMod;
        string prefabFileFilter = "";
        string prefabObjectFilter = "";
        string targetPattern = ""; // Used as an extra argument for commands

        string factor1 = "1.0";

        string shiftX = "0.0";
        string shiftY = "0.0";
        string shiftZ = "0.0";

        List<string> filteredPrefabs;
        Vector2 scrollFilteredPrefabPos;

        [MenuItem("Daggerfall Tools/Location Data Refactor")]
        static void Init()
        {
            LocationDataRefactor window = (LocationDataRefactor)GetWindow(typeof(LocationDataRefactor));
            window.titleContent = new GUIContent("Location Data Refactor");
        }

        private void OnGUI()
        {
            float baseX = 0;
            float baseY = 0;
            float availableWidth;

            GUI.Label(new Rect(baseX + 4, baseY + 4, 84, 16), "Active Mod: ");
            baseX += 88;

            if (EditorGUI.DropdownButton(new Rect(baseX + 4, baseY + 4, 160, 16), new GUIContent(workingMod), FocusType.Passive))
            {
                void OnItemClicked(object mod)
                {
                    workingMod = (string)mod;

                    filteredPrefabs = new List<string>();
                    foreach (var (prefabPath, _) in GetFilteredPrefabs())
                    {
                        filteredPrefabs.Add(Path.GetFileName(prefabPath));
                    }
                }

                GenericMenu menu = new GenericMenu();
                foreach (string mod in LocationModManager.GetDevMods())
                {
                    menu.AddItem(new GUIContent(mod), workingMod == mod, OnItemClicked, mod);
                }

                menu.DropDown(new Rect(92, baseY + 8, 160, 16));
            }

            baseX = 0;
            baseY += 20;

            GUI.Label(new Rect(baseX + 4, baseY + 4, 80, 16), "Prefab Filter: ");
            baseX += 84;

            availableWidth = position.width - baseX - 4;

            prefabFileFilter = GUI.TextField(new Rect(baseX + 4, baseY + 4, availableWidth - 120, 16), prefabFileFilter);
            
            baseX += availableWidth - 116;

            if(GUI.Button(new Rect(baseX + 4, baseY + 4, 112, 16), "Refresh"))
            {
                filteredPrefabs = new List<string>();
                foreach (var (prefabPath, _) in GetFilteredPrefabs())
                {
                    filteredPrefabs.Add(Path.GetFileName(prefabPath));
                }
            }

            // End line
            baseX = 0;
            baseY += 20;

            if (filteredPrefabs != null)
            {
                availableWidth = position.width - baseX - 4;
                scrollFilteredPrefabPos = GUI.BeginScrollView(new Rect(baseX + 4, baseY + 4, availableWidth - 4, 200), scrollFilteredPrefabPos, new Rect(0, 0, availableWidth - 16, filteredPrefabs.Count * 24));

                for(int i = 0; i < filteredPrefabs.Count; ++i)
                {
                    GUI.Label(new Rect(0, i * 24, availableWidth - 16, 24), filteredPrefabs[i]);
                }

                GUI.EndScrollView();

                baseY += 200;
            }

            // Separator
            baseY += 32;

            // Prefab Object operations
            GUI.Label(new Rect(baseX + 4, baseY + 4, 92, 16), "Object Filter: ");
            baseX += 100;

            availableWidth = position.width - baseX - 4;
            prefabObjectFilter = GUI.TextField(new Rect(baseX + 4, baseY + 4, availableWidth - 4, 16), prefabObjectFilter);

            baseX = 0;
            baseY += 20;

            GUI.Label(new Rect(baseX + 4, baseY + 4, 96, 16), "Target Pattern: ");
            baseX += 100;

            availableWidth = position.width - baseX - 4;
            targetPattern = GUI.TextField(new Rect(baseX + 4, baseY + 4, availableWidth - 4, 16), targetPattern);

            baseX = 0;
            baseY += 20;

            string input;
            int digitSize;

            if(GUI.Button(new Rect(baseX + 4, baseY + 4, 112, 32), "Clean objects"))
            {
                RemoveInvalidObjects();
            }

            baseY += 36;

            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(prefabObjectFilter)))
            {
                if (GUI.Button(new Rect(baseX + 4, baseY + 4, 112, 32), "Remove objects"))
                {
                    RemoveObject();
                }

                baseY += 36;

                if (GUI.Button(new Rect(baseX + 4, baseY + 4, 112, 32), "Shift Objects"))
                {
                    ShiftObject();
                }

                baseX += 116;

                GUI.Label(new Rect(baseX + 30, baseY + 4, 38, 16), "X");
                input = GUI.TextField(new Rect(baseX + 4, baseY + 20, 64, 16), shiftX);
                if (input.StartsWith("-"))
                {
                    digitSize = input.Skip(1).TakeWhile(c => char.IsDigit(c) || c == '.').Count() + 1;
                }
                else
                {
                    digitSize = input.TakeWhile(c => char.IsDigit(c) || c == '.').Count();
                }
                shiftX = input.Substring(0, digitSize);

                baseX += 68;

                GUI.Label(new Rect(baseX + 30, baseY + 4, 38, 16), "Y");
                input = GUI.TextField(new Rect(baseX + 4, baseY + 20, 64, 16), shiftY);
                if (input.StartsWith("-"))
                {
                    digitSize = input.Skip(1).TakeWhile(c => char.IsDigit(c) || c == '.').Count() + 1;
                }
                else
                {
                    digitSize = input.TakeWhile(c => char.IsDigit(c) || c == '.').Count();
                }
                shiftY = input.Substring(0, digitSize);

                baseX += 68;

                GUI.Label(new Rect(baseX + 30, baseY + 4, 38, 16), "Z");
                input = GUI.TextField(new Rect(baseX + 4, baseY + 20, 64, 16), shiftZ);
                if (input.StartsWith("-"))
                {
                    digitSize = input.Skip(1).TakeWhile(c => char.IsDigit(c) || c == '.').Count() + 1;
                }
                else
                {
                    digitSize = input.TakeWhile(c => char.IsDigit(c) || c == '.').Count();
                }
                shiftZ = input.Substring(0, digitSize);

                baseX = 0;
                baseY += 36;

                using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(targetPattern)))
                {
                    using (new EditorGUI.DisabledScope(!float.TryParse(factor1, out float _)))
                    {
                        if (GUI.Button(new Rect(baseX + 4, baseY + 4, 112, 32), "Iceberg"))
                        {
                            Iceberg();
                        }
                    }

                    baseX += 116;

                    GUI.Label(new Rect(baseX + 16, baseY + 4, 52, 16), "Scale");
                    input = GUI.TextField(new Rect(baseX + 4, baseY + 20, 64, 16), factor1);
                    digitSize = input.TakeWhile(c => char.IsDigit(c) || c == '.').Count();
                    factor1 = input.Substring(0, digitSize);

                    baseX = 0;
                    baseY += 36;
                }
            }
        }

        static Regex MakeFilePattern(string filePattern)
        {
            if (string.IsNullOrEmpty(filePattern))
                return null;

            var regexPattern = Regex.Escape(filePattern).Replace("\\?", ".").Replace("\\*", ".*");

            return new Regex(regexPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        IEnumerable<(string, LocationPrefab)> GetFilteredPrefabs()
        {
            ModInfo modInfo = LocationModManager.GetModInfo(workingMod);

            var filterPattern = MakeFilePattern(prefabFileFilter);

            string modDirectory = LocationModManager.GetDevModDirectory(workingMod);
            string pathPefab = $"Assets/Game/Mods/{Path.GetFileName(modDirectory)}/Locations/LocationPrefab/";

            string prefabDirectory = Path.Combine(Application.dataPath, pathPefab.Substring(7));
            
            return modInfo.Files.Where(file => file.StartsWith(pathPefab, StringComparison.OrdinalIgnoreCase)
                && file.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)
            ).Where(file => filterPattern == null || filterPattern.IsMatch(Path.GetFileNameWithoutExtension(file)))
            .Select(file => Path.Combine(prefabDirectory, Path.GetFileName(file)))
            .Select(prefabPath => (prefabPath, LocationHelper.LoadLocationPrefab(prefabPath)));
        }

        void RemoveInvalidObjects()
        {
            int prefabCount = 0;
            int removedCount = 0;

            ModInfo modInfo = LocationModManager.GetModInfo(workingMod);

            foreach (var (prefabPath, prefab) in GetFilteredPrefabs())
            {
                if (prefab == null)
                {
                    Debug.LogError($"Could not load prefab at '{prefabPath}'");
                    continue;
                }

                int initialCount = prefab.obj.Count;

                List<LocationObject> previousObject = prefab.obj;
                prefab.obj = new List<LocationObject>();
                foreach(LocationObject obj in previousObject)
                {
                    if (obj.type == 3)
                    {                        
                        if (modInfo.Files.Any(file => file.EndsWith(".txt") && string.Equals(Path.GetFileNameWithoutExtension(file), obj.name, StringComparison.OrdinalIgnoreCase)))
                        {
                            prefab.obj.Add(obj);
                        }
                    }
                    else if(obj.type == 4)
                    {
                        var prefabName = Path.GetFileNameWithoutExtension(obj.name);
                        if (modInfo.Files.Any(file => file.EndsWith(".prefab") && string.Equals(Path.GetFileNameWithoutExtension(file), prefabName, StringComparison.OrdinalIgnoreCase)))
                        {
                            prefab.obj.Add(obj);
                        }
                    }
                    else
                    {
                        prefab.obj.Add(obj);
                    }                    
                }
                
                if (prefab.obj.Count == initialCount)
                {
                    continue;
                }

                ++prefabCount;

                removedCount += initialCount - prefab.obj.Count;

                LocationHelper.SaveLocationPrefab(prefab, prefabPath);
            }

            EditorUtility.DisplayDialog("Operation done", $"{removedCount} objects removed in {prefabCount} prefabs.", "Ok");
        }

        void RemoveObject()
        {
            int prefabCount = 0;
            int removedCount = 0;

            var objDeletePattern = MakeFilePattern(prefabObjectFilter);
            foreach(var (prefabPath, prefab) in GetFilteredPrefabs())
            {
                if(prefab == null)
                {
                    Debug.LogError($"Could not load prefab at '{prefabPath}'");
                    continue;
                }
                
                int initialCount = prefab.obj.Count;

                prefab.obj = prefab.obj.Where(obj => !objDeletePattern.IsMatch(obj.name)).ToList();

                if(prefab.obj.Count == initialCount)
                {
                    continue;
                }

                ++prefabCount;

                removedCount += initialCount - prefab.obj.Count;

                LocationHelper.SaveLocationPrefab(prefab, prefabPath);
            }

            EditorUtility.DisplayDialog("Operation done", $"{removedCount} objects removed in {prefabCount} prefabs.", "Ok");
        }

        void ShiftObject()
        {
            int prefabCount = 0;
            int shiftCount = 0;

            float posX = float.Parse(shiftX);
            float posY = float.Parse(shiftY);
            float posZ = float.Parse(shiftZ);
            Vector3 shift = new Vector3(posX, posY, posZ);

            var objPattern = MakeFilePattern(prefabObjectFilter);
            foreach (var (prefabPath, prefab) in GetFilteredPrefabs())
            {
                var targetObj = prefab.obj.Where(obj => objPattern.IsMatch(obj.name));

                if (targetObj.Count() == 0)
                    continue;

                foreach (LocationObject obj in targetObj)
                {
                    obj.pos += shift;
                    ++shiftCount;
                }
                ++prefabCount;

                LocationHelper.SaveLocationPrefab(prefab, prefabPath);
            }

            EditorUtility.DisplayDialog("Operation done", $"{shiftCount} objects shifted in {prefabCount} prefabs.", "Ok");
        }

        string ReplacePatternTargets(string pattern, List<string> captureValues)
        {
            StringBuilder builder = new StringBuilder();

            for(int i = 0; i < pattern.Length; ++i)
            {
                char c = pattern[i];

                if(c == '$')
                {
                    var rest = pattern.Substring(i + 1);
                    var digitCount = rest.TakeWhile(l => char.IsDigit(l)).Count();
                    rest = rest.Substring(0, digitCount);

                    int captureIndex = int.Parse(rest);
                    builder.Append(captureValues[captureIndex]);

                    i += digitCount;
                }
                else
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }

        int GetAvailableId(LocationPrefab prefab)
        {
            prefab.obj.Sort((lhs, rhs) => lhs.objectID.CompareTo(rhs.objectID));

            int idTest = 0;
            LocationObject found = prefab.obj.FirstOrDefault(obj => obj.objectID != idTest++);
            if(found != null)
            {
                return idTest - 1;
            }
            else
            {
                return idTest;
            }
        }

        void Iceberg()
        {
            int prefabCount = 0;
            int addedCount = 0;

            float scaleFactor = float.Parse(factor1);

            try
            {
                var objToIcerbergPattern = MakeFilePattern(prefabObjectFilter);
                foreach (var (prefabPath, prefab) in GetFilteredPrefabs())
                {
                    if (prefab == null)
                    {
                        Debug.LogError($"Could not load prefab at '{prefabPath}'");
                        continue;
                    }
                                        
                    var targettedObjects = prefab.obj.Where(obj => objToIcerbergPattern.IsMatch(obj.name)).ToList();

                    if (targettedObjects.Count() == 0)
                    {
                        continue;
                    }

                    ++prefabCount;

                    foreach (var targettedObject in targettedObjects)
                    {
                        Match match = objToIcerbergPattern.Match(targettedObject.name);

                        List<string> captureValues = new List<string>();
                        foreach (Group captureGroup in match.Groups)
                        {
                            captureValues.Add(captureGroup.Value);
                        }

                        var currentTargetPattern = ReplacePatternTargets(targetPattern, captureValues);

                        LocationObject icebergObj = new LocationObject();
                        icebergObj.type = 4; // Unity prefab
                        icebergObj.objectID = GetAvailableId(prefab);
                        icebergObj.name = currentTargetPattern;
                        icebergObj.pos = targettedObject.pos;
                        icebergObj.rot = targettedObject.rot;
                        icebergObj.scale = new Vector3(-targettedObject.scale.x * scaleFactor, -targettedObject.scale.y * scaleFactor, -targettedObject.scale.z * scaleFactor);

                        prefab.obj.Add(icebergObj);

                        ++addedCount;
                    }

                    LocationHelper.SaveLocationPrefab(prefab, prefabPath);
                }
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Operation failed", $"Exception: \"{ex}\"", "Ok");
                return;
            }

            EditorUtility.DisplayDialog("Operation done", $"{addedCount} icebergs added in {prefabCount} prefabs.", "Ok");
        }
    }
}
