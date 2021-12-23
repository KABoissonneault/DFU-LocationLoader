using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System;

namespace LocationLoader
{
#if UNITY_EDITOR
    public class LocationInstanceEditor : LocationEditor
    {
        List<LocationInstance> locationInstance;
        List<string> locationInstaneNames;

        Dictionary<string, LocationPrefab> prefabInfos = new Dictionary<string, LocationPrefab>();

        int listSelector;
        Vector2 scrollPosition;
        string[] locationTypes = { "Smooth Terrain", "No Smoothing" };

        int selectedPrefab = -1;

        [MenuItem("Daggerfall Tools/Location Instance Editor")]
        static void Init()
        {
            LocationInstanceEditor window = (LocationInstanceEditor)GetWindow(typeof(LocationInstanceEditor));
            window.titleContent = new GUIContent("Location Instance Editor");

            string modsfolder = Path.Combine(Application.dataPath, "Game", "Mods");
            foreach (var directory in Directory.EnumerateDirectories(modsfolder))
            {
                string locationPrefabs = Path.Combine(directory, "Locations", "LocationPrefab");
                if (!Directory.Exists(locationPrefabs))
                    continue;

                foreach (var file in Directory.EnumerateFiles(locationPrefabs, "*.txt"))
                {
                    LocationPrefab prefab = LocationHelper.LoadLocationPrefab(file);
                    if (prefab != null)
                    {
                        try
                        {
                            string prefabName = Path.GetFileNameWithoutExtension(file.ToLower());
                            window.prefabInfos[prefabName] = prefab;
                        }
                        catch(Exception e)
                        {
                            Debug.LogError(e.Message);
                        }
                    }
                }
            }
        }

        private void Awake()
        {
            CreateGUIStyles();
        }

        void OnGUI()
        {
            if (GUI.Button(Rect_NewFile, "New File"))
            {
                locationInstance = new List<LocationInstance>();
                locationInstaneNames = new List<string>();
            }

            if (GUI.Button(Rect_SaveFile, "Save File"))
            {
                string path = EditorUtility.SaveFilePanel("Save as", LocationHelper.locationInstanceFolder, "new location", "txt");
                if (path.Length != 0)
                {
                    LocationHelper.SaveLocationInstance(locationInstance.ToArray(), path);
                }
            }

            if (GUI.Button(Rect_LoadFile, "Load File"))
            {
                string path = EditorUtility.OpenFilePanel("Open", LocationHelper.locationInstanceFolder, "txt");

                if (path.Length == 0)
                    return;

                locationInstance = LocationHelper.LoadLocationInstance(path).ToList();

                if (locationInstance == null)
                    return;

                locationInstaneNames = new List<string>();

                foreach (LocationInstance loc in locationInstance)
                {
                    locationInstaneNames.Add(loc.name);
                }
            }

            if (locationInstance != null)
                ShowLocationList();
        }

        void ShowLocationList()
        {
            scrollPosition = GUI.BeginScrollView(new Rect(32, 64, 256, 472), scrollPosition, new Rect(0, 0, 236, 20 + (locationInstaneNames.Count * 24)), false, true);
            listSelector = GUI.SelectionGrid(new Rect(10, 10, 216, locationInstaneNames.Count * 24), listSelector, locationInstaneNames.ToArray(), 1);

            GUI.EndScrollView();

            if (locationInstance.Count > listSelector)
            {
                GUI.Label(new Rect(312, 64, 96, 16), "Location ID: ");
                GUI.TextField(new Rect(388, 64, 96, 16), "#" + locationInstance[listSelector].locationID.ToString("00000000"));

                if (GUI.Button(new Rect(506, 64, 128, 16), "Generate New ID"))
                {
                    locationInstance[listSelector].UpdateLocationID();
                }

                    GUI.Label(new Rect(312, 96, 64, 16), "Name: ");
                locationInstance[listSelector].name = GUI.TextField(new Rect(372, 96, 128, 16), locationInstance[listSelector].name);

                if (GUI.changed)
                    locationInstaneNames[listSelector] = locationInstance[listSelector].name;

                GUI.Label(new Rect(312, 128, 64, 16), "Prefab: ");
                GUI.TextField(new Rect(372, 128, 128, 16), locationInstance[listSelector].prefab);

                if(GUI.Button(new Rect(506, 128, 96, 16), "Select Prefab"))
                {
                    var content = prefabInfos.Keys.Select(name => new GUIContent(name)).ToArray();
                    EditorUtility.DisplayCustomMenu(new Rect(612, 64, 128, 256), content, selectedPrefab, (_, prefabs, selected) =>
                    {
                        selectedPrefab = selected;
                        if (selected != -1)
                        {
                            locationInstance[listSelector].prefab = prefabs[selected];
                        }
                    }, null);
                }

                LocationPrefab locationPrefab = null;
                if (locationInstance[listSelector].prefab != "" && !prefabInfos.TryGetValue(locationInstance[listSelector].prefab, out locationPrefab))
                {
                    GUI.contentColor = Color.red;
                    GUI.Label(new Rect(614, 128, 128, 16), "Prefab not found!");
                    GUI.contentColor = Color.white;
                }

                GUI.Label(new Rect(312, 160, 64, 16), "Type: ");
                locationInstance[listSelector].type = EditorGUI.Popup(new Rect(372, 160, 128, 16), locationInstance[listSelector].type, locationTypes);

                GUI.Label(new Rect(312, 192, 64, 16), "World X: ");
                locationInstance[listSelector].worldX = EditorGUI.IntSlider(new Rect(372, 192, 256, 16), locationInstance[listSelector].worldX,3, 998);

                GUI.Label(new Rect(312, 224, 64, 16), "World Y: ");
                locationInstance[listSelector].worldY = EditorGUI.IntSlider(new Rect(372, 224, 256, 16), locationInstance[listSelector].worldY,3, 498);

                if (locationPrefab != null)
                {
                    GUI.Label(new Rect(312, 256, 64, 16), "Terrain X: ");
                    locationInstance[listSelector].terrainX = EditorGUI.IntSlider(new Rect(372, 256, 256, 16), locationInstance[listSelector].terrainX, 1, 127 - locationPrefab.width);

                    GUI.Label(new Rect(312, 288, 64, 16), "Terrain Y: ");
                    locationInstance[listSelector].terrainY = EditorGUI.IntSlider(new Rect(372, 288, 256, 16), locationInstance[listSelector].terrainY, 1, 127 - locationPrefab.height);

                    GUI.Label(new Rect(454, 320, 64, 16), "- N -");
                    GUI.Box(new Rect(336, 336, 256, 256), "", blackBG);
                    GUI.Box(new Rect((336 + (locationInstance[listSelector].terrainX * 2)), (592 - (locationInstance[listSelector].terrainY * 2)) - locationPrefab.height * 2, locationPrefab.width * 2, locationPrefab.height * 2), "", lightGreenBG);
                    GUI.Label(new Rect(454, 592, 64, 16), "- S -");
                }
            }

            if (GUI.Button(new Rect(32, 562, 96, 20), "Add Location"))
            {
                var loc = new LocationInstance();
                loc.name = "New Location";
                locationInstance.Add(loc);
                loc.UpdateLocationID();
                locationInstaneNames.Add("New Location");
            }

            if (GUI.Button(new Rect(160, 562, 128, 20), "Remove Location"))
            {
                locationInstance.RemoveAt(listSelector);
                locationInstaneNames.RemoveAt(listSelector);
            }
        }
    }
#endif
}
