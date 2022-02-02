using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Serialization;
using System;
using static DaggerfallWorkshop.Game.Utility.NameHelper;
using static DaggerfallWorkshop.Utility.ContentReader;

namespace LocationLoader
{
    static class LocationNameGenerator
    {
        static bool IsInHammerfell(LocationInstance instance)
        {
            var climateIndex = DaggerfallUnity.Instance.ContentReader.MapFileReader.GetClimateIndex(instance.worldX, instance.worldY);
            var politicIndex = DaggerfallUnity.Instance.ContentReader.MapFileReader.GetPoliticIndex(instance.worldX, instance.worldY);

            switch ((MapsFile.Climates)climateIndex)
            {
                case MapsFile.Climates.Desert:
                case MapsFile.Climates.Desert2:
                case MapsFile.Climates.Swamp:
                case MapsFile.Climates.Rainforest:
                case MapsFile.Climates.Subtropical:
                    return true;
            }
                        
            switch (politicIndex & 0x7f)
            {
                case 1: // Dragontail Mountains
                case 53: // Ephesus
                    return true;
            }

            return false;
        }

        public static string GenerateDockName(LocationInstance instance, string context="")
        {
            if (!string.IsNullOrEmpty(instance.extraData))
            {
                var dockExtraData = (DockExtraData)SaveLoadManager.Deserialize(typeof(DockExtraData), instance.extraData);
                DFPosition locPos = MapsFile.GetPixelFromPixelID(dockExtraData.LinkedMapId);

                var contentReader = DaggerfallUnity.Instance.ContentReader;
                if (!contentReader.HasLocation(locPos.X, locPos.Y, out MapSummary mapSummary))
                    throw new Exception($"Could not find location for mapID '{dockExtraData.LinkedMapId}' ({context})");

                if (!contentReader.GetLocation(mapSummary.RegionIndex, mapSummary.MapIndex, out DFLocation locData))
                    throw new Exception($"Could not get location data for '{dockExtraData.LinkedMapId}' ({context})");

                return locData.Name + " Docks";
            }
            return instance.name;
        }

        // Name structure:
        // Adjective Name's Object
        // ex: Stabby Uthyrick's Raiders
        public static string GenerateBanditCampName(LocationInstance instance, string context="")
        {
            ulong seed = instance.locationID;
            Random rng1 = new Random((int)((seed & 0xFFFFFFFF00000000) >> 32));
            Random rng2 = new Random((int)(seed & 0xFFFFFFFF));

            int r1 = rng1.Next() ^ rng2.Next();
            int r2 = rng1.Next() ^ rng2.Next();
            int r3 = rng1.Next() ^ rng2.Next();

            string[] Adjectives =
            {
                "Cruel",
                "Dark",
                "Violent",
                "Pointy",
                "Bloody",
                "Stabby",
                "Black",
                "Red",
            };

            string adjective = Adjectives[r1 % Adjectives.Length];

            BankTypes bankType = IsInHammerfell(instance) ? BankTypes.Redguard : BankTypes.Breton;

            DFRandom.Seed = (uint)r2;
            string name = DaggerfallUnity.Instance.NameHelper.FirstName(bankType, Genders.Male);

            string[] Objects =
            {
                "Hideout",
                "Lair",
                "Killers",
                "Raiders",
                "Looters",
                "Muggers",
                "Brutes",
                "Secret Base",
            };

            string obj = Objects[r3 % Objects.Length];

            return $"{adjective} {name}'s {obj}";
        }
    }
}
