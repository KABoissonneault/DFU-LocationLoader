using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Utility.AssetInjection;
using DaggerfallWorkshop.Game.Items;
using System.Globalization;
using DaggerfallWorkshop.Game.Serialization;

namespace LocationLoader
{
    public static class LocationHelper
    {
        public const string locationInstanceFolder = "StreamingAssets/Locations/";
        public const string locationPrefabFolder = "StreamingAssets/Locations/LocationPrefab/";

        const float animalSoundMaxDistance = 768 * MeshReader.GlobalScale; //Used for the objects with sound

        //List of objects
        //Add more over time, 
        public static Dictionary<string, string> models = new Dictionary<string, string>()
        {
            {"448", "Dungeon Wall Corner" },
            {"449", "Dungeon Wall" },
            {"450", "Dungeon Wall Gate" },
            {"451", "Fort 5"},
            {"457", "Fort"},
            {"730", "Stronghold 01"},
            {"732", "Fort 3"},
            {"852", "Fort 4"},
            {"853", "Fort 2"},
            {"3200", "Door" },
            {"3204", "Wall" },
            {"6804", "Wood Wall 01"},
            {"21410", "Bridge of Daggerfall"},
            {"21411", "Water Mill"},
            {"21810", "Mountain_01"},
            {"21811", "Mountain_02"},
            {"21812", "Mountain_03"},
            {"21813", "Mountain_04"},
            {"24715", "Roof" },
            {"40112", "Stone Door"},
            {"41001", "Bed"},
            {"41241", "Wagon Hay"},
            {"41507", "Wrecked Ship"},
            {"41508", "Wrecked Ship"},
            {"41509", "Wrecked Ship"},
            {"41600", "Windmill"},
            {"41601", "Windmill Unknown"},
            {"41719", "Mossy Ruin"},
            {"41720", "Mossy Ruin"},
            {"41721", "Mossy Ruin"},
            {"41722", "Mossy Ruin"},
            {"41723", "Mossy Ruin"},
            {"41724", "Mossy Ruin"},
            {"41725", "Mossy Ruin"},
            {"41726", "Mossy Ruin"},
            {"41727", "Mossy Ruin"},
            {"41728", "Mossy Ruin"},
            {"41731", "Stone Ruin"},
            {"41732", "Stone Ruin"},
            {"41733", "Stone Ruin"},
            {"41702", "Market_Post"},
            {"41703", "Market Box"},
            {"42501", "Banner"},
            {"42514", "Flower Banner Long"},
            {"42558", "Banner_Blue_Large"},
            {"43703", "Clothing Sign"},
            {"51117", "Painting" },
            {"55014", "Gate Door" },
            {"58041", "Stone Platform" },
            {"58055", "Stone Corner" },
            {"60107", "Cave" },
            {"60113", "Cave End" },
            {"61027", "Wooden Beam"},
            {"62227", "Stone Wall" },
            {"63007", "Dungeon Entrance" },
            {"63047", "Dungeon Mine Entrance" },
            {"70806", "DungeonHouse" },
            {"74088", "Shrine Stairs" },
            {"74204", "Platform Dirt"},
            {"74019", "Wood Wall 02"},
            {"74222", "Wood Spike"},
            {"73005", "Water"},
            {"99901", "Blue Box"},
            {"51115", "Painting_1"},
            {"60706", "Wooden Gate"},
            {"62112", "Stone Bridge01"},
            {"41211", "Stone Bridge02"},
            {"41212", "Stone Bridge03"},
            {"41213", "Stone Bridge04"},
            {"43500", "Building Ruin"},
            {"517", "Stone Corner"},
            {"518", "Stone Straight"},
            {"520", "Opening Stone"},
            {"62131", "Wooden Bridge"},
            {"41402", "Shield"},
            {"41049", "Sword Shelf"},
            {"42304", "Pedestal"},
            {"41218", "Grass Top"},
            {"41729", "Ruin Moss"},
            {"41606", "Tent"},
            {"41607", "Tent"},
            {"41608", "Tent"},
            {"41609", "Tent"},
            {"41020", "Book Pedestal"},
            {"41021", "Book Pedestal"},
            {"41022", "Book Pedestal"},
            {"41023", "Book Pedestal"},
            {"41024", "Book Pedestal"},
            {"74095", "Claymore Sword"},
            {"43601", "Lab Entrance"},
            {"74226", "Knight Armor"},
            {"322", "Small House"},
            {"123", "Big House"},
            {"329", "Stonewall House"},
            //{"210", "Small House"},
			//{"9832", "OrangeWall House"},
			{"43502", "Yellow Ruins"},
            {"43503", "Yellow Ruins"},
            {"43504", "Yellow Ruins"},
            {"43505", "Yellow Ruins"},
            {"43506", "Yellow Ruins"},
            {"43507", "Yellow Ruins"},
            {"43508", "Yellow Ruins"},
            {"43509", "Yellow Ruins"},
            {"43510", "Yellow Ruins"},
            {"43511", "Yellow Ruins"},
            {"43512", "Yellow Ruins"},
            {"43513", "Yellow Ruins"},
            {"43514", "Yellow Ruins"},
            {"43515", "Yellow Ruins"},
            {"43516", "Yellow Ruins"},
            {"43517", "Yellow Ruins"},
            {"43401", "Yellow Ruins"},
            {"43402", "Yellow Ruins"},
            {"43403", "Yellow Ruins"},
            {"43404", "Yellow Ruins"},
            {"43405", "Yellow Ruins"},
            {"43406", "Yellow Ruins"},
            {"43407", "Yellow Ruins"},
            {"43408", "Yellow Ruins"},
            {"43409", "Yellow Ruins"},
            {"43410", "Yellow Ruins"},
            {"43411", "Yellow Ruins"},
            {"43412", "Yellow Ruins"},
            {"43413", "Yellow Ruins"},
            {"43414", "Yellow Ruins"},
            {"43415", "Yellow Ruins"},
            {"43416", "Yellow Ruins"},
            {"43417", "Yellow Ruins"},
            {"74223", "Circle"},
            {"744", "Bush Corner"},
            {"752", "Bush Entrance"},
            {"754", "Bush Long"},
            {"60201", "Cave Corner"},
            {"30303", "Wood"},
            {"1101", "Wood"},
            {"41005", "Shelf"},
            {"21127", "Stairs"},
            {"21108", "Brick Wall 01"},
            {"21109", "Brick Wall 02"},
            {"210", "Small Building 01"},
            {"323", "Walled Building 01"},
            {"608", "Walled Building 02"},
            {"310", "Building 01" },
            {"909", "Long Ship"},
            {"910", "Ship" },
            {"21103", "Broken Wooden Fence"},
            {"21104", "Brown Stone Wall Middle"},
            {"21105", "Brown Stone Wall Corner"},
            {"21106", "Brown Stone Wall Opening" },
            {"21107", "Brown Stone Wall Endpiece" },
            {"22235", "Crystal" },
            {"40501", "Gray Brick Wall"},
            {"40504", "Gray Brick Wall 2"},
            {"41002", "Canopy Bed" },
            {"41100", "Wooden Chair"},
            {"41106", "Wooden Bench Simple"},
            {"41122", "Wooden Throne"},
            {"41123", "Wooden Throne 2"},
            {"41130", "Table"},
            {"41206", "Wooden Fence Endpiece 2"},
            {"41208", "Wooden Fence Endpiece"},
            {"41209", "Water Tank"},
            {"41210", "Water Tank Empty"},
            {"41214", "Wooden Cart"},
            {"41220", "Fountain 1"},
            {"41221", "Fountain 2"},
            {"41222", "Fountain 3"},
            {"41227", "Tower Hex"},
            {"41228", "Tower Hex 2"},
            {"41233", "Tower Star 1"},
            {"41234", "Tower Star 2"},
            {"41313", "Cage"},
            {"41325", "Wooden Casket w. Zombie"},
            {"41400", "Wooden Pole"},
            {"41403", "Construction Plattform 1"},
            {"41404", "Construction Plattform 2"},
            {"41407", "Catapult"},
            {"41409", "Ladder"},
            {"41501", "Small Ship"},
            {"41700", "Medieval Stock 1"},
            {"41701", "Medieval Stock 2"},
            {"41734", "Wooden Tree Log"},
            {"41735", "Wooden Tree Log 2"},
            {"41736", "Wooden Tree Log 3"},
            {"41737", "Wooden Tree Log 4"},
            {"41738", "Wooden Tree Log 5"},
            {"41739", "Sign"},
            {"41801", "Dresser"},
            {"41825", "Wooden Crate short"},
            {"41818", "Wooden Crate"},
            {"41821", "Wooden Crate Small"},
            {"41832", "Wooden Crate"},
            {"41833", "Wooden Crate .w lid"},
            {"43001", "Wooden Boards"},
            {"43003", "Stone Gate Dark"},
            {"43004", "Stone Gate Red"},
            {"43005", "Stone Gate Gray"},
            {"43006", "Stone Gate White"},
            {"43007", "Stone Gate Dark"},
            {"43008", "Stone Gate Red"},
            {"43009", "Stone Gate Gray"},
            {"43010", "Stone Gate White"},
            {"43011", "Gravestone Dark"},
            {"43012", "Gravestone Red"},
            {"43013", "Gravestone Gray"},
            {"43014", "Gravestone White"},
            {"43015", "Gravestone Dark"},
            {"43016", "Gravestone Red"},
            {"43017", "Gravestone Gray"},
            {"43018", "Gravestone White"},
            {"43019", "Gravestone Dark"},
            {"43020", "Gravestone Red"},
            {"43021", "Gravestone Gray"},
            {"43022", "Gravestone White"},
            {"43023", "Gravestone Dark"},
            {"43024", "Gravestone Red"},
            {"43025", "Gravestone Gray"},
            {"43026", "Gravestone White"},
            {"43027", "Gravestone Ground Dark"},
            {"43028", "Gravestone Ground Red"},
            {"43029", "Gravestone Ground Gray"},
            {"43030", "Gravestone Ground White"},
            {"43031", "Gravestone Ground Dark"},
            {"43032", "Gravestone Ground Red"},
            {"43033", "Gravestone Ground Gray"},
            {"43034", "Gravestone Ground White"},
            {"43035", "Gravestone Ground Dark"},
            {"43036", "Gravestone Ground Red"},
            {"43037", "Gravestone Ground Gray"},
            {"43038", "Gravestone Ground White"},
            {"43051", "Gravestone Dark"},
            {"43052", "Gravestone Red"},
            {"43053", "Gravestone Gray"},
            {"43054", "Gravestone White"},
            {"43055", "Gravestone Dark"},
            {"43056", "Gravestone Red"},
            {"43057", "Gravestone Gray"},
            {"43058", "Gravestone White"},
            {"43059", "Gravestone Dark"},
            {"43060", "Gravestone Red"},
            {"43061", "Gravestone Gray"},
            {"43062", "Gravestone White"},
            {"43063", "Gravestone Dark"},
            {"43064", "Gravestone Red"},
            {"43065", "Gravestone Gray"},
            {"43066", "Gravestone White"},
            {"43071", "Gravestone Tall Dark"},
            {"43072", "Gravestone Tall Red"},
            {"43073", "Gravestone Tall Gray"},
            {"43074", "Gravestone Tall White"},
            {"43075", "Stone Casket Dark"},
            {"43076", "Stone Casket Red"},
            {"43077", "Stone Casket Gray"},
            {"43078", "Stone Casket White"},
            {"43079", "Stone Tomb Dark"},
            {"43080", "Stone Tomb Red"},
            {"43081", "Stone Tomb Gray"},
            {"43082", "Stone Tomb White"},
            {"43083", "Stone Ankh Dark"},
            {"43084", "Stone Ankh Red"},
            {"43085", "Stone Ankh Gray"},
            {"43086", "Stone Ankh White"},
            {"43101", "Gravestone Tall Dark"},
            {"43102", "Gravestone Tall Red"},
            {"43103", "Gravestone Tall Gray"},
            {"43104", "Gravestone Tall White"},
            {"43105", "Small Obelisk Dark"},
            {"43106", "Small Obelisk Red"},
            {"43107", "Small Obelisk Gray"},
            {"43108", "Small Obelisk White"},
            {"43109", "Cemetery Gazebo Dark"},
            {"43110", "Cemetery Gazebo Red"},
            {"43111", "Cemetery Gazebo Gray"},
            {"43112", "Cemetery Gazebo White"},
            {"43113", "Gravestone Wall Dark"},
            {"43114", "Gravestone Wall Red"},
            {"43115", "Gravestone Wall Gray"},
            {"43116", "Gravestone Wall White"},
            {"43117", "Gravestone Dark"},
            {"43118", "Gravestone Red"},
            {"43119", "Gravestone Gray"},
            {"43120", "Gravestone White"},
            {"43121", "Stone Ankh Dark"},
            {"43122", "Stone Ankh Red"},
            {"43123", "Stone Ankh Gray"},
            {"43124", "Stone Ankh White"},
            {"43125", "Gravestone Tall Dark"},
            {"43126", "Gravestone Tall Red"},
            {"43127", "Gravestone Tall Gray"},
            {"43128", "Gravestone Tall White"},
            {"43129", "Gravestone Broken Dark"},
            {"43130", "Gravestone Broken Red"},
            {"43131", "Gravestone Broken Gray"},
            {"43132", "Gravestone Broken White"},
            {"43133", "Gravestone Dark"},
            {"43134", "Gravestone Red"},
            {"43135", "Gravestone Gray"},
            {"43136", "Gravestone White"},
            {"43137", "Mausoleum Gray"},
            {"43138", "Mausoleum Dark"},
            {"43139", "Mausoleum Red"},
            {"43140", "Mausoleum Gray"},
            {"43141", "Mausoleum White"},
            {"43142", "Gravestone Dark"},
            {"43143", "Gravestone Red"},
            {"43144", "Gravestone Gray"},
            {"43145", "Gravestone White"},
            { "43201", "Gravestone"},
            {"43202", "Broken Fancy Gravestone"},
            {"43204", "Mausoleum Entrance"},
            {"43205", "Broken Gravestone"},
            {"43206", "Broken Gravestone"},
            {"43300", "Gravestone"},
            {"43301", "Gravestone"},
            {"43302", "Gravestone"},
            {"43303", "Tall Gravestone"},
            {"43304", "Stone Casket"},
            {"43305", "Stone Casket 2"},
            {"43306", "Stone Casket 3"},
            {"43307", "Wooden Bench"},
            {"60610", "Black Rock"},
            {"60711", "Rock 2"},
            {"60712", "Rock 3"},
            {"60713", "Rock 4"},
            {"60714", "Rock 5"},
            {"60715", "Rock 6"},
            {"60716", "Rock 7"},
            {"60717", "Rock 8"},
            {"60718", "Rock 9"},
            {"60719", "Rock 10"},
            {"60720", "Rock 11"},
            {"62310", "Stone Arch Round"},
            {"62313", "Stone Arch Square"},
            {"62314", "Stone Obelisk"},
            {"62315", "Marble Pillar"},
            {"62317", "Marble Arch"},
            {"62318", "Wooden Piece"},
            {"62319", "Wooden Board"},
            {"62322", "Marble Slab"},
            {"62324", "Stone Statue LightGrey"},
            {"62325", "Stone Statue LightGrey 2"},
            {"62328", "Stone Statue Dark"},
            {"62330", "Stone Statue Dark 2"},
            {"74009", "Marble Pillar 2"},
            {"74082", "Wood Pedestal"},
            {"74086", "Marble Pedestal"},
            {"74091", "Stone Pedestal"},
            {"74094", "Open Pillar"},
            {"74212", "Anvil"},
            {"74219", "Sickle"},
            {"74221", "Crossbow"},
            {"74224", "Sword"},
            {"74225", "Axe"},
            {"99800", "Arrow"}
        };
        public static Dictionary<string, string> billboards = new Dictionary<string, string>()
        {
            {"053.12", "TempleSign"},
            {"301.0", "Hay 01"},
            {"301.1", "Hay 02"},
            {"212.1", "Hay Stack"},
            {"301.3", "Sunflower 01"},
            {"301.4", "Sunflower 02"},
            {"301.5", "Berry Bush"},
            {"205.31", "Potion Stack"},
            {"097.1", "Statue of Zenithar"},
            {"097.2", "Statue of Mara"},
            {"097.3", "Statue of Stendarr"},
            {"097.12", "Statue of Dibella"},
            {"097.13", "Statue of Kynareth"},
            {"100.0", "Blood"},
            {"100.1", "Blood 2"},
            {"100.2", "Skeleton Hand"},
            {"100.3", "Hand"},
            {"100.4", "Cage from Ceiling"},
            {"100.5", "Short Chain from Ceiling"},
            {"100.6", "Dual Chains from Ceiling"},
            {"100.7", "Dual Chains from Celing"},
            {"100.8", "Long Chain from Ceiling"},
            {"100.9", "Horned Skull"},
            {"100.10", "Decapitated Heads"},
            {"100.11", "Head on a Pole"},
            {"100.12", "Head on a Pole 2"},
            {"100.13", "Bloody Stick"},
            {"100.14", "Wood Pillar"},
            {"100.15", "Wood Pillar 2"},
            {"100.16", "Wood Pillar 3"},
            {"100.17", "Pile pf skulls"},
            {"100.18", "Ribcage"},
            {"100.19", "Single Vine"},
            {"100.20", "Dual Vines"},
            {"100.21", "Dual Vines 2"},
            {"100.22", "Dual Vines 3"},
            {"100.23", "White Skull"},
            {"100.24", "Skull on a Stick"},
            {"100.25", "Broken Skull"},
            {"100.26", "Beast Skull"},
            {"100.27", "Gray Skull"},
            {"100.28", "Impaled Body"},
            {"100.29", "Impaled Body 2"},
            {"101.1", "Chandelier"},
            {"101.2", "Chandelier Blue"},
            {"101.3", "Chandelier 2"},
            {"101.4", "Chandelier 3"},
            {"101.6", "Hanging Light"},
            {"101.7", "Hanging Light 2"},
            {"101.8", "Hanging Light 3"},
            {"101.9", "Hanging Light 4"},
            {"101.10", "Hanging Sphere Light"},
            {"101.11", "White Skull w. Candle"},
            {"101.12", "Burning Skull on Stick"},
            {"201.00", "Horse White"},
            {"201.01", "Horse Gray"},
            {"201.02", "Camel"},
            {"201.03", "Cow 1"},
            {"201.04", "Cow 2"},
            {"201.05", "Pig 1"},
            {"201.06", "Pig 2"},
            {"201.07", "Cat 1"},
            {"201.08", "Cat 2"},
            {"201.09", "Dog"},
            {"201.10", "Dog 2"},
            {"201.11", "Seagull"},
            {"205.3", "Bottle Big Clear"},
            {"208.3", "Scale"},
            {"208.6", "Hour Glass"},
            {"210.0", "Bowl of Fire"},
            {"210.1", "Camp Fire"},
            {"210.2", "Skull Candle"},
            {"210.3", "Candle"},
            {"210.4", "Candle w. Base"},
            {"210.5", "Candleholder with 3 candles"},
            {"210.6", "Skull torch"},
            {"210.7", "Wooden Chandelier w. Extinguished Candles"},
            {"210.8", "Turkis Lamp"},
            {"210.9", "Metallic Chandelier w. Burning Candles"},
            {"210.10", "Metallic Chandelier w. Extinguished Candles"},
            {"210.11", "Candle in Lamp"},
            {"210.12", "Extinguished Lamp"},
            {"210.13", "Round Lamp"},
            {"210.14", "Standing Lantern"},
            {"210.15", "Standing Lantern Round"},
            {"210.16", "Mounted Torch w. Thin Holder"},
            {"210.17", "Mounted Torch 1"},
            {"210.18", "Mounted Torch 2"},
            {"210.19", "Pillar w. Firebowl"},
            {"210.20", "Brazier Torch"},
            {"210.21", "Standing Candle"},
            {"210.22", "Round Lantern w. Medium Chain"},
            {"210.23", "Wooden Chandelier w. Burning Candles"},
            {"210.24", "Lantern w. Long Chain"},
            {"210.25", "Lantern w. Medium Chain"},
            {"210.26", "Lantern w. Short Chain"},
            {"210.27", "Lantern w, No Chain"},
            {"210.28", "Street Lantern 1"},
            {"210.29", "Street Lantern 2"},
            {"212.0", "Well"},
            {"212.5", "Blank Sign"},
            {"212.11", "Wood Pile"},
            {"205.0", "Barrel"},
            {"254.26", "Red Flower"},
            {"254.27", "Yellow Flower"},
            {"254.28", "Purple Flower"},
            {"254.29", "White Flower"},
            {"432.19", "Rose"},
            {"216.0", "Goldpile 1"},
            {"216.1", "Goldpile 2"},
            {"216.2", "Goldpile 3"},
            {"216.3", "Gold Casket"},
            {"216.4", "Gold Coin"},
            {"216.5", "Silver Coin"},
            {"216.6", "Gold Crown 1"},
            {"216.7", "Silver Crown 1"},
            {"216.8", "Silver Crown 2"},
            {"216.9", "Gold Crown 2"},
            {"216.10", "Silver plate"},
            {"216.11", "Treasure 1"},
            {"216.12", "Treasure 2"},
            {"216.13", "Treasure 3"},
            {"216.14", "Treasure 4"},
            {"216.15", "Treasure 5"},
            {"216.16", "Treasure 6"},
            {"216.17", "Treasure 7"},
            {"216.18", "Treasure 8"},
            {"216.19", "Treasure 9"},
            {"216.20", "Treasure 10"},
            {"216.21", "Treasure 11"},
            {"216.22", "Treasure 12"},
            {"216.23", "Treasure 13"},
            {"216.24", "Treasure 14"},
            {"216.25", "Treasure 15"},
            {"216.26", "Treasure 16"},
            {"216.27", "Treasure 17"},
            {"216.28", "Treasure 18"},
            {"216.30", "Treasure 19"},
            {"216.31", "Treasure 20"},
            {"216.32", "Treasure 21"},
            {"216.33", "Treasure 22"},
            {"216.34", "Treasure 23"},
            {"216.35", "Treasure 24"},
            {"216.36", "Treasure 25"},
            {"216.37", "Treasure 26"},
            {"216.38", "Treasure 27"},
            {"216.39", "Treasure 28"},
            {"216.40", "Treasure 29"},
            {"216.41", "Treasure 30"},
            {"216.42", "Treasure 31"},
            {"216.43", "Treasure 32"},
            {"216.44", "Treasure 33"},
            {"216.45", "Treasure 34"},
            {"216.46", "Treasure 35"},
            {"216.47", "Treasure 36"},
            {"500.01", "Rain Forest Bush"},
            {"500.02", "Rain Forest Bush"},
            {"500.03", "Rain Forest Bush"},
            {"500.04", "Rain Forest Rock"},
            {"500.05", "Rain Forest Plant"},
            {"500.06", "Rain Forest Plant"},
            {"500.07", "Rain Forest Plant"},
            {"500.08", "Rain Forest Plant"},
            {"500.09", "Rain Forest Fern"},
            {"500.10", "Rain Forest Fern"},
            {"500.11", "Rain Forest Plant"},
            {"500.12", "Rain Forest Tree"},
            {"500.13", "Rain Forest Tree"},
            {"500.14", "Rain Forest Tree"},
            {"500.15", "Rain Forest Tree"},
            {"500.16", "Rain Forest Tree"},
            {"500.17", "Rain Forest Rock"},
            {"500.18", "Rain Forest Tree"},
            {"500.19", "Rain Forest Plant"},
            {"500.20", "Rain Forest Plant"},
            {"500.21", "Rain Forest Plant"},
            {"500.22", "Rain Forest Plant"},
            {"500.23", "Rain Forest Plant"},
            {"500.24", "Rain Forest Plant"},
            {"500.25", "Rain Forest Plant"},
            {"500.26", "Rain Forest Plant"},
            {"500.27", "Rain Forest Plant"},
            {"500.28", "Rain Forest Plant"},
            {"500.29", "Rain Forest Plant"},
            {"500.30", "Rain Forest Tree"},
            {"500.31", "Rain Forest Plant"},
            {"501.01", "Sub_Tropical Plant"},
            {"501.02", "Sub_Tropical Plant"},
            {"501.03", "Sub_Tropical Rock"},
            {"501.04", "Sub_Tropical Rock"},
            {"501.05", "Sub_Tropical Rock"},
            {"501.06", "Sub_Tropical Rock"},
            {"501.07", "Sub_Tropical Plant"},
            {"501.08", "Sub_Tropical Plant"},
            {"501.09", "Sub_Tropical Plant"},
            {"501.10", "Sub_Tropical Plant"},
            {"501.11", "Sub_Tropical Tree"},
            {"501.12", "Sub_Tropical Tree"},
            {"501.13", "Sub_Tropical Tree"},
            {"501.14", "Sub_Tropical Plant"},
            {"501.15", "Sub_Tropical Plant"},
            {"501.16", "Sub_Tropical Tree"},
            {"501.17", "Sub_Tropical Plant"},
            {"501.18", "Sub_Tropical Plant"},
            {"501.19", "Sub_Tropical Plant"},
            {"501.20", "Sub_Tropical Plant"},
            {"501.21", "Sub_Tropical Plant"},
            {"501.22", "Sub_Tropical Plant"},
            {"501.23", "Sub_Tropical Rock"},
            {"501.24", "Sub_Tropical Plant"},
            {"501.25", "Sub_Tropical Plant"},
            {"501.26", "Sub_Tropical Plant"},
            {"501.27", "Sub_Tropical Plant"},
            {"501.28", "Sub_Tropical Plant"},
            {"501.29", "Sub_Tropical Plant"},
            {"501.30", "Sub_Tropical Tree"},
            {"501.31", "Sub_Tropical Plant"},
            {"502.01", "Swamp Plant"},
            {"502.02", "Swamp Plant"},
            {"502.03", "Swamp Rock"},
            {"502.04", "Swamp Rock"},
            {"502.05", "Swamp Rock"},
            {"502.06", "Swamp Rock"},
            {"502.07", "Swamp Plant"},
            {"502.08", "Swamp Plant"},
            {"502.09", "Swamp Plant"},
            {"502.10", "Swamp Rock"},
            {"502.11", "Swamp Plant"},
            {"502.12", "Swamp Tree"},
            {"502.13", "Swamp Tree"},
            {"502.14", "Swamp Plant"},
            {"502.15", "Swamp Tree"},
            {"502.16", "Swamp Tree"},
            {"502.17", "Swamp Tree"},
            {"502.18", "Swamp Tree"},
            {"502.19", "Swamp Plant"},
            {"502.20", "Swamp Plant"},
            {"502.21", "Swamp Plant"},
            {"502.22", "Swamp Plant"},
            {"502.23", "Swamp Plant"},
            {"502.24", "Swamp Plant"},
            {"502.25", "Swamp Plant"},
            {"502.26", "Swamp Plant"},
            {"502.27", "Swamp Plant"},
            {"502.28", "Swamp Plant"},
            {"502.29", "Swamp Plant"},
            {"502.30", "Swamp Tree"},
            {"502.31", "Swamp Plant"},
            {"503.01", "Desert Plant"},
            {"503.02", "Desert Rock"},
            {"503.03", "Desert Rock"},
            {"503.04", "Desert Rick"},
            {"503.05", "Desert Tree"},
            {"503.06", "Desert Plant"},
            {"503.07", "Desert Plant"},
            {"503.08", "Desert Plant"},
            {"503.09", "Desert Plant"},
            {"503.10", "Desert Plant"},
            {"503.11", "Desert Tree"},
            {"503.12", "Desert Tree"},
            {"503.13", "Desert Tree"},
            {"503.14", "Desert Cactus"},
            {"503.15", "Desert Cactus"},
            {"503.16", "Desert Cactus"},
            {"503.17", "Desert Plant"},
            {"503.18", "Desert Rock"},
            {"503.19", "Desert Rock"},
            {"503.20", "Desert Rock"},
            {"503.21", "Desert Rock"},
            {"503.22", "Desert Rock"},
            {"503.23", "Desert Plant"},
            {"503.24", "Desert Plant"},
            {"503.25", "Desert Plant"},
            {"503.26", "Desert Plant"},
            {"503.27", "Desert Plant"},
            {"503.28", "Desert Tree"},
            {"503.29", "Desert Plant"},
            {"503.30", "Desert Tree"},
            {"504.01", "Woodland Bush"},
            {"504.02", "Woodland Bush"},
            {"504.03", "Woodland Rock"},
            {"504.04", "Woodland Rock"},
            {"504.05", "Woodland Rock"},
            {"504.06", "Woodland Rock"},
            {"504.07", "Woodland Bush"},
            {"504.08", "Woodland Bush"},
            {"504.09", "Woodland Bush"},
            {"504.10", "Woodland Bush"},
            {"504.11", "Woodland Bush"},
            {"504.12", "Woodland Tree"},
            {"504.13", "Woodland Tree"},
            {"504.14", "Woodland Tree"},
            {"504.15", "Woodland Tree"},
            {"504.16", "Woodland Tree"},
            {"504.17", "Woodland Tree"},
            {"504.18", "Woodland Tree"},
            {"504.19", "Woodland Trunk"},
            {"504.20", "Woodland Trunk"},
            {"504.21", "Woodland Flower"},
            {"504.22", "Woodland Flower"},
            {"504.23", "Woodland Mushroom"},
            {"504.24", "Woodland Bush"},
            {"504.25", "Woodland Tree"},
            {"504.26", "Woodland Fern"},
            {"504.27", "Woodland Bush"},
            {"504.28", "Woodland Bush"},
            {"504.29", "Woodland Bush"},
            {"504.30", "Woodland Tree"},
            {"504.31", "Woodland Logs"},
            {"506.01", "Woodland Hills Rock"},
            {"506.02", "Woodland Hills Grass"},
            {"506.03", "Woodland Hills Rock"},
            {"506.04", "Woodland Hills Rock"},
            {"506.05", "Woodland Hills Tree"},
            {"506.06", "Woodland Hills Rock"},
            {"506.07", "Woodland Hills Grass"},
            {"506.08", "Woodland Hills Grass"},
            {"506.09", "Woodland Hills Bush"},
            {"506.10", "Woodland Hills Plant"},
            {"506.11", "Woodland Hills Tree"},
            {"506.12", "Woodland Hills Tree"},
            {"506.13", "Woodland Hills Tree"},
            {"506.14", "Woodland Hills Tree"},
            {"506.15", "Woodland Hills Tree"},
            {"506.16", "Woodland Hills Tree"},
            {"506.17", "Woodland Hills Rock"},
            {"506.18", "Woodland Hills Rock"},
            {"506.19", "Woodland Hills Tree Trunk"},
            {"506.20", "Woodland Hills Tree Trunk"},
            {"506.21", "Woodland Hills Flower"},
            {"506.22", "Woodland Hills Flower"},
            {"506.23", "Woodland Hills Plant"},
            {"506.24", "Woodland Hills Tree"},
            {"506.25", "Woodland Hills Tree"},
            {"506.26", "Woodland Hills Plant"},
            {"506.27", "Woodland Hills Plant"},
            {"506.28", "Woodland Hills Rock"},
            {"506.29", "Woodland Hills Grass"},
            {"506.30", "Woodland Hills Tree"},
            {"506.31", "Woodland Hills Fern"},
            {"508.01", "Haunted Woodland Rock"},
            {"508.02", "Haunted Woodland Flower"},
            {"508.03", "Haunted Woodland Rock"},
            {"508.04", "Haunted Woodland Rock"},
            {"508.05", "Haunted Woodland Rock"},
            {"508.06", "Haunted Woodland Rock"},
            {"508.07", "Haunted Woodland Plant"},
            {"508.08", "Haunted Woodland Plant"},
            {"508.09", "Haunted Woodland Plant"},
            {"508.10", "Haunted Woodland Plant"},
            {"508.11", "Haunted Woodland Ribcage"},
            {"508.12", "Haunted Woodland Rock"},
            {"508.13", "Haunted Woodland Tree"},
            {"508.14", "Haunted Woodland Grass"},
            {"508.15", "Haunted Woodland Tree"},
            {"508.16", "Haunted Woodland Tree"},
            {"508.17", "Haunted Woodland Rock"},
            {"508.18", "Haunted Woodland Tree"},
            {"508.19", "Haunted Woodland Tree Trunk"},
            {"508.20", "Haunted Woodland Tree Trunk"},
            {"508.21", "Haunted Woodland Flower"},
            {"508.22", "Haunted Woodland Mushroom"},
            {"508.23", "Haunted Woodland Mushroom"},
            {"508.24", "Haunted Woodland Tree"},
            {"508.25", "Haunted Woodland Tree"},
            {"508.26", "Haunted Woodland Fern"},
            {"508.27", "Haunted Woodland Bush"},
            {"508.28", "Haunted Woodland Bush"},
            {"508.29", "Haunted Woodland Grass"},
            {"508.30", "Haunted Woodland Tree"},
            {"508.31", "Haunted Woodland Logs"},
            {"510.01", "Mountain Rock"},
            {"510.02", "Mountain Grass"},
            {"510.03", "Mountain Rock"},
            {"510.04", "Mountain Rock"},
            {"510.05", "Mountain Tree"},
            {"510.06", "Mountain Rock"},
            {"510.07", "Mountain Grass"},
            {"510.08", "Mountain Plant"},
            {"510.09", "Mountain Flower"},
            {"510.10", "Mountain Plant"},
            {"510.11", "Mountain Tree"},
            {"510.12", "Mountain Tree"},
            {"510.13", "Mountain Tree"},
            {"510.14", "Mountain Rock"},
            {"510.15", "Mountain Tree"},
            {"510.16", "Mountain Tree"},
            {"510.17", "Mountain Rock"},
            {"510.18", "Mountain Rock"},
            {"510.19", "Mountain Tree Trunk"},
            {"510.20", "Mountain Tree Trunk"},
            {"510.21", "Mountain Bush"},
            {"510.22", "Mountain Flower"},
            {"510.23", "Mountain Flower"},
            {"510.24", "Mountain Tree"},
            {"510.25", "Mountain Tree"},
            {"510.26", "Mountain Plant"},
            {"510.27", "Mountain Rock"},
            {"510.28", "Mountain Rock"},
            {"510.29", "Mountain Grass"},
            {"510.30", "Mountain Tree"},
            {"510.31", "Mountain Plant"},
        };

        public static Dictionary<string, string> billboardsPeople = new Dictionary<string, string>()
        {
                {"182.24", "Sitting_Alchemist"},
                {"184.22", "Woman w.Staff"},
                {"184.34", "Horseback_Man"},
                {"184.18", "Redguard_Woman"},
                {"184.21", "Wise_Man"},
                {"179.4", "Woman_Spear"},
                {"184.1", "Wise_Woman"},
                {"184.2", "Man_Sitting"},
                {"181.0", "Man_Bluerobe_Praying"},
                {"181.1", "Woman_Bluecloth_Praying"},
                {"181.2", "Priest_Bluerobe"},
                {"181.4", "Priestest_Bluerobe"},
                {"181.5", "Woman_Bluerobe_Dancing_1"},
                {"181.6", "Woman_Bluerobe_Dancing_2"},
                {"181.7", "Woman_Bluerobe_Dancing_3"},
                {"195.2", "Bandit_1"},
                {"195.7", "Bandit_2"},
                {"195.10", "Bandit_3"},
                {"195.12", "Woman_w.Cape"},
                {"184.31", "Prisoner_Male"},
        };

        public static Dictionary<string, string> billboardslights = new Dictionary<string, string>()
        {
                {"210.0", "Bowl of Fire"},
                {"210.1", "Camp Fire"},
                {"210.2", "Skull Candle"},
                {"210.3", "Candle"},
                {"210.4", "Candle w. Base"},
                {"210.5", "Candleholder with 3 candles"},
                {"210.6", "Skull torch"},
                {"210.7", "Wooden Chandelier w. Extinguished Candles"},
                {"210.8", "Turkis Lamp"},
                {"210.9", "Metallic Chandelier w. Burning Candles"},
                {"210.10", "Metallic Chandelier w. Extinguished Candles"},
                {"210.11", "Candle in Lamp"},
                {"210.12", "Extinguished Lamp"},
                {"210.13", "Round Lamp"},
                {"210.14", "Standing Lantern"},
                {"210.15", "Standing Lantern Round"},
                {"210.16", "Mounted Torch w. Thin Holder"},
                {"210.17", "Mounted Torch 1"},
                {"210.18", "Mounted Torch 2"},
                {"210.19", "Pillar w. Firebowl"},
                {"210.20", "Brazier Torch"},
                {"210.21", "Standing Candle"},
                {"210.22", "Round Lantern w. Medium Chain"},
                {"210.23", "Wooden Chandelier w. Burning Candles"},
                {"210.24", "Lantern w. Long Chain"},
                {"210.25", "Lantern w. Medium Chain"},
                {"210.26", "Lantern w. Short Chain"},
                {"210.27", "Lantern w, No Chain"},
                {"210.28", "Street Lantern 1"},
                {"210.29", "Street Lantern 2"},
        };
        public static Dictionary<string, string> billboardsTreasure = new Dictionary<string, string>()
        {
                {"216.0", "Goldpile 1"},
                {"216.1", "Goldpile 2"},
                {"216.2", "Goldpile 3"},
                {"216.3", "Gold Casket"},
                {"216.4", "Gold Coin"},
                {"216.5", "Silver Coin"},
                {"216.6", "Gold Crown 1"},
                {"216.7", "Silver Crown 1"},
                {"216.8", "Silver Crown 2"},
                {"216.9", "Gold Crown 2"},
                {"216.10", "Silver plate"},
                {"216.11", "Treasure 1"},
                {"216.12", "Treasure 2"},
                {"216.13", "Treasure 3"},
                {"216.14", "Treasure 4"},
                {"216.15", "Treasure 5"},
                {"216.16", "Treasure 6"},
                {"216.17", "Treasure 7"},
                {"216.18", "Treasure 8"},
                {"216.19", "Treasure 9"},
                {"216.20", "Treasure 10"},
                {"216.21", "Treasure 11"},
                {"216.22", "Treasure 12"},
                {"216.23", "Treasure 13"},
                {"216.24", "Treasure 14"},
                {"216.25", "Treasure 15"},
                {"216.26", "Treasure 16"},
                {"216.27", "Treasure 17"},
                {"216.28", "Treasure 18"},
                {"216.30", "Treasure 19"},
                {"216.31", "Treasure 20"},
                {"216.32", "Treasure 21"},
                {"216.33", "Treasure 22"},
                {"216.34", "Treasure 23"},
                {"216.35", "Treasure 24"},
                {"216.36", "Treasure 25"},
                {"216.37", "Treasure 26"},
                {"216.38", "Treasure 27"},
                {"216.39", "Treasure 28"},
                {"216.40", "Treasure 29"},
                {"216.41", "Treasure 30"},
                {"216.42", "Treasure 31"},
                {"216.43", "Treasure 32"},
                {"216.44", "Treasure 33"},
                {"216.45", "Treasure 34"},
                {"216.46", "Treasure 35"},
                {"216.47", "Treasure 36"},
        };

        public static Dictionary<string, string> editor = new Dictionary<string, string>()
        {
            { "199.4", "Rest Marker" },
            { "199.8", "Enter Marker" },
            { "199.10", "Start Marker" },
            { "199.11", "Quest Marker" },
            { "199.15", "Random Monster" },
            { "199.16", "Monster" },
            { "199.18", "Quest Item" },
            { "199.19", "Random Treasure" },
            { "199.21", "Ladder Bottom" },
            { "199.22", "Ladder Top" },
        };

        /// <summary>
        /// Returns list of locations based on file path. Return null if not found or wrong file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IEnumerable<LocationInstance> LoadLocationInstance(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning($"Location instance file could not be found at '{path}'");
                return null;
            }


            if (path.EndsWith(".csv"))
            {
                TextReader reader = File.OpenText(path);

                return LoadLocationInstanceCsv(reader, $"file={path}");
            }
            else
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(path);

                return LoadLocationInstance(xmlDoc, $"file={path}");
            }
        }

        public static IEnumerable<LocationInstance> LoadLocationInstance(Mod mod, string assetName)
        {
            TextAsset asset = mod.GetAsset<TextAsset>(assetName);
            if (asset == null)
            {
                Debug.LogWarning($"Asset '{assetName}' could not be found in mod '{mod.Title}'");
                return Enumerable.Empty<LocationInstance>();
            }

            TextReader reader = new StringReader(asset.text);
            if (assetName.EndsWith(".csv"))
            {
                return LoadLocationInstanceCsv(reader, $"mod={mod.Title}, asset={assetName}");
            }
            else
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(reader);

                return LoadLocationInstance(xmlDoc, $"mod={mod.Title}, asset={assetName}");
            }
        }

        public static IEnumerable<LocationInstance> LoadLocationInstance(XmlDocument xmlDoc, string contextString)
        {
            if (xmlDoc.SelectSingleNode("//locations") == null)
            {
                Debug.LogWarning("Wrong file format");
                yield return null;
            }

            CultureInfo cultureInfo = new CultureInfo("en-US");

            XmlNodeList instanceNodes = xmlDoc.GetElementsByTagName("locationInstance");
            for (int i = 0; i < instanceNodes.Count; i++)
            {
                XmlNode node = instanceNodes[i];
                if (node["prefab"].InnerXml == "")
                {
                    Debug.LogWarning("Locationinstance must have a assigned prefab to be valid");
                    continue;
                }

                LocationInstance tmpInst = new LocationInstance();
                try
                {
                    tmpInst.name = node["name"].InnerXml;
                    tmpInst.locationID = ulong.Parse(node["locationID"].InnerXml);
                    tmpInst.type = int.Parse(node["type"].InnerXml);
                    tmpInst.prefab = node["prefab"].InnerXml;
                    tmpInst.worldX = int.Parse(node["worldX"].InnerXml);
                    tmpInst.worldY = int.Parse(node["worldY"].InnerXml);
                    tmpInst.terrainX = int.Parse(node["terrainX"].InnerXml);
                    tmpInst.terrainY = int.Parse(node["terrainY"].InnerXml);

                    XmlNode child = node["rotW"];
                    if (child != null)
                    {
                        tmpInst.rot.w = float.Parse(child.InnerXml, cultureInfo);
                        tmpInst.rot.x = float.Parse(node["rotX"].InnerXml, cultureInfo);
                        tmpInst.rot.y = float.Parse(node["rotY"].InnerXml, cultureInfo);
                        tmpInst.rot.z = float.Parse(node["rotZ"].InnerXml, cultureInfo);
                    }
                    else
                    {
                        child = node["rotYAxis"];
                        if (child != null)
                        {
                            float yRot = float.Parse(child.InnerXml, cultureInfo);
                            tmpInst.rot.eulerAngles = new Vector3(0, yRot, 0);
                        }
                    }

                    child = node["heightOffset"];
                    if (child != null)
                    {
                        tmpInst.heightOffset = float.Parse(child.InnerXml, cultureInfo);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Error while parsing location instance: {e.Message}");
                    continue;
                }

                if (tmpInst.terrainX < 0 || tmpInst.terrainY < 0 || tmpInst.terrainX >= 128 || tmpInst.terrainY >= 128)
                {
                    Debug.LogWarning($"Invalid location instance '{tmpInst.name}' ({contextString}): terrainX and terrainY must be higher than 0 and lower than 128");
                    continue;
                }

                yield return tmpInst;
            }
        }

        public static IEnumerable<LocationInstance> LoadLocationInstanceCsv(TextReader csvStream, string contextString)
        {
            string header = csvStream.ReadLine();
            string[] fields = header.Split(';', ',');

            bool GetIndex(string fieldName, out int index)
            {
                index = -1;
                for (int i = 0; i < fields.Length; ++i)
                {
                    if (fields[i].Equals(fieldName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        index = i;
                        break;
                    }
                }
                if (index == -1)
                {
                    Debug.LogError($"Location instance file failed ({contextString}): could not find field '{fieldName}' in header");
                    return false;
                }
                return true;
            }

            int? GetIndexOpt(string fieldName)
            {
                int index = -1;
                for (int i = 0; i < fields.Length; ++i)
                {
                    if (fields[i].Equals(fieldName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        index = i;
                        break;
                    }
                }
                if (index == -1)
                {
                    return null;
                }
                return index;
            }

            if (!GetIndex("name", out int nameIndex)) yield break;
            if (!GetIndex("type", out int typeIndex)) yield break;
            if (!GetIndex("prefab", out int prefabIndex)) yield break;
            if (!GetIndex("worldX", out int worldXIndex)) yield break;
            if (!GetIndex("worldY", out int worldYIndex)) yield break;
            if (!GetIndex("terrainX", out int terrainXIndex)) yield break;
            if (!GetIndex("terrainY", out int terrainYIndex)) yield break;
            if (!GetIndex("locationID", out int locationIDIndex)) yield break;
            int? rotWIndex = GetIndexOpt("rotW");
            int? rotXIndex = GetIndexOpt("rotX");
            int? rotYIndex = GetIndexOpt("rotY");
            int? rotZIndex = GetIndexOpt("rotZ");
            int? rotXAxisIndex = GetIndexOpt("rotXAxis");
            int? rotYAxisIndex = GetIndexOpt("rotYAxis");
            int? rotZAxisIndex = GetIndexOpt("rotZAxis");
            int? heightOffsetIndex = GetIndexOpt("heightOffset");

            CultureInfo cultureInfo = new CultureInfo("en-US");
            int lineNumber = 1;
            while (csvStream.Peek() >= 0)
            {
                ++lineNumber;
                string line = csvStream.ReadLine();
                string[] tokens = line.Split(';', ',');

                LocationInstance tmpInst = new LocationInstance();

                try
                {
                    tmpInst.name = tokens[nameIndex];
                    tmpInst.type = int.Parse(tokens[typeIndex]);
                    tmpInst.prefab = tokens[prefabIndex];
                    tmpInst.worldX = int.Parse(tokens[worldXIndex]);
                    tmpInst.worldY = int.Parse(tokens[worldYIndex]);
                    tmpInst.terrainX = int.Parse(tokens[terrainXIndex]);
                    tmpInst.terrainY = int.Parse(tokens[terrainYIndex]);
                    tmpInst.locationID = ulong.Parse(tokens[locationIDIndex]);

                    if (rotWIndex.HasValue) tmpInst.rot.w = float.Parse(tokens[rotWIndex.Value], cultureInfo);
                    if (rotXIndex.HasValue) tmpInst.rot.x = float.Parse(tokens[rotXIndex.Value], cultureInfo);
                    if (rotYIndex.HasValue) tmpInst.rot.y = float.Parse(tokens[rotYIndex.Value], cultureInfo);
                    if (rotZIndex.HasValue) tmpInst.rot.z = float.Parse(tokens[rotZIndex.Value], cultureInfo);
                    if (rotXAxisIndex.HasValue) tmpInst.rot.eulerAngles = new Vector3(float.Parse(tokens[rotXAxisIndex.Value], cultureInfo), tmpInst.rot.eulerAngles.y, tmpInst.rot.eulerAngles.z);
                    if (rotYAxisIndex.HasValue) tmpInst.rot.eulerAngles = new Vector3(tmpInst.rot.eulerAngles.x, float.Parse(tokens[rotYAxisIndex.Value], cultureInfo), tmpInst.rot.eulerAngles.z);
                    if (rotZAxisIndex.HasValue) tmpInst.rot.eulerAngles = new Vector3(tmpInst.rot.eulerAngles.x, tmpInst.rot.eulerAngles.y, float.Parse(tokens[rotZAxisIndex.Value], cultureInfo));
                    if (heightOffsetIndex.HasValue) tmpInst.heightOffset = float.Parse(tokens[heightOffsetIndex.Value], cultureInfo);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to parse a location instance ({contextString}, line {lineNumber}): {e.Message}");
                    continue;
                }

                yield return tmpInst;

            }
        }

        public static LocationInstance LoadSingleLocationInstanceCsv(string line, string[] fields, string contextString)
        {
            bool GetIndex(string fieldName, out int index)
            {
                index = -1;
                for (int i = 0; i < fields.Length; ++i)
                {
                    if (fields[i].Equals(fieldName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        index = i;
                        break;
                    }
                }
                if (index == -1)
                {
                    Debug.LogError($"Location instance file failed ({contextString}): could not find field '{fieldName}' in header");
                    return false;
                }
                return true;
            }

            int? GetIndexOpt(string fieldName)
            {
                int index = -1;
                for (int i = 0; i < fields.Length; ++i)
                {
                    if (fields[i].Equals(fieldName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        index = i;
                        break;
                    }
                }
                if (index == -1)
                {
                    return null;
                }
                return index;
            }

            if (!GetIndex("name", out int nameIndex)) return null;
            if (!GetIndex("type", out int typeIndex)) return null;
            if (!GetIndex("prefab", out int prefabIndex)) return null;
            if (!GetIndex("worldX", out int worldXIndex)) return null;
            if (!GetIndex("worldY", out int worldYIndex)) return null;
            if (!GetIndex("terrainX", out int terrainXIndex)) return null;
            if (!GetIndex("terrainY", out int terrainYIndex)) return null;
            if (!GetIndex("locationID", out int locationIDIndex)) return null;
            int? rotWIndex = GetIndexOpt("rotW");
            int? rotXIndex = GetIndexOpt("rotX");
            int? rotYIndex = GetIndexOpt("rotY");
            int? rotZIndex = GetIndexOpt("rotZ");
            int? rotXAxisIndex = GetIndexOpt("rotXAxis");
            int? rotYAxisIndex = GetIndexOpt("rotYAxis");
            int? rotZAxisIndex = GetIndexOpt("rotZAxis");
            int? heightOffsetIndex = GetIndexOpt("heightOffset");

            CultureInfo cultureInfo = new CultureInfo("en-US");

            string[] tokens = line.Split(';', ',');

            LocationInstance tmpInst = new LocationInstance();

            try
            {
                tmpInst.name = tokens[nameIndex];
                tmpInst.type = int.Parse(tokens[typeIndex]);
                tmpInst.prefab = tokens[prefabIndex];
                tmpInst.worldX = int.Parse(tokens[worldXIndex]);
                tmpInst.worldY = int.Parse(tokens[worldYIndex]);
                tmpInst.terrainX = int.Parse(tokens[terrainXIndex]);
                tmpInst.terrainY = int.Parse(tokens[terrainYIndex]);
                tmpInst.locationID = ulong.Parse(tokens[locationIDIndex]);

                if (rotWIndex.HasValue) tmpInst.rot.w = float.Parse(tokens[rotWIndex.Value], cultureInfo);
                if (rotXIndex.HasValue) tmpInst.rot.x = float.Parse(tokens[rotXIndex.Value], cultureInfo);
                if (rotYIndex.HasValue) tmpInst.rot.y = float.Parse(tokens[rotYIndex.Value], cultureInfo);
                if (rotZIndex.HasValue) tmpInst.rot.z = float.Parse(tokens[rotZIndex.Value], cultureInfo);
                if (rotXAxisIndex.HasValue) tmpInst.rot.eulerAngles = new Vector3(float.Parse(tokens[rotXAxisIndex.Value], cultureInfo), tmpInst.rot.eulerAngles.y, tmpInst.rot.eulerAngles.z);
                if (rotYAxisIndex.HasValue) tmpInst.rot.eulerAngles = new Vector3(tmpInst.rot.eulerAngles.x, float.Parse(tokens[rotYAxisIndex.Value], cultureInfo), tmpInst.rot.eulerAngles.z);
                if (rotZAxisIndex.HasValue) tmpInst.rot.eulerAngles = new Vector3(tmpInst.rot.eulerAngles.x, tmpInst.rot.eulerAngles.y, float.Parse(tokens[rotZAxisIndex.Value], cultureInfo));
                if (heightOffsetIndex.HasValue) tmpInst.heightOffset = float.Parse(tokens[heightOffsetIndex.Value], cultureInfo);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse a location instance ({contextString}): {e.Message}");
                return null;
            }

            return tmpInst;
        }

        /// <summary>
        /// Save location list to path
        /// </summary>
        /// <param name="locationInstance"></param>
        /// <param name="path"></param>
        public static void SaveLocationInstance(LocationInstance[] locationInstance, string path)
        {
            StreamWriter writer = new StreamWriter(path, false);
            writer.WriteLine("<locations>");

            foreach (LocationInstance inst in locationInstance)
            {
                writer.WriteLine("\t<locationInstance>");
                writer.WriteLine("\t\t<name>" + inst.name + "</name>");
                writer.WriteLine("\t\t<locationID>" + inst.locationID + "</locationID>");
                writer.WriteLine("\t\t<type>" + inst.type + "</type>");
                writer.WriteLine("\t\t<prefab>" + inst.prefab + "</prefab>");
                writer.WriteLine("\t\t<worldX>" + inst.worldX + "</worldX>");
                writer.WriteLine("\t\t<worldY>" + inst.worldY + "</worldY>");
                writer.WriteLine("\t\t<terrainX>" + inst.terrainX + "</terrainX>");
                writer.WriteLine("\t\t<terrainY>" + inst.terrainY + "</terrainY>");
                if(inst.rot != Quaternion.identity)
                {
                    writer.WriteLine($"\t\t<rotW>{inst.rot.w}</rotW");
                    writer.WriteLine($"\t\t<rotX>{inst.rot.x}</rotW");
                    writer.WriteLine($"\t\t<rotY>{inst.rot.y}</rotW");
                    writer.WriteLine($"\t\t<rotZ>{inst.rot.z}</rotW");
                }
                if(inst.heightOffset != 0f)
                {
                    writer.WriteLine($"\t\t<heightOffset>{inst.heightOffset}</heightOffset>");
                }
                writer.WriteLine("\t</locationInstance>");
            }

            writer.WriteLine("</locations>");
            writer.Close();
        }

        /// <summary>
        /// Returns locationprefab based on file path. Return null if not found or wrong file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static LocationPrefab LoadLocationPrefab(string path)
        {
            if (!File.Exists(path))
                return null;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);

            return LoadLocationPrefab(xmlDoc);
        }

        public static LocationPrefab LoadLocationPrefab(Mod mod, string assetName)
        {
            TextAsset asset = mod.GetAsset<TextAsset>(assetName);
            if (asset == null)
                return null;

            TextReader reader = new StringReader(asset.text);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(reader);

            return LoadLocationPrefab(xmlDoc);
        }

        public static LocationPrefab LoadLocationPrefab(XmlDocument xmlDoc)
        {
            XmlNode prefabNode = xmlDoc.SelectSingleNode("//locationPrefab");
            if (prefabNode == null)
            {
                Debug.LogWarning("Wrong file format");
                return null;
            }

            CultureInfo cultureInfo = new CultureInfo("en-US");

            LocationPrefab locationPrefab = new LocationPrefab();

            try
            {
                locationPrefab.height = int.Parse(prefabNode["height"].InnerXml);
                locationPrefab.width = int.Parse(prefabNode["width"].InnerXml);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Error while parsing location prefab: {e.Message}");
                return null;
            }

            var objects = xmlDoc.GetElementsByTagName("object");
            for (int i = 0; i < objects.Count; i++)
            {
                XmlNode objectNode = objects[i];

                var obj = new LocationObject();

                try
                {
                    obj.type = int.Parse(objectNode["type"].InnerXml);
                    obj.name = objectNode["name"].InnerXml;

                    obj.objectID = int.Parse(objectNode["objectID"].InnerXml);

                    obj.pos.x = float.Parse(objectNode["posX"].InnerXml, cultureInfo);
                    obj.pos.y = float.Parse(objectNode["posY"].InnerXml, cultureInfo);
                    obj.pos.z = float.Parse(objectNode["posZ"].InnerXml, cultureInfo);

                    obj.scale.x = float.Parse(objectNode["scaleX"].InnerXml, cultureInfo);
                    obj.scale.y = float.Parse(objectNode["scaleY"].InnerXml, cultureInfo);
                    obj.scale.z = float.Parse(objectNode["scaleZ"].InnerXml, cultureInfo);

                    if (obj.type == 0)
                    {
                        obj.rot.w = float.Parse(objectNode["rotW"].InnerXml, cultureInfo);
                        obj.rot.x = float.Parse(objectNode["rotX"].InnerXml, cultureInfo);
                        obj.rot.y = float.Parse(objectNode["rotY"].InnerXml, cultureInfo);
                        obj.rot.z = float.Parse(objectNode["rotZ"].InnerXml, cultureInfo);
                    }

                    var extraDataNode = objectNode["extraData"];
                    if (extraDataNode != null)
                    {
                        obj.extraData = extraDataNode.InnerXml;
                    }

                    if (!ValidateValue(obj.type, obj.name))
                        continue;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Error while parsing location prefab object: {e.Message}");
                    continue;
                }

                locationPrefab.obj.Add(obj);
            }
            return locationPrefab;
        }

        /// <summary>
        /// Save locationprefab to path
        /// </summary>
        /// <param name="locationPrefab"></param>
        /// <param name="path"></param>
        public static void SaveLocationPrefab(LocationPrefab locationPrefab, string path)
        {
            //Write some text to the test.txt file
            StreamWriter writer = new StreamWriter(path, false);

            writer.WriteLine("<locationPrefab>");
            writer.WriteLine("\t<height>" + locationPrefab.height + "</height>");
            writer.WriteLine("\t<width>" + locationPrefab.width + "</width>");

            foreach (LocationObject obj in locationPrefab.obj)
            {
                writer.WriteLine("\t<object>");
                writer.WriteLine("\t\t<type>" + obj.type + "</type>");
                writer.WriteLine("\t\t<objectID>" + obj.objectID + "</objectID>");
                writer.WriteLine("\t\t<name>" + obj.name + "</name>");

                writer.WriteLine("\t\t<posX>" + obj.pos.x + "</posX>");
                writer.WriteLine("\t\t<posY>" + obj.pos.y + "</posY>");
                writer.WriteLine("\t\t<posZ>" + obj.pos.z + "</posZ>");

                writer.WriteLine("\t\t<scaleX>" + obj.scale.x + "</scaleX>");
                writer.WriteLine("\t\t<scaleY>" + obj.scale.y + "</scaleY>");
                writer.WriteLine("\t\t<scaleZ>" + obj.scale.z + "</scaleZ>");

                if (!string.IsNullOrEmpty(obj.extraData))
                {
                    writer.WriteLine("\t\t<extraData>" + obj.extraData + "</extraData>");
                }

                if (obj.type == 0)
                {
                    writer.WriteLine("\t\t<rotW>" + obj.rot.w + "</rotW>");
                    writer.WriteLine("\t\t<rotX>" + obj.rot.x + "</rotX>");
                    writer.WriteLine("\t\t<rotY>" + obj.rot.y + "</rotY>");
                    writer.WriteLine("\t\t<rotZ>" + obj.rot.z + "</rotZ>");
                }

                writer.WriteLine("\t</object>");
            }

            writer.WriteLine("</locationPrefab>");
            writer.Close();
        }

        /// <summary>
        /// Validate the name of the object
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool ValidateValue(int type, string name)
        {
            if (type == 0)
            {
                try
                {
                    int.Parse(name);
                }
                catch (FormatException)
                {
                    Debug.LogWarning("Object type is set incorrectly: 0 = Model, 1 = Flat");
                    return false;
                }
                catch (OverflowException)
                {
                    Debug.LogWarning("Object type is set incorrectly: 0 = Model, 1 = Flat");
                    return false;
                }

                return true;
            }

            else if (type == 1)
            {
                string[] arg = name.Split('.');

                if (arg.Length == 2)
                {
                    try
                    {
                        int.Parse(arg[0]);
                        int.Parse(arg[1]);
                    }
                    catch (FormatException)
                    {
                        Debug.LogWarning("Billboard string format is invalid, use ARCHIVEID.RECORDID");
                        return false;
                    }
                    catch (OverflowException)
                    {
                        Debug.LogWarning("Billboard string format is invalid, use ARCHIVEID.RECORDID");
                        return false;
                    }

                    return true;
                }

                Debug.LogWarning("Billboard string format is invalid, use ARCHIVEID.RECORDID");
                return false;

            }
            else if (type == 2)
            {
                string[] arg = name.Split('.');

                if (arg.Length == 2)
                {
                    try
                    {
                        if (int.Parse(arg[0]) != 199)
                        {
                            Debug.LogWarning("Editor marker name format is invalid, use 199.RECORDID");
                            return false;
                        }
                        int.Parse(arg[1]);
                    }
                    catch (FormatException)
                    {
                        Debug.LogWarning("Editor marker name format is invalid, use 199.RECORDID");
                        return false;
                    }
                    catch (OverflowException)
                    {
                        Debug.LogWarning("Editor marker name format is invalid, use 199.RECORDID");
                        return false;
                    }

                    return true;
                }

                Debug.LogWarning("Editor marker name format is invalid, use 199.RECORDID");
                return false;
            }
            else
            {
                Debug.LogWarning($"Invalid obj type found: {type}");
                return false;
            }
        }

        /// <summary>
        /// Load a Game Object
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static GameObject LoadStaticObject(int type, string name, Transform parent, Vector3 pos, Quaternion rot, Vector3 scale, ulong locationID, int objID, ModelCombiner modelCombiner = null)
        {
            GameObject go = null;
            //Model
            if (type == 0)
            {
                if (rot.x == 0 && rot.y == 0 && rot.z == 0 && rot.w == 0)
                {
                    Debug.LogWarning($"Object {name} inside prefab has invalid rotation: {rot}");
                    rot = Quaternion.identity;
                }

                Matrix4x4 mat = Matrix4x4.TRS(pos, rot, scale);

                uint modelId = uint.Parse(name);

                go = MeshReplacement.ImportCustomGameobject(modelId, parent, mat);

                if (go == null) //if no mesh replacment exist
                {
                    if (modelCombiner != null && !PlayerActivate.HasCustomActivation(modelId))
                    {
                        ModelData modelData;
                        DaggerfallUnity.Instance.MeshReader.GetModelData(modelId, out modelData);

                        modelCombiner.Add(ref modelData, mat);
                    }
                    else
                    {
                        go = GameObjectHelper.CreateDaggerfallMeshGameObject(modelId, parent);
                        if (go != null)
                        {
                            go.transform.localPosition = pos;
                            go.transform.localRotation = rot;
                            go.transform.localScale = scale;
                        }
                    }
                }
            }

            //Flat
            else if (type == 1)
            {
                string[] arg = name.Split('.');

                go = MeshReplacement.ImportCustomFlatGameobject(int.Parse(arg[0]), int.Parse(arg[1]), pos, parent);

                if (go == null)
                {
                    go = GameObjectHelper.CreateDaggerfallBillboardGameObject(int.Parse(arg[0]), int.Parse(arg[1]), parent);

                    if (go != null)
                    {
                        go.transform.localPosition = pos;

                        if (arg[0] == "210")
                            AddLight(int.Parse(arg[1]), go.transform);

                        if (arg[0] == "201")
                            AddAnimalAudioSource(int.Parse(arg[1]), go);
                    }
                }

                if (go != null)
                {
                    go.transform.localScale = new Vector3(go.transform.localScale.x * scale.x, go.transform.localScale.y * scale.y, go.transform.localScale.z * scale.z);
                }
            }

            return go;
        }

        /// <summary>
        /// Adds a light to a flat. This is a modified copy of a method with the same name, found in DaggerfallInterior.cs
        /// </summary>
        /// <param name="id"></param>
        /// <param name="parent"></param>
        public static void AddLight(int textureRecord, Transform parent)
        {
            Debug.Log("Add Light");

            GameObject go = GameObjectHelper.InstantiatePrefab(DaggerfallUnity.Instance.Option_InteriorLightPrefab.gameObject, string.Empty, parent, parent.position);
            Vector2 size = DaggerfallUnity.Instance.MeshReader.GetScaledBillboardSize(210, textureRecord) * MeshReader.GlobalScale;
            Light light = go.GetComponent<Light>();
            switch (textureRecord)
            {
                case 0:         // Bowl with fire
                    go.transform.localPosition += new Vector3(0, -0.1f, 0);
                    break;
                case 1:         // Campfire
                                // todo
                    break;
                case 2:         // Skull candle
                    go.transform.localPosition += new Vector3(0, 0.1f, 0);
                    break;
                case 3:         // Candle
                    go.transform.localPosition += new Vector3(0, 0.1f, 0);
                    break;
                case 4:         // Candle in bowl
                                // todo
                    break;
                case 5:         // Candleholder with 3 candles
                    go.transform.localPosition += new Vector3(0, 0.15f, 0);
                    break;
                case 6:         // Skull torch
                    go.transform.localPosition += new Vector3(0, 0.6f, 0);
                    break;
                case 7:         // Wooden chandelier with extinguished candles
                                // todo
                    break;
                case 8:         // Turkis lamp
                                // do nothing
                    break;
                case 9:        // Metallic chandelier with burning candles
                    go.transform.localPosition += new Vector3(0, 0.4f, 0);
                    break;
                case 10:         // Metallic chandelier with extinguished candles
                                 // todo
                    break;
                case 11:        // Candle in lamp
                    go.transform.localPosition += new Vector3(0, -0.4f, 0);
                    break;
                case 12:         // Extinguished lamp
                                 // todo
                    break;
                case 13:        // Round lamp (e.g. main lamp in mages guild)
                    go.transform.localPosition += new Vector3(0, -0.35f, 0);
                    break;
                case 14:        // Standing lantern
                    go.transform.localPosition += new Vector3(0, size.y / 2, 0);
                    break;
                case 15:        // Standing lantern round
                    go.transform.localPosition += new Vector3(0, size.y / 2, 0);
                    break;
                case 16:         // Mounted Torch with thin holder
                                 // todo
                    break;
                case 17:        // Mounted torch 1
                    go.transform.localPosition += new Vector3(0, 0.2f, 0);
                    break;
                case 18:         // Mounted Torch 2
                                 // todo
                    break;
                case 19:         // Pillar with firebowl
                                 // todo
                    break;
                case 20:        // Brazier torch
                    go.transform.localPosition += new Vector3(0, 0.6f, 0);
                    break;
                case 21:        // Standing candle
                    go.transform.localPosition += new Vector3(0, size.y / 2.4f, 0);
                    break;
                case 22:         // Round lantern with medium chain
                    go.transform.localPosition += new Vector3(0, -0.5f, 0);
                    break;
                case 23:         // Wooden chandelier with burning candles
                                 // todo
                    break;
                case 24:        // Lantern with long chain
                    go.transform.localPosition += new Vector3(0, -1.85f, 0);
                    break;
                case 25:        // Lantern with medium chain
                    go.transform.localPosition += new Vector3(0, -1.0f, 0);
                    break;
                case 26:        // Lantern with short chain
                                // todo
                    break;
                case 27:        // Lantern with no chain
                    go.transform.localPosition += new Vector3(0, -0.02f, 0);
                    break;
                case 28:        // Street Lantern 1
                                // todo
                    break;
                case 29:        // Street Lantern 2
                    go.transform.localPosition += new Vector3(0, size.y / 2, 0);
                    break;
            }
            switch (textureRecord)
            {
                case 0:         // Bowl with fire
                    light.intensity = 1.2f;
                    light.range = 15f;
                    light.color = new Color32(255, 147, 41, 255);
                    break;
                case 1:         // Campfire
                                // todo
                    break;
                case 2:         // Skull candle
                    light.range /= 3f;
                    light.intensity = 0.6f;
                    light.color = new Color(1.0f, 0.99f, 0.82f);
                    break;
                case 3:         // Candle
                    light.range /= 3f;
                    break;
                case 4:         // Candle with base
                    light.range /= 3f;
                    break;
                case 5:         // Candleholder with 3 candles
                    light.range = 7.5f;
                    light.intensity = 0.33f;
                    light.color = new Color(1.0f, 0.89f, 0.61f);
                    break;
                case 6:         // Skull torch
                    light.range = 15.0f;
                    light.intensity = 0.75f;
                    light.color = new Color(1.0f, 0.93f, 0.62f);
                    break;
                case 7:         // Wooden chandelier with extinguished candles
                                // todo
                    break;
                case 8:         // Turkis lamp
                    light.color = new Color(0.68f, 1.0f, 0.94f);
                    break;
                case 9:        // metallic chandelier with burning candles
                    light.range = 15.0f;
                    light.intensity = 0.65f;
                    light.color = new Color(1.0f, 0.92f, 0.6f);
                    break;
                case 10:         // Metallic chandelier with extinguished candles
                                 // todo
                    break;
                case 11:        // Candle in lamp
                    light.range = 5.0f;
                    light.intensity = 0.5f;
                    break;
                case 12:         // Extinguished lamp
                                 // todo
                    break;
                case 13:        // Round lamp (e.g. main lamp in mages guild)
                    light.range *= 1.2f;
                    light.intensity = 1.1f;
                    light.color = new Color(0.93f, 0.84f, 0.49f);
                    break;
                case 14:        // Standing lantern
                                // todo
                    break;
                case 15:        // Standing lantern round
                                // todo
                    break;
                case 16:         // Mounted Torch with thin holder
                                 // todo
                    break;
                case 17:        // Mounted torch 1
                    light.intensity = 0.8f;
                    light.color = new Color(1.0f, 0.97f, 0.87f);
                    break;
                case 18:         // Mounted Torch 2
                                 // todo
                    break;
                case 19:         // Pillar with firebowl
                                 // todo
                    break;
                case 20:        // Brazier torch
                    light.range = 12.0f;
                    light.intensity = 0.75f;
                    light.color = new Color(1.0f, 0.92f, 0.72f);
                    break;
                case 21:        // Standing candle
                    light.range /= 3f;
                    light.intensity = 0.5f;
                    light.color = new Color(1.0f, 0.95f, 0.67f);
                    break;
                case 22:         // Round lantern with medium chain
                    light.intensity = 1.5f;
                    light.color = new Color(1.0f, 0.95f, 0.78f);
                    break;
                case 23:         // Wooden chandelier with burning candles
                                 // todo
                    break;
                case 24:        // Lantern with long chain
                    light.intensity = 1.4f;
                    light.color = new Color(1.0f, 0.98f, 0.64f);
                    break;
                case 25:        // Lantern with medium chain
                    light.intensity = 1.4f;
                    light.color = new Color(1.0f, 0.98f, 0.64f);
                    break;
                case 26:        // Lantern with short chain
                    light.intensity = 1.4f;
                    light.color = new Color(1.0f, 0.98f, 0.64f);
                    break;
                case 27:        // Lantern with no chain
                    light.intensity = 1.4f;
                    light.color = new Color(1.0f, 0.98f, 0.64f);
                    break;
                case 28:        // Street Lantern 1
                                // todo
                    break;
                case 29:        // Street Lantern 2
                                // todo
                    break;
                default:
                    light.intensity = 1.2f;
                    light.range = 15f;
                    light.color = new Color32(255, 147, 41, 255);
                    break;
            }
        }
        /// <summary>
        /// Add audioSource to animals. Is a modified copy version of a method with the same name, found in RMBLayout.cs
        /// </summary>
        /// <param name="id"></param>
        /// <param name="go"></param>
        public static void AddAnimalAudioSource(int textureRecord, GameObject go)
        {
            DaggerfallAudioSource source = go.AddComponent<DaggerfallAudioSource>();
            source.AudioSource.maxDistance = animalSoundMaxDistance;

            SoundClips sound = SoundClips.None;
            switch (textureRecord)
            {
                case 0:
                case 1:
                    sound = SoundClips.AnimalHorse;
                    break;
                case 3:
                case 4:
                    sound = SoundClips.AnimalCow;
                    break;
                case 5:
                case 6:
                    sound = SoundClips.AnimalPig;
                    break;
                case 7:
                case 8:
                    sound = SoundClips.AnimalCat;
                    break;
                case 9:
                case 10:
                    sound = SoundClips.AnimalDog;
                    break;
                default:
                    sound = SoundClips.None;
                    break;
            }

            source.SetSound(sound, AudioPresets.PlayRandomlyIfPlayerNear);
        }

        /// <summary>
        /// Creates a loot container. Is a modified copy version of a method with the same name, found in GameObjectHelper.cs
        /// </summary>
        /// <param name="billboardPosition"></param>
        /// <param name="parent"></param>
        /// <param name="locationID"></param>
        /// <param name="objID"></param>
        /// <param name="textureArchive"></param>
        /// <param name="textureRecord"></param>
        public static GameObject CreateLootContainer(ulong locationID, int objID, int textureArchive, int textureRecord, Transform parent)
        {
            GameObject go = GameObject.Instantiate(DaggerfallUnity.Instance.Option_LootContainerPrefab.gameObject);

            // We use our own serializer, get rid of the DFU one
            SerializableLootContainer serializableLootContainer = go.GetComponent<SerializableLootContainer>();
            if (serializableLootContainer != null)
            {
                GameObject.Destroy(serializableLootContainer);
            }

            
            // Setup DaggerfallLoot component to make lootable
            DaggerfallLoot loot = go.GetComponent<DaggerfallLoot>();
            if (loot)
            {
                ulong v = (uint)objID;
                loot.LoadID = (locationID << 16) | v;
                loot.WorldContext = WorldContext.Exterior;
                loot.ContainerType = LootContainerTypes.RandomTreasure;
                loot.TextureArchive = textureArchive;
                loot.TextureRecord = textureRecord;

                LocationLootSerializer serializer = go.AddComponent<LocationLootSerializer>();
                if (!serializer.TryLoadSavedData())
                {
                    // We had no existing save, generate new loot
                    if (!LootTables.GenerateLoot(loot, 2))
                        DaggerfallUnity.LogMessage(string.Format("DaggerfallInterior: Location type {0} is out of range or unknown.", 0, true));

                    var billboard = go.GetComponent<DaggerfallBillboard>();
                    if (billboard != null)
                        go.GetComponent<DaggerfallBillboard>().SetMaterial(textureArchive, textureRecord);

                    loot.stockedDate = DaggerfallLoot.CreateStockedDate(DaggerfallUnity.Instance.WorldTime.Now);
                }
            }
            
            go.transform.parent = parent;

            return go;
        }

        const byte Road_N = 128;//0b_1000_0000;
        const byte Road_NE = 64; //0b_0100_0000;
        const byte Road_E = 32; //0b_0010_0000;
        const byte Road_SE = 16; //0b_0001_0000;
        const byte Road_S = 8;  //0b_0000_1000;
        const byte Road_SW = 4;  //0b_0000_0100;
        const byte Road_W = 2;  //0b_0000_0010;
        const byte Road_NW = 1;  //0b_0000_0001;

        /// <summary>
        /// Checks if a location overlaps with a BasicRoad road.
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="locationPrefab"></param>
        /// <param name="pathsDataPoint">Bytefield representing which cardinal directions have roads on the terrain</param>
        /// <returns></returns>
        public static bool OverlapsRoad(LocationInstance loc, LocationPrefab locationPrefab, byte pathsDataPoint)
        {
            RectInt locationRect = new RectInt(loc.terrainX, loc.terrainY, locationPrefab.width, locationPrefab.height);
            return OverlapsRoad(locationRect, pathsDataPoint);
        }

        public static bool OverlapsRoad(RectInt locationRect, byte pathsDataPoint)
        {
            Vector2Int locationTopLeft = new Vector2Int(locationRect.xMin, locationRect.yMax);
            Vector2Int locationTopRight = new Vector2Int(locationRect.xMax, locationRect.yMax);
            Vector2Int locationBottomLeft = new Vector2Int(locationRect.xMin, locationRect.yMin);
            Vector2Int locationBottomRight = new Vector2Int(locationRect.xMax, locationRect.yMin);

            const int TERRAIN_SIZE = LocationLoader.TERRAIN_SIZE;
            const int HALF_TERRAIN_SIZE = LocationLoader.TERRAIN_SIZE / 2;
            const int ROAD_WIDTH = LocationLoader.ROAD_WIDTH;
            const int HALF_ROAD_WIDTH = LocationLoader.ROAD_WIDTH / 2;

            if ((pathsDataPoint & Road_N) != 0)
            {
                if (locationRect.Overlaps(new RectInt(HALF_TERRAIN_SIZE - HALF_ROAD_WIDTH, HALF_TERRAIN_SIZE, ROAD_WIDTH, HALF_TERRAIN_SIZE)))
                    return true;
            }

            if ((pathsDataPoint & Road_E) != 0)
            {
                if (locationRect.Overlaps(new RectInt(HALF_TERRAIN_SIZE, HALF_TERRAIN_SIZE - HALF_ROAD_WIDTH, HALF_TERRAIN_SIZE, ROAD_WIDTH)))
                    return true;
            }

            if ((pathsDataPoint & Road_S) != 0)
            {
                if (locationRect.Overlaps(new RectInt(HALF_TERRAIN_SIZE - HALF_ROAD_WIDTH, 0, ROAD_WIDTH, HALF_TERRAIN_SIZE)))
                    return true;
            }

            if ((pathsDataPoint & Road_W) != 0)
            {
                if (locationRect.Overlaps(new RectInt(0, HALF_TERRAIN_SIZE - HALF_ROAD_WIDTH, HALF_TERRAIN_SIZE, ROAD_WIDTH)))
                    return true;
            }

            if ((pathsDataPoint & Road_NE) != 0)
            {
                // Location can only overlap if anywhere in the top-right quadrant
                if (locationTopRight.x >= HALF_TERRAIN_SIZE && locationTopRight.y >= HALF_TERRAIN_SIZE)
                {
                    float topLeftDiff = locationTopLeft.x - locationTopLeft.y;
                    float bottomRightDiff = locationBottomRight.x - locationBottomRight.y;

                    // Corner overlaps the path
                    if (Mathf.Abs(topLeftDiff) <= HALF_ROAD_WIDTH || Mathf.Abs(bottomRightDiff) <= HALF_ROAD_WIDTH)
                    {
                        return true;
                    }

                    // If corners are on different sides of the path, we have an overlap
                    if (Mathf.Sign(topLeftDiff) != Mathf.Sign(bottomRightDiff))
                    {
                        return true;
                    }
                }
            }

            if ((pathsDataPoint & Road_SE) != 0)
            {
                // Location can only overlap if anywhere in the bottom-right quadrant
                if (locationBottomRight.x >= HALF_TERRAIN_SIZE && locationBottomRight.y <= HALF_TERRAIN_SIZE)
                {
                    float bottomLeftDiff = locationBottomLeft.x + locationBottomLeft.y - TERRAIN_SIZE;
                    float topRightDiff = locationTopRight.x + locationTopRight.y - TERRAIN_SIZE;

                    // Corner overlaps the path
                    if (Mathf.Abs(bottomLeftDiff) <= HALF_ROAD_WIDTH || Mathf.Abs(topRightDiff) <= HALF_ROAD_WIDTH)
                    {
                        return true;
                    }

                    // If corners are on different sides of the path, we have an overlap
                    if (Mathf.Sign(bottomLeftDiff) != Mathf.Sign(topRightDiff))
                    {
                        return true;
                    }
                }
            }

            if ((pathsDataPoint & Road_SW) != 0)
            {
                // Location can only overlap if anywhere in the bottom-left quadrant
                if (locationBottomLeft.x <= HALF_TERRAIN_SIZE && locationBottomLeft.y <= HALF_TERRAIN_SIZE)
                {
                    float topLeftDiff = locationTopLeft.x - locationTopLeft.y;
                    float bottomRightDiff = locationBottomRight.x - locationBottomRight.y;

                    // Corner overlaps the path
                    if (Mathf.Abs(topLeftDiff) <= HALF_ROAD_WIDTH || Mathf.Abs(bottomRightDiff) <= HALF_ROAD_WIDTH)
                    {
                        return true;
                    }

                    // If corners are on different sides of the path, we have an overlap
                    if (Mathf.Sign(topLeftDiff) != Mathf.Sign(bottomRightDiff))
                    {
                        return true;
                    }
                }
            }

            if ((pathsDataPoint & Road_NW) != 0)
            {
                // Location can only overlap if anywhere in the bottom-right quadrant
                if (locationTopLeft.x <= HALF_TERRAIN_SIZE && locationTopLeft.y >= HALF_TERRAIN_SIZE)
                {
                    float bottomLeftDiff = locationBottomLeft.x + locationBottomLeft.y - TERRAIN_SIZE;
                    float topRightDiff = locationTopRight.x + locationTopRight.y - TERRAIN_SIZE;

                    // Corner overlaps the path
                    if (Mathf.Abs(bottomLeftDiff) <= HALF_ROAD_WIDTH || Mathf.Abs(topRightDiff) <= HALF_ROAD_WIDTH)
                    {
                        return true;
                    }

                    // If corners are on different sides of the path, we have an overlap
                    if (Mathf.Sign(bottomLeftDiff) != Mathf.Sign(topRightDiff))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}