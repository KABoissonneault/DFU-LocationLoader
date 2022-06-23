using DaggerfallWorkshop.Game.Utility.ModSupport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            GUI.Label(new Rect(baseX + 4, baseY + 4, 84, 16), "Active Mod: ");
            baseX += 88;

            if (EditorGUI.DropdownButton(new Rect(baseX + 4, baseY + 4, 160, 16), new GUIContent(workingMod), FocusType.Passive))
            {
                void OnItemClicked(object mod)
                {
                    workingMod = (string)mod;
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

            prefabFileFilter = GUI.TextField(new Rect(baseX + 4, baseY + 4, 192, 16), prefabFileFilter);

            // End line
            baseX = 0;
            baseY += 20;

            // Separator
            baseY += 32;

            // Prefab Object operations
            GUI.Label(new Rect(baseX + 4, baseY + 4, 80, 16), "Object Filter: ");
            baseX += 84;

            prefabObjectFilter = GUI.TextField(new Rect(baseX + 4, baseY + 4, 192, 16), prefabObjectFilter);

            baseX = 0;
            baseY += 20;

            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(prefabObjectFilter)))
            {
                if (GUI.Button(new Rect(baseX + 4, baseY + 4, 112, 32), "Remove object"))
                {
                    RemoveObject();
                }
            }
        }

        static Regex MakeFilePattern(string filePattern)
        {
            if (string.IsNullOrEmpty(filePattern))
                return null;

            var lowerPattern = filePattern.ToLower();

            var regexPattern = Regex.Escape(lowerPattern).Replace("\\?", ".").Replace("\\*", ".*");
            return new Regex(regexPattern, RegexOptions.Compiled);
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
            ).Where(file => filterPattern == null || filterPattern.IsMatch(Path.GetFileNameWithoutExtension(file).ToLower()))
            .Select(file => Path.Combine(prefabDirectory, Path.GetFileName(file)))
            .Select(prefabPath => (prefabPath, LocationHelper.LoadLocationPrefab(prefabPath)));
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

                prefab.obj = prefab.obj.Where(obj => !objDeletePattern.IsMatch(obj.name.ToLower())).ToList();

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
    }
}
