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
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.Questing;

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
    public struct LocationSaveData_v1
    {
        public ulong[] clearedEnemies;
        public EnemyData_v1[] activeEnemies;
        public LocationLootData_v1[] lootContainers;
    }

    public class LocationLootSerializer : MonoBehaviour, ISerializableGameObject
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
                float diffY = GameManager.Instance.StreamingWorld.WorldCompensation.y - data.worldCompensation.y;
                loot.transform.position = data.currentPosition + new Vector3(0, diffY, 0);

                DaggerfallBillboard billboard = loot.GetComponent<DaggerfallBillboard>();
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

    public class LocationEnemySerializer : MonoBehaviour, ISerializableGameObject
    {
        #region Fields

        DaggerfallEnemy enemy;
        Vector3 worldCompensation;

        #endregion

        #region Unity

        void Awake()
        {
            enemy = GetComponent<DaggerfallEnemy>();
            if (!enemy)
                throw new Exception("DaggerfallEnemy not found.");
        }

        void Start()
        {
            if (LoadID != 0)
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
                    if (IsDead())
                        GetSaveDataInterface().AddDeadEnemy(this);
                    GetSaveDataInterface().DeregisterActiveSerializer(this);
                }
            }                
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            UnityEditor.Handles.Label(transform.position + Vector3.up, "LoadID=" + LoadID);
        }
#endif

        #endregion

        public bool IsDead()
        {
            DaggerfallEntityBehaviour entityBehaviour = enemy.GetComponent<DaggerfallEntityBehaviour>();
            if (!entityBehaviour)
                return false;

            EnemyEntity entity = (EnemyEntity)entityBehaviour.Entity;
            return entity.CurrentHealth <= 0;
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

        public ulong LoadID { get { return GetLoadID(); } }
        public bool ShouldSave { get { return HasChanged(); } }

        public object GetSaveData()
        {
            if (!enemy)
                return null;

            // Get entity behaviour
            DaggerfallEntityBehaviour entityBehaviour = enemy.GetComponent<DaggerfallEntityBehaviour>();
            if (!entityBehaviour)
                return null;

            // Create save data
            EnemyEntity entity = (EnemyEntity)entityBehaviour.Entity;
            EnemyMotor motor = enemy.GetComponent<EnemyMotor>();
            EnemySenses senses = enemy.GetComponent<EnemySenses>();
            var mobileEnemy = enemy.GetComponentInChildren<MobileUnit>();
            EnemyData_v1 data = new EnemyData_v1();
            data.loadID = LoadID;
            data.gameObjectName = entityBehaviour.gameObject.name;
            data.currentPosition = enemy.transform.position;
            data.localPosition = enemy.transform.localPosition;
            data.currentRotation = enemy.transform.rotation;
            data.worldContext = entity.WorldContext;
            data.worldCompensation = worldCompensation;
            data.entityType = entity.EntityType;
            data.careerName = entity.Career.Name;
            data.careerIndex = entity.CareerIndex;
            data.startingHealth = entity.MaxHealth;
            data.currentHealth = entity.CurrentHealth;
            data.currentFatigue = entity.CurrentFatigue;
            data.currentMagicka = entity.CurrentMagicka;
            data.isHostile = motor.IsHostile;
            data.hasEncounteredPlayer = senses.HasEncounteredPlayer;
            data.isDead = entity.CurrentHealth <= 0;
            data.questSpawn = enemy.QuestSpawn;
            data.mobileGender = mobileEnemy.Enemy.Gender;
            data.items = entity.Items.SerializeItems();
            data.equipTable = entity.ItemEquipTable.SerializeEquipTable();
            data.instancedEffectBundles = GetComponent<EntityEffectManager>().GetInstancedBundlesSaveData();
            data.alliedToPlayer = mobileEnemy.Enemy.Team == MobileTeams.PlayerAlly;
            data.questFoeSpellQueueIndex = entity.QuestFoeSpellQueueIndex;
            data.questFoeItemQueueIndex = entity.QuestFoeItemQueueIndex;
            data.wabbajackActive = entity.WabbajackActive;
            data.team = (int)entity.Team + 1;
            data.specialTransformationCompleted = mobileEnemy.SpecialTransformationCompleted;

            // Add quest resource data if present
            QuestResourceBehaviour questResourceBehaviour = GetComponent<QuestResourceBehaviour>();
            if (questResourceBehaviour)
            {
                data.questResource = questResourceBehaviour.GetSaveData();
            }

            return data;
        }

        public void RestoreSaveData(object dataIn)
        {
            if (!enemy)
                return;

            EnemyData_v1 data = (EnemyData_v1)dataIn;
            if (data.loadID != LoadID)
                return;

            DaggerfallEntityBehaviour entityBehaviour = enemy.GetComponent<DaggerfallEntityBehaviour>();
            EnemySenses senses = enemy.GetComponent<EnemySenses>();
            EnemyMotor motor = enemy.GetComponent<EnemyMotor>();
            EnemyEntity entity = entityBehaviour.Entity as EnemyEntity;
            MobileUnit mobileEnemy = enemy.GetComponentInChildren<MobileUnit>();

            bool genderChanged = false;
            if(data.mobileGender != MobileGender.Unspecified)
            {
                if (entity.Gender == Genders.Male && data.mobileGender == MobileGender.Female)
                    genderChanged = true;
                else if (entity.Gender == Genders.Female && data.mobileGender == MobileGender.Male)
                    genderChanged = true;
            }

            // Restore enemy career or class if different
            if (entity == null || entity.EntityType != data.entityType || entity.CareerIndex != data.careerIndex || genderChanged)
            {
                SetupDemoEnemy setupEnemy = enemy.GetComponent<SetupDemoEnemy>();
                setupEnemy.ApplyEnemySettings(data.entityType, data.careerIndex, data.mobileGender, data.isHostile, alliedToPlayer: data.alliedToPlayer);
                setupEnemy.AlignToGround();

                if (entity == null)
                    entity = entityBehaviour.Entity as EnemyEntity;
            }

            // Quiesce entity during state restore
            entity.Quiesce = true;

            // Restore enemy data
            entityBehaviour.gameObject.name = data.gameObjectName;
            enemy.transform.rotation = data.currentRotation;
            entity.QuestFoeSpellQueueIndex = data.questFoeSpellQueueIndex;
            entity.QuestFoeItemQueueIndex = data.questFoeItemQueueIndex;
            entity.WabbajackActive = data.wabbajackActive;
            entity.Items.DeserializeItems(data.items);
            entity.ItemEquipTable.DeserializeEquipTable(data.equipTable, entity.Items);
            entity.MaxHealth = data.startingHealth;
            entity.SetHealth(data.currentHealth, true);
            entity.SetFatigue(data.currentFatigue, true);
            entity.SetMagicka(data.currentMagicka, true);
            int team = data.team;
            if (team > 0)   // Added 1 to made backwards compatible. 0 = no team saved
                entity.Team = (MobileTeams)(team - 1);
            motor.IsHostile = data.isHostile;
            senses.HasEncounteredPlayer = data.hasEncounteredPlayer;

            // Restore enemy position and migrate to floating y support for exteriors
            // Need to get relative difference between current and serialized world compensation to get actual y position
            float diffY = GameManager.Instance.StreamingWorld.WorldCompensation.y - data.worldCompensation.y;
            enemy.transform.position = data.currentPosition + new Vector3(0, diffY, 0);

            // Disable dead enemies
            if (data.isDead)
            {
                entityBehaviour.gameObject.SetActive(false);
            }

            // Restore quest resource link
            enemy.QuestSpawn = data.questSpawn;
            if (enemy.QuestSpawn)
            {
                // Add QuestResourceBehaviour to GameObject
                QuestResourceBehaviour questResourceBehaviour = entityBehaviour.gameObject.AddComponent<QuestResourceBehaviour>();
                questResourceBehaviour.RestoreSaveData(data.questResource);

                // Destroy QuestResourceBehaviour if no actual quest properties are restored from save
                if (questResourceBehaviour.QuestUID == 0 || questResourceBehaviour.TargetSymbol == null)
                {
                    enemy.QuestSpawn = false;
                    Destroy(questResourceBehaviour);
                }
            }

            // Restore instanced effect bundles
            GetComponent<EntityEffectManager>().RestoreInstancedBundleSaveData(data.instancedEffectBundles);

            // Restore special transformation state if completed
            if (data.specialTransformationCompleted && mobileEnemy)
            {
                mobileEnemy.SetSpecialTransformationCompleted();
            }

            // Resume entity
            entity.Quiesce = false;
        }

        #endregion

        #region Private Methods

        bool HasChanged()
        {
            if (!enemy)
                return false;

            // Always serialize enemy
            return true;
        }

        ulong GetLoadID()
        {
            if (!enemy)
                return 0;

            return enemy.LoadID;
        }

        #endregion
    }

    public class LocationSaveDataInterface : MonoBehaviour, IHasModSaveData
    {
        Dictionary<ulong, LocationLootData_v1> savedLoot = new Dictionary<ulong, LocationLootData_v1>();
        Dictionary<ulong, LocationLootSerializer> activeLootSerializers = new Dictionary<ulong, LocationLootSerializer>();
        Dictionary<ulong, LocationEnemySerializer> activeEnemySerializers = new Dictionary<ulong, LocationEnemySerializer>();
        HashSet<ulong> clearedEnemies = new HashSet<ulong>();

        #region Unity
        void OnEnable()
        {
            StreamingWorld.OnFloatingOriginChange += StreamingWorld_OnFloatingOriginChange;
            SaveLoadManager.OnStartLoad += SaveLoadManager_OnStartLoad;
        }

        void OnDisable()
        {
            StreamingWorld.OnFloatingOriginChange -= StreamingWorld_OnFloatingOriginChange;
            SaveLoadManager.OnStartLoad -= SaveLoadManager_OnStartLoad;
        }

        private void SaveLoadManager_OnStartLoad(SaveData_v1 saveData)
        {
            clearedEnemies.Clear();
        }

        private void StreamingWorld_OnFloatingOriginChange()
        {
            foreach (var loot in activeLootSerializers.Values)
            {
                loot.RefreshWorldCompensation();
            }

            foreach(var enemy in activeEnemySerializers.Values)
            {
                enemy.RefreshWorldCompensation();
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

        public void AddDeadEnemy(LocationEnemySerializer serializer)
        {
            clearedEnemies.Add(serializer.LoadID);
        }

        public bool IsEnemyDead(ulong loadID)
        {
            return clearedEnemies.Contains(loadID);
        }

        public void RegisterActiveSerializer(LocationEnemySerializer serializer)
        {
            activeEnemySerializers.Add(serializer.LoadID, serializer);
        }

        public void DeregisterActiveSerializer(LocationEnemySerializer serializer)
        {
            activeEnemySerializers.Remove(serializer.LoadID);
        }


        void FlushActiveInstances()
        {
            foreach(var loot in activeLootSerializers.Values)
            {
                SerializeLoot(loot);
            }

            foreach (var enemy in activeEnemySerializers.Values)
            {
                if (enemy.IsDead())
                    AddDeadEnemy(enemy);
            }
        }

        void ReloadActiveInstances(EnemyData_v1[] enemies)
        {
            foreach (var loot in activeLootSerializers.Values)
            {
                TryDeserializeLoot(loot);
            }

            foreach (EnemyData_v1 enemy in enemies)
            {
                if (activeEnemySerializers.TryGetValue(enemy.loadID, out LocationEnemySerializer serializer))
                {
                    if (clearedEnemies.Contains(enemy.loadID))
                    {
                        Destroy(serializer.gameObject);
                    }
                    else
                    {
                        serializer.RestoreSaveData(enemy);
                    }
                }
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
            defaultData.activeEnemies = new EnemyData_v1[0];
            defaultData.clearedEnemies = new ulong[0];
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
            data.activeEnemies = activeEnemySerializers.Values.Select(serializer => (EnemyData_v1)serializer.GetSaveData()).ToArray();
            data.clearedEnemies = clearedEnemies.ToArray();

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
            foreach (ulong clearedEnemy in data.clearedEnemies)
                clearedEnemies.Add(clearedEnemy);

            ReloadActiveInstances(data.activeEnemies);
        }
    }
}