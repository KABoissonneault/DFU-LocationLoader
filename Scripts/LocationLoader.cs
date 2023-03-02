using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Utility;
using System;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Game.Entity;
using static DaggerfallWorkshop.Utility.ContentReader;
using DaggerfallWorkshop.Utility.AssetInjection;
using DaggerfallConnect;

namespace LocationLoader
{
    public class LocationLoader : MonoBehaviour
    {           
        Dictionary<Vector2Int, WeakReference<DaggerfallTerrain>> loadedTerrain = new Dictionary<Vector2Int, WeakReference<DaggerfallTerrain>>();
        Dictionary<Vector2Int, List<LocationData>> pendingIncompleteLocations = new Dictionary<Vector2Int, List<LocationData>>();
        Dictionary<ulong, List<Vector2Int>> instancePendingTerrains = new Dictionary<ulong, List<Vector2Int>>();

        LocationResourceManager resourceManager;

        public const int TERRAIN_SIZE = 128;
        public const int ROAD_WIDTH = 4; // Actually 2, but let's leave a bit of a gap   
        public const float TERRAINPIXELSIZE = 819.2f;
        public const float TERRAIN_SIZE_MULTI = TERRAINPIXELSIZE / TERRAIN_SIZE;

        bool sceneLoading = false; 

        void Start()
        {
            Debug.Log("Begin mod init: Location Loader");

            LocationConsole.RegisterCommands();
            LocationRMBVariant.RegisterCommands();
            resourceManager = GetComponent<LocationResourceManager>();

            Debug.Log("Finished mod init: Location Loader");
        }

        private void OnEnable()
        {
            DaggerfallTerrain.OnPromoteTerrainData += OnTerrainPromoted;
            StreamingWorld.OnInitWorld += StreamingWorld_OnInitWorld;
            StreamingWorld.OnUpdateTerrainsEnd += StreamingWorld_OnUpdateTerrainsEnd;
            LocationData.OnLocationEnabled += LocationData_OnLocationEnabled;
        }

        private void OnDisable()
        {
            DaggerfallTerrain.OnPromoteTerrainData -= OnTerrainPromoted;
            StreamingWorld.OnInitWorld -= StreamingWorld_OnInitWorld;
            StreamingWorld.OnUpdateTerrainsEnd -= StreamingWorld_OnUpdateTerrainsEnd;
            LocationData.OnLocationEnabled -= LocationData_OnLocationEnabled;
        }

        private void StreamingWorld_OnInitWorld()
        {
            sceneLoading = true;
        }

        private void StreamingWorld_OnUpdateTerrainsEnd()
        {
            if(sceneLoading)
            {
                StartCoroutine(InstantiateAllDynamicObjectsNextFrame());
            }
        }

        private void LocationData_OnLocationEnabled(object sender, EventArgs _)
        {
            if(!sceneLoading)
            {
                LocationData instance = sender as LocationData;
                if(instance != null && !instance.HasSpawnedDynamicObjects)
                {
                    InstantiateInstanceDynamicObjects(instance);
                }
            }
        }

        System.Collections.IEnumerator InstantiateAllDynamicObjectsNextFrame()
        {
            yield return new WaitForEndOfFrame();

            var instances = FindObjectsOfType<LocationData>();
            foreach (var instance in instances)
            {
                // Ignore embedded prefabs
                // Or instances which already have dynamic objects spawned
                if (instance.Location == null || instance.HasSpawnedDynamicObjects)
                    continue;

                InstantiateInstanceDynamicObjects(instance);
            }

            sceneLoading = false;

            yield break;
        }

        public bool TryGetTerrain(int worldX, int worldY, out DaggerfallTerrain terrain)
        {
            var worldCoord = new Vector2Int(worldX, worldY);
            if (loadedTerrain.TryGetValue(worldCoord, out WeakReference<DaggerfallTerrain> terrainReference))
            {
                if(terrainReference.TryGetTarget(out terrain))
                {
                    // Terrain has been pooled and placed somewhere else
                    // Happens with Distant Terrain
                    if(terrain.MapPixelX != worldX || terrain.MapPixelY != worldY)
                    {
                        loadedTerrain.Remove(worldCoord);
                        return false;
                    }
                    return true;
                }
                else
                {
                    loadedTerrain.Remove(worldCoord);
                }
            }

            terrain = null;
            return false;
        }
        
        void InstantiateInstanceDynamicObjects(LocationData locationData)
        {
            GameObject instance = locationData.gameObject;
            LocationInstance loc = locationData.Location;
            LocationPrefab locationPrefab = locationData.Prefab;

            var saveInterface = LocationModLoader.modObject.GetComponent<LocationSaveDataInterface>();

            foreach (LocationObject obj in locationPrefab.obj)
            {
                GameObject go = null;

                if (obj.type == 1)
                {
                    go = LocationHelper.LoadStaticObject(
                        obj.type,
                        obj.name,
                        instance.transform,
                        obj.pos,
                        obj.rot,
                        obj.scale
                        );
                }
                else if (obj.type == 2)
                {
                    string[] arg = obj.name.Split('.');

                    if (arg.Length != 2)
                    {
                        Debug.LogError($"Invalid type 2 obj name '{obj.name}' in prefab '{loc.prefab}'");
                        continue;
                    }

                    if (arg[0] == "199")
                    {
                        switch (arg[1])
                        {
                            case "16":
                                var extraData = (EnemyMarkerExtraData)SaveLoadManager.Deserialize(typeof(EnemyMarkerExtraData), obj.extraData);

                                if (!Enum.IsDefined(typeof(MobileTypes), extraData.EnemyId) && DaggerfallEntity.GetCustomCareerTemplate(extraData.EnemyId) == null)
                                {
                                    Debug.LogError($"Could not spawn enemy, unknown mobile type '{extraData.EnemyId}'");
                                    break;
                                }

                                ulong v = (uint)obj.objectID;
                                ulong loadId = (loc.locationID << 16) | v;

                                // Enemy is dead, don't spawn anything
                                
                                if (saveInterface.IsEnemyDead(loadId))
                                {
                                    break;
                                }

                                MobileTypes mobileType = (MobileTypes)extraData.EnemyId;
                                go = GameObjectHelper.CreateEnemy(TextManager.Instance.GetLocalizedEnemyName((int)mobileType), mobileType, obj.pos, MobileGender.Unspecified, instance.transform);
                                SerializableEnemy serializable = go.GetComponent<SerializableEnemy>();
                                if (serializable != null)
                                {
                                    Destroy(serializable);
                                }

                                DaggerfallEntityBehaviour behaviour = go.GetComponent<DaggerfallEntityBehaviour>();
                                EnemyEntity entity = (EnemyEntity)behaviour.Entity;
                                if (entity.MobileEnemy.Gender == MobileGender.Male)
                                {
                                    entity.Gender = Genders.Male;
                                }
                                else if (entity.MobileEnemy.Gender == MobileGender.Female)
                                {
                                    entity.Gender = Genders.Female;
                                }

                                if (extraData.TeamOverride != 0 && Enum.IsDefined(typeof(MobileTeams), extraData.TeamOverride))
                                {
                                    entity.Team = (MobileTeams)extraData.TeamOverride;
                                }

                                DaggerfallEnemy enemy = go.GetComponent<DaggerfallEnemy>();
                                if (enemy != null)
                                {
                                    enemy.LoadID = loadId;
                                    go.AddComponent<LocationEnemySerializer>();
                                }
                                break;

                            case "19":
                                {
                                    int iconIndex = UnityEngine.Random.Range(0, DaggerfallLootDataTables.randomTreasureIconIndices.Length);
                                    int iconRecord = DaggerfallLootDataTables.randomTreasureIconIndices[iconIndex];
                                    go = LocationHelper.CreateLootContainer(loc.locationID, obj.objectID, 216, iconRecord, instance.transform);
                                    go.transform.localPosition = obj.pos;
                                    break;
                                }
                        }
                    }
                }

                if (go != null)
                {
                    if (go.GetComponent<DaggerfallBillboard>())
                    {
                        float tempY = go.transform.position.y;
                        go.GetComponent<DaggerfallBillboard>().AlignToBase();
                        go.transform.position = new Vector3(go.transform.position.x, tempY + ((go.transform.position.y - tempY) * go.transform.localScale.y), go.transform.position.z);
                    }
                }
            }

            locationData.HasSpawnedDynamicObjects = true;
        }

        Vector3 GetLocationPosition(LocationData locationData, DaggerfallTerrain daggerTerrain)
        {
            var loc = locationData.Location;

            if (loc.type == 2)
            {
                return new Vector3(loc.terrainX * TERRAIN_SIZE_MULTI, DaggerfallUnity.Instance.TerrainSampler.OceanElevation * daggerTerrain.TerrainScale, loc.terrainY * TERRAIN_SIZE_MULTI);
            }
            else
            {
                float terrainHeightMax = DaggerfallUnity.Instance.TerrainSampler.MaxTerrainHeight * daggerTerrain.TerrainScale;
                float sinkOffset = Mathf.Lerp(0, locationData.HeightOffset, loc.sink);
                return new Vector3(loc.terrainX * TERRAIN_SIZE_MULTI, locationData.OverlapAverageHeight * terrainHeightMax + sinkOffset, loc.terrainY * TERRAIN_SIZE_MULTI);
            }
        }

        void SetActiveRecursively(GameObject go)
        {
            go.SetActive(true);
            foreach(Transform child in go.transform)
            {
                SetActiveRecursively(child.gameObject);
            }
        }

        void InstantiateTopLocationPrefab(string prefabName, float overlapAverageHeight, LocationPrefab locationPrefab, LocationInstance loc, DaggerfallTerrain daggerTerrain)
        {
            GameObject instance = resourceManager.InstantiateLocationPrefab(prefabName, locationPrefab, daggerTerrain.transform);

            LocationData data = instance.AddComponent<LocationData>();
            data.Location = loc;
            data.Prefab = locationPrefab;
            data.OverlapAverageHeight = overlapAverageHeight;

            if(loc.type == 1 && loc.sink > 0.0f)
            {
                FindAdjustedHeightOffset(data);
            }

            Vector3 terrainOffset = GetLocationPosition(data, daggerTerrain);
            instance.transform.localPosition = terrainOffset;
            instance.transform.localRotation = loc.rot;
            instance.transform.localScale = new Vector3(loc.scale, loc.scale, loc.scale);
            
            // Now that we have the LocationData, add it to "pending instances" if needed
            if(instancePendingTerrains.TryGetValue(loc.locationID, out List<Vector2Int> pendingTerrains))
            {
                foreach (Vector2Int terrainCoord in pendingTerrains)
                {
                    if(!pendingIncompleteLocations.TryGetValue(terrainCoord, out List<LocationData> terrainPendingLocations))
                    {
                        terrainPendingLocations = new List<LocationData>();
                        pendingIncompleteLocations.Add(terrainCoord, terrainPendingLocations);
                    }

                    terrainPendingLocations.Add(data);
                }
            }

            SetActiveRecursively(instance);

            if (!sceneLoading)
            {
                InstantiateInstanceDynamicObjects(data);
            }
        }

        bool IsInSnowFreeClimate(DaggerfallTerrain daggerTerrain)
        {
            int climateIndex = DaggerfallUnity.Instance.ContentReader.MapFileReader.GetClimateIndex(daggerTerrain.MapPixelX, daggerTerrain.MapPixelY);
            return WeatherManager.IsSnowFreeClimate(climateIndex);
        }
        
        void OnTerrainPromoted(DaggerfallTerrain daggerTerrain, TerrainData terrainData)
        {
            Vector2Int worldLocation = new Vector2Int(daggerTerrain.MapPixelX, daggerTerrain.MapPixelY);
            loadedTerrain[worldLocation] = new WeakReference<DaggerfallTerrain>(daggerTerrain);
            
            // Terrain can be reused in terrain mods (ex: Distant Terrain)
            // Delete existing locations left on reused terrain
            foreach(var existingLoot in daggerTerrain.GetComponentsInChildren<LocationLootSerializer>())
            {
                existingLoot.InvalidateSave();
            }

            foreach (var existingEnemy in daggerTerrain.GetComponentsInChildren<LocationEnemySerializer>())
            {
                existingEnemy.InvalidateSave();
            }

            foreach (var existingLocation in daggerTerrain.GetComponentsInChildren<LocationData>())
            {
                Destroy(existingLocation.gameObject);
            }

            bool terrainOccupied = false;

            // Spawn the terrain's instances            
            foreach (LocationInstance loc in resourceManager.GetTerrainInstances(daggerTerrain))
            {
                string context = $"location=\"{loc.name}\"";

                if (daggerTerrain.MapData.hasLocation)
                {
                    if (loc.type == 0)
                    {
                        Debug.LogWarning($"Location already present at ({daggerTerrain.MapPixelX}, {daggerTerrain.MapPixelY}) ({context})");
                        continue;
                    }
                }

                LocationPrefab locationPrefab = resourceManager.GetPrefabInfo(loc.prefab);
                if (locationPrefab == null)
                    continue;

                if(DaggerfallUnity.Instance.WorldTime.Now.SeasonValue == DaggerfallDateTime.Seasons.Winter
                    && !IsInSnowFreeClimate(daggerTerrain)
                    && !string.IsNullOrEmpty(locationPrefab.winterPrefab))
                {
                    var winterPrefab = resourceManager.GetPrefabInfo(locationPrefab.winterPrefab);
                    if (winterPrefab == null)
                        Debug.LogError($"Winter prefab '{locationPrefab.winterPrefab}' could not be loaded");
                    else
                        locationPrefab = winterPrefab;
                }

                if (LocationHelper.IsOutOfBounds(loc, locationPrefab))
                {
                    Debug.LogWarning($"Out-of-bounds location at ({daggerTerrain.MapPixelX}, {daggerTerrain.MapPixelY}) ({context})");
                    continue;
                }

                if(PruneInstance(loc, locationPrefab))
                {
                    continue;
                }

                if (loc.type == 2)
                {
                    // We find and adjust the type 2 instance position here
                    // So that terrain can be flattened in consequence
                    // If the current tile has no coast and adjacent terrain are not loaded,
                    // then we don't care about flattening, since it means the instance
                    // is gonna be on water at the edge of this tile anyway
                    if (FindNearestCoast(loc, daggerTerrain, out Vector2Int coastTileCoord))
                    {
                        loc.terrainX = coastTileCoord.x;
                        loc.terrainY = coastTileCoord.y;
                    }
                }

                if(loc.type == 0 || loc.type == 2)
                {
                    if(terrainOccupied)
                    {
                        Debug.LogWarning($"Location instance already present at ({daggerTerrain.MapPixelX}, {daggerTerrain.MapPixelY}) ({context})");
                        continue;
                    }
                }

                //Smooth the terrain
                int count = 0;
                float tmpAverageHeight = 0;

                // Treating odd dimensions as ceiled-to-even
                int halfWidth = (locationPrefab.width + 1) / 2;
                int halfHeight = (locationPrefab.height + 1) / 2;

                // Type 1 instances can overlap beyond terrain boundaries
                // Estimate height using only the part in the current terrain tile for now
                int minX = Math.Max(loc.terrainX - halfWidth, 0);
                int minY = Math.Max(loc.terrainY - halfHeight, 0);
                int maxX = Math.Min(loc.terrainX + halfWidth, 128);
                int maxY = Math.Min(loc.terrainY + halfHeight, 128);
                for (int y = minY; y <= maxY; y++)
                {
                    for (int x = minX; x <= maxX; x++)
                    {
                        tmpAverageHeight += daggerTerrain.MapData.heightmapSamples[y, x];
                        count++;
                    }
                }

                var averageHeight = tmpAverageHeight /= count;

                if (loc.type == 0 || loc.type == 2)
                {
                    daggerTerrain.MapData.locationRect = new Rect(minX, minY, maxX - minX, maxY - minY);
                    terrainOccupied = true;

                    var locationRect = new Rect(minX, minY, maxX - minX, maxY - minY);

                    for (int y = 1; y < 127; y++)
                    {
                        for (int x = 1; x < 127; x++)
                        {
                            // Don't flatten heightmap samples touching water tiles
                            if (daggerTerrain.MapData.tilemapSamples[x, y] == 0
                                || daggerTerrain.MapData.tilemapSamples[x + 1, y] == 0
                                || daggerTerrain.MapData.tilemapSamples[x, y + 1] == 0
                                || daggerTerrain.MapData.tilemapSamples[x + 1, y + 1] == 0)
                            {
                                continue;
                            }

                            daggerTerrain.MapData.heightmapSamples[y, x] = Mathf.Lerp(daggerTerrain.MapData.heightmapSamples[y, x], averageHeight, 1 / (GetDistanceFromRect(daggerTerrain.MapData.locationRect, new Vector2(x, y)) + 1));
                        }
                    }
                }

                terrainData.SetHeights(0, 0, daggerTerrain.MapData.heightmapSamples);

                InstantiateTopLocationPrefab(loc.prefab, averageHeight, locationPrefab, loc, daggerTerrain);
            }

            // Check for pending instances waiting on this terrain
            if(pendingIncompleteLocations.TryGetValue(worldLocation, out List<LocationData> pendingLocations))
            {
                for(int i = 0; i < pendingLocations.Count; ++i)
                {
                    LocationData pendingLoc = pendingLocations[i];

                    if(pendingLoc == null)
                    {
                        // We got no info left on this instance
                        continue;
                    }

                    if(!instancePendingTerrains.TryGetValue(pendingLoc.Location.locationID, out List<Vector2Int> pendingTerrains))
                    {
                        // Invalid locations?
                        continue;
                    }

                    // Removes the instance from all "pending terrains"
                    void ClearPendingInstance()
                    {
                        foreach(Vector2Int pendingTerrainCoord in pendingTerrains)
                        {
                            if (pendingTerrainCoord == worldLocation)
                                continue;

                            if(pendingIncompleteLocations.TryGetValue(pendingTerrainCoord, out List<LocationData> pendingTerrainPendingInstances))
                            {
                                pendingTerrainPendingInstances.Remove(pendingLoc);
                                if (pendingTerrainPendingInstances.Count == 0)
                                    pendingIncompleteLocations.Remove(pendingTerrainCoord);
                            }
                        }

                        instancePendingTerrains.Remove(pendingLoc.Location.locationID);
                    }

                    if(!TryGetTerrain(pendingLoc.WorldX, pendingLoc.WorldY, out DaggerfallTerrain pendingLocTerrain))
                    {
                        // Terrain the location was on has expired
                        ClearPendingInstance();
                        continue;
                    }

                    // Type 2 location try to see if they found a coast to snap to
                    if (pendingLoc.Location.type == 2)
                    {
                        if (FindNearestCoast(pendingLoc.Location, pendingLocTerrain, out Vector2Int coastCoord))
                        {
                            pendingLoc.Location.terrainX = coastCoord.x;
                            pendingLoc.Location.terrainY = coastCoord.y;
                            pendingLoc.gameObject.transform.localPosition = GetLocationPosition(pendingLoc, pendingLocTerrain);

                            // Instance is not pending anymore
                            ClearPendingInstance();
                            continue;
                        }
                    }
                    // Adjust type 1 location height sink
                    else if(pendingLoc.Location.type == 1)
                    {
                        if (FindAdjustedHeightOffset(pendingLoc))
                        {
                            pendingLoc.gameObject.transform.localPosition = GetLocationPosition(pendingLoc, pendingLocTerrain);
                            // Instance is not pending anymore
                            ClearPendingInstance();
                            continue;
                        }
                        else
                        {
                            pendingLoc.gameObject.transform.localPosition = GetLocationPosition(pendingLoc, pendingLocTerrain);
                        }
                    }

                    // Remove this terrain from the location's pending terrains
                    pendingTerrains.Remove(worldLocation);
                }

                pendingIncompleteLocations.Remove(worldLocation);
            }
        }

        bool PruneInstance(LocationInstance loc, LocationPrefab prefab)
        {
            foreach(var terrainSection in LocationHelper.GetOverlappingTerrainSections(loc, prefab))
            {
                Vector2Int worldCoord = terrainSection.WorldCoord;

                if(DaggerfallUnity.Instance.ContentReader.HasLocation(worldCoord.x, worldCoord.y, out MapSummary summary))
                {
                    // Check World Data locations only
                    if(WorldDataReplacement.GetDFLocationReplacementData(summary.RegionIndex, summary.MapIndex, out DFLocation wdLoc))
                    {
                        int locationWidth = wdLoc.Exterior.ExteriorData.Width;
                        int locationHeight = wdLoc.Exterior.ExteriorData.Height;
                        int locationX = (RMBLayout.RMBTilesPerTerrain - locationWidth * RMBLayout.RMBTilesPerBlock) / 2;
                        int locationY = (RMBLayout.RMBTilesPerTerrain - locationHeight * RMBLayout.RMBTilesPerBlock) / 2;
                        RectInt locationArea = new RectInt(locationX, locationY, locationWidth * RMBLayout.RMBTilesPerBlock, locationHeight * RMBLayout.RMBTilesPerBlock);

                        // Instance is on a World Data location. Prune it
                        if (locationArea.Overlaps(terrainSection.Section))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
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

        // Takes a type 2 Location instance and searches for the nearest coast it can snap to
        // Returns true if an answer was found
        // If false, and any surrounding terrains weren't loaded, it will be added to the list of instances waiting on terrain
        bool FindNearestCoast(LocationInstance loc, DaggerfallTerrain daggerTerrain, out Vector2Int tileCoord)
        {
            byte GetTerrainSample(DaggerfallTerrain terrain, int x, int y)
            {
                return terrain.MapData.tilemapSamples[x, y];
            }

            byte GetSample(int x, int y)
            {
                return GetTerrainSample(daggerTerrain, x, y);
            }

            byte initial = GetSample(loc.terrainX, loc.terrainY);

            // If we start in water
            if(initial == 0)
            {
                // Find non-water in any direction
                for(int i = 1;;++i)
                {
                    bool anyValid = false;

                    // North
                    if(loc.terrainY + i < 128)
                    {
                        if(GetSample(loc.terrainX, loc.terrainY + i) != 0)
                        {
                            tileCoord = new Vector2Int(loc.terrainX, loc.terrainY + i - 1);
                            return true;
                        }
                        anyValid = true;
                    }

                    // East
                    if (loc.terrainX + i < 128)
                    {
                        if (GetSample(loc.terrainX + i, loc.terrainY) != 0)
                        {
                            tileCoord = new Vector2Int(loc.terrainX + i - 1, loc.terrainY);
                            return true;
                        }
                        anyValid = true;
                    }

                    // South
                    if (loc.terrainY - i >= 0)
                    {
                        if (GetSample(loc.terrainX, loc.terrainY - i) != 0)
                        {
                            tileCoord = new Vector2Int(loc.terrainX, loc.terrainY - i + 1);
                            return true;
                        }
                        anyValid = true;
                    }

                    // West
                    if (loc.terrainX - i >= 0)
                    {
                        if (GetSample(loc.terrainX - i, loc.terrainY) != 0)
                        {
                            tileCoord = new Vector2Int(loc.terrainX - i + 1, loc.terrainY);
                            return true;
                        }
                        anyValid = true;
                    }

                    if (!anyValid)
                        break;
                }
                                
                // Look the edges of adjacent terrain
                if (GetNorthNeighbor(daggerTerrain, out DaggerfallTerrain northNeighbor))
                {
                    if(GetTerrainSample(northNeighbor, loc.terrainX, 0) != 0)
                    {
                        tileCoord = new Vector2Int(loc.terrainX, 127);
                        return true;
                    }
                }

                if (GetEastNeighbor(daggerTerrain, out DaggerfallTerrain eastNeighbor))
                {
                    if (GetTerrainSample(eastNeighbor, 0, loc.terrainY) != 0)
                    {
                        tileCoord = new Vector2Int(127, loc.terrainY);
                        return true;
                    }
                }

                if (GetSouthNeighbor(daggerTerrain, out DaggerfallTerrain southNeighbor))
                {
                    if (GetTerrainSample(southNeighbor, loc.terrainX, 127) != 0)
                    {
                        tileCoord = new Vector2Int(loc.terrainX, 0);
                        return true;
                    }
                }

                if (GetWestNeighbor(daggerTerrain, out DaggerfallTerrain westNeighbor))
                {
                    if (GetTerrainSample(westNeighbor, 127, loc.terrainY) != 0)
                    {
                        tileCoord = new Vector2Int(0, loc.terrainY);
                        return true;
                    }
                }

                List<Vector2Int> pendingTerrain = new List<Vector2Int>();
                if(northNeighbor == null && loc.worldY != 0)
                {
                    pendingTerrain.Add(new Vector2Int(loc.worldX, loc.worldY - 1));
                }

                if (eastNeighbor == null && loc.worldX != 1000)
                {
                    pendingTerrain.Add(new Vector2Int(loc.worldX + 1, loc.worldY));
                }

                if (southNeighbor == null && loc.worldY != 500)
                {
                    pendingTerrain.Add(new Vector2Int(loc.worldX, loc.worldY + 1));
                }

                if (westNeighbor == null && loc.worldX != 0)
                {
                    pendingTerrain.Add(new Vector2Int(loc.worldX - 1, loc.worldY));
                }

                if (pendingTerrain.Count != 0)
                {
                    instancePendingTerrains[loc.locationID] = pendingTerrain;
                }
            }
            else
            {
                // Find water in any direction
                for (int i = 1; ; ++i)
                {
                    bool anyValid = false;

                    // North
                    if (loc.terrainY + i < 128)
                    {
                        if (GetSample(loc.terrainX, loc.terrainY + i) == 0)
                        {
                            tileCoord = new Vector2Int(loc.terrainX, loc.terrainY + i);
                            return true;
                        }
                        anyValid = true;
                    }

                    // East
                    if (loc.terrainX + i < 128)
                    {
                        if (GetSample(loc.terrainX + i, loc.terrainY) == 0)
                        {
                            tileCoord = new Vector2Int(loc.terrainX + i, loc.terrainY);
                            return true;
                        }
                        anyValid = true;
                    }

                    // South
                    if (loc.terrainY - i >= 0)
                    {
                        if (GetSample(loc.terrainX, loc.terrainY - i) == 0)
                        {
                            tileCoord = new Vector2Int(loc.terrainX, loc.terrainY - i);
                            return true;
                        }
                        anyValid = true;
                    }

                    // West
                    if (loc.terrainX - i >= 0)
                    {
                        if (GetSample(loc.terrainX - i, loc.terrainY) == 0)
                        {
                            tileCoord = new Vector2Int(loc.terrainX - i, loc.terrainY);
                            return true;
                        }
                        anyValid = true;
                    }

                    if (!anyValid)
                        break;
                }
            }

            tileCoord = new Vector2Int(loc.terrainX, loc.terrainY);
            return false;
        }

        bool GetNorthNeighbor(DaggerfallTerrain daggerTerrain, out DaggerfallTerrain northNeighbor)
        {
            if(daggerTerrain.MapPixelY == 0)
            {
                northNeighbor = null;
                return false;
            }

            return TryGetTerrain(daggerTerrain.MapPixelX, daggerTerrain.MapPixelY - 1, out northNeighbor);
        }

        bool GetEastNeighbor(DaggerfallTerrain daggerTerrain, out DaggerfallTerrain eastNeighbor)
        {
            if (daggerTerrain.MapPixelX == 1000)
            {
                eastNeighbor = null;
                return false;
            }

            return TryGetTerrain(daggerTerrain.MapPixelX + 1, daggerTerrain.MapPixelY, out eastNeighbor);
        }

        bool GetSouthNeighbor(DaggerfallTerrain daggerTerrain, out DaggerfallTerrain southNeighbor)
        {
            if (daggerTerrain.MapPixelY == 500)
            {
                southNeighbor = null;
                return false;
            }

            return TryGetTerrain(daggerTerrain.MapPixelX, daggerTerrain.MapPixelY + 1, out southNeighbor);
        }

        bool GetWestNeighbor(DaggerfallTerrain daggerTerrain, out DaggerfallTerrain westNeighbor)
        {
            if (daggerTerrain.MapPixelX == 0)
            {
                westNeighbor = null;
                return false;
            }

            return TryGetTerrain(daggerTerrain.MapPixelX - 1, daggerTerrain.MapPixelY, out westNeighbor);
        }

        // Returns true if the loc is done adjusting
        bool FindAdjustedHeightOffset(LocationData locationData)
        {
            var loc = locationData.Location;
            var locationPrefab = locationData.Prefab;

            if (!TryGetTerrain(loc.worldX, loc.worldY, out DaggerfallTerrain locBaseTerrain))
            {
                return true;
            }

            float baseHeightMax = DaggerfallUnity.Instance.TerrainSampler.MaxTerrainHeight * locBaseTerrain.TerrainScale;
            float baseHeightAverage = locationData.OverlapAverageHeight * baseHeightMax;

            List<Vector2Int> pendingTerrain = new List<Vector2Int>();

            foreach (LocationHelper.TerrainSection terrainSection in LocationHelper.GetOverlappingTerrainSections(loc, locationPrefab))
            {
                if(!TryGetTerrain(terrainSection.WorldCoord.x, terrainSection.WorldCoord.y, out DaggerfallTerrain sectionTerrain))
                {
                    pendingTerrain.Add(terrainSection.WorldCoord);
                    continue;
                }

                float terrainHeightMax = DaggerfallUnity.Instance.TerrainSampler.MaxTerrainHeight * sectionTerrain.TerrainScale;

                for (int i = terrainSection.Section.min.x; i <= terrainSection.Section.max.x; i++)
                {
                    for(int j = terrainSection.Section.min.y; j <= terrainSection.Section.max.y; j++)
                    {
                        float sampleHeight = sectionTerrain.MapData.heightmapSamples[j, i];
                        float unitHeight = sampleHeight * terrainHeightMax;

                        float currentHeight = baseHeightAverage + locationData.HeightOffset;

                        if (unitHeight < currentHeight)
                        {
                            locationData.HeightOffset = unitHeight - baseHeightAverage;
                        }
                    }
                }
            }

            if(pendingTerrain.Count > 0)
            {
                instancePendingTerrains[loc.locationID] = pendingTerrain;

                return false;
            }

            return true;
        }
    }
}
