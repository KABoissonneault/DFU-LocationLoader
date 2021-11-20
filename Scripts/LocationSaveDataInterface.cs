using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Utility.AssetInjection;
using DaggerfallWorkshop.Utility;
using FullSerializer;

namespace LocationLoader
{
    [fsObject("v1")]
    public class LocationLootData_v1
    {
        public ulong loadID;
        public LootContainerTypes containerType;
        public InventoryContainerImages containerImage;
        public Vector3 currentPosition;
        public Vector3 localPosition;
        public Vector3 worldCompensation;
        public float heightScale;
        public int textureArchive;
        public int textureRecord;
        public string lootTableKey;
        public string entityName;
        public int stockedDate;
        public bool playerOwned;
        public bool customDrop;
        public bool isEnemyClass;
        public ItemData_v1[] items;
    }

    [fsObject("v1")]
    struct LocationSaveData_v1
    {
        public EnemyData_v1[] enemyData;
        public LocationLootData_v1[] lootContainers;
    }

    class LocationLootSerializer : MonoBehaviour, ISerializableGameObject
    {
        #region Fields

        DaggerfallLoot loot;
        Vector3 worldCompensation;

        #endregion

        #region Unity

        void Awake()
        {
            loot = GetComponent<DaggerfallLoot>();
            if (!loot)
                throw new Exception("DaggerfallLoot not found.");
        }

        void Start()
        {
            if(LoadID != 0)
            {
                RefreshWorldCompensation();
                GetSaveDataInterface().RegisterActiveSerializer(this);
            }
        }

        void OnDestroy()
        {
            if (LoadID != 0)
            {
                if (LocationModLoader.modObject != null)
                {
                    var saveDataInterface = GetSaveDataInterface();
                    saveDataInterface.SerializeLoot(this);
                    saveDataInterface.DeregisterActiveSerializer(this);
                }
            }
        }
        
        #endregion

        public bool TryLoadSavedData()
        {
            if (LoadID == 0)
                return false;

            return GetSaveDataInterface().TryDeserializeLoot(this);
        }

        public void RefreshWorldCompensation()
        {
            worldCompensation = GameManager.Instance.StreamingWorld.WorldCompensation;
        }

        LocationSaveDataInterface GetSaveDataInterface()
        {
            return LocationModLoader.modObject.GetComponent<LocationSaveDataInterface>();
        }

        #region ISerializableGameObject
        public ulong LoadID { get { return loot.LoadID; } }
        public bool ShouldSave { get { return true; } }

        public object GetSaveData()
        {
            if (!loot)
                return null;

            // Create save data
            LocationLootData_v1 data = new LocationLootData_v1();
            data.loadID = LoadID;
            data.containerType = loot.ContainerType;
            data.containerImage = loot.ContainerImage;
            data.currentPosition = loot.transform.position;
            data.localPosition = loot.transform.localPosition;
            data.worldCompensation = worldCompensation;
            data.heightScale = loot.transform.localScale.y;
            data.textureArchive = loot.TextureArchive;
            data.textureRecord = loot.TextureRecord;
            data.stockedDate = loot.stockedDate;
            data.playerOwned = loot.playerOwned;
            data.customDrop = loot.customDrop;
            data.items = loot.Items.SerializeItems();
            data.entityName = loot.entityName;
            data.isEnemyClass = loot.isEnemyClass;

            return data;
        }

        public void RestoreSaveData(object dataIn)
        {
            if (!loot)
                return;

            LocationLootData_v1 data = (LocationLootData_v1)dataIn;
            if (data.loadID != LoadID)
                return;

            // Restore billboard only if this is a billboard-based loot container
            if (loot.ContainerType == LootContainerTypes.RandomTreasure ||
                loot.ContainerType == LootContainerTypes.CorpseMarker ||
                loot.ContainerType == LootContainerTypes.DroppedLoot)
            {
                DaggerfallBillboard billboard = loot.GetComponent<DaggerfallBillboard>();

                float diffY = GameManager.Instance.StreamingWorld.WorldCompensation.y - data.worldCompensation.y;
                loot.transform.position = data.currentPosition + new Vector3(0, diffY, 0);

                // Restore appearance
                if (MeshReplacement.SwapCustomFlatGameobject(data.textureArchive, data.textureRecord, loot.transform, Vector3.zero, inDungeon: false))
                {
                    // Use imported model instead of billboard
                    if (billboard) Destroy(billboard);
                    Destroy(GetComponent<MeshRenderer>());
                }
                else
                {
                    // Restore billboard if previously replaced by custom model
                    // This happens when the record is changed and new model is not provided by mods
                    if (!billboard)
                        billboard = loot.transform.gameObject.AddComponent<DaggerfallBillboard>();

                    // Restore billboard appearance
                    billboard.SetMaterial(data.textureArchive, data.textureRecord);

                    // Fix position if custom scale changed
                    if (data.heightScale == 0)
                        data.heightScale = 1;
                    if (data.heightScale != billboard.transform.localScale.y)
                    {
                        float height = billboard.Summary.Size.y * (data.heightScale / billboard.transform.localScale.y);
                        billboard.transform.Translate(0, (billboard.Summary.Size.y - height) / 2f, 0);
                    }
                }
            }

            // Restore items
            loot.Items.DeserializeItems(data.items);

            // Restore other data
            loot.ContainerType = data.containerType;
            loot.ContainerImage = data.containerImage;
            loot.TextureArchive = data.textureArchive;
            loot.TextureRecord = data.textureRecord;
            loot.stockedDate = data.stockedDate;
            loot.playerOwned = data.playerOwned;
            loot.customDrop = data.customDrop;
            loot.entityName = data.entityName;
            loot.isEnemyClass = data.isEnemyClass;

            // Remove loot container if empty
            if (loot.Items.Count == 0)
                GameObjectHelper.RemoveLootContainer(loot);
        }


        #endregion
    }

    class LocationSaveDataInterface : MonoBehaviour, IHasModSaveData
    {
        Dictionary<ulong, LocationLootData_v1> savedLoot = new Dictionary<ulong, LocationLootData_v1>();
        Dictionary<ulong, LocationLootSerializer> activeLootSerializers = new Dictionary<ulong, LocationLootSerializer>();

        #region Unity
        void OnEnable()
        {
            StreamingWorld.OnFloatingOriginChange += StreamingWorld_OnFloatingOriginChange;
        }

        void OnDisable()
        {
            StreamingWorld.OnFloatingOriginChange -= StreamingWorld_OnFloatingOriginChange;
        }

        private void StreamingWorld_OnFloatingOriginChange()
        {
            foreach (var kvp in activeLootSerializers)
            {
                kvp.Value.RefreshWorldCompensation();
            }
        }

        #endregion

        public void SerializeLoot(LocationLootSerializer serializer)
        {
            savedLoot[serializer.LoadID] = (LocationLootData_v1)serializer.GetSaveData();
        }

        public bool TryDeserializeLoot(LocationLootSerializer serializer)
        {
            if(!savedLoot.TryGetValue(serializer.LoadID, out LocationLootData_v1 value))
                return false;

            serializer.RestoreSaveData(value);
            return true;
        }

        public void RegisterActiveSerializer(LocationLootSerializer serializer)
        {
            activeLootSerializers.Add(serializer.LoadID, serializer);
        }

        public void DeregisterActiveSerializer(LocationLootSerializer serializer)
        {
            activeLootSerializers.Remove(serializer.LoadID);
        }

        void FlushActiveInstances()
        {
            foreach(var kvp in activeLootSerializers)
            {
                SerializeLoot(kvp.Value);
            }
        }

        void ReloadActiveInstances()
        {
            foreach (var kvp in activeLootSerializers)
            {
                TryDeserializeLoot(kvp.Value);
            }
        }

        /// <summary>
        /// The type of a custom class that holds save data and optionally use <see cref="FullSerializer.fsObjectAttribute"/> for versioning.
        /// </summary>
        public Type SaveDataType { get { return typeof(LocationSaveData_v1); } }

        /// <summary>
        /// Makes a new instance of <see cref="SaveDataType"/> with default values.
        /// </summary>
        public object NewSaveData()
        {
            LocationSaveData_v1 defaultData = new LocationSaveData_v1();
            defaultData.lootContainers = new LocationLootData_v1[0];
            defaultData.enemyData = new EnemyData_v1[0];
            return defaultData;
        }

        /// <summary>
        /// Makes a new instance of <see cref="SaveDataType"/> for the current state or null if there is nothing to save.
        /// </summary>
        public object GetSaveData()
        {
            FlushActiveInstances();

            LocationSaveData_v1 data = new LocationSaveData_v1();

            data.lootContainers = savedLoot.Values.ToArray();

            return data;
        }

        /// <summary>
        /// Restores retrieved data when a save is loaded.
        /// </summary>
        /// <param name="saveData">An instance of <see cref="SaveDataType"/>.</param>
        public void RestoreSaveData(object saveData)
        {
            LocationSaveData_v1 data = (LocationSaveData_v1)saveData;

            savedLoot = data.lootContainers.ToDictionary(loot => loot.loadID);

            ReloadActiveInstances();
        }
    }
}