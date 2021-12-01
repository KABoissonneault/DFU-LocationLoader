using System.Collections.Generic;
using UnityEngine;

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
        public float heightOffset = 0f;

        public void UpdateLocationID()
        {            
            uint first = (uint)Random.Range(int.MinValue, int.MaxValue);
            uint second = (uint)Random.Range(int.MinValue, int.MaxValue);
            locationID = ((ulong)first) << 32 | second;                      
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

    public class LocationData : MonoBehaviour
    {
        [SerializeField]
        public LocationInstance Location;

        [SerializeField]
        public LocationPrefab Prefab;

        public bool FindClosestMarker(EditorMarkerTypes type, Vector3 sourcePos, out Vector3 closestMarkerOut)
        {
            bool foundOne = false;
            float minDistance = float.MaxValue;
            closestMarkerOut = Vector3.zero;
            foreach (var obj in Prefab.obj)
            {
                string markerName = $"199.{(int)type}";
                // Exclude markers of incorrect type
                if (obj.type != 2 || obj.name != markerName)
                    continue;

                // Refine to closest marker
                Vector3 markerPos = transform.position + obj.pos;
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