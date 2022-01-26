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

        GameObject parent, ground, areaReference, preview;
        List<GameObject> objScene = new List<GameObject>();

        string searchField = "";
        string currentPrefabName;
        List<string> searchListNames = new List<string>();
        List<string[]> searchListIDSets = new List<string[]>();
        Dictionary<string, string> idName = new Dictionary<string, string>();
        Dictionary<string, LocationPrefab> prefabInfos = new Dictionary<string, LocationPrefab>();

        HashSet<int> usedIds = new HashSet<int>();

        EditMode editMode;
        int objectPicker;
        int listMode;
        int sublistMode;
        int setIndex = 0;
        int maxAreaLength = 128;
        Vector2 scrollPosition = Vector2.zero, scrollPosition2 = Vector2.zero, scrollPosition3 = Vector2.zero;
        string[] listModeName = { "3D Model", "Billboard", "Editor", "Interior Parts", "Prefab" };
        string[] modelLists = { "All", "Structure", "Clutter", "Dungeon", "Furniture", "Graveyard" };
        string[] billboardLists = { "All", "People", "Interior", "Nature", "Lights", "Treasure", "Dungeon" };
        string[] partsLists = { "All", "House", "Dungeon Rooms", "Dungeon Corridors", "Dungeon Misc", "Caves", "Dungeon Doors/Exits" };

        string extraData = "";
        List<string> dataIDFields = new List<string>();

        LocationPrefab locationPrefab;

        int GetCurrentObjectType()
        {
            switch(listMode)
            {
                case 0:
                case 3:
                    return 0;
                case 1: return 1;
                case 2: return 2;
                case 4: return 3;
            }
            throw new Exception("GetCurrentObjectType called with invalid list mode");
        }

        [MenuItem("Daggerfall Tools/Location Prefab Editor")]
        static void Init()
        {
            LocationPrefabEditor window = (LocationPrefabEditor)GetWindow(typeof(LocationPrefabEditor));
            window.titleContent = new GUIContent("Location Prefab Editor");
        }

        private void OnEnable()
        {
            idName.Clear();
            foreach (LocationHelper.ObjectSet set in LocationHelper.objects)
            {
                foreach (string id in set.Ids)
                    idName.Add(id, set.Name);
            }

            foreach(var kvp in LocationHelper.editor)
            {
                idName.Add(kvp.Key, kvp.Value);
            }

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
                            prefabInfos[prefabName] = prefab;
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e.Message);
                        }
                    }
                }
            }

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

                if (preview != null)
                    DestroyImmediate(preview);
                
                EditLocationWindow();
            }
            else if (editMode == EditMode.ObjectPicker)
            {
                if (parent != null && parent.activeSelf)
                    parent.SetActive(false);

                if (preview == null)
                {
                    UpdatePreview();
                }

                if (preview != null && !preview.activeSelf)
                    preview.SetActive(true);

                ObjectPickerWindow();
            }
        }

        private void EditLocationWindow()
        {
            if (GUI.Button(Rect_NewFile, "New Prefab"))
            {
                if (parent != null)
                    DestroyImmediate(parent);
                objScene = new List<GameObject>();

                locationPrefab = new LocationPrefab();
                parent = new GameObject("Location Prefab");
                Selection.activeGameObject = parent;
                currentPrefabName = "";

                maxAreaLength = 128;
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
                    AddObject(obj, selectNew:false);
                    usedIds.Add(obj.objectID);
                }

                Selection.activeGameObject = parent;
                currentPrefabName = Path.GetFileName(path);

                maxAreaLength = Math.Min(Math.Max(Math.Max(128, locationPrefab.width), locationPrefab.height), 9999);
            }

            if (parent != null && locationPrefab != null)
            {
                if(lightGrayBG.normal.background == null)
                {
                    CreateGUIStyles();
                }

                GUI.Box(new Rect(4, 32, 656, 56), "", lightGrayBG);
                                
                GUI.Label(new Rect(16, 40, 64, 16), "Area X:");
                int previousWidth = locationPrefab.width;
                locationPrefab.width = EditorGUI.IntSlider(new Rect(90, 40, 400, 16), previousWidth, 1, maxAreaLength);

                GUI.Label(new Rect(16, 64, 64, 16), "Area Y:");
                int previousHeight = locationPrefab.height;
                locationPrefab.height = EditorGUI.IntSlider(new Rect(90, 64, 400, 16), previousHeight, 1, maxAreaLength);
                                
                if(areaReference != null && GUI.changed)
                {
                    BoxCollider box = areaReference.GetComponent<BoxCollider>();
                    box.size = new Vector3(locationPrefab.width * terrainTileSize, 50f, locationPrefab.height * terrainTileSize);
                    GUI.changed = false;
                }

                GUI.Label(new Rect(498, 40, 80, 16), "Max Length:");
                string maxAreaLengthText = GUI.TextField(new Rect(586, 40, 64, 16), maxAreaLength.ToString(), 4);
                if(GUI.changed)
                {
                    if(int.TryParse(maxAreaLengthText, out int parsedMaxAreaLength))
                    {
                        if(parsedMaxAreaLength < 0)
                        {
                            maxAreaLength = 0;
                        }
                        else
                        {
                            maxAreaLength = parsedMaxAreaLength;
                        }
                    }
                    
                    GUI.changed = false;
                }

                scrollPosition = GUI.BeginScrollView(new Rect(2, 128, 532, 512), scrollPosition, new Rect(0, 0, 512, 20 + ((objScene.Count+1) * 60)),false, true);

                for (int i = 0; i < objScene.Count; ++i)
                {
                    GameObject sceneObj = objScene[i];
                    LocationObject obj = locationPrefab.obj[i];

                    if (sceneObj == null)
                    {                        
                        usedIds.Remove(obj.objectID);
                        objScene.RemoveAt(i);
                        locationPrefab.obj.RemoveAt(i);
                        --i;
                        continue;
                    }

                    if (obj.type != 1)
                    {
                        obj.pos = sceneObj.transform.localPosition;
                    }
                    else
                    {
                        Vector3 scenePos = sceneObj.transform.localPosition;

                        // Reverse base alignment
                        float billboardHeight = sceneObj.GetComponent<DaggerfallBillboard>().Summary.Size.y;
                        obj.pos = new Vector3(scenePos.x, scenePos.y - (billboardHeight / 2) * sceneObj.transform.localScale.y, scenePos.z);
                    }

                    if(obj.type == 0)
                        obj.rot = sceneObj.transform.rotation;
                    obj.scale = sceneObj.transform.localScale;

                    if (Selection.Contains(sceneObj))
                    {
                        GUI.BeginGroup(new Rect(6, 10 + (i * 60), 496, 52), lightGreenBG);
                    }
                    else
                        GUI.BeginGroup(new Rect(6, 10 + (i * 60), 496, 52), lightGrayBG);

                    GUI.Label(new Rect(2, 4, 128, 16), "" + sceneObj.name);
                    GUI.Label(new Rect(2, 20, 128, 16), "Name: " + obj.name);
                    GUI.Label(new Rect(2, 36, 128, 16), "ID: " + obj.objectID);

                    GUI.Label(new Rect(136, 4, 256, 16), "Position : " + obj.pos);
                    if(obj.type == 0)
                        GUI.Label(new Rect(136, 20, 256, 16), "Rotation : " + obj.rot.eulerAngles);
                    GUI.Label(new Rect(136, 36, 256, 16), "Scale    : " + obj.scale);

                    if (GUI.Button(new Rect(392, 4, 64, 16), "Duplicate"))
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
                        AddObject(duplicatedObj, selectNew: true);
                        //locationPrefab.obj.Sort((a, b) => a.objectID.CompareTo(b.objectID));
                    }

                    if(Selection.Contains(sceneObj))
                    {
                        if(TryGetObjectSet(sceneObj.name, obj.name, obj.type, out string[] objectSet))
                        {
                            if (objectSet.Length > 1)
                            {
                                int setIndex = Array.FindIndex(objectSet, id => id == obj.name);
                                if (setIndex != -1)
                                {
                                    if (GUI.Button(new Rect(320, 36, 16, 16), "<"))
                                    {
                                        if (setIndex == 0)
                                            setIndex = objectSet.Length - 1;
                                        else
                                            setIndex = setIndex - 1;

                                        obj.name = objectSet[setIndex];

                                        // Replace object
                                        DestroyImmediate(sceneObj);
                                        sceneObj = objScene[i] = CreateObject(obj, parent.transform);
                                        Selection.activeGameObject = sceneObj;
                                    }

                                    if (setIndex + 1 < 10)
                                        GUI.Label(new Rect(346, 36, 12, 16), (setIndex + 1).ToString());
                                    else
                                        GUI.Label(new Rect(340, 36, 24, 16), (setIndex + 1).ToString());

                                    GUI.Label(new Rect(358, 36, 12, 16), "/");
                                    GUI.Label(new Rect(368, 36, 24, 16), objectSet.Length.ToString());

                                    if (GUI.Button(new Rect(388, 36, 16, 16), ">"))
                                    {
                                        if (setIndex == objectSet.Length - 1)
                                            setIndex = 0;
                                        else
                                            setIndex = setIndex + 1;

                                        obj.name = objectSet[setIndex];

                                        // Replace object
                                        DestroyImmediate(sceneObj);
                                        sceneObj = objScene[i] = CreateObject(obj, parent.transform);
                                        Selection.activeGameObject = sceneObj;
                                    }
                                }
                            }
                        }
                    }

                    GUI.color = new Color(0.9f, 0.5f, 0.5f);
                    if (GUI.Button(new Rect(476, 0, 20, 20), "X") || (Event.current.Equals(Event.KeyboardEvent("Delete")) && Selection.Contains(sceneObj)))
                    {
                        usedIds.Remove(obj.objectID);
                        DestroyImmediate(sceneObj);
                        objScene.RemoveAt(i);
                        locationPrefab.obj.RemoveAt(i);
                    }
                    GUI.color = Color.white;

                    if (GUI.Button(new Rect(0, 0, 758, 64), "", emptyBG))
                    {
                        Selection.activeGameObject = sceneObj;
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
                }
            }
        }

        bool TryGetObjectSet(string setName, string objId, int objType, out string[] objectSet)
        {
            if(objType == 0)
            {
                int index = Array.FindIndex(LocationHelper.models, set => set.Name == setName && set.Ids.Contains(objId));
                if(index != -1)
                {
                    objectSet = LocationHelper.models[index].Ids;
                    return true;
                }

                index = Array.FindIndex(LocationHelper.interiorParts, set => set.Name == setName && set.Ids.Contains(objId));
                if (index != -1)
                {
                    objectSet = LocationHelper.interiorParts[index].Ids;
                    return true;
                }
            }
            else if(objType == 1)
            {
                int index = Array.FindIndex(LocationHelper.billboards, set => set.Name == setName && set.Ids.Contains(objId));
                if (index != -1)
                {
                    objectSet = LocationHelper.billboards[index].Ids;
                    return true;
                }
            }
            else if(objType == 2)
            {
                objectSet = new string[] { objId };
                return true;
            }

            objectSet = null;
            return false;
        }

        private void ObjectPickerWindow()
        {
            listMode = GUI.SelectionGrid(new Rect(16, 8, listModeName.Length * 100, 20), listMode, listModeName, listModeName.Length);
            if (GUI.changed)
            {
                sublistMode = 0;
                objectPicker = 0;
                setIndex = 0;
                UpdateObjList();
                UpdatePreview();
                GUI.changed = false;
            }

            if (listMode == 0)
                sublistMode = GUI.SelectionGrid(new Rect(16, 42, modelLists.Length * 100, 16), sublistMode, modelLists, modelLists.Length);
            else if (listMode == 1)
                sublistMode = GUI.SelectionGrid(new Rect(16, 42, billboardLists.Length * 100, 16), sublistMode, billboardLists, billboardLists.Length);
            else if(listMode == 3)
                sublistMode = GUI.SelectionGrid(new Rect(16, 42, partsLists.Length * 130, 16), sublistMode, partsLists, partsLists.Length);

            if (GUI.changed)
            {
                objectPicker = 0;
                setIndex = 0;
                UpdateObjList();
                UpdatePreview();
                GUI.changed = false;
            }

            GUI.Label(new Rect(new Rect(4, 72, 64, 16)), "Search: ");
            searchField = EditorGUI.TextField(new Rect(70, 72, 156, 16), searchField);

            if (GUI.changed)
            {
                objectPicker = 0;
                setIndex = 0;
                UpdateObjList();
                UpdatePreview();
                GUI.changed = false;
            }

            scrollPosition2 = GUI.BeginScrollView(new Rect(4, 96, 256, 472), scrollPosition2, new Rect(0, 0, 236, 20 + (searchListNames.Count * 24)));

            int previousObjectPicker = objectPicker;
            objectPicker = GUI.SelectionGrid(new Rect(10, 10, 216, searchListNames.Count * 24), previousObjectPicker, searchListNames.ToArray(), 1);
            if (GUI.changed)
            {
                setIndex = 0;
                UpdatePreview();
                GUI.changed = false;
            }

            GUI.EndScrollView();

            if(listMode == 2)
            {
                if(searchListIDSets[objectPicker][setIndex] == "199.16")
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
            else
            {
                if (searchListIDSets.Count > 0)
                {
                    string[] currentSetIds = searchListIDSets[objectPicker];
                    if (currentSetIds.Length > 1)
                    {
                        if (GUI.Button(new Rect(60, 582, 16, 16), "<"))
                        {
                            if (setIndex == 0)
                                setIndex = currentSetIds.Length - 1;
                            else
                                setIndex = setIndex - 1;

                            UpdatePreview();
                        }

                        if (setIndex + 1 < 10)
                            GUI.Label(new Rect(98, 582, 12, 16), (setIndex + 1).ToString());
                        else
                            GUI.Label(new Rect(92, 582, 24, 16), (setIndex + 1).ToString());

                        GUI.Label(new Rect(116, 582, 12, 16), "/");
                        GUI.Label(new Rect(128, 582, 24, 16), currentSetIds.Length.ToString());

                        if (GUI.Button(new Rect(160, 582, 16, 16), ">"))
                        {
                            if (setIndex == currentSetIds.Length - 1)
                                setIndex = 0;
                            else
                                setIndex = setIndex + 1;

                            UpdatePreview();
                        }
                    }
                }
            }

            if (GUI.Button(new Rect(16, 612, 96, 20), "OK"))
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
                obj.type = GetCurrentObjectType();
                obj.name = searchListIDSets[objectPicker][setIndex];
                locationPrefab.obj.Add(obj);

                obj.objectID = newID;
                obj.extraData = extraData;
                AddObject(obj, selectNew: true);
                //locationPrefab.obj.Sort((a, b) => a.objectID.CompareTo(b.objectID));
                editMode = EditMode.EditLocation;
            }

            if (GUI.Button(new Rect(128, 612, 96, 20), "Cancel"))
            {
                editMode = EditMode.EditLocation;
            }
        }

        private GameObject CreateObject(LocationObject locationObject, Transform objectParent, ModelCombiner combiner = null)
        {
            if (!LocationHelper.ValidateValue(locationObject.type, locationObject.name))
                return null;

            if (locationObject.type == 2)
            {
                string[] arg = locationObject.name.Split('.');

                var newObject = GameObjectHelper.CreateDaggerfallBillboardGameObject(199, int.Parse(arg[1]), objectParent);
                newObject.transform.localPosition = locationObject.pos;
                if (LocationHelper.editor.TryGetValue(locationObject.name, out string editorName))
                {
                    newObject.name = editorName;
                }
                else
                {
                    newObject.name = $"Unknown editor marker ({locationObject.name})";
                }
                return newObject;
            }
            else if(locationObject.type == 3)
            {
                var newObject = new GameObject(locationObject.name);
                newObject.transform.parent = objectParent;
                newObject.transform.localPosition = locationObject.pos;

                if (!prefabInfos.TryGetValue(locationObject.name.ToLower(), out LocationPrefab prefabInfo))
                {
                    newObject.name = $"Unknown prefab ({locationObject.name})";
                }
                else
                {
                    bool topPrefab = false;
                    if(combiner == null)
                    {
                        combiner = new ModelCombiner();
                        topPrefab = true;
                    }
                    
                    foreach (LocationObject obj in prefabInfo.obj)
                    {
                        CreateObject(obj, newObject.transform, combiner);
                    }

                    if (topPrefab && combiner.VertexCount > 0)
                    {
                        combiner.Apply();
                        GameObjectHelper.CreateCombinedMeshGameObject(combiner, $"{locationObject.name}_CombinedModels", newObject.transform, makeStatic: true);
                    }
                }

                return newObject;
            }
            else
            {
                var newObject = LocationHelper.LoadStaticObject(locationObject.type, locationObject.name, objectParent,
                                     locationObject.pos,
                                     locationObject.rot,
                                     locationObject.scale, combiner
                );

                if (newObject != null)
                {
                    if (newObject.GetComponent<DaggerfallBillboard>())
                    {
                        float tempY = newObject.transform.position.y;
                        newObject.GetComponent<DaggerfallBillboard>().AlignToBase();
                        newObject.transform.position = new Vector3(newObject.transform.position.x, tempY + ((newObject.transform.position.y - tempY) * newObject.transform.localScale.y), newObject.transform.position.z);
                    }

                    if (locationObject.type == 0)
                    {
                        if (idName.TryGetValue(locationObject.name, out string modelName))
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
                        if (idName.TryGetValue(locationObject.name, out string billboardName))
                        {
                            newObject.name = billboardName;
                        }
                        else
                        {
                            newObject.name = $"Unknown billboard ({locationObject.name})";
                        }
                    }
                }

                return newObject;
            }
        }

        private void UpdatePreview()
        {
            if (preview != null)
            {
                DestroyImmediate(preview);
                preview = null;
            }

            if (searchListIDSets.Count > 0)
            {
                LocationObject previewObject = new LocationObject();

                previewObject.type = GetCurrentObjectType();
                previewObject.name = searchListIDSets[objectPicker][setIndex];

                preview = CreateObject(previewObject, null);
                preview.name = "Location Prefab Object Preview";
            }
        }

        private void AddObject(LocationObject locationObject, bool selectNew)
        {
            GameObject newObject = CreateObject(locationObject, parent.transform);            

            if (newObject != null)
            {
                objScene.Add(newObject);
                
                if (selectNew)
                    Selection.activeGameObject = objScene[objScene.Count - 1];
            }
            else
            {
                Debug.LogError("Failed to load object " + name);
            }
        }

        void AddName(string name, string[] ids)
        {
            if (ids.Length > 1)
            {
                searchListNames.Add($"{name} [{ids.Length}]");
            }
            else
            {
                searchListNames.Add(name);
            }

            searchListIDSets.Add(ids);
        }

        void AddNames(LocationHelper.ObjectSet[] setList)
        {
            if (string.IsNullOrEmpty(searchField))
            {
                foreach (var set in setList)
                {
                    AddName(set.Name, set.Ids);
                }
            }
            else
            {
                foreach (var set in setList
                            .Where(set => set.Name.IndexOf(searchField, StringComparison.InvariantCultureIgnoreCase) >= 0)
                            )
                {
                    AddName(set.Name, set.Ids);
                }
            }
        }

        void AddNames(Dictionary<string, string> editorList)
        {
            if (string.IsNullOrEmpty(searchField))
            {
                foreach (var kvp in editorList)
                {
                    AddName(kvp.Value, new string[] { kvp.Key });
                }
            }
            else
            {
                foreach (var kvp in editorList
                            .Where(kvp => kvp.Value.IndexOf(searchField, StringComparison.InvariantCultureIgnoreCase) >= 0)
                            )
                {
                    AddName(kvp.Value, new string[] { kvp.Key });
                }
            }
        }

        private void UpdateObjList()
        {
            searchListNames.Clear();
            searchListIDSets.Clear();

            if (listMode == 0)
            {
                switch (sublistMode)
                {
                    case 0:
                        AddNames(LocationHelper.models);
                        break;

                    case 1:
                        AddNames(LocationHelper.modelsStructure);
                        break;

                    case 2:
                        AddNames(LocationHelper.modelsClutter);
                        break;

                    case 3:
                        AddNames(LocationHelper.modelsDungeon);
                        break;

                    case 4:
                        AddNames(LocationHelper.modelsFurniture);
                        break;

                    case 5:
                        AddNames(LocationHelper.modelsGraveyard);
                        break;
                }
            }
            else if (listMode == 1)
            {
                switch (sublistMode)
                {
                    case 0:
                        AddNames(LocationHelper.billboards);
                        break;

                    case 1:
                        AddNames(LocationHelper.billboardsPeople);
                        break;

                    case 2:
                        AddNames(LocationHelper.billboardsInterior);
                        break;

                    case 3:
                        AddNames(LocationHelper.billboardsNature);
                        break;

                    case 4:
                        AddNames(LocationHelper.billboardsLights);
                        break;

                    case 5:
                        AddNames(LocationHelper.billboardsTreasure);
                        break;

                    case 6:
                        AddNames(LocationHelper.billboardsDungeon);
                        break;
                }
            }
            else if (listMode == 2)
            {
                AddNames(LocationHelper.editor);
            }
            else if (listMode == 3)
            {
                switch (sublistMode)
                {
                    case 0:
                        AddNames(LocationHelper.interiorParts);
                        break;

                    case 1:
                        AddNames(LocationHelper.houseParts);
                        break;

                    case 2:
                        AddNames(LocationHelper.dungeonPartsRooms);
                        break;

                    case 3:
                        AddNames(LocationHelper.dungeonPartsCorridors);
                        break;

                    case 4:
                        AddNames(LocationHelper.dungeonPartsMisc);
                        break;

                    case 5:
                        AddNames(LocationHelper.dungeonPartsCaves);
                        break;

                    case 6:
                        AddNames(LocationHelper.dungeonPartsDoors);
                        break;
                }
            }
            else if(listMode == 4)
            {
                var prefabs = prefabInfos.Keys.Where(prefab => prefab != currentPrefabName.ToLower());
                searchListNames.AddRange(prefabs);
                searchListIDSets.AddRange(prefabs.Select(prefab => new string[] { prefab }));
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

            if (preview != null)
                DestroyImmediate(preview);
        }
    }
#endif
}