using DaggerfallWorkshop.Utility;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DaggerfallWorkshop;
using System.IO;

namespace LocationLoader
{
#if UNITY_EDITOR
    public class LocationPrefabEditor : LocationEditor
    {
        enum EditMode { EditLocation, ObjectPicker };

        const float mapPixelSize = 800f;
        const float terrainTileSize = mapPixelSize / 128f;

        GameObject parent, ground, areaReference;
        List<GameObject> objScene = new List<GameObject>();

        string searchField = "";
        string currentPrefabName;
        List<string> searchListNames = new List<string>();
        List<string> searchListID = new List<string>();

        HashSet<int> usedIds = new HashSet<int>();

        EditMode editMode;
        int objectPicker;
        int listMode;
        int sublistMode;
        Vector2 scrollPosition = Vector2.zero, scrollPosition2 = Vector2.zero, scrollPosition3 = Vector2.zero;
        string[] listModeName = { "3D Model", "Billboard", "Editor" };
        string[] billboardLists = { "All", "Lights", "Treasure"};

        string extraData = "";
        List<string> dataIDFields = new List<string>();

        LocationPrefab locationPrefab;

        [MenuItem("Daggerfall Tools/Location Prefab Editor")]
        static void Init()
        {
            LocationPrefabEditor window = (LocationPrefabEditor)GetWindow(typeof(LocationPrefabEditor));
            window.titleContent = new GUIContent("Location Prefab Editor");
        }

        private void OnEnable()
        {
            UpdateObjList();
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        void OnGUI()
        {
            if (editMode == EditMode.EditLocation)
            {
                if (parent != null && !parent.activeSelf)
                    parent.SetActive(true);

                EditLocationWindow();
            }
            else if (editMode == EditMode.ObjectPicker)
            {
                if (parent != null && parent.activeSelf)
                    parent.SetActive(false);

                ObjectPickerWindow();
            }
        }

        private void EditLocationWindow()
        {
            if (GUI.Button(Rect_NewFile, "New Prefab"))
            {
                if (parent != null)
                    DestroyImmediate(parent);

                locationPrefab = new LocationPrefab();
                parent = new GameObject("Location Prefab");
                Selection.activeGameObject = parent;
                currentPrefabName = "";
            }

            if (GUI.Button(Rect_SaveFile, "Save Prefab"))
            {
                string path;
                if (!string.IsNullOrEmpty(currentPrefabName))
                {
                    path = EditorUtility.SaveFilePanel("Save as", LocationHelper.locationPrefabFolder, Path.GetFileNameWithoutExtension(currentPrefabName), Path.GetExtension(currentPrefabName).Replace(".", ""));
                }
                else
                {
                    path = EditorUtility.SaveFilePanel("Save as", LocationHelper.locationPrefabFolder, "NewLocation", "txt");
                }

                if (!string.IsNullOrEmpty(path))
                {
                    LocationHelper.SaveLocationPrefab(locationPrefab, path);
                }
            }

            if (GUI.Button(Rect_LoadFile, "Load Prefab"))
            {
                string path = EditorUtility.OpenFilePanel("Open", LocationHelper.locationPrefabFolder, "txt");

                if (string.IsNullOrEmpty(path))
                    return;

                objScene = new List<GameObject>();
                locationPrefab = LocationHelper.LoadLocationPrefab(path);

                if (locationPrefab == null)
                    return;

                if (parent != null)
                    DestroyImmediate(parent);

                parent = new GameObject("Location Prefab");

                foreach (LocationObject obj in locationPrefab.obj)
                {
                    CreateObject(obj);
                    usedIds.Add(obj.objectID);
                }

                Selection.activeGameObject = parent;
                currentPrefabName = Path.GetFileName(path);
            }

            if (parent != null && locationPrefab != null)
            {
                if(lightGrayBG.normal.background == null)
                {
                    CreateGUIStyles();
                }

                GUI.Box(new Rect(4, 32, 516, 56), "", lightGrayBG);
                                
                GUI.Label(new Rect(16, 40, 64, 16), "Area X:");
                int previousWidth = locationPrefab.width;
                locationPrefab.width = EditorGUI.IntSlider(new Rect(90, 40, 400, 16), previousWidth, 1, 126);

                GUI.Label(new Rect(16, 64, 64, 16), "Area Y:");
                int previousHeight = locationPrefab.height;
                locationPrefab.height = EditorGUI.IntSlider(new Rect(90, 64, 400, 16), previousHeight, 1, 126);

                if(areaReference != null && (previousWidth != locationPrefab.width || previousHeight != locationPrefab.height))
                {
                    BoxCollider box = areaReference.GetComponent<BoxCollider>();
                    box.size = new Vector3(locationPrefab.width * terrainTileSize, 50f, locationPrefab.height * terrainTileSize);
                    areaReference.transform.localPosition = new Vector3(box.size.x / 2f, 25f, box.size.z / 2f);
                }

                scrollPosition = GUI.BeginScrollView(new Rect(2, 128, 532, 512), scrollPosition, new Rect(0, 0, 512, 20 + ((objScene.Count+1) * 60)),false, true);

                for (int i = 0; i < objScene.Count; i++)
                {
                    LocationObject obj = locationPrefab.obj[i];
                    if (objScene[i] == null)
                    {                        
                        usedIds.Remove(obj.objectID);
                        objScene.RemoveAt(i);
                        locationPrefab.obj.RemoveAt(i);
                        break;
                    }

                    obj.pos = new Vector3(objScene[i].transform.localPosition.x, objScene[i].transform.localPosition.y, objScene[i].transform.localPosition.z);
                    if(obj.type == 0)
                        obj.rot = objScene[i].transform.rotation;
                    obj.scale = objScene[i].transform.localScale;

                    if (Selection.Contains(objScene[i]))
                    {
                        GUI.BeginGroup(new Rect(6, 10 + (i * 60), 496, 52), lightGreenBG);
                    }
                    else
                        GUI.BeginGroup(new Rect(6, 10 + (i * 60), 496, 52), lightGrayBG);

                    GUI.Label(new Rect(2, 4, 128, 16), "" + objScene[i].name);
                    GUI.Label(new Rect(2, 20, 128, 16), "Name: " + obj.name);
                    GUI.Label(new Rect(2, 36, 128, 16), "ID: " + obj.objectID);

                    GUI.Label(new Rect(136, 4, 256, 16), "Position : " + obj.pos);
                    if(obj.type == 0)
                        GUI.Label(new Rect(136, 20, 256, 16), "Rotation : " + obj.rot.eulerAngles);
                    GUI.Label(new Rect(136, 36, 256, 16), "Scale    : " + obj.scale);

                    if (GUI.Button(new Rect(392, 20, 64, 16), "Duplicate"))
                    {
                        int newID = 0;

                        while(true)
                        {
                            if (!usedIds.Contains(newID))
                                break;
                            ++newID;
                        }

                        usedIds.Add(newID);

                        var duplicatedObj = new LocationObject();
                        duplicatedObj.type = obj.type;
                        duplicatedObj.name = obj.name;
                        duplicatedObj.objectID = newID;
                        duplicatedObj.extraData = obj.extraData;
                        duplicatedObj.pos = obj.pos;
                        duplicatedObj.rot = obj.rot;
                        duplicatedObj.scale = obj.scale;
                        locationPrefab.obj.Add(duplicatedObj);
                        CreateObject(duplicatedObj, true);
                        //locationPrefab.obj.Sort((a, b) => a.objectID.CompareTo(b.objectID));
                    }

                    GUI.color = new Color(0.9f, 0.5f, 0.5f);
                    if (GUI.Button(new Rect(476, 0, 20, 20), "X") || (Event.current.Equals(Event.KeyboardEvent("Delete")) && Selection.Contains(objScene[i])))
                    {
                        usedIds.Remove(obj.objectID);
                        DestroyImmediate(objScene[i]);
                        objScene.RemoveAt(i);
                        locationPrefab.obj.RemoveAt(i);
                    }
                    GUI.color = Color.white;

                    if (GUI.Button(new Rect(0, 0, 758, 64), "", emptyBG))
                    {
                        Selection.activeGameObject = objScene[i];
                    }

                    GUI.EndGroup();
                }

                if (GUI.Button(new Rect(6, 10 + (objScene.Count * 60), 496, 52), "Add New Object"))
                {
                    editMode = EditMode.ObjectPicker;
                }

                GUI.EndScrollView();

                // Make sure we always have a ground
                if (ground == null)
                {
                    ground = GameObject.CreatePrimitive(PrimitiveType.Plane);

                    //Always make sure that the ground is set correctly
                    ground.transform.SetParent(parent.transform);
                    ground.name = "Surface";
                    ground.transform.localPosition = new Vector3(mapPixelSize / 2.0f, 0.0f, mapPixelSize / 2.0f);
                    ground.transform.localScale = new Vector3(mapPixelSize / 10.0f, 0, mapPixelSize / 10.0f);
                    var meshRenderer = ground.GetComponent<MeshRenderer>();
                    string terrainGridPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("TerrainGrid t:material")[0]);
                    meshRenderer.sharedMaterial = AssetDatabase.LoadMainAssetAtPath(terrainGridPath) as Material;
                }

                // Make sure we always have our area reference too
                if(areaReference == null)
                {
                    areaReference = new GameObject();
                    areaReference.name = "Area Reference";
                    areaReference.transform.parent = parent.transform;

                    BoxCollider box = areaReference.AddComponent<BoxCollider>();
                    box.size = new Vector3(locationPrefab.width * terrainTileSize, 50f, locationPrefab.height * terrainTileSize);

                    areaReference.transform.localPosition = new Vector3(box.size.x / 2f, 25f, box.size.z / 2f);
                }
            }
        }

        private void ObjectPickerWindow()
        {
            listMode = GUI.SelectionGrid(new Rect(4, 8, 384, 20), listMode, listModeName, 3);

            if(listMode == 1)
                sublistMode = GUI.SelectionGrid(new Rect(16, 42, 384, 16), sublistMode, billboardLists, 3);

            GUI.Label(new Rect(new Rect(4, 72, 64, 16)), "Search: ");
            searchField = EditorGUI.TextField(new Rect(70, 72, 156, 16), searchField);

            if (GUI.changed)
                UpdateObjList();

            scrollPosition2 = GUI.BeginScrollView(new Rect(4, 96, 256, 472), scrollPosition2, new Rect(0, 0, 236, 20 + (searchListNames.Count * 24)));

            int previousObjectPicker = objectPicker;
            objectPicker = GUI.SelectionGrid(new Rect(10, 10, 216, searchListNames.Count * 24), previousObjectPicker, searchListNames.ToArray(), 1);

            GUI.EndScrollView();

            if(listMode == 2)
            {
                if(searchListID[objectPicker] == "199.16")
                {
                    var mobileIds = Enum.GetValues(typeof(MobileTypes)).Cast<MobileTypes>().ToArray();

                    if (previousObjectPicker != objectPicker)
                    {
                        dataIDFields.Clear();
                        var mobileNames = mobileIds
                            .Where(id => id != MobileTypes.Horse_Invalid && id != MobileTypes.Dragonling_Alternate && id != MobileTypes.Knight_CityWatch)
                            .Select(id => string.Concat(id.ToString().Select(x => char.IsUpper(x) ? " " + x : x.ToString())));
                        dataIDFields.AddRange(mobileNames);
                    }

                    if(string.IsNullOrEmpty(extraData))
                    {
                        extraData = "0";
                    }

                    scrollPosition3 = GUI.BeginScrollView(new Rect(264, 96, 256, 472), scrollPosition3, new Rect(0, 0, 236, 20 + dataIDFields.Count * 24));

                    int previousSelectedIndex = Array.IndexOf(mobileIds, (MobileTypes)int.Parse(extraData));
                    int newSelectedIndex = GUI.SelectionGrid(new Rect(10, 10, 216, dataIDFields.Count * 24), previousSelectedIndex, dataIDFields.ToArray(), 1);

                    extraData = ((int)mobileIds[newSelectedIndex]).ToString();

                    GUI.EndScrollView();
                }
            }

            if (GUI.Button(new Rect(16, 582, 96, 20), "OK"))
            {
                int newID = 0;

                while (true)
                {
                    if (!usedIds.Contains(newID))
                        break;
                    ++newID;
                }

                usedIds.Add(newID);

                var obj = new LocationObject();
                obj.type = listMode;
                obj.name = searchListID[objectPicker];
                locationPrefab.obj.Add(obj);

                obj.objectID = newID;
                obj.extraData = extraData;
                CreateObject(obj, true);
                //locationPrefab.obj.Sort((a, b) => a.objectID.CompareTo(b.objectID));
                editMode = EditMode.EditLocation;
            }

            if (GUI.Button(new Rect(128, 582, 96, 20), "Cancel"))
            {
                editMode = EditMode.EditLocation;
            }
        }

        private void CreateObject(LocationObject locationObject, bool selectNew = false)
        {
            if (!LocationHelper.ValidateValue(locationObject.type, locationObject.name))
                return;

            GameObject newObject;
            if (locationObject.type == 2)
            {
                string[] arg = locationObject.name.Split('.');

                newObject = GameObjectHelper.CreateDaggerfallBillboardGameObject(199, int.Parse(arg[1]), parent.transform);
                newObject.transform.localPosition = locationObject.pos;
            }
            else
            {
                newObject = LocationHelper.LoadStaticObject(locationObject.type, locationObject.name, parent.transform,
                                     new Vector3(locationObject.pos.x, locationObject.pos.y, locationObject.pos.z),
                                     locationObject.rot,
                                     locationObject.scale, 0, 0
                );
            }
            
            if (newObject != null)
            {

                objScene.Add(newObject);

                if (locationObject.type == 0)
                {
                    if(LocationHelper.models.TryGetValue(locationObject.name, out string modelName))
                    {
                        newObject.name = modelName;
                    }
                    else
                    {
                        newObject.name = $"Unknown model ({locationObject.name})";
                    }
                }
                else if (locationObject.type == 1)
                {
                    if (sublistMode == 0)
                    {
                        if (LocationHelper.billboards.TryGetValue(locationObject.name, out string billboardName))
                        {
                            newObject.name = billboardName;
                        }
                        else
                        {
                            newObject.name = $"Unknown billboard ({locationObject.name})";
                        }
                    }
                    else if (sublistMode == 1)
                    {
                        if (LocationHelper.billboardslights.TryGetValue(locationObject.name, out string billboardName))
                        {
                            newObject.name = billboardName;
                        }
                        else
                        {
                            newObject.name = $"Unknown light billboard ({locationObject.name})";
                        }
                    }
                    else if (sublistMode == 2)
                    {
                        if (LocationHelper.billboardsTreasure.TryGetValue(locationObject.name, out string billboardName))
                        {
                            newObject.name = billboardName;
                        }
                        else
                        {
                            newObject.name = $"Unknown treasure billboard ({locationObject.name})";
                        }
                    }
                }
                else if(listMode == 2)
                {
                    if(LocationHelper.editor.TryGetValue(locationObject.name, out string editorName))
                    {
                        newObject.name = editorName;
                    }
                    else
                    {
                        newObject.name = $"Unknown editor marker ({locationObject.name})";
                    }
                }

                if (selectNew)
                    Selection.activeGameObject = objScene[objScene.Count - 1];
            }
            else
            {
                Debug.LogError("Failed to load object " + name);
            }
        }

        private void UpdateObjList()
        {
            searchListNames.Clear();
            searchListID.Clear();

            if (listMode == 0)
            {
                foreach (KeyValuePair<string, string> pair in LocationHelper.models)
                {
                    if (pair.Value.ToLower().Contains(searchField.ToLower()))
                    {
                        searchListNames.Add(pair.Value);
                        searchListID.Add(pair.Key);
                    }     
                }
            }
            else if (listMode == 1)
            {
                if (sublistMode == 0)
                {
                    foreach (KeyValuePair<string, string> pair in LocationHelper.billboards)
                    {
                        if (pair.Value.ToLower().Contains(searchField.ToLower()))
                        {
                            searchListNames.Add(pair.Value);
                            searchListID.Add(pair.Key);
                        }
                    }
                }
                else if (sublistMode == 1)
                {
                    foreach (KeyValuePair<string, string> pair in LocationHelper.billboardslights)
                    {
                        if (pair.Value.ToLower().Contains(searchField.ToLower()))
                        {
                            searchListNames.Add(pair.Value);
                            searchListID.Add(pair.Key);
                        }
                    }
                }
                else if (sublistMode == 2)
                {
                    foreach (KeyValuePair<string, string> pair in LocationHelper.billboardsTreasure)
                    {
                        if (pair.Value.ToLower().Contains(searchField.ToLower()))
                        {
                            searchListNames.Add(pair.Value);
                            searchListID.Add(pair.Key);
                        }
                    }
                }
            }
            else if(listMode == 2)
            {
                foreach (KeyValuePair<string, string> pair in LocationHelper.editor)
                {
                    if (pair.Value.ToLower().Contains(searchField.ToLower()))
                    {
                        searchListNames.Add(pair.Value);
                        searchListID.Add(pair.Key);
                    }
                }
            }
        }

        private void OnDisable()
        {
            if(parent != null)
            {
                DestroyImmediate(parent);
                parent = null;
            }
        }

        private void OnDestroy()
        {
            if (parent != null)
                DestroyImmediate(parent);
        }
    }
#endif
}