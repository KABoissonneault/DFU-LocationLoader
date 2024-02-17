using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using FullSerializer;

namespace LocationLoader
{
    [System.Serializable]
    public class LocationObject
    {
        public int type = 0; //0 == Mesh, 1 == Billboard, 2 == Editor
        public string name = "";
        public int objectID = 0;
        public string extraData = ""; // Data specific to a given object type and name
        public Vector3 pos = Vector3.zero;
        public Quaternion rot = Quaternion.Euler(0, 0, 0);
        public Vector3 scale = new Vector3(1, 1, 1);
    }

    /// <summary>
    /// Holds data locationPrefab
    /// </summary>
    [System.Serializable]
    public class LocationPrefab
    {
        public int height = 8;
        public int width = 8;
        public List<LocationObject> obj = new List<LocationObject>();
        public string winterPrefab = "";

        public int HalfWidth
        {
            get { return (width + 1) / 2; }
        }

        public int TerrainWidth
        {
            get { return HalfWidth * 2; }
        }

        public int HalfHeight
        {
            get { return (height + 1) / 2; }
        }

        public int TerrainHeight
        {
            get { return HalfHeight * 2; }
        }
    }
    /// <summary>
    /// Holds data for locationInstances
    /// </summary>
    [System.Serializable]
    public class LocationInstance
    {
        public ulong locationID;
        public string name = "";
        public int type;
        public string prefab = "";
        public int worldX = 0;
        public int worldY = 0;
        public int terrainX = 0;
        public int terrainY = 0;
        public Quaternion rot = Quaternion.identity;
        public float sink = 0f;
        public float scale = 1f;
        public string extraData;

        public void UpdateLocationID()
        {            
            uint first = (uint)UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            uint second = (uint)UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            locationID = ((ulong)first) << 32 | second;                      
        }

        public bool TryGetExtraDataAsInt64(string key, out long value)
        {
            fsResult parseResult = fsJsonParser.Parse(extraData, out fsData data);
            if (parseResult.Failed || !data.IsDictionary)
            {
                value = 0;
                return false;
            }

            if(!data.AsDictionary.TryGetValue(key, out fsData valueData) || !valueData.IsInt64)
            {
                value = 0;
                return false;
            }

            value = valueData.AsInt64;
            return true;            
        }

        public void SetExtraDataField(string key, long value)
        {
            fsResult parseResult = fsJsonParser.Parse(extraData, out fsData data);
            if (parseResult.Failed || !data.IsDictionary)
            {
                data = new fsData(new Dictionary<string, fsData>());
            }

            data.AsDictionary[key] = new fsData(value);

            extraData = fsJsonPrinter.CompressedJson(data);
        }
    }

    public enum EditorMarkerTypes
    {
        Enter = 8,
        Start = 10,
        Quest = 11,
        RandomMonster = 15,
        Monster = 16,
        QuestItem = 18,
        RandomTreasure = 19,
        LadderBottom = 21,
        LadderTop = 22,
    }

    public struct WorldArea
    {
        public Vector2Int WorldCoord;
        public RectInt Area;
    }

    public class LocationData : MonoBehaviour
    {
        public static System.EventHandler OnLocationEnabled;

        [SerializeField]
        public LocationInstance Location;

        [SerializeField]
        public LocationPrefab Prefab;

        public float HeightOffset { get; set; }

        public float OverlapAverageHeight { get; set; }

        public bool HasSpawnedDynamicObjects = false;
                

        void OnEnable()
        {
            if(OnLocationEnabled != null)
            {
                OnLocationEnabled(this, null);
            }
        }

        public IEnumerable<WorldArea> GetOverlappingWorldAreas()
        {
            if(Location == null || Prefab == null)
            {
                yield break;
            }

            if (Location.type == 1)
            {
                int xOffsetMin = (int)Math.Floor((Location.terrainX - Prefab.HalfWidth) / (float)LocationLoader.TERRAIN_SIZE);
                int yOffsetMin = (int)Math.Floor((Location.terrainY - Prefab.HalfHeight) / (float)LocationLoader.TERRAIN_SIZE);
                int xOffsetMax = (Location.terrainX + Prefab.HalfWidth) / LocationLoader.TERRAIN_SIZE;
                int yOffsetMax = (Location.terrainY + Prefab.HalfHeight) / LocationLoader.TERRAIN_SIZE;

                // Find all overlapping coordinates and their overlap rectangle
                for (int xOffset = xOffsetMin; xOffset <= xOffsetMax; ++xOffset)
                {
                    for (int yOffset = yOffsetMin; yOffset <= yOffsetMax; ++yOffset)
                    {
                        int xMin = Math.Max(Location.terrainX - Prefab.HalfWidth - xOffset * LocationLoader.TERRAIN_SIZE, 0);
                        int xMax = Math.Min(Location.terrainX + Prefab.HalfWidth - xOffset * LocationLoader.TERRAIN_SIZE, 128);
                        int yMin = Math.Max(Location.terrainY - Prefab.HalfHeight - yOffset * LocationLoader.TERRAIN_SIZE, 0);
                        int yMax = Math.Min(Location.terrainY + Prefab.HalfHeight - yOffset * LocationLoader.TERRAIN_SIZE, 128);

                        yield return new WorldArea()
                        {
                            WorldCoord = new Vector2Int(Location.worldX + xOffset, Location.worldY + yOffset),
                            Area = new RectInt(xMin, yMin, xMax - xMin, yMax - yMin)
                        };
                    }
                }
            }
            else
            {
                yield return new WorldArea()
                {
                    WorldCoord = new Vector2Int(Location.worldX, Location.worldY),
                    Area = new RectInt(Location.terrainX - Prefab.HalfWidth, Location.terrainY - Prefab.HalfHeight, Prefab.TerrainWidth, Prefab.TerrainHeight)
                };
            }
        }

        public IEnumerable<Vector2Int> GetOverlappingCoordinates()
        {
            return GetOverlappingWorldAreas().Select(worldArea => worldArea.WorldCoord);
        }

        public bool FindClosestMarker(EditorMarkerTypes type, Vector3 sourcePos, out Vector3 closestMarkerOut)
        {
            bool foundOne = false;
            float minDistance = float.MaxValue;
            closestMarkerOut = Vector3.zero;
            string markerName = $"199.{(int)type}";

            foreach (var obj in Prefab.obj)
            {
                // Exclude markers of incorrect type
                if (obj.type != 2 || obj.name != markerName)
                    continue;

                // Refine to closest marker
                
                Vector3 markerPos = transform.position + transform.rotation * obj.pos;
                float distance = Vector3.Distance(sourcePos, markerPos);
                if (distance < minDistance || !foundOne)
                {
                    closestMarkerOut = markerPos;
                    minDistance = distance;
                    foundOne = true;
                }
            }

            if (!foundOne)
            {
                return false;
            }

            return true;
        }
    }
}