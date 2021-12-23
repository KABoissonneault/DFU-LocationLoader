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
                
        public struct ObjectSet
        {
            public string Name;
            public string[] Ids;
        }

        public static ObjectSet[] modelsStructure = new ObjectSet[]
        {
            new ObjectSet { Name = "Buildings - Barracks", Ids = new string[] { "516" } },
            new ObjectSet { Name = "Buildings - Fighters Guild", Ids = new string[] { "300" } },
            new ObjectSet { Name = "Buildings - Knightly Orders", Ids = new string[] { "343", "345", "346", "347", "349", "462" } },
            new ObjectSet { Name = "Buildings - Mages Guild", Ids = new string[] { "223", "224", "225", "226", "227", "228", "229", "230", "317", "361", "423" } },
            new ObjectSet { Name = "Buildings - Medium", Ids = new string[] { "109", "111", "115", "116", "117", "122", "127", "128", "129", "130", "131", "132", "133", "134", "135", "136", "137", "138", "139", "221", "231", "301", "302", "303", "304", "307", "308", "309", "311", "312", "313", "315", "320", "321", "325", "326", "337", "338", "339", "434", "440", "460", "461", "762", "763", "800", "801", "804", "814", "212", "213", "214", "216" } },
            new ObjectSet { Name = "Buildings - Medium - Fenced", Ids = new string[] { "323" } },
            new ObjectSet { Name = "Buildings - Medium - Flat", Ids = new string[] { "526", "528", "537", "538", "539", "540", "541", "542", "543", "544", "545", "546", "601", "602", "606" } },
            new ObjectSet { Name = "Buildings - Medium - Flat - Fenced", Ids = new string[] { "605" } },
            new ObjectSet { Name = "Buildings - Medium - Flat - L Shape", Ids = new string[] { "560", "561", "562", "563" } },
            new ObjectSet { Name = "Buildings - Medium - Flat - Round", Ids = new string[] { "603", "604", "613" } },
            new ObjectSet { Name = "Buildings - Medium - Flat - Semi-Detached", Ids = new string[] { "707", "708", "709" } },
            new ObjectSet { Name = "Buildings - Medium - L Shape", Ids = new string[] { "126", "202", "203", "204", "205", "206", "207", "208", "209", "211", "439", "755" } },
            new ObjectSet { Name = "Buildings - Medium - Noble", Ids = new string[] { "759", "760", "761", "802", "803", "805", "806", "807", "810", "811", "813" } },
            new ObjectSet { Name = "Buildings - Medium - Round", Ids = new string[] { "502", "503", "504", "505" } },
            new ObjectSet { Name = "Buildings - Medium - Tower", Ids = new string[] { "501" } },
            new ObjectSet { Name = "Buildings - Large", Ids = new string[] { "112", "123", "140", "141", "142", "143", "144", "145", "146", "147", "148", "149", "150", "151", "152", "153", "215", "222", "233", "234", "235", "237", "239", "240", "241", "242", "243", "245", "246", "262", "316", "319", "340", "413", "414", "415", "416", "417", "418", "419", "420", "421", "422", "424", "437", "463", "757", "808", "812", "816", "817", "835" } },
            new ObjectSet { Name = "Buildings - Large - Flat", Ids = new string[] { "121", "238", "244", "247", "547", "548", "549", "550", "551", "552", "553", "616", "618", "621", "623", "624", "640", "641", "642", "643", "660", "661", "700", "862", "863" } },
            new ObjectSet { Name = "Buildings - Large - Flat - Round", Ids = new string[] { "617", "620" } },
            new ObjectSet { Name = "Buildings - Large - Flat - Terraced", Ids = new string[] { "658", "659", "701", "702", "703", "704", "705", "706", "710" } },
            new ObjectSet { Name = "Buildings - Large - L Shape", Ids = new string[] { "0", "108" } },
            new ObjectSet { Name = "Buildings - Large - Noble", Ids = new string[] { "758", "809", "815", "819", "820", "821", "825", "826", "827", "833", "834", "842", "849", "850", "851" } },
            new ObjectSet { Name = "Buildings - Large - Semi-Detached", Ids = new string[] { "843", "845", "848" } },
            new ObjectSet { Name = "Buildings - Large - Semi-Detached - Noble", Ids = new string[] { "822", "823", "824" } },
            new ObjectSet { Name = "Buildings - Large - Terraced - Noble", Ids = new string[] { "828", "829", "830", "831", "832", "844", "846", "847" } },
            new ObjectSet { Name = "Buildings - Palaces", Ids = new string[] { "407", "408", "409", "451", "458", "459", "507", "508", "509", "510", "511", "730", "731", "732", "733", "734" } },
            new ObjectSet { Name = "Buildings - Small", Ids = new string[] { "107", "118", "119", "120", "125", "236", "318", "322", "327", "328", "330", "331", "332", "333", "334", "335", "336", "410", "411", "412" } },
            new ObjectSet { Name = "Buildings - Small - Fenced", Ids = new string[] { "329" } },
            new ObjectSet { Name = "Buildings - Small - Flat", Ids = new string[] { "527", "529", "530", "531", "532", "533", "534", "535", "536", "609", "610", "611", "614", "615", "622", "627", "637" } },
            new ObjectSet { Name = "Buildings - Small - Flat - Fenced", Ids = new string[] { "607", "608" } },
            new ObjectSet { Name = "Buildings - Small - Flat - L Shape", Ids = new string[] { "554", "555", "556", "557", "558", "559", "600" } },
            new ObjectSet { Name = "Buildings - Small - Flat - Round", Ids = new string[] { "655", "656", "657" } },
            new ObjectSet { Name = "Buildings - Small - L SHape", Ids = new string[] { "154", "155", "156", "157", "158", "159", "160", "161", "162", "163", "200", "201", "210", "756" } },
            new ObjectSet { Name = "Buildings - Small - Tower", Ids = new string[] { "263" } },
            new ObjectSet { Name = "Buildings - Taverns - Large", Ids = new string[] { "220", "248", "250", "253", "428", "836", "838", "839", "840", "841" } },
            new ObjectSet { Name = "Buildings - Taverns - Large - Flat", Ids = new string[] { "113", "626", "662", "663" } },
            new ObjectSet { Name = "Buildings - Taverns - Medium - Flat", Ids = new string[] { "625" } },
            new ObjectSet { Name = "Buildings - Taverns - Medium", Ids = new string[] { "249", "251", "252", "429", "430", "431", "432", "438", "837" } },
            new ObjectSet { Name = "Buildings - Taverns - Small", Ids = new string[] { "110" } },
            new ObjectSet { Name = "Buildings - Templar Orders", Ids = new string[] { "103", "259", "305", "435", "260" } },
            new ObjectSet { Name = "Buildings - Temples", Ids = new string[] { "100", "101", "102", "257", "258", "261", "362", "363", "400", "402", "406", "433", "436" } },
            new ObjectSet { Name = "Buildings - Two Houses", Ids = new string[] { "324" } },
            new ObjectSet { Name = "Brown Stone Fence Corner", Ids = new string[] { "517", "21105" } },
            new ObjectSet { Name = "Brown Stone Fence Straight", Ids = new string[] { "518", "21104" } },
            new ObjectSet { Name = "Brown Stone Fence Gateway", Ids = new string[] { "21106" } },
            new ObjectSet { Name = "Brown Stone Fence End Cap", Ids = new string[] { "21107" } },
            new ObjectSet { Name = "City Walls - Corner Tower", Ids = new string[] { "444" } },
            new ObjectSet { Name = "City Walls - Gateway Closed", Ids = new string[] { "447" } },
            new ObjectSet { Name = "City Walls - Gateway Opened", Ids = new string[] { "446" } },
            new ObjectSet { Name = "City Walls - Straight", Ids = new string[] { "445", "20026", "20028" } },
            new ObjectSet { Name = "Column", Ids = new string[] { "41900" } },
            new ObjectSet { Name = "Dungeon - Cairn Entrance", Ids = new string[] { "42000" } },
            new ObjectSet { Name = "Dungeon - Castle 00", Ids = new string[] { "512", "513" } },
            new ObjectSet { Name = "Dungeon - Castle 01", Ids = new string[] { "644", "645", "646" } },
            new ObjectSet { Name = "Dungeon - Castle 02", Ids = new string[] { "711" } },
            new ObjectSet { Name = "Dungeon - Castle 03", Ids = new string[] { "647", "648" } },
            new ObjectSet { Name = "Dungeon - Castle 04", Ids = new string[] { "649", "650", "651", "652", "653", "654" } },
            new ObjectSet { Name = "Dungeon - Castle 05", Ids = new string[] { "712", "713", "714", "715", "716" } },
            new ObjectSet { Name = "Dungeon - Castle 06", Ids = new string[] { "717", "718" } },
            new ObjectSet { Name = "Dungeon - Castle 07", Ids = new string[] { "719", "720", "721", "722" } },
            new ObjectSet { Name = "Dungeon - Castle 08", Ids = new string[] { "726", "727", "728", "729" } },
            new ObjectSet { Name = "Dungeon - Castle 09", Ids = new string[] { "723", "724", "725" } },
            new ObjectSet { Name = "Dungeon - Castle 10", Ids = new string[] { "852" } },
            new ObjectSet { Name = "Dungeon - Castle 11", Ids = new string[] { "853" } },
            new ObjectSet { Name = "Dungeon - Castle 12", Ids = new string[] { "854" } },
            new ObjectSet { Name = "Dungeon - Castle 13", Ids = new string[] { "855" } },
            new ObjectSet { Name = "Dungeon - Castle 14", Ids = new string[] { "856" } },
            new ObjectSet { Name = "Dungeon - Castle 15", Ids = new string[] { "857" } },
            new ObjectSet { Name = "Dungeon - Castle 16", Ids = new string[] { "858" } },
            new ObjectSet { Name = "Dungeon - Castle 17", Ids = new string[] { "859" } },
            new ObjectSet { Name = "Dungeon - Castle 18", Ids = new string[] { "860" } },
            new ObjectSet { Name = "Dungeon - Castle 19", Ids = new string[] { "861" } },
            new ObjectSet { Name = "Dungeon - Ground Entrance", Ids = new string[] { "43600" } },
            new ObjectSet { Name = "Dungeon - Mound Entrance", Ids = new string[] { "43601", "43602", "43603" } },
            new ObjectSet { Name = "Dungeon - Ruin Entrance", Ids = new string[] { "43604" } },
            new ObjectSet { Name = "Dungeon - Stone Entrance", Ids = new string[] { "40012" } },
            new ObjectSet { Name = "Dungeon - Tree Entrance", Ids = new string[] { "42001" } },
            new ObjectSet { Name = "Fortifications - Corner", Ids = new string[] { "448", "39002" } },
            new ObjectSet { Name = "Fortifications - Gateway", Ids = new string[] { "450", "39001" } },
            new ObjectSet { Name = "Fortifications - S Shape", Ids = new string[] { "39003" } },
            new ObjectSet { Name = "Fortifications - Straight", Ids = new string[] { "449", "39000", "39004", "39005" } },
            new ObjectSet { Name = "Fountains", Ids = new string[] { "41220", "41221", "41222" } },
            new ObjectSet { Name = "Guard Tower", Ids = new string[] { "522" } },
            new ObjectSet { Name = "Guard Tower - Top", Ids = new string[] { "20027", "20033", "20127", "20227", "20327", "20427", "20527", "20627", "20727", "20827" } },
            new ObjectSet { Name = "Hedge Maze", Ids = new string[] { "735", "736", "737", "738", "740", "741", "742", "743" } },
            new ObjectSet { Name = "Hedgerow - Corner - Entrance", Ids = new string[] { "745" } },
            new ObjectSet { Name = "Hedgerow - Corner", Ids = new string[] { "744", "750", "40015", "41235" } },
            new ObjectSet { Name = "Hedgerow - 3 Way", Ids = new string[] { "751", "40016" } },
            new ObjectSet { Name = "Hedgerow - 4 Way", Ids = new string[] { "40017" } },
            new ObjectSet { Name = "Hedgerow - End", Ids = new string[] { "739" } },
            new ObjectSet { Name = "Hedgerow - End Cap", Ids = new string[] { "753", "905", "40013" } },
            new ObjectSet { Name = "Hedgerow - Entrance", Ids = new string[] { "746", "749", "752" } },
            new ObjectSet { Name = "Hedgerow - Single", Ids = new string[] { "41236", "41237" } },
            new ObjectSet { Name = "Hedgerow - Straight", Ids = new string[] { "747", "748", "754", "40014" } },
            new ObjectSet { Name = "Large Shrines", Ids = new string[] { "74094", "74088" } },
            new ObjectSet { Name = "Mounds", Ids = new string[] { "41215", "41218", "41219" } },
            new ObjectSet { Name = "Mounds - Carved", Ids = new string[] { "41216", "41217" } },
            new ObjectSet { Name = "Pillars - Wood", Ids = new string[] { "41901", "41902" } },
            new ObjectSet { Name = "Pillars - Marble", Ids = new string[] { "62315", "62320", "62316" } },
            new ObjectSet { Name = "Pyramid", Ids = new string[] { "74121" } },
            new ObjectSet { Name = "Ruins - Columns", Ids = new string[] { "41722", "41723", "41730", "41731", "41732", "41733" } },
            new ObjectSet { Name = "Ruins - Buildings", Ids = new string[] { "41720", "41721", "41724", "41725", "41726", "41727", "41728", "41729", "43400", "43401", "43402", "43403", "43404", "43405", "43406", "43407", "43408", "43409", "43410", "43411", "43412", "43413", "43414", "43415", "43416", "43417", "43500", "43501", "43502", "43503", "43504", "43505", "43506", "43507", "43508", "43509", "43510", "43511", "43512", "43513", "43514", "43515", "43516", "43517" } },
            new ObjectSet { Name = "Ships / Boats", Ids = new string[] { "41502", "41504", "41501", "909", "910" } },
            new ObjectSet { Name = "Shipwreck", Ids = new string[] { "41509" } },
            new ObjectSet { Name = "Signs - Akatosh Temple", Ids = new string[] { "43733", "43752", "43714" } },
            new ObjectSet { Name = "Signs - Alchemist Shop", Ids = new string[] { "43720", "43739", "43702" } },
            new ObjectSet { Name = "Signs - Arkay Temple", Ids = new string[] { "43718", "43737", "43700" } },
            new ObjectSet { Name = "Signs - Armor Shop", Ids = new string[] { "43725", "43744" } },
            new ObjectSet { Name = "Signs - Bank", Ids = new string[] { "43730", "43749", "43711" } },
            new ObjectSet { Name = "Signs - Clothes Shop", Ids = new string[] { "43721", "43740", "43703" } },
            new ObjectSet { Name = "Signs - Dibella Temple", Ids = new string[] { "43724", "43743", "43706" } },
            new ObjectSet { Name = "Signs - General Store", Ids = new string[] { "43731", "43750", "43712" } },
            new ObjectSet { Name = "Signs - Jewelry Shop", Ids = new string[] { "43726", "43745", "43707" } },
            new ObjectSet { Name = "Signs - Julianos Temple", Ids = new string[] { "43719", "43738", "43701" } },
            new ObjectSet { Name = "Signs - Kynareth Temple", Ids = new string[] { "43734", "43753", "43715" } },
            new ObjectSet { Name = "Signs - Library", Ids = new string[] { "43735", "43754", "43716" } },
            new ObjectSet { Name = "Signs - Mages Guild", Ids = new string[] { "43722", "43741", "43704" } },
            new ObjectSet { Name = "Signs - Mara Temple", Ids = new string[] { "43728", "43747", "43709" } },
            new ObjectSet { Name = "Signs - Pawn Shop", Ids = new string[] { "43732", "43751", "43713" } },
            new ObjectSet { Name = "Signs - Stendarr Temple", Ids = new string[] { "43729", "43748", "43710" } },
            new ObjectSet { Name = "Signs - Tavern", Ids = new string[] { "43727", "43746", "43708" } },
            new ObjectSet { Name = "Signs - Weapon Shop", Ids = new string[] { "43736", "43755", "43717" } },
            new ObjectSet { Name = "Signs - Zenithar Temple", Ids = new string[] { "43723", "43742", "43705" } },
            new ObjectSet { Name = "Stone Bridges", Ids = new string[] { "41211", "41212", "41213" } },
            new ObjectSet { Name = "Towers - Hexagonal", Ids = new string[] { "41223", "41224", "41225", "41226", "41227", "41228", "41229" } },
            new ObjectSet { Name = "Towers - Pentagonal", Ids = new string[] { "41230", "41231", "41232", "41233", "41234" } },
            new ObjectSet { Name = "Unique - Castle Daggerfall", Ids = new string[] { "521", "523", "524", "525" } },
            new ObjectSet { Name = "Unique - Castle Llugwych", Ids = new string[] { "457" } },
            new ObjectSet { Name = "Unique - Castle Wayrest", Ids = new string[] { "900", "901", "902", "903", "904" } },
            new ObjectSet { Name = "Unique - Direnni Tower", Ids = new string[] { "500" } },
            new ObjectSet { Name = "Unique - Lysandus' Tomb", Ids = new string[] { "454" } },
            new ObjectSet { Name = "Unique - Orsinium Palace", Ids = new string[] { "455" } },
            new ObjectSet { Name = "Unique - Scourg Barrow", Ids = new string[] { "456" } },
            new ObjectSet { Name = "Unique - Sentinel Palace", Ids = new string[] { "633", "634", "635" } },
            new ObjectSet { Name = "Unique - Sentinel Palace - Walls?", Ids = new string[] { "628", "629", "630", "631", "632", "636" } },
            new ObjectSet { Name = "Unique - Shedungent", Ids = new string[] { "452" } },
            new ObjectSet { Name = "Unique - Woodborne Hall", Ids = new string[] { "453" } },
            new ObjectSet { Name = "Windmill", Ids = new string[] { "41600" } },
            new ObjectSet { Name = "Wooden Fence - Broken", Ids = new string[] { "21103" } },
            new ObjectSet { Name = "Wooden Fence 01 - End Cap", Ids = new string[] { "41200", "41202" } },
            new ObjectSet { Name = "Wooden Fence 01 - Mid", Ids = new string[] { "41201" } },
            new ObjectSet { Name = "Wooden Fence 02 - End Cap", Ids = new string[] { "41203", "41205" } },
            new ObjectSet { Name = "Wooden Fence 02 - Mid", Ids = new string[] { "41204" } },
            new ObjectSet { Name = "Wooden Fence 03 - End Cap", Ids = new string[] { "41206", "41208" } },
            new ObjectSet { Name = "Wooden Fence 03 - Mid", Ids = new string[] { "41207" } },
        };

        public static ObjectSet[] modelsClutter = new ObjectSet[]
        {
            new ObjectSet { Name = "Anvil", Ids = new string[] { "41118" } },
            new ObjectSet { Name = "Armor", Ids = new string[] { "74226" } },
            new ObjectSet { Name = "Arrow", Ids = new string[] { "99800" } },
            new ObjectSet { Name = "Axe", Ids = new string[] { "74225" } },
            new ObjectSet { Name = "Banners (Daggerfall)", Ids = new string[] { "42536", "42548", "42560", "42500", "42512", "42524" } },
            new ObjectSet { Name = "Banners (Direnni)", Ids = new string[] { "42540", "42552", "42564", "42504", "42516", "42528" } },
            new ObjectSet { Name = "Banners (Dwynnen)", Ids = new string[] { "42541", "42553", "42565", "42505", "42517", "42529" } },
            new ObjectSet { Name = "Banners (Order of the Flame)", Ids = new string[] { "42543", "42507", "42519", "42531", "42555", "42567" } },
            new ObjectSet { Name = "Banners (Order of the Lamp)", Ids = new string[] { "42542", "42554", "42566", "42506", "42518", "42530" } },
            new ObjectSet { Name = "Banners (Sentinel)", Ids = new string[] { "42537", "42549", "42561", "42501", "42513", "42525" } },
            new ObjectSet { Name = "Banners (Wayrest)", Ids = new string[] { "42538", "42550", "42562", "42502", "42514", "42526" } },
            new ObjectSet { Name = "Banners (Large)", Ids = new string[] { "42558", "42570", "42546", "42557", "42569", "42545", "42544", "42547", "42556", "42568", "42559", "42571", "42539", "42551", "42563" } },
            new ObjectSet { Name = "Banners (Small)", Ids = new string[] { "42510", "42522", "42534", "42533", "42509", "42521", "42508", "42520", "42532", "42511", "42523", "42503", "42515", "42527", "42535" } },
            new ObjectSet { Name = "Block Noose Support", Ids = new string[] { "41703" } },
            new ObjectSet { Name = "Bulletin Board", Ids = new string[] { "41739" } },
            new ObjectSet { Name = "Carpets", Ids = new string[] { "74800", "74801", "74802", "74803", "74804", "74805", "74806", "74807", "74808" } },
            new ObjectSet { Name = "Cart - Empty", Ids = new string[] { "41214" } },
            new ObjectSet { Name = "Cart - Full", Ids = new string[] { "41241" } },
            new ObjectSet { Name = "Catapult", Ids = new string[] { "41407" } },
            new ObjectSet { Name = "Corner Shelf", Ids = new string[] { "41128" } },
            new ObjectSet { Name = "Crossbow", Ids = new string[] { "74228" } },
            new ObjectSet { Name = "Fireplace", Ids = new string[] { "41116" } },
            new ObjectSet { Name = "Fireplace - Corner", Ids = new string[] { "41117" } },
            new ObjectSet { Name = "Ladder", Ids = new string[] { "41409" } },
            new ObjectSet { Name = "Large Raised Wooden Platform", Ids = new string[] { "41405" } },
            new ObjectSet { Name = "Mantlet", Ids = new string[] { "41402" } },
            new ObjectSet { Name = "Mantlet - Large", Ids = new string[] { "41401" } },
            new ObjectSet { Name = "Organ", Ids = new string[] { "41120" } },
            new ObjectSet { Name = "Paintings", Ids = new string[] { "51115", "51116", "51117", "51118", "51119", "51120" } },
            new ObjectSet { Name = "Rocks - Small", Ids = new string[] { "41704", "41710", "41712", "60711", "60712", "60713", "60714", "60715", "60716", "60717", "60718", "60719", "60720" } },
            new ObjectSet { Name = "Rocks - Medium", Ids = new string[] { "41705", "41706", "41707", "41708", "41709", "41711", "41713" } },
            new ObjectSet { Name = "Rocks - Large", Ids = new string[] { "41714", "41715", "41716", "41717", "41718", "41719", "60710" } },
            new ObjectSet { Name = "Sawhorse", Ids = new string[] { "41125" } },
            new ObjectSet { Name = "Scaffolding 1", Ids = new string[] { "41403" } },
            new ObjectSet { Name = "Scaffolding 2", Ids = new string[] { "41404" } },
            new ObjectSet { Name = "Spinning Wheel", Ids = new string[] { "41009" } },
            new ObjectSet { Name = "Stocks 1", Ids = new string[] { "41700" } },
            new ObjectSet { Name = "Stocks 2", Ids = new string[] { "41701" } },
            new ObjectSet { Name = "Support Beam", Ids = new string[] { "62319" } },
            new ObjectSet { Name = "Support Beam - Arched", Ids = new string[] { "62321" } },
            new ObjectSet { Name = "Support Beam - Diagonal", Ids = new string[] { "62318" } },
            new ObjectSet { Name = "Swords", Ids = new string[] { "74224", "74227" } },
            new ObjectSet { Name = "Sword - Large", Ids = new string[] { "74095" } },
            new ObjectSet { Name = "Tents", Ids = new string[] { "41606", "41607", "41608", "41609", "41610" } },
            new ObjectSet { Name = "Trebuchet", Ids = new string[] { "41406" } },
            new ObjectSet { Name = "Trellis", Ids = new string[] { "41240" } },
            new ObjectSet { Name = "Water Trough", Ids = new string[] { "41209" } },
            new ObjectSet { Name = "Water Trough - Empty", Ids = new string[] { "41210" } },
            new ObjectSet { Name = "Wood Stake", Ids = new string[] { "41400" } },
            new ObjectSet { Name = "Wooden Plank", Ids = new string[] { "41242" } },
            new ObjectSet { Name = "Wooden Tree Log", Ids = new string[] { "41734", "41735", "41736", "41737", "41738" } },
        };

        public static ObjectSet[] modelsDungeon = new ObjectSet[]
        {
            new ObjectSet { Name = "Altars", Ids = new string[] { "51110", "51111", "41304", "41307", "41309", "41310", "41311", "41305", "51112", "51113", "51114" } },
            new ObjectSet { Name = "Cage - Medium", Ids = new string[] { "41313" } },
            new ObjectSet { Name = "Cage - Small", Ids = new string[] { "41312" } },
            new ObjectSet { Name = "Coffins - Stone", Ids = new string[] { "41319", "41320", "41321", "41324", "41327" } },
            new ObjectSet { Name = "Coffins - Wood", Ids = new string[] { "41315", "41317", "41316", "41318", "41322", "41323", "41325", "41326" } },
            new ObjectSet { Name = "Statues", Ids = new string[] { "62323", "62325", "62327", "62329" } },
            new ObjectSet { Name = "Statues - Large", Ids = new string[] { "62324", "62326", "62328", "62330" } },
            new ObjectSet { Name = "Pedestal - Stone", Ids = new string[] { "74091" } },
            new ObjectSet { Name = "Pedestal - Wood", Ids = new string[] { "74082" } },
            new ObjectSet { Name = "Torture Recliner - Knives", Ids = new string[] { "41300" } },
            new ObjectSet { Name = "Torture Table - Knives", Ids = new string[] { "41301" } },
            new ObjectSet { Name = "Torture Table - Rack", Ids = new string[] { "41303" } },
            new ObjectSet { Name = "Torture Table - Spikes", Ids = new string[] { "41302" } },
        };

        public static ObjectSet[] modelsFurniture = new ObjectSet[]
        {
            new ObjectSet { Name = "Beds", Ids = new string[] { "41000", "41001", "41002" } },
            new ObjectSet { Name = "Benches", Ids = new string[] { "41105", "41106", "41107", "41126", "51108", "51109", "43307" } },
            new ObjectSet { Name = "Cabinet", Ids = new string[] { "41007" } },
            new ObjectSet { Name = "Cabinet - Double", Ids = new string[] { "41051" } },
            new ObjectSet { Name = "Chairs", Ids = new string[] { "41100", "41101", "41102", "41103" } },
            new ObjectSet { Name = "Chests", Ids = new string[] { "41811", "41812", "41813" } },
            new ObjectSet { Name = "Crates", Ids = new string[] { "41815", "41816", "41817", "41818", "41819", "41820", "41821", "41822", "41823", "41824", "41825", "41826", "41827", "41828", "41829", "41830", "41831", "41832", "41833", "41834" } },
            new ObjectSet { Name = "Cupboard", Ids = new string[] { "41003" } },
            new ObjectSet { Name = "Cupboard - Double", Ids = new string[] { "41004" } },
            new ObjectSet { Name = "Drawers", Ids = new string[] { "41034", "41050", "41037", "41036", "41035" } },
            new ObjectSet { Name = "Dresser", Ids = new string[] { "41032" } },
            new ObjectSet { Name = "Lectern", Ids = new string[] { "41024", "41020", "41021", "41022" } },
            new ObjectSet { Name = "Stool", Ids = new string[] { "41113", "41114" } },
            new ObjectSet { Name = "Shelves - Alchemy", Ids = new string[] { "41042", "41043", "41041", "41044" } },
            new ObjectSet { Name = "Shelves - Books", Ids = new string[] { "41006", "41019", "41026", "41015", "41018", "41025", "41014" } },
            new ObjectSet { Name = "Shelves - Clothes", Ids = new string[] { "41013", "41011", "41012", "41010" } },
            new ObjectSet { Name = "Shelves - Drinks", Ids = new string[] { "41124" } },
            new ObjectSet { Name = "Shelves - Empty", Ids = new string[] { "41030" } },
            new ObjectSet { Name = "Shelves - Food", Ids = new string[] { "41040", "41029", "41039", "41027", "41046" } },
            new ObjectSet { Name = "Shelves - Utility", Ids = new string[] { "41005", "41045" } },
            new ObjectSet { Name = "Shelves - Weapons", Ids = new string[] { "41031", "41028", "41048", "41049", "41047" } },
            new ObjectSet { Name = "Tables", Ids = new string[] { "41108", "41109", "41110", "41111", "41112", "41130", "51103", "51104" } },
            new ObjectSet { Name = "Thrones", Ids = new string[] { "41122", "41123", "41104" } },
        };

        public static ObjectSet[] modelsGraveyard = new ObjectSet[]
        {
            new ObjectSet { Name = "Graveyard Gate Door Right", Ids = new string[] { "43000" } },
            new ObjectSet { Name = "Graveyard Gate Door Mid", Ids = new string[] { "43001" } },
            new ObjectSet { Name = "Graveyard Gate Door Left", Ids = new string[] { "43002" } },
            new ObjectSet { Name = "Graveyard Gates", Ids = new string[] { "43003", "43004", "43005", "43006", "43007", "43008", "43009", "43010", "62310", "62312", "62317" } },
            new ObjectSet { Name = "Graveyard Monuments", Ids = new string[] { "43079", "43080", "43081", "43082", "43109", "43110", "43111", "43112", "43202", "43204", "62314" } },
            new ObjectSet { Name = "Mausoleum Dark", Ids = new string[] { "43138" } },
            new ObjectSet { Name = "Mausoleum Gray", Ids = new string[] { "43140" } },
            new ObjectSet { Name = "Mausoleum Red 1", Ids = new string[] { "43139" } },
            new ObjectSet { Name = "Mausoleum Red 2", Ids = new string[] { "41619" } },
            new ObjectSet { Name = "Mausoleum White 1", Ids = new string[] { "43141" } },
            new ObjectSet { Name = "Mausoleum White 2", Ids = new string[] { "43137" } },
            new ObjectSet { Name = "Pillar Tombs (Dark)", Ids = new string[] { "43071", "43105", "43125" } },
            new ObjectSet { Name = "Pillar Tombs (Gray)", Ids = new string[] { "43073", "43107", "43127" } },
            new ObjectSet { Name = "Pillar Tombs (Red)", Ids = new string[] { "43072", "43106", "43126" } },
            new ObjectSet { Name = "Pillar Tombs (White)", Ids = new string[] { "43074", "43108", "43128", "43303" } },
            new ObjectSet { Name = "Slabs - Stone (Dark)", Ids = new string[] { "43027", "43031", "43035" } },
            new ObjectSet { Name = "Slabs - Stone (Gray)", Ids = new string[] { "43029", "43033", "43037" } },
            new ObjectSet { Name = "Slabs - Stone (Red)", Ids = new string[] { "43028", "43032", "43036" } },
            new ObjectSet { Name = "Slabs - Stone (White)", Ids = new string[] { "43030", "43034", "43038" } },
            new ObjectSet { Name = "Stone Ankhs", Ids = new string[] { "43083", "43084", "43085", "43086", "43121", "43122", "43123", "43124" } },
            new ObjectSet { Name = "Stone Caskets", Ids = new string[] { "43075", "43076", "43077", "43078", "43304", "43305", "43306" } },
            new ObjectSet { Name = "Tombstone Wall Dark", Ids = new string[] { "43113" } },
            new ObjectSet { Name = "Tombstone Wall Red", Ids = new string[] { "43114" } },
            new ObjectSet { Name = "Tombstone Wall Gray", Ids = new string[] { "43115" } },
            new ObjectSet { Name = "Tombstone Wall White", Ids = new string[] { "43116" } },
            new ObjectSet { Name = "Tombstones - Broken", Ids = new string[] { "43129", "43130", "43131", "43132", "43206" } },
            new ObjectSet { Name = "Tombstones - Large (Dark)", Ids = new string[] { "43133", "43142", "43200", "43201", "43203", "43205" } },
            new ObjectSet { Name = "Tombstones - Large (Gray)", Ids = new string[] { "43135", "43144" } },
            new ObjectSet { Name = "Tombstones - Large (Red)", Ids = new string[] { "43134", "43143" } },
            new ObjectSet { Name = "Tombstones - Large (White)", Ids = new string[] { "43136", "43145" } },
            new ObjectSet { Name = "Tombstones - Medium (Dark)", Ids = new string[] { "43055", "43059", "43063", "43067", "43101", "43117" } },
            new ObjectSet { Name = "Tombstones - Medium (Gray)", Ids = new string[] { "43057", "43061", "43065", "43069", "43103", "43119" } },
            new ObjectSet { Name = "Tombstones - Medium (Red)", Ids = new string[] { "43056", "43060", "43064", "43068", "43102", "43118" } },
            new ObjectSet { Name = "Tombstones - Medium (White)", Ids = new string[] { "43058", "43062", "43066", "43070", "43104", "43120" } },
            new ObjectSet { Name = "Tombstones - Small (Dark)", Ids = new string[] { "43011", "43015", "43019", "43023", "43039", "43043", "43047", "43051" } },
            new ObjectSet { Name = "Tombstones - Small (Gray)", Ids = new string[] { "43013", "43017", "43021", "43025", "43041", "43045", "43049", "43053", "43300", "43301" } },
            new ObjectSet { Name = "Tombstones - Small (Red)", Ids = new string[] { "43012", "43016", "43020", "43024", "43040", "43044", "43048", "43052", "43302" } },
            new ObjectSet { Name = "Tombstones - Small (White)", Ids = new string[] { "43014", "43018", "43022", "43026", "43042", "43046", "43050", "43054" } },
        };

        public static ObjectSet[] models = modelsStructure
            .Concat(modelsClutter)
            .Concat(modelsDungeon)
            .Concat(modelsFurniture)
            .Concat(modelsGraveyard)
            .OrderBy(set => set.Name)
            .ToArray();

        public static ObjectSet[] billboardsPeople = new ObjectSet[]
        {
            new ObjectSet { Name = "Beggars", Ids = new string[] { "182.30", "182.21", "182.31", "182.44", "183.15", "183.17", "182.29", "184.27" } },
            new ObjectSet { Name = "Children", Ids = new string[] { "182.4", "184.15", "182.38", "182.42", "182.43", "182.53", "182.52" } },
            new ObjectSet { Name = "Commoners - Men", Ids = new string[] { "182.20", "197.1", "357.1", "184.17", "182.46", "182.17", "182.16", "182.24", "182.23", "184.20", "184.24", "184.25", "182.25", "334.0", "182.39", "182.18", "182.13", "182.14", "182.19" } },
            new ObjectSet { Name = "Commoners - Women", Ids = new string[] { "184.30", "184.32", "182.47", "182.26", "184.28", "184.29", "197.7", "184.33", "334.5", "182.45", "184.18", "184.23", "184.1", "184.22", "184.26" } },
            new ObjectSet { Name = "Cooks", Ids = new string[] { "182.7", "182.8" } },
            new ObjectSet { Name = "Daedric Princes", Ids = new string[] { "175.0", "175.1", "175.2", "175.3", "175.4", "175.5", "175.6", "175.7", "175.8", "175.9", "175.10", "175.11", "175.12", "175.13", "175.14", "175.15" } },
            new ObjectSet { Name = "Dark Brotherhood", Ids = new string[] { "176.6", "176.5", "176.4", "176.3", "176.2", "176.1", "176.0" } },
            new ObjectSet { Name = "Elders", Ids = new string[] { "184.21" } },
            new ObjectSet { Name = "Horse Rider", Ids = new string[] { "184.34" } },
            new ObjectSet { Name = "Innkeepers", Ids = new string[] { "346.1", "184.16", "182.1", "182.2", "182.3" } },
            new ObjectSet { Name = "Jesters", Ids = new string[] { "182.5", "182.6", "182.49" } },
            new ObjectSet { Name = "Knights", Ids = new string[] { "183.2", "183.3", "183.4" } },
            new ObjectSet { Name = "Mages", Ids = new string[] { "177.4", "177.3", "177.2", "177.1", "177.0", "182.41", "334.1", "182.40" } },
            new ObjectSet { Name = "Minstrels", Ids = new string[] { "182.37", "184.3", "182.50", "182.51" } },
            new ObjectSet { Name = "Necromancers", Ids = new string[] { "178.5", "178.6", "178.1", "178.0", "178.4", "178.2", "178.3" } },
            new ObjectSet { Name = "Noblemen", Ids = new string[] { "183.5", "183.16", "180.3", "183.0", "183.10", "183.11", "183.13", "183.20", "183.6", "197.10", "197.4", "197.9", "182.15", "184.0", "184.4", "195.11", "334.17", "334.18", "346.7", "357.3", "180.2", "183.7" } },
            new ObjectSet { Name = "Noblewomen", Ids = new string[] { "180.0", "197.8", "182.27", "184.19", "182.9", "182.10", "184.5", "334.4", "346.0", "180.1", "183.1", "183.14", "183.18", "183.21", "183.8", "183.9" } },
            new ObjectSet { Name = "Orc King", Ids = new string[] { "183.19" } },
            new ObjectSet { Name = "Prisoner", Ids = new string[] { "184.31" } },
            new ObjectSet { Name = "Prostitutes", Ids = new string[] { "184.6", "184.7", "184.8", "184.9", "184.10", "184.11", "184.12", "184.13", "184.14", "182.34", "182.48" } },
            new ObjectSet { Name = "Serving Girl", Ids = new string[] { "182.11", "182.12" } },
            new ObjectSet { Name = "Shopkeeper", Ids = new string[] { "182.0" } },
            new ObjectSet { Name = "Smiths", Ids = new string[] { "177.5", "182.59" } },
            new ObjectSet { Name = "Snake Charmer", Ids = new string[] { "182.36" } },
            new ObjectSet { Name = "Temple", Ids = new string[] { "183.12", "182.33", "182.57", "181.7", "181.6", "181.5", "182.58", "181.4", "182.32", "182.28", "182.22", "181.3", "181.2", "181.1", "181.0" } },
            new ObjectSet { Name = "Vampires", Ids = new string[] { "182.56", "182.54", "182.55" } },
            new ObjectSet { Name = "Witch Covens", Ids = new string[] { "179.3", "179.2", "179.1", "179.0", "179.4" } },
        };

        public static ObjectSet[] billboardsInterior = new ObjectSet[]
        {
            new ObjectSet { Name = "Clothing - Boots", Ids = new string[] { "204.1", "204.2" } },
            new ObjectSet { Name = "Clothing - Pile of Clothes", Ids = new string[] { "204.0" } },
            new ObjectSet { Name = "Clothing - Hats", Ids = new string[] { "204.3", "204.4", "204.5" } },
            new ObjectSet { Name = "Clothing - Rolls of Cloth", Ids = new string[] { "204.6", "204.7", "204.8" } },
            new ObjectSet { Name = "Containers - Barrel", Ids = new string[] { "205.0" } },
            new ObjectSet { Name = "Containers - Baskets", Ids = new string[] { "205.8", "205.9", "205.10" } },
            new ObjectSet { Name = "Containers - Buckets", Ids = new string[] { "205.29", "205.30" } },
            new ObjectSet { Name = "Containers - Chests", Ids = new string[] { "205.21", "205.22", "205.23", "205.24", "205.25", "205.26" } },
            new ObjectSet { Name = "Containers - Grain Sacks", Ids = new string[] { "205.17", "205.18", "205.19", "205.20" } },
            new ObjectSet { Name = "Containers - Pots", Ids = new string[] { "218.0", "218.1", "218.2", "218.3", "211.2", "205.41" } },
            new ObjectSet { Name = "Containers - Pouch", Ids = new string[] { "205.36" } },
            new ObjectSet { Name = "Containers - Sack", Ids = new string[] { "205.44" } },
            new ObjectSet { Name = "Equipment - Lance Rack", Ids = new string[] { "211.13" } },
            new ObjectSet { Name = "Equipment - Armor", Ids = new string[] { "207.9", "207.10", "207.11", "207.12", "207.13", "207.14" } },
            new ObjectSet { Name = "Equipment - Quiver", Ids = new string[] { "205.42" } },
            new ObjectSet { Name = "Equipment - Saddle", Ids = new string[] { "204.9" } },
            new ObjectSet { Name = "Equipment - Spear Rack", Ids = new string[] { "211.14" } },
            new ObjectSet { Name = "Equipment - Sword Rack", Ids = new string[] { "211.12" } },
            new ObjectSet { Name = "Equipment - Weapons", Ids = new string[] { "207.0", "207.1", "207.2", "207.3", "207.4", "207.5", "207.6", "207.7", "207.8", "207.15", "207.16" } },
            new ObjectSet { Name = "Food - Apple", Ids = new string[] { "213.1" } },
            new ObjectSet { Name = "Food - Bread", Ids = new string[] { "211.31" } },
            new ObjectSet { Name = "Food - Fish Fillets", Ids = new string[] { "211.41", "211.42" } },
            new ObjectSet { Name = "Food - Fishes", Ids = new string[] { "211.8", "211.9", "211.10", "211.11" } },
            new ObjectSet { Name = "Food - Meat", Ids = new string[] { "211.40" } },
            new ObjectSet { Name = "Food - Orange", Ids = new string[] { "213.0" } },
            new ObjectSet { Name = "Housing - Bottles", Ids = new string[] { "205.11", "205.12", "205.13", "205.14", "205.15", "205.16" } },
            new ObjectSet { Name = "Housing - Chair", Ids = new string[] { "200.14" } },
            new ObjectSet { Name = "Housing - Cooking Pan", Ids = new string[] { "218.4" } },
            new ObjectSet { Name = "Housing - Cradle", Ids = new string[] { "200.18" } },
            new ObjectSet { Name = "Housing - Cups", Ids = new string[] { "200.0", "200.1", "200.2", "200.3", "200.4", "200.5", "200.6" } },
            new ObjectSet { Name = "Housing - Drapes", Ids = new string[] { "211.43", "211.44", "211.45", "211.46" } },
            new ObjectSet { Name = "Housing - Flowers", Ids = new string[] { "254.26", "254.27", "254.28", "254.29", "432.19" } },
            new ObjectSet { Name = "Housing - Hanging Spoon", Ids = new string[] { "218.6" } },
            new ObjectSet { Name = "Housing - Pillows", Ids = new string[] { "200.11", "200.13" } },
            new ObjectSet { Name = "Housing - Rocking Horse", Ids = new string[] { "211.21" } },
            new ObjectSet { Name = "Laboratory - Boiling Potions", Ids = new string[] { "208.2", "253.41" } },
            new ObjectSet { Name = "Laboratory - Alchemy Bottles", Ids = new string[] { "205.1", "205.2", "205.3", "205.4", "205.5", "205.6", "205.7" } },
            new ObjectSet { Name = "Laboratory - Flasks", Ids = new string[] { "205.31", "205.32", "205.33", "205.34", "205.35", "205.43" } },
            new ObjectSet { Name = "Laboratory - Globe", Ids = new string[] { "208.0" } },
            new ObjectSet { Name = "Laboratory - Hourglass", Ids = new string[] { "208.6" } },
            new ObjectSet { Name = "Laboratory - Magnifying Glasses", Ids = new string[] { "208.1", "208.5" } },
            new ObjectSet { Name = "Laboratory - Scales", Ids = new string[] { "208.3" } },
            new ObjectSet { Name = "Laboratory - Telescope", Ids = new string[] { "208.4" } },
            new ObjectSet { Name = "Library - Books", Ids = new string[] { "209.0", "209.1", "209.2", "209.3", "209.4" } },
            new ObjectSet { Name = "Library - Parchments", Ids = new string[] { "209.5", "209.6", "209.7", "209.8", "209.9", "209.10" } },
            new ObjectSet { Name = "Library - Quill", Ids = new string[] { "211.1" } },
            new ObjectSet { Name = "Library - Tablets", Ids = new string[] { "209.11", "209.12", "209.13", "209.14", "209.15" } },
            new ObjectSet { Name = "Misc. - Bandages", Ids = new string[] { "211.0" } },
            new ObjectSet { Name = "Misc. - Bell", Ids = new string[] { "211.47" } },
            new ObjectSet { Name = "Misc. - Candle Snuffer", Ids = new string[] { "211.23" } },
            new ObjectSet { Name = "Misc. - Coal Pile", Ids = new string[] { "200.17" } },
            new ObjectSet { Name = "Misc. - Holy Water", Ids = new string[] { "211.49" } },
            new ObjectSet { Name = "Misc. - Icon", Ids = new string[] { "211.51" } },
            new ObjectSet { Name = "Misc. - Meat Hanger", Ids = new string[] { "211.34" } },
            new ObjectSet { Name = "Misc. - Miniature Houses", Ids = new string[] { "211.37", "211.38", "211.39" } },
            new ObjectSet { Name = "Misc. - Painting", Ids = new string[] { "211.57" } },
            new ObjectSet { Name = "Misc. - Smoking Pipes", Ids = new string[] { "211.24", "211.25" } },
            new ObjectSet { Name = "Misc. - Statuettes", Ids = new string[] { "202.5", "202.6" } },
            new ObjectSet { Name = "Misc. - Training Dummy", Ids = new string[] { "211.20" } },
            new ObjectSet { Name = "Misc. - Training Pole", Ids = new string[] { "211.30" } },
            new ObjectSet { Name = "Plants - Hanged", Ids = new string[] { "213.13", "213.14" } },
            new ObjectSet { Name = "Plants - Potted", Ids = new string[] { "213.2", "213.3", "213.4", "213.5", "213.6" } },
            new ObjectSet { Name = "Statues - Dibella", Ids = new string[] { "97.12" } },
            new ObjectSet { Name = "Statues - Julianos", Ids = new string[] { "97.6", "97.7", "97.8" } },
            new ObjectSet { Name = "Statues - Kynareth", Ids = new string[] { "97.13", "97.14", "97.15", "97.16", "97.17" } },
            new ObjectSet { Name = "Statues - Man", Ids = new string[] { "97.0", "97.9" } },
            new ObjectSet { Name = "Statues - Stendarr", Ids = new string[] { "97.3" } },
            new ObjectSet { Name = "Statues - Women", Ids = new string[] { "97.2", "97.4", "97.5", "97.10", "97.11" } },
            new ObjectSet { Name = "Statues - Zenithar", Ids = new string[] { "97.1", "97.18", "97.19", "97.20", "97.21" } },
            new ObjectSet { Name = "Statues - Monsters", Ids = new string[] { "98.0", "98.1", "98.2", "98.3", "98.4", "98.5", "98.6", "98.7", "98.8", "98.9", "98.10", "98.11", "98.12", "98.13", "98.14", "202.0", "202.1", "202.2", "202.3", "202.4" } },
            new ObjectSet { Name = "Tools - Anvil", Ids = new string[] { "211.35" } },
            new ObjectSet { Name = "Tools - Bellows", Ids = new string[] { "214.9" } },
            new ObjectSet { Name = "Tools - Broom", Ids = new string[] { "214.10" } },
            new ObjectSet { Name = "Tools - Brush", Ids = new string[] { "214.12" } },
            new ObjectSet { Name = "Tools - Butter Churn", Ids = new string[] { "214.5" } },
            new ObjectSet { Name = "Tools - Fish Net", Ids = new string[] { "212.7" } },
            new ObjectSet { Name = "Tools - Hammers", Ids = new string[] { "214.2", "214.3" } },
            new ObjectSet { Name = "Tools - Hoe", Ids = new string[] { "214.6" } },
            new ObjectSet { Name = "Tools - Iron", Ids = new string[] { "214.15" } },
            new ObjectSet { Name = "Tools - Loom Tables", Ids = new string[] { "200.15", "200.16" } },
            new ObjectSet { Name = "Tools - Meat Hook", Ids = new string[] { "211.36" } },
            new ObjectSet { Name = "Tools - Rope", Ids = new string[] { "214.8" } },
            new ObjectSet { Name = "Tools - Scoops", Ids = new string[] { "214.4", "214.11" } },
            new ObjectSet { Name = "Tools - Scythe", Ids = new string[] { "214.7" } },
            new ObjectSet { Name = "Tools - Shears", Ids = new string[] { "214.14" } },
            new ObjectSet { Name = "Tools - Shovels", Ids = new string[] { "214.0", "214.1" } },
            new ObjectSet { Name = "Tools - Tongs", Ids = new string[] { "214.13" } },
        };

        public static ObjectSet[] billboardsNature = new ObjectSet[]
        {
            new ObjectSet { Name = "Animals - Camel", Ids = new string[] { "201.2" } },
            new ObjectSet { Name = "Animals - Cats", Ids = new string[] { "201.7", "201.8" } },
            new ObjectSet { Name = "Animals - Cows", Ids = new string[] { "201.3", "201.4" } },
            new ObjectSet { Name = "Animals - Dogs", Ids = new string[] { "201.9", "201.10" } },
            new ObjectSet { Name = "Animals - Horses", Ids = new string[] { "201.0", "201.1" } },
            new ObjectSet { Name = "Animals - Pigs", Ids = new string[] { "201.5", "201.6" } },
            new ObjectSet { Name = "Animals - Seagull", Ids = new string[] { "201.11" } },
            new ObjectSet { Name = "Crops", Ids = new string[] { "301.0", "301.1", "301.2", "301.3", "301.4", "301.5", "301.6", "301.7", "301.8", "301.9", "301.10", "301.11", "301.12", "301.13", "301.14", "301.15", "301.16", "301.17", "301.18", "301.19", "301.20", "301.21", "301.22", "301.23" } },
            new ObjectSet { Name = "Dead Wood", Ids = new string[] { "213.11", "213.12" } },
            new ObjectSet { Name = "Fountains", Ids = new string[] { "212.2", "212.3" } },
            new ObjectSet { Name = "Hay Ricks", Ids = new string[] { "212.15", "212.16" } },
            new ObjectSet { Name = "Hay Stack", Ids = new string[] { "212.1" } },
            new ObjectSet { Name = "Manure", Ids = new string[] { "253.21" } },
            new ObjectSet { Name = "Plants - Desert", Ids = new string[] { "503.1", "503.7", "503.8", "503.9", "503.10", "503.14", "503.15", "503.16", "503.17", "503.23", "503.24", "503.25", "503.26", "503.27", "503.29", "503.31" } },
            new ObjectSet { Name = "Plants - Haunted Woodland", Ids = new string[] { "508.2", "508.7", "508.8", "508.9", "508.11", "508.14", "508.21", "508.22", "508.23", "508.26", "508.27", "508.28", "508.29" } },
            new ObjectSet { Name = "Plants - Mountains", Ids = new string[] { "510.2", "510.7", "510.8", "510.9", "510.10", "510.21", "510.22", "510.23", "510.26", "510.29" } },
            new ObjectSet { Name = "Plants - Rain Forest", Ids = new string[] { "500.1", "500.2", "500.5", "500.6", "500.7", "500.8", "500.9", "500.10", "500.11", "500.20", "500.21", "500.22", "500.23", "500.24", "500.26", "500.27", "500.29", "500.31" } },
            new ObjectSet { Name = "Plants - Steppes", Ids = new string[] { "5.7", "5.8", "5.9", "5.10", "5.17", "5.23", "5.24", "5.25", "5.26", "5.27", "5.29", "5.31" } },
            new ObjectSet { Name = "Plants - Subtropical", Ids = new string[] { "501.1", "501.2", "501.7", "501.8", "501.9", "501.14", "501.18", "501.20", "501.21", "501.22", "501.25", "501.26", "501.27", "501.28", "501.29", "501.31" } },
            new ObjectSet { Name = "Plants - Swamp", Ids = new string[] { "502.1", "502.7", "502.8", "502.9", "502.11", "502.14", "502.20", "502.21", "502.22", "502.23", "502.26", "502.27", "502.28", "502.29", "502.31" } },
            new ObjectSet { Name = "Plants - Woodland", Ids = new string[] { "504.21", "504.22", "504.23", "504.26", "504.2", "504.1", "504.7", "504.8", "504.9", "504.10", "504.24", "504.27", "504.28", "504.29" } },
            new ObjectSet { Name = "Plants - Woodland Hills", Ids = new string[] { "506.2", "506.7", "506.8", "506.9", "506.21", "506.22", "506.23", "506.26", "506.27", "506.29", "506.31" } },
            new ObjectSet { Name = "Rocks - Desert", Ids = new string[] { "503.2", "503.3", "503.4", "503.6", "503.18", "503.19", "503.20", "503.21", "503.22" } },
            new ObjectSet { Name = "Rocks - Haunted Woodland", Ids = new string[] { "508.1", "508.3", "508.4", "508.5", "508.6", "508.10", "508.12", "508.17" } },
            new ObjectSet { Name = "Rocks - Mountains", Ids = new string[] { "510.1", "510.3", "510.4", "510.6", "510.14", "510.17", "510.18", "510.27", "510.28", "510.31" } },
            new ObjectSet { Name = "Rocks - Rain Forest", Ids = new string[] { "500.4", "500.17", "500.19", "500.28" } },
            new ObjectSet { Name = "Rocks - Steppes", Ids = new string[] { "5.1", "5.2", "5.3", "5.4", "5.6", "5.18", "5.19", "5.20", "5.21", "5.22" } },
            new ObjectSet { Name = "Rocks - Subtropical", Ids = new string[] { "501.3", "501.4", "501.5", "501.6", "501.10", "501.23" } },
            new ObjectSet { Name = "Rocks - Swamp", Ids = new string[] { "502.2", "502.3", "502.4", "502.5", "502.6", "502.10" } },
            new ObjectSet { Name = "Rocks - Woodland", Ids = new string[] { "504.4", "504.3", "504.5", "504.6" } },
            new ObjectSet { Name = "Rocks - Woodland Hills", Ids = new string[] { "506.1", "506.3", "506.4", "506.6", "506.17", "506.18", "506.28" } },
            new ObjectSet { Name = "Shrubs", Ids = new string[] { "213.15", "213.16", "213.17" } },
            new ObjectSet { Name = "Signposts", Ids = new string[] { "212.4", "212.5", "212.6" } },
            new ObjectSet { Name = "Standing Stones", Ids = new string[] { "212.17", "212.18" } },
            new ObjectSet { Name = "Trees - Desert", Ids = new string[] { "503.5", "503.11", "503.12", "503.13", "503.28", "503.30" } },
            new ObjectSet { Name = "Trees - Haunted Woodland", Ids = new string[] { "508.13", "508.15", "508.16", "508.18", "508.19", "508.20", "508.24", "508.25", "508.30", "508.31" } },
            new ObjectSet { Name = "Trees - Mountains", Ids = new string[] { "510.5", "510.11", "510.12", "510.13", "510.15", "510.16", "510.19", "510.20", "510.24", "510.25", "510.30" } },
            new ObjectSet { Name = "Trees - Rain Forest", Ids = new string[] { "500.3", "500.12", "500.13", "500.14", "500.15", "500.16", "500.18", "500.25", "500.30" } },
            new ObjectSet { Name = "Trees - Steppes", Ids = new string[] { "5.5", "5.11", "5.12", "5.13", "5.14", "5.15", "5.16", "5.28", "5.30" } },
            new ObjectSet { Name = "Trees - Subtropical", Ids = new string[] { "501.11", "501.12", "501.13", "501.15", "501.16", "501.17", "501.19", "501.24", "501.30" } },
            new ObjectSet { Name = "Trees - Swamp", Ids = new string[] { "502.12", "502.13", "502.15", "502.16", "502.17", "502.18", "502.19", "502.24", "502.25", "502.30" } },
            new ObjectSet { Name = "Trees - Woodland", Ids = new string[] { "504.11", "504.12", "504.13", "504.14", "504.15", "504.16", "504.17", "504.18", "504.19", "504.20", "504.25", "504.30", "504.31" } },
            new ObjectSet { Name = "Trees - Woodland Hills", Ids = new string[] { "506.5", "506.10", "506.11", "506.12", "506.13", "506.14", "506.15", "506.16", "506.19", "506.20", "506.24", "506.25", "506.30" } },
            new ObjectSet { Name = "Wagon Wheels", Ids = new string[] { "211.15", "211.16" } },
            new ObjectSet { Name = "Well", Ids = new string[] { "212.0" } },
            new ObjectSet { Name = "Well Pumps", Ids = new string[] { "212.8", "212.9", "212.10" } },
            new ObjectSet { Name = "Wheelbarrows", Ids = new string[] { "205.27", "205.28" } },
            new ObjectSet { Name = "Wood Pile", Ids = new string[] { "212.11" } },
            new ObjectSet { Name = "Wood Posts", Ids = new string[] { "212.13", "212.14" } },
        };

        public static ObjectSet[] billboardsLights = new ObjectSet[]
        {
            new ObjectSet { Name = "Brazier", Ids = new string[] { "210.0" } },
            new ObjectSet { Name = "Brazier - Pillar", Ids = new string[] { "210.19" } },
            new ObjectSet { Name = "Camp Fire", Ids = new string[] { "210.1" } },
            new ObjectSet { Name = "Candles", Ids = new string[] { "210.2", "210.3", "210.4" } },
            new ObjectSet { Name = "Candlestick", Ids = new string[] { "210.5" } },
            new ObjectSet { Name = "Chandleliers", Ids = new string[] { "210.7", "210.9", "210.10", "210.23" } },
            new ObjectSet { Name = "Hanging Lamps", Ids = new string[] { "210.8", "210.11", "210.12", "210.13" } },
            new ObjectSet { Name = "Hanging Lantern", Ids = new string[] { "210.22", "210.24", "210.25", "210.26", "210.27" } },
            new ObjectSet { Name = "Mounted Torches", Ids = new string[] { "210.16", "210.17", "210.18" } },
            new ObjectSet { Name = "Standing Candle", Ids = new string[] { "210.21" } },
            new ObjectSet { Name = "Standing Lanterns", Ids = new string[] { "210.14", "210.15" } },
            new ObjectSet { Name = "Torch - Skull", Ids = new string[] { "210.6" } },
            new ObjectSet { Name = "Torch - Standing", Ids = new string[] { "210.20" } },
            new ObjectSet { Name = "Street Lamps", Ids = new string[] { "210.28", "210.29" } },
        };

        public static ObjectSet[] billboardsTreasure = new ObjectSet[]
        {
                new ObjectSet { Name = "Goldpile", Ids = new string[] { "216.0", "216.1", "216.2" } },
                new ObjectSet { Name = "Gold Ingot", Ids = new string[] { "216.3" } },
                new ObjectSet { Name = "Gold Coin", Ids = new string[] { "216.4" } },
                new ObjectSet { Name = "Silver Coin", Ids = new string[] { "216.5" } },
                new ObjectSet { Name = "Crowns", Ids = new string[] { "216.6", "216.7", "216.8", "216.9" } },
                new ObjectSet { Name = "Gems", Ids = new string[] { "216.10", "216.11", "216.12", "216.13", "216.14", "216.15", "216.16", "216.17", "216.18", "216.19" } },
                new ObjectSet { Name = "Bracer", Ids = new string[] { "216.21" } },
                new ObjectSet { Name = "Treasure Pile", Ids = new string[] { "216.20", "216.22", "216.23", "216.24", "216.25", "216.26", "216.27", "216.28", "216.29", "216.30"
                    , "216.31", "216.32", "216.33", "216.34", "216.35", "216.36", "216.37", "216.38", "216.39", "216.40", "216.41", "216.42", "216.43"
                    , "216.44", "216.45", "216.46", "216.47"
                } },
        };

        public static ObjectSet[] billboardsDungeon = new ObjectSet[]
        {
            new ObjectSet { Name = "Blood", Ids = new string[] { "206.33", "206.34" } },
            new ObjectSet { Name = "Bloody Pike", Ids = new string[] { "206.27" } },
            new ObjectSet { Name = "Bones", Ids = new string[] { "206.29", "206.30", "206.31", "206.32", "206.8" } },
            new ObjectSet { Name = "Ceiling Roots", Ids = new string[] { "213.7", "213.8", "213.9", "213.10" } },
            new ObjectSet { Name = "Chained Woman", Ids = new string[] { "211.33" } },
            new ObjectSet { Name = "Columns", Ids = new string[] { "203.2", "203.3", "203.4", "203.0", "203.1", "203.5", "203.6" } },
            new ObjectSet { Name = "Corpse Pile", Ids = new string[] { "206.22", "206.23", "206.24" } },
            new ObjectSet { Name = "Cross", Ids = new string[] { "211.32" } },
            new ObjectSet { Name = "Crucified Corpse", Ids = new string[] { "206.15", "206.16" } },
            new ObjectSet { Name = "Eviscerated Animal", Ids = new string[] { "206.35" } },
            new ObjectSet { Name = "Hanged Person", Ids = new string[] { "206.36" } },
            new ObjectSet { Name = "Hanging Chains", Ids = new string[] { "211.4", "211.5", "211.6", "211.7" } },
            new ObjectSet { Name = "Hanging Skeleton", Ids = new string[] { "206.10" } },
            new ObjectSet { Name = "Head Pile", Ids = new string[] { "206.14" } },
            new ObjectSet { Name = "Heads on Pikes", Ids = new string[] { "206.17", "206.18" } },
            new ObjectSet { Name = "Impaled Corpses", Ids = new string[] { "206.11", "206.12", "206.13" } },
            new ObjectSet { Name = "Iron Maidens", Ids = new string[] { "211.26", "211.27" } },
            new ObjectSet { Name = "Noose", Ids = new string[] { "211.22" } },
            new ObjectSet { Name = "Skeleton in Cage", Ids = new string[] { "206.26" } },
            new ObjectSet { Name = "Skulls", Ids = new string[] { "206.0", "206.1", "206.3", "206.4", "206.5", "206.6" } },
            new ObjectSet { Name = "Skulls on Pikes", Ids = new string[] { "206.2", "206.7", "206.9" } },
            new ObjectSet { Name = "Stalactites", Ids = new string[] { "300.0", "300.1", "300.2", "300.3", "300.4", "300.5" } },
            new ObjectSet { Name = "Stalagmites", Ids = new string[] { "300.6", "300.7", "300.8", "300.9", "300.10", "300.11", "300.12", "300.13", "300.14", "300.15" } },
            new ObjectSet { Name = "Statues - Monsters", Ids = new string[] { "098.0" } },
            new ObjectSet { Name = "Stocks", Ids = new string[] { "211.18", "211.19" } },
            new ObjectSet { Name = "Tombstones", Ids = new string[] { "206.19", "206.20", "206.21" } },
            new ObjectSet { Name = "Torture Rack", Ids = new string[] { "211.28" } },
            new ObjectSet { Name = "Underwater", Ids = new string[] { "105.0", "105.1", "105.2", "105.3", "105.4", "105.5", "105.6", "105.7", "105.8", "105.9", "105.10" } },
            new ObjectSet { Name = "Underwater - Animated", Ids = new string[] { "106.0", "106.1", "106.2", "106.3", "106.4", "106.5", "106.6" } },
        };

        public static ObjectSet[] billboards = billboardsPeople
            .Concat(billboardsInterior)
            .Concat(billboardsNature)
            .Concat(billboardsLights)
            .Concat(billboardsTreasure)
            .Concat(billboardsDungeon)
            .OrderBy(set => set.Name)
            .ToArray();

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

        public static ObjectSet[] houseParts = new ObjectSet[]
        {
            new ObjectSet { Name = "3 Way", Ids = new string[] { "31024" } },
            new ObjectSet { Name = "3 Way - Exit", Ids = new string[] { "31030" } },
            new ObjectSet { Name = "4 Way", Ids = new string[] { "31025" } },
            new ObjectSet { Name = "Corner", Ids = new string[] { "31006" } },
            new ObjectSet { Name = "Corner - 1 Door", Ids = new string[] { "31026", "31027" } },
            new ObjectSet { Name = "Corner - 2 Doors", Ids = new string[] { "31028" } },
            new ObjectSet { Name = "Corner - Diagonal", Ids = new string[] { "31031" } },
            new ObjectSet { Name = "Dead End", Ids = new string[] { "31018" } },
            new ObjectSet { Name = "Dead End - Exit", Ids = new string[] { "31019" } },
            new ObjectSet { Name = "Dead End - 1 Door", Ids = new string[] { "31003", "31004", "31005" } },
            new ObjectSet { Name = "Dead End - 2 Doors", Ids = new string[] { "31008", "31009", "31010" } },
            new ObjectSet { Name = "Dead End - 2 Doors - Exit", Ids = new string[] { "31020", "31021" } },
            new ObjectSet { Name = "Dead End - 3 Doors", Ids = new string[] { "31017" } },
            new ObjectSet { Name = "Hall", Ids = new string[] { "31000" } },
            new ObjectSet { Name = "Hall - Exit", Ids = new string[] { "31029" } },
            new ObjectSet { Name = "Hall - 1 Door", Ids = new string[] { "31007" } },
            new ObjectSet { Name = "Hall - 2 Doors", Ids = new string[] { "31016" } },
            new ObjectSet { Name = "Single Floor", Ids = new string[] { "1000" } },
            new ObjectSet { Name = "Single Ceiling", Ids = new string[] { "2000" } },
            new ObjectSet { Name = "Single Doorway", Ids = new string[] { "3000" } },
            new ObjectSet { Name = "Single Exit", Ids = new string[] { "3002" } },
            new ObjectSet { Name = "Single Wall", Ids = new string[] { "3004" } },
            new ObjectSet { Name = "Single Pillar", Ids = new string[] { "4004" } },
            new ObjectSet { Name = "Stairwell", Ids = new string[] { "5000", "5001", "5002", "5003", "5004", "5005", "5006", "5007", "31022", "31023" } },
            new ObjectSet { Name = "Room 1x1 - 1 Door", Ids = new string[] { "8000" } },
            new ObjectSet { Name = "Room 1x1 - 2 Doors", Ids = new string[] { "31011" } },
            new ObjectSet { Name = "Room 1x1 - 2 Doors - Exit", Ids = new string[] { "31014" } },
            new ObjectSet { Name = "Room 1x2 - 1 Door", Ids = new string[] { "8001", "10001" } },
            new ObjectSet { Name = "Room 1x3 - 1 Door", Ids = new string[] { "8002", "10002" } },
            new ObjectSet { Name = "Room 1x4 - 1 Door", Ids = new string[] { "8003", "10003" } },
            new ObjectSet { Name = "Room 1x5 - 1 Door", Ids = new string[] { "8004", "10004" } },
            new ObjectSet { Name = "Room 1x6 - 1 Door", Ids = new string[] { "8005", "10005" } },
            new ObjectSet { Name = "Room 2x1 - 1 Door", Ids = new string[] { "8006", "10006" } },
            new ObjectSet { Name = "Room 2x2 - 1 Door", Ids = new string[] { "8007", "10007" } },
            new ObjectSet { Name = "Room 2x2 - 2 Doors", Ids = new string[] { "34000", "34004", "34008" } },
            new ObjectSet { Name = "Room 2x2 - 3 Doors", Ids = new string[] { "35000" } },
            new ObjectSet { Name = "Room 2x2 - 4 Doors", Ids = new string[] { "35001" } },
            new ObjectSet { Name = "Room 2x3 - 1 Door", Ids = new string[] { "8008", "10008" } },
            new ObjectSet { Name = "Room 2x3 - 3 Doors", Ids = new string[] { "35003", "35004" } },
            new ObjectSet { Name = "Room 2x4 - 1 Door", Ids = new string[] { "8009", "10009" } },
            new ObjectSet { Name = "Room 2x4 - 2 Doors", Ids = new string[] { "34005" } },
            new ObjectSet { Name = "Room 2x5 - 1 Door", Ids = new string[] { "8010", "10010" } },
            new ObjectSet { Name = "Room 2x6 - 1 Door", Ids = new string[] { "8011", "10011" } },
            new ObjectSet { Name = "Room 3x1 - 1 Door", Ids = new string[] { "8012", "10012" } },
            new ObjectSet { Name = "Room 3x2 - 1 Door", Ids = new string[] { "8013" } },
            new ObjectSet { Name = "Room 3x2 - 2 Doors", Ids = new string[] { "34002", "34006" } },
            new ObjectSet { Name = "Room 3x2 - 3 Doors", Ids = new string[] { "34009" } },
            new ObjectSet { Name = "Room 3x3 - 1 Door", Ids = new string[] { "8014" } },
            new ObjectSet { Name = "Room 3x3 - 4 Doors", Ids = new string[] { "35002", "35009" } },
            new ObjectSet { Name = "Room 3x4 - 1 Door", Ids = new string[] { "8015" } },
            new ObjectSet { Name = "Room 3x5 - 1 Door", Ids = new string[] { "8016" } },
            new ObjectSet { Name = "Room 3x6 - 1 Door", Ids = new string[] { "8017" } },
            new ObjectSet { Name = "Room 4x1 - 1 Door", Ids = new string[] { "8018" } },
            new ObjectSet { Name = "Room 4x2 - 1 Door", Ids = new string[] { "8019" } },
            new ObjectSet { Name = "Room 4x2 - 2 Doors", Ids = new string[] { "34001", "34003" } },
            new ObjectSet { Name = "Room 4x3 - 1 Door", Ids = new string[] { "8020" } },
            new ObjectSet { Name = "Room 4x4 - 1 Door", Ids = new string[] { "8021" } },
            new ObjectSet { Name = "Room 4x4 - 2 Doors", Ids = new string[] { "34007" } },
            new ObjectSet { Name = "Room 4x5 - 1 Door", Ids = new string[] { "8022" } },
            new ObjectSet { Name = "Room 4x6 - 1 Door", Ids = new string[] { "8023" } },
            new ObjectSet { Name = "Room 5x1 - 1 Door", Ids = new string[] { "8024" } },
            new ObjectSet { Name = "Room 5x2 - 1 Door", Ids = new string[] { "8025" } },
            new ObjectSet { Name = "Room 5x3 - 1 Door", Ids = new string[] { "8026" } },
            new ObjectSet { Name = "Room 5x4 - 1 Door", Ids = new string[] { "8027" } },
            new ObjectSet { Name = "Room 5x5 - 1 Door", Ids = new string[] { "8028" } },
            new ObjectSet { Name = "Room 5x6 - 1 Door", Ids = new string[] { "8029" } },
            new ObjectSet { Name = "Room 6x1 - 1 Door", Ids = new string[] { "8030" } },
            new ObjectSet { Name = "Room 6x2 - 1 Door", Ids = new string[] { "8031" } },
            new ObjectSet { Name = "Room 6x3 - 1 Door", Ids = new string[] { "8032" } },
            new ObjectSet { Name = "Room 6x4 - 1 Door", Ids = new string[] { "8033" } },
            new ObjectSet { Name = "Room 6x5 - 1 Door", Ids = new string[] { "8034" } },
            new ObjectSet { Name = "Room 6x6 - 1 Door", Ids = new string[] { "8035" } },
            new ObjectSet { Name = "Room 3x2 - Angled 1 Door", Ids = new string[] { "35005" } },
            new ObjectSet { Name = "Room 3x3 - Angled - 1 Door", Ids = new string[] { "32000" } },
            new ObjectSet { Name = "Room 5x4 - Angled - 1 Door", Ids = new string[] { "32001" } },
            new ObjectSet { Name = "Room 3x2 - Closet - 1 Door", Ids = new string[] { "32003" } },
            new ObjectSet { Name = "Room 3x2 - Closet - 2 Doors", Ids = new string[] { "34010" } },
            new ObjectSet { Name = "Room 3x3 - Closet - 1 Door", Ids = new string[] { "32002" } },
            new ObjectSet { Name = "Room 3x3 - Closet - 2 Doors", Ids = new string[] { "35012" } },
            new ObjectSet { Name = "Room 4x4 - Closet - 4 doors", Ids = new string[] { "34011" } },
            new ObjectSet { Name = "Room 4x4 - L Shape - 3 Doors", Ids = new string[] { "35008" } },
            new ObjectSet { Name = "Room 3x3 - M Shape - 1 Door", Ids = new string[] { "33001" } },
            new ObjectSet { Name = "Room 3x3 - M Shape - 3 Doors", Ids = new string[] { "35010", "35011" } },
            new ObjectSet { Name = "Room 3x2 - P Shape - 3 Doors", Ids = new string[] { "35007" } },
            new ObjectSet { Name = "Room 4x5 - Splitted - 1 Door", Ids = new string[] { "33002" } },
            new ObjectSet { Name = "Room 2x3 - Vaulted - 1 Door", Ids = new string[] { "33003" } },
            new ObjectSet { Name = "Room 3x3 - Vaulted - 1 Door", Ids = new string[] { "33004" } },
            new ObjectSet { Name = "Room 3x4 - Vaulted - 1 Door", Ids = new string[] { "33005" } },
            new ObjectSet { Name = "Room 3x5 - Vaulted - 1 Door", Ids = new string[] { "33006" } },
            new ObjectSet { Name = "Room 4x6 - Vaulted - 1 Door", Ids = new string[] { "33007" } },
        };

        public static ObjectSet[] dungeonPartsRooms = new ObjectSet[]
        {
            new ObjectSet { Name = "Large Hall", Ids = new string[] { "58009", "58050" } },
            new ObjectSet { Name = "Large Hall - Corner", Ids = new string[] { "58022" } },
            new ObjectSet { Name = "Large Hall - Corner - 1 Door", Ids = new string[] { "58048", "58049" } },
            new ObjectSet { Name = "Large Hall - Corner - Floor - 1 Door", Ids = new string[] { "58029" } },
            new ObjectSet { Name = "Large Hall - Corner - Floor - 4 Doors", Ids = new string[] { "58031" } },
            new ObjectSet { Name = "Large Hall - Corner - Floor - 5 Doors", Ids = new string[] { "58053" } },
            new ObjectSet { Name = "Lrg Hall - Corner - Flr. - Pltfrm - 1 Chute", Ids = new string[] { "58030", "58028" } },
            new ObjectSet { Name = "Large Hall - Corner - Ledge Gaps - 1 Door", Ids = new string[] { "58021" } },
            new ObjectSet { Name = "Large Hall - Corner - Ledge Gaps", Ids = new string[] { "58020", "58023" } },
            new ObjectSet { Name = "Large Hall - Corner - Walls/Ceiling", Ids = new string[] { "58024" } },
            new ObjectSet { Name = "Lrg Hall - Corner - Walls/Ceiling - Chute", Ids = new string[] { "58026", "58025", "58027" } },
            new ObjectSet { Name = "Large Hall - Diagonal Wall - Platform", Ids = new string[] { "58052" } },
            new ObjectSet { Name = "Large Hall - External Corridor", Ids = new string[] { "58054" } },
            new ObjectSet { Name = "Ledge - Arched - 2 Way", Ids = new string[] { "61304", "61316", "61317", "61404", "61416", "61417", "61504", "61516", "61517" } },
            new ObjectSet { Name = "Ledge - Arched - 2 Way - 1 Door", Ids = new string[] { "61308", "61309", "61310", "61324", "61408", "61409", "61410", "61424", "61508", "61509", "61510", "61524" } },
            new ObjectSet { Name = "Ledge - Arched - 2 Way - 2 Doors", Ids = new string[] { "61305", "61306", "61307", "61320", "61405", "61406", "61407", "61420", "61505", "61506", "61507", "61520" } },
            new ObjectSet { Name = "Ledge - Arched - Corner", Ids = new string[] { "61303", "61311", "61312", "61321", "61323", "61403", "61411", "61412", "61421", "61423", "61503", "61511", "61512", "61521", "61523" } },
            new ObjectSet { Name = "Ledge - Arched - Corner - Inner", Ids = new string[] { "61313", "61314", "61315", "61322", "61413", "61414", "61415", "61422", "61513", "61514", "61515", "61522" } },
            new ObjectSet { Name = "Ledge - Arched - Dead End - 1 Door", Ids = new string[] { "61318", "61319", "61418", "61419", "61518", "61519" } },
            new ObjectSet { Name = "Ledge - Square - 2 Way", Ids = new string[] { "67303", "67317", "67319", "67320", "67331" } },
            new ObjectSet { Name = "Ledge - Square - 2 Way - 1 Door", Ids = new string[] { "67304", "67305", "67306", "67318", "67323" } },
            new ObjectSet { Name = "Ledge - Square - 2 Way - 2 Doors", Ids = new string[] { "67307", "67308", "67309", "67324" } },
            new ObjectSet { Name = "Ledge - Square - Corner", Ids = new string[] { "67311", "67312", "67313", "67321" } },
            new ObjectSet { Name = "Ledge - Square - Corner - Inner", Ids = new string[] { "67314", "67315", "67316", "67322" } },
            new ObjectSet { Name = "Ledge - Square - Dead End", Ids = new string[] { "67332", "67333", "67334", "67336", "67337" } },
            new ObjectSet { Name = "Ledge - Square - Dead End - 1 Door", Ids = new string[] { "67310" } },
            new ObjectSet { Name = "Pyramid Room", Ids = new string[] { "58013", "58014", "58015", "58016" } },
            new ObjectSet { Name = "Room 1x1 - 1 Door", Ids = new string[] { "67033", "67133", "67233", "70000", "70100", "70036", "70136" } },
            new ObjectSet { Name = "Room 1x1 - 2 Doors", Ids = new string[] { "60502", "60602", "60702" } },
            new ObjectSet { Name = "Room 1x2 - 1 Door", Ids = new string[] { "70001", "70037", "70101", "70137" } },
            new ObjectSet { Name = "Room 1x3 - 1 Door", Ids = new string[] { "70038", "70102", "70138" } },
            new ObjectSet { Name = "Room 1x4 - 1 Door", Ids = new string[] { "70039", "70103", "70139", "70003" } },
            new ObjectSet { Name = "Room 1x5 - 1 Door", Ids = new string[] { "70004", "70040", "70104", "70140" } },
            new ObjectSet { Name = "Room 1x6 - 1 Door", Ids = new string[] { "70005", "70041", "70105", "70141" } },
            new ObjectSet { Name = "Room 2x1 - 1 Door", Ids = new string[] { "70006", "70106", "70042", "70142" } },
            new ObjectSet { Name = "Room 2x2 - 1 Door", Ids = new string[] { "62008", "62108", "62208", "64300", "64301", "64302", "64303", "68000", "68100", "68200", "70007", "70043", "70107", "70143" } },
            new ObjectSet { Name = "Room 2x2 - 2 Doors", Ids = new string[] { "58038", "62028", "62128", "62228", "64305", "71101", "71106", "71201", "71206", "71301", "71306" } },
            new ObjectSet { Name = "Room 2x2 - 3 Doors", Ids = new string[] { "71400", "71402", "71500", "71502", "71600" } },
            new ObjectSet { Name = "Room 2x3 - 1 Door", Ids = new string[] { "70008", "70108", "70144", "70044" } },
            new ObjectSet { Name = "Room 2x2 - 4 Doors", Ids = new string[] { "71403", "71503", "71603", } },
            new ObjectSet { Name = "Room 2x4 - 1 Door", Ids = new string[] { "70009", "70045", "70109", "70145", } },
            new ObjectSet { Name = "Room 2x4 - 2 Doors", Ids = new string[] { "71107", "71207", "71307" } },
            new ObjectSet { Name = "Room 2x5 - 1 Door", Ids = new string[] { "70010", "70046", "70110", "70146" } },
            new ObjectSet { Name = "Room 2x6 - 1 Door", Ids = new string[] { "70011", "70047", "70111", "70147" } },
            new ObjectSet { Name = "Room 3x1 - 1 Door", Ids = new string[] { "70012", "70048", "70112", "70148" } },
            new ObjectSet { Name = "Room 3x2 - 1 Door", Ids = new string[] { "70113", "70013", "70049", "70149" } },
            new ObjectSet { Name = "Room 3x2 - 3 Doors", Ids = new string[] { "71405", "71406", "71505", "71506", "71605", "71606", "71401", "71501", "71601" } },
            new ObjectSet { Name = "Room 3x2 - 2 Doors", Ids = new string[] { "71104", "71108", "71204", "71208", "71304", "71308" } },
            new ObjectSet { Name = "Room 3x3 - 1 Doors", Ids = new string[] { "70014", "70114", "70050", "70150" } },
            new ObjectSet { Name = "Room 3x3 - 2 Doors", Ids = new string[] { "71710", "71810", "71910" } },
            new ObjectSet { Name = "Room 3x3 - 4 Doors", Ids = new string[] { "71404", "71504", "71604", "71705", "71805", "71905" } },
            new ObjectSet { Name = "Room 3x4 - 1 Door", Ids = new string[] { "70015", "70051", "70115", "70151" } },
            new ObjectSet { Name = "Room 3x5 - 4 Doors", Ids = new string[] { "70016", "70052", "70116", "70152" } },
            new ObjectSet { Name = "Room 3x6 - 1 Door", Ids = new string[] { "70017", "70053", "70117", "70153" } },
            new ObjectSet { Name = "Room 4x1 - 1 Door", Ids = new string[] { "70018", "70054", "70118", "70154" } },
            new ObjectSet { Name = "Room 4x2 - 1 Door", Ids = new string[] { "62009", "62109", "62209", "70019", "70055", "70119", "70155", "71100", "71200", "71300" } },
            new ObjectSet { Name = "Room 4x2 - 2 Doors", Ids = new string[] { "71102", "71202", "71302", "71105", "71205", "71305", "71701", "71801", "71901" } },
            new ObjectSet { Name = "Room 4x3 - 1 Door", Ids = new string[] { "70020", "70056", "70120", "70156" } },
            new ObjectSet { Name = "Room 4x4 - 1 Door", Ids = new string[] { "62010", "62110", "62210", "68001", "68101", "72010", "68201", "70021", "70057", "70121", "70157" } },
            new ObjectSet { Name = "Room 4x4 - 2 Doors", Ids = new string[] { "72000" } },
            new ObjectSet { Name = "Room 4x4 - 3 Doors", Ids = new string[] { "71109", "71209", "71309", "72016" } },
            new ObjectSet { Name = "Room 4x4 - 8 Doors", Ids = new string[] { "58007" } },
            new ObjectSet { Name = "Room 4x5 - 1 Door", Ids = new string[] { "70022", "70058", "70122", "70158" } },
            new ObjectSet { Name = "Room 4x6 - 1 Door", Ids = new string[] { "70023", "70059", "70123", "70159", "72011" } },
            new ObjectSet { Name = "Room 4x6 - 2 Doors", Ids = new string[] { "58039", "72001", "72002" } },
            new ObjectSet { Name = "Room 4x6 - 3 Doors", Ids = new string[] { "72006", "72007", "72008" } },
            new ObjectSet { Name = "Room 4x10 - 1 Door", Ids = new string[] { "62011", "62111", "62211" } },
            new ObjectSet { Name = "Room 5x1 - 1 Door", Ids = new string[] { "70024", "70060", "70124", "70160" } },
            new ObjectSet { Name = "Room 5x2 - 1 Door", Ids = new string[] { "70025", "70061", "70125", "70161" } },
            new ObjectSet { Name = "Room 5x3 - 1 Door", Ids = new string[] { "70026", "70062", "70126", "70162" } },
            new ObjectSet { Name = "Room 5x4 - 1 Door", Ids = new string[] { "70027", "70063", "70127", "70163" } },
            new ObjectSet { Name = "Room 5x5 - 1 Door", Ids = new string[] { "70028", "70064", "70128", "70164" } },
            new ObjectSet { Name = "Room 5x6 - 1 Door", Ids = new string[] { "70029", "70065", "70129", "70165" } },
            new ObjectSet { Name = "Room 6x1 - 1 Door", Ids = new string[] { "70030", "70066", "70130", "70166" } },
            new ObjectSet { Name = "Room 6x2 - 1 Door", Ids = new string[] { "70031", "70067", "70131", "70167" } },
            new ObjectSet { Name = "Room 6x3 - 1 Door", Ids = new string[] { "70032", "70068", "70132", "70168" } },
            new ObjectSet { Name = "Room 6x4 - 1 Door", Ids = new string[] { "70033", "70069", "70133", "70169", "72012" } },
            new ObjectSet { Name = "Room 6x5 - 1 Door", Ids = new string[] { "70034", "70070", "70134", "70170" } },
            new ObjectSet { Name = "Room 6x6 - 1 Door", Ids = new string[] { "70035", "70071", "70135", "70171", "72009" } },
            new ObjectSet { Name = "Room 6x6 - 2 Doors", Ids = new string[] { "58037", "72003", "72004" } },
            new ObjectSet { Name = "Room 6x6 - 3 Doors", Ids = new string[] { "72005" } },
            new ObjectSet { Name = "Room 2x4 - 1 Door Arched", Ids = new string[] { "62306", "62406", "62506" } },
            new ObjectSet { Name = "Room 3x5 - 1 Door Arched", Ids = new string[] { "64304" } },
            new ObjectSet { Name = "Room 4x2 - 1 Door Arched", Ids = new string[] { "62305", "62405", "62505" } },
            new ObjectSet { Name = "Room 4x4 - 1 Door Arched", Ids = new string[] { "62307", "62407", "62507" } },
            new ObjectSet { Name = "Room 4x4 - 2 Doors Arched", Ids = new string[] { "62308", "62309", "62408", "62409", "62508", "62509" } },
            new ObjectSet { Name = "Room 4x4 - Ledge Gap - 1 Door Arched", Ids = new string[] { "62029", "62129", "62229" } },
            new ObjectSet { Name = "Room 6x4 - 3 Doors Arched", Ids = new string[] { "62005", "62105", "62205" } },
            new ObjectSet { Name = "Room 6x6 - 2 Doors Arched", Ids = new string[] { "62003", "62006", "62103", "62106", "62203", "62206" } },
            new ObjectSet { Name = "Room 6x6 - 4 Doors Arched", Ids = new string[] { "62301", "62401", "62501" } },
            new ObjectSet { Name = "Room 10x10 - 2 Doors Arched", Ids = new string[] { "62004", "62104", "62204" } },
            new ObjectSet { Name = "Room 2x3 - Angled - 1 Door", Ids = new string[] { "70506", "70706", "70606" } },
            new ObjectSet { Name = "Room 3x2 - Angled - 1 Door", Ids = new string[] { "71408", "71508", "71608" } },
            new ObjectSet { Name = "Room 3x3 - Angled - 1 Door", Ids = new string[] { "70608", "70708", "70507", "70508", "70607", "70707" } },
            new ObjectSet { Name = "Room 4x3 - Angled - 1 Door", Ids = new string[] { "70502", "70602", "70702" } },
            new ObjectSet { Name = "Room 6x10 - Angled - 1 Door", Ids = new string[] { "58006" } },
            new ObjectSet { Name = "Room 6x6 - Bridge - 4 Doors Arched", Ids = new string[] { "62302", "62402", "62502" } },
            new ObjectSet { Name = "Room 1x1 - Chute Ceiling - 1 Door", Ids = new string[] { "67326", "67426", "67526" } },
            new ObjectSet { Name = "Room 3x2 - Chute Ceiling - 2 Doors", Ids = new string[] { "72013" } },
            new ObjectSet { Name = "Room 3x2 - Chute Floor - 2 Doors", Ids = new string[] { "72015" } },
            new ObjectSet { Name = "Room 3x2 - Chute Floor/Celing - 1 Door", Ids = new string[] { "72014" } },
            new ObjectSet { Name = "Room 3x3 - Chute Floor - 3 Doors", Ids = new string[] { "72017" } },
            new ObjectSet { Name = "Room 4x3 - Chute Ceiling - 2 Doors", Ids = new string[] { "72018" } },
            new ObjectSet { Name = "Room 1x1 - Closet - 2 Doors", Ids = new string[] { "71409", "71509", "71609", "71700", "71800", "71900" } },
            new ObjectSet { Name = "Room 2x2 - Closet - 1 Door", Ids = new string[] { "71407", "71507", "71607", "71708", "71808", "71908", } },
            new ObjectSet { Name = "Room 3x2 - Closet - 1 Door", Ids = new string[] { "70504", "70604", "70704" } },
            new ObjectSet { Name = "Room 3x2 - Closet - 2 Doors", Ids = new string[] { "71103", "71203", "71303" } },
            new ObjectSet { Name = "Room 3x3 - Closet - 1 Door", Ids = new string[] { "70503", "70703", "70603" } },
            new ObjectSet { Name = "Room 3x3 - Closet - 2 Doors", Ids = new string[] { "71709", "71809", "71909" } },
            new ObjectSet { Name = "Room 1x1 - Closets - 3 Doors", Ids = new string[] { "71702", "71802", "71902" } },
            new ObjectSet { Name = "Room 3x3 - Closets - 1 Door", Ids = new string[] { "70505", "70605", "70705" } },
            new ObjectSet { Name = "Room 2x1 - J Shape - 1 Door", Ids = new string[] { "70501", "70601", "70701" } },
            new ObjectSet { Name = "Room 3x3 - M Shape - 1 Door", Ids = new string[] { "71012" } },
            new ObjectSet { Name = "Room 3x2 - B Shape - 1 Door", Ids = new string[] { "70509", "70609", "70709" } },
            new ObjectSet { Name = "Room 3x2 - P Shape - 3 Doors", Ids = new string[] { "71703", "71803", "71903" } },
            new ObjectSet { Name = "Room 3x3 - L Shape - 3 Doors", Ids = new string[] { "71704", "71804", "71904" } },
            new ObjectSet { Name = "Room 3x3 - M Shape - 1 Door", Ids = new string[] { "70800", "71000", "70900" } },
            new ObjectSet { Name = "Room 3x3 - M Shape - 2 Doors Arched", Ids = new string[] { "62007", "62107", "62207" } },
            new ObjectSet { Name = "Room 3x3 - M Shape - 3 Doors", Ids = new string[] { "71706", "71707", "71806", "71807", "71906", "71907" } },
            new ObjectSet { Name = "Room 4x2 - B Shape - 1 Door", Ids = new string[] { "70500", "70600", "70700" } },
            new ObjectSet { Name = "Room 4x5 - Splitted - 1 Door", Ids = new string[] { "70901", "71001", "70801" } },
            new ObjectSet { Name = "Room 4x5 - Splitted - 1 Door", Ids = new string[] { "71010" } },
            new ObjectSet { Name = "Room 10x8 - 2 Stories - Ledge - 4 Doors", Ids = new string[] { "58010" } },
            new ObjectSet { Name = "Room 10x8 - 2 Stories - Ramp - 2 Doors", Ids = new string[] { "58011" } },
            new ObjectSet { Name = "Room 2x3 - Vaulted - 1 Door", Ids = new string[] { "70802", "70806", "71002", "70902", "70906", "71006" } },
            new ObjectSet { Name = "Room 2x4 - Vaulted - 1 Door", Ids = new string[] { "71011" } },
            new ObjectSet { Name = "Room 2x4 - Vaulted - 2 Doors", Ids = new string[] { "70807", "70907", "71007" } },
            new ObjectSet { Name = "Room 3x3 - Vaulted - 1 Door", Ids = new string[] { "70803", "70903", "71003" } },
            new ObjectSet { Name = "Room 3x4 - Vaulted - 1 Door", Ids = new string[] { "70804", "70904", "71004" } },
            new ObjectSet { Name = "Room 3x5 - Vaulted - 1 Door", Ids = new string[] { "70805", "70905", "71005" } },
            new ObjectSet { Name = "Room 3x5 - Vaulted - 2 Door", Ids = new string[] { "70808", "70908", "71008" } },
            new ObjectSet { Name = "Room 4x5 - Vaulted - 2 Doors", Ids = new string[] { "70809", "70909", "71009" } },
};

        public static ObjectSet[] dungeonPartsCorridors = new ObjectSet[]
        {
            new ObjectSet { Name = "Arched - 2 Way", Ids = new string[] { "61003", "61103", "61203", "61008", "61108", "61208", "61006", "61106", "61206", "61007", "61107", "61207" } },
            new ObjectSet { Name = "Arched - 2 Way - 1 Door", Ids = new string[] { "61011", "61111", "61211" } },
            new ObjectSet { Name = "Arched - 2 Way - 2 Doors", Ids = new string[] { "61009", "61010", "61109", "61110", "61209", "61210" } },
            new ObjectSet { Name = "Arched - 2 Way - Chute - Floor", Ids = new string[] { "61016", "61116", "61216", "61302", "61402", "61502" } },
            new ObjectSet { Name = "Arched - 2 Way - Window", Ids = new string[] { "61029", "61129", "61229" } },
            new ObjectSet { Name = "Arched - 2 Way - Wooden Beams", Ids = new string[] { "60509", "60609", "60709" } },
            new ObjectSet { Name = "Arched - 3 Way", Ids = new string[] { "61012", "61112", "61212" } },
            new ObjectSet { Name = "Arched - 3 Way - 1 Door", Ids = new string[] { "61000", "61100", "61200" } },
            new ObjectSet { Name = "Arched - 4 Way", Ids = new string[] { "61001", "61101", "61201" } },
            new ObjectSet { Name = "Arched - Corner - 1 Door", Ids = new string[] { "61023", "61024", "61123", "61124", "61223", "61224" } },
            new ObjectSet { Name = "Arched - Corner - Chute - Floor", Ids = new string[] { "61300", "61400", "61500" } },
            new ObjectSet { Name = "Arched - Corner - Chute - Ceiling/Floor", Ids = new string[] { "60501", "60601", "60701" } },
            new ObjectSet { Name = "Arched - Corner - Porticullis", Ids = new string[] { "60504", "60604", "60704" } },
            new ObjectSet { Name = "Arched - Corner - Porticullis - 1 Door", Ids = new string[] { "60505", "60605", "60705" } },
            new ObjectSet { Name = "Arched - Corner", Ids = new string[] { "61002", "61102", "61202", "61004", "61104", "61204", "61005", "61105", "61205" } },
            new ObjectSet { Name = "Arched - Dead End", Ids = new string[] { "61013", "61113", "61213" } },
            new ObjectSet { Name = "Arched - Dead End - 1 Door", Ids = new string[] { "61014", "61019", "61020", "61114", "61119", "61120", "61219", "61220" } },
            new ObjectSet { Name = "Arched - Dead End - 2 Doors", Ids = new string[] { "61021", "61022", "61121", "61122", "61221", "61222" } },
            new ObjectSet { Name = "Arched - Dead End - Chute - Floor", Ids = new string[] { "61301", "61401", "61501" } },
            new ObjectSet { Name = "Arched - Junction 2x2 - 2 Way", Ids = new string[] { "62303", "62403", "62503" } },
            new ObjectSet { Name = "Arched - Junction 2x2 - 3 Way", Ids = new string[] { "62300", "62400", "62500" } },
            new ObjectSet { Name = "Arched - Junction 2x2 - 4 Way", Ids = new string[] { "62000", "62100", "62200" } },
            new ObjectSet { Name = "Arched - Junction 2x2 - Dead End", Ids = new string[] { "62304", "62404", "62504" } },
            new ObjectSet { Name = "Arched - Junction 2x4 - 4 Way", Ids = new string[] { "62001", "62101", "62201" } },
            new ObjectSet { Name = "Arched - Junction 2x6 - 4 Way", Ids = new string[] { "62002", "62102", "62202" } },
            new ObjectSet { Name = "Arched - Ramp", Ids = new string[] { "60500", "60600", "60700", "61015", "61031", "61115", "61131", "61215", "61231" } },
            new ObjectSet { Name = "Arched - Slope", Ids = new string[] { "61030", "61130", "61230" } },
            new ObjectSet { Name = "Arched - Stairs", Ids = new string[] { "61017", "61018", "61117", "61118", "61217", "61218" } },
            new ObjectSet { Name = "Arched - Stairs - Ledge", Ids = new string[] { "59011", "59013" } },
            new ObjectSet { Name = "Hexagon - 2 Way", Ids = new string[] { "63000", "63024", "63100", "63124", "63200", "63224", "63001", "63101", "63201", "63002", "63102", "63202" } },
            new ObjectSet { Name = "Hexagon - 2 Way - 1 Door", Ids = new string[] { "63008", "63009", "63010", "63011", "63030", "63037", "63108", "63109", "63110", "63111", "63130", "63137", "63208", "63209", "63210", "63211", "63230", "63237" } },
            new ObjectSet { Name = "Hexagon - 2 Way - 2 Doors", Ids = new string[] { "63012", "63013", "63018", "63019", "63020", "63021", "63038", "63112", "63113", "63118", "63119", "63120", "63121", "63138", "63212", "63213", "63218", "63219", "63220", "63221", "63238" } },
            new ObjectSet { Name = "Hexagon - 2 Way - 3 Doors", Ids = new string[] { "63014", "63015", "63016", "63017", "63114", "63115", "63116", "63117", "63214", "63215", "63216", "63217" } },
            new ObjectSet { Name = "Hexagon - 2 Way - 4 Doors", Ids = new string[] { "63025", "63125", "63225" } },
            new ObjectSet { Name = "Hexagon - 2 Way - Beams", Ids = new string[] { "63022", "63023", "63059", "63122", "63123", "63159", "63222", "63223", "63259" } },
            new ObjectSet { Name = "Hexagon - 2 Way - Diagonal", Ids = new string[] { "63035", "63036", "63135", "63136", "63235", "63236" } },
            new ObjectSet { Name = "Hexagon - 2 Way - Diagonal - 1 Door", Ids = new string[] { "63056", "63156", "63256" } },
            new ObjectSet { Name = "Hexagon - 2 Way - 2 Corrs. - Niches", Ids = new string[] { "63050", "63150", "63250" } },
            new ObjectSet { Name = "Hexagon - 2 Way - Niches", Ids = new string[] { "63049", "63149", "63249" } },
            new ObjectSet { Name = "Hexagon - 2 Way - S", Ids = new string[] { "63039", "63139", "63239" } },
            new ObjectSet { Name = "Hexagon - 2 Way - Window", Ids = new string[] { "63053", "63153", "63253", "63054", "63154", "63254", "63055", "63155", "63255" } },
            new ObjectSet { Name = "Hexagon - 3 Way", Ids = new string[] { "63028", "63128", "63228" } },
            new ObjectSet { Name = "Hexagon - 3 Way - 1 Door", Ids = new string[] { "63029", "63129", "63229" } },
            new ObjectSet { Name = "Hexagon - 3 Way - Diagonal", Ids = new string[] { "63032", "63132", "63232" } },
            new ObjectSet { Name = "Hexagon - 4 Way", Ids = new string[] { "63042", "63142", "63242" } },
            new ObjectSet { Name = "Hexagon - 4 Way - Diagonal", Ids = new string[] { "63031", "63131", "63231" } },
            new ObjectSet { Name = "Hexagon - Corner", Ids = new string[] { "63033", "63034", "63133", "63134", "63233", "63234" } },
            new ObjectSet { Name = "Hexagon - Corner - 1 Door", Ids = new string[] { "63051", "63052", "63151", "63152", "63251", "63252" } },
            new ObjectSet { Name = "Hexagon - Dead End", Ids = new string[] { "63003", "63103", "63203" } },
            new ObjectSet { Name = "Hexagon - Dead End - 1 Door", Ids = new string[] { "63004", "63005", "63007", "63104", "63105", "63107", "63204", "63205", "63207" } },
            new ObjectSet { Name = "Hexagon - Dead End - 2 Doors", Ids = new string[] { "63006", "63058", "63106", "63158", "63206", "63258" } },
            new ObjectSet { Name = "Hexagon - Ramp", Ids = new string[] { "63040", "63041", "63140", "63141", "63240", "63241" } },
            new ObjectSet { Name = "Hexagon - Ramp - Diagonal", Ids = new string[] { "63057", "63157", "63257" } },
            new ObjectSet { Name = "Hexagon - Ramp/Corridor", Ids = new string[] { "63043", "63044", "63143", "63144" } },
            new ObjectSet { Name = "Hexagon - Stairs", Ids = new string[] { "63026", "63027", "63126", "63127", "63226", "63227" } },
            new ObjectSet { Name = "Hexagon - Transition - Cave", Ids = new string[] { "63047", "63147", "63247" } },
            new ObjectSet { Name = "Hexagon - Transition - Narrow", Ids = new string[] { "63045", "63145", "63245" } },
            new ObjectSet { Name = "Hexagon - Transition - Square", Ids = new string[] { "63048", "63148", "63248" } },
            new ObjectSet { Name = "Hexagon - Transition - Arched", Ids = new string[] { "63046", "63146", "63246" } },
            new ObjectSet { Name = "Narrow - 2 Way", Ids = new string[] { "69010", "69007" } },
            new ObjectSet { Name = "Narrow - 2 Way - 1 Door", Ids = new string[] { "69008" } },
            new ObjectSet { Name = "Narrow - 2 Way - 2 Doors", Ids = new string[] { "69009" } },
            new ObjectSet { Name = "Narrow - 3 Way", Ids = new string[] { "69002", "69004" } },
            new ObjectSet { Name = "Narrow - 3 Way - 1 Door", Ids = new string[] { "69000" } },
            new ObjectSet { Name = "Narrow - 4 Way", Ids = new string[] { "69003" } },
            new ObjectSet { Name = "Narrow - Corner", Ids = new string[] { "69005", "69006" } },
            new ObjectSet { Name = "Narrow - Dead End", Ids = new string[] { "69011" } },
            new ObjectSet { Name = "Narrow - Dead End - 1 Door", Ids = new string[] { "69012" } },
            new ObjectSet { Name = "Narrow - Ramped Stairwell", Ids = new string[] { "59016" } },
            new ObjectSet { Name = "Narrow - Stairs", Ids = new string[] { "69001" } },
            new ObjectSet { Name = "Sewers - 2 Way", Ids = new string[] { "74523", "74524", "74525", "74526", "74528", "74529", "74533", "74534" } },
            new ObjectSet { Name = "Sewers - 2 Way - Door", Ids = new string[] { "74535", "74536", "74537" } },
            new ObjectSet { Name = "Sewers - 3 Way", Ids = new string[] { "74541", "74543" } },
            new ObjectSet { Name = "Sewers - 4 Way", Ids = new string[] { "74540", "74542" } },
            new ObjectSet { Name = "Sewers - Corner", Ids = new string[] { "74532", "74538", "74539" } },
            new ObjectSet { Name = "Sewers - Dead End", Ids = new string[] { "74527" } },
            new ObjectSet { Name = "Sewers - Ramp", Ids = new string[] { "74530", "74531" } },
            new ObjectSet { Name = "Sewers - Transition - Deep", Ids = new string[] { "74500" } },
            new ObjectSet { Name = "Sewers - Deep - 2 Way", Ids = new string[] { "74501", "74502", "74503", "74504", "74506", "74507" } },
            new ObjectSet { Name = "Sewers - Deep - 2 Way - 1 Door", Ids = new string[] { "74513", "74514", "74515", "74516" } },
            new ObjectSet { Name = "Sewers - Deep - 3 Way", Ids = new string[] { "74520", "74522" } },
            new ObjectSet { Name = "Sewers - Deep - 4 Way", Ids = new string[] { "74519", "74521" } },
            new ObjectSet { Name = "Sewers - Deep - Corner", Ids = new string[] { "74510", "74511", "74517", "74518" } },
            new ObjectSet { Name = "Sewers - Deep - Dead End", Ids = new string[] { "74505" } },
            new ObjectSet { Name = "Sewers - Deep - Ramp", Ids = new string[] { "74508", "74509" } },
            new ObjectSet { Name = "Square - 2 Way", Ids = new string[] { "66000", "66100", "66200", "67000", "67007", "67100", "67107", "67200", "67207", "66001", "66101", "66201", "67001", "67101", "67201", "66002", "66102", "66202", "67002", "67003", "67102", "67103", "67202", "67203" } },
            new ObjectSet { Name = "Square - 2 Way - 1 Door", Ids = new string[] { "66007", "66107", "66207", "67011", "67111", "67211" } },
            new ObjectSet { Name = "Square - 2 Way - 2 Doors", Ids = new string[] { "66021", "66121", "66221", "67012", "67013", "67112", "67113", "67212", "67213" } },
            new ObjectSet { Name = "Square - 3 Way - 1 Door", Ids = new string[] { "67032", "67132", "67232" } },
            new ObjectSet { Name = "Square - 2 Way - Chute - Ceiling", Ids = new string[] { "67325", "67425", "67525" } },
            new ObjectSet { Name = "Square - 2 Way - Chute - Floor", Ids = new string[] { "67302", "67402", "67502" } },
            new ObjectSet { Name = "Square - 2 Way - Chute - Ceiling/Floor", Ids = new string[] { "67335", "67435", "67535" } },
            new ObjectSet { Name = "Square - 3 Way", Ids = new string[] { "67005", "67006", "67105", "67106", "67205", "67206" } },
            new ObjectSet { Name = "Square - 4 Way", Ids = new string[] { "67004", "67104", "67204" } },
            new ObjectSet { Name = "Square - Corner", Ids = new string[] { "66006", "66106", "66206", "67009", "67109", "67209", "67008", "67108", "67208", "67010", "67110", "67210" } },
            new ObjectSet { Name = "Square - Corner - 1 Door", Ids = new string[] { "67022", "67023", "67122", "67123", "67222", "67223" } },
            new ObjectSet { Name = "Square - Corner - Chute - Ceiling", Ids = new string[] { "67328", "67428", "67528" } },
            new ObjectSet { Name = "Square - Corner - Chute - Floor", Ids = new string[] { "67300", "67400", "67500" } },
            new ObjectSet { Name = "Square - Corner - Chute - Ceiling/Floor", Ids = new string[] { "67330", "67430", "67530" } },
            new ObjectSet { Name = "Square - Corner - Porticullis Slot", Ids = new string[] { "67031", "67131", "67231" } },
            new ObjectSet { Name = "Square - Dead End", Ids = new string[] { "66023", "66123", "66223", "67014", "67114", "67214" } },
            new ObjectSet { Name = "Square - Dead End - 1 Door", Ids = new string[] { "66003", "66004", "66005", "66103", "66104", "66105", "66203", "66204", "66205", "67017", "67018", "67019", "67117", "67118", "67119", "67217", "67218", "67219" } },
            new ObjectSet { Name = "Square - Dead End - 2 Doors", Ids = new string[] { "66008", "66009", "66020", "66108", "66109", "66120", "66208", "66209", "66220", "67020", "67021", "67120", "67121", "67220", "67221" } },
            new ObjectSet { Name = "Square - Dead End - 3 Doors", Ids = new string[] { "66022", "66122", "66222" } },
            new ObjectSet { Name = "Square - Dead End - Chute - Ceiling", Ids = new string[] { "67327", "67427", "67527" } },
            new ObjectSet { Name = "Square - Dead End - Chute - Floor", Ids = new string[] { "67301", "67401", "67501" } },
            new ObjectSet { Name = "Square - Dead End - Chute - Ceiling/Floor", Ids = new string[] { "67329", "67429", "67529" } },
            new ObjectSet { Name = "Square - Junction 2x2 - 2 Way", Ids = new string[] { "68003" } },
            new ObjectSet { Name = "Square - Junction 2x2 - 3 Way", Ids = new string[] { "68004" } },
            new ObjectSet { Name = "Square - Junction 2x2 - 4 Way", Ids = new string[] { "68005" } },
            new ObjectSet { Name = "Square - Junction 2x2 - Dead End", Ids = new string[] { "68002" } },
            new ObjectSet { Name = "Square - Ramp", Ids = new string[] { "65017", "67015", "67024", "67026", "67115", "67124", "67126", "67215", "67224", "67226" } },
            new ObjectSet { Name = "Square - Ramp - 1 Door", Ids = new string[] { "67030", "67130", "67230" } },
            new ObjectSet { Name = "Square - Slope", Ids = new string[] { "67027", "67028", "67029", "67127", "67128", "67129", "67227", "67228", "67229" } },
            new ObjectSet { Name = "Square - Stairs", Ids = new string[] { "65018", "67016", "67025", "67116", "67125", "67216", "67225" } },
            new ObjectSet { Name = "Square - Stairs - 2 Doors", Ids = new string[] { "58008" } },
        };

        public static ObjectSet[] dungeonPartsMisc = new ObjectSet[]
        {
            new ObjectSet { Name = "Bridge - Rope", Ids = new string[] { "62031" } },
            new ObjectSet { Name = "Bridge - Stone", Ids = new string[] { "62012", "62112", "62212" } },
            new ObjectSet { Name = "Bridge - Stone - Mid", Ids = new string[] { "61608", "61609" } },
            new ObjectSet { Name = "Bridge - Stone - End", Ids = new string[] { "61600", "61601" } },
            new ObjectSet { Name = "Ceiling 2x2", Ids = new string[] { "64000" } },
            new ObjectSet { Name = "Ceiling 6x6", Ids = new string[] { "58019" } },
            new ObjectSet { Name = "Chute - Ceiling - Hole", Ids = new string[] { "58003" } },
            new ObjectSet { Name = "Chute - Dirt Floor", Ids = new string[] { "58002" } },
            new ObjectSet { Name = "Chutes", Ids = new string[] { "58004", "58012" } },
            new ObjectSet { Name = "Circular Staircase - Bottom", Ids = new string[] { "56000", "56300" } },
            new ObjectSet { Name = "Circular Staircase - Landing", Ids = new string[] { "56002", "56303", "56305" } },
            new ObjectSet { Name = "Circular Staircase - Mid", Ids = new string[] { "56001", "56301" } },
            new ObjectSet { Name = "Circular Staircase - Room - Ceiling", Ids = new string[] { "63700" } },
            new ObjectSet { Name = "Circular Staircase - Room - Mid", Ids = new string[] { "56006", "56007", "56008", "56009" } },
            new ObjectSet { Name = "Circular Staircase - Room - Top", Ids = new string[] { "56010" } },
            new ObjectSet { Name = "Doorway", Ids = new string[] { "58005" } },
            new ObjectSet { Name = "Floor 6x3", Ids = new string[] { "64001" } },
            new ObjectSet { Name = "Floor 6x6", Ids = new string[] { "64003" } },
            new ObjectSet { Name = "Floor 7x3", Ids = new string[] { "64004" } },
            new ObjectSet { Name = "Floor 7x6", Ids = new string[] { "64005" } },
            new ObjectSet { Name = "Floor/Ceiling 2x2 - Chute 1x0.5", Ids = new string[] { "58001" } },
            new ObjectSet { Name = "Floor/Ceiling 2x2 - Chute 1x1", Ids = new string[] { "58000" } },
            new ObjectSet { Name = "Pit - Floor 4x4 - 4 Chutes", Ids = new string[] { "58017" } },
            new ObjectSet { Name = "Pit - Half-Room - 1 Door", Ids = new string[] { "58018" } },
            new ObjectSet { Name = "Platform - 1x1", Ids = new string[] { "54000" } },
            new ObjectSet { Name = "Platform - Block Stairs", Ids = new string[] { "59010" } },
            new ObjectSet { Name = "Platform - Bridge - Rectangle", Ids = new string[] { "58041" } },
            new ObjectSet { Name = "Platform - Bridge - Spike", Ids = new string[] { "58040" } },
            new ObjectSet { Name = "Platform - Chipped Beam", Ids = new string[] { "58042" } },
            new ObjectSet { Name = "Platform - Corner Beam", Ids = new string[] { "58055" } },
            new ObjectSet { Name = "Platform - Marble", Ids = new string[] { "62322" } },
            new ObjectSet { Name = "Platform - Mid - 1x1", Ids = new string[] { "58034" } },
            new ObjectSet { Name = "Platform - Mid - 2x2", Ids = new string[] { "58035" } },
            new ObjectSet { Name = "Platform - Mid - 2x2 - 1 Door", Ids = new string[] { "58036" } },
            new ObjectSet { Name = "Platform - Mid - 2x2 - Gap", Ids = new string[] { "58046", "58047" } },
            new ObjectSet { Name = "Platform - Mid", Ids = new string[] { "59014", "59015" } },
            new ObjectSet { Name = "Platform - Side", Ids = new string[] { "58043" } },
            new ObjectSet { Name = "Platform - Spike", Ids = new string[] { "58044" } },
            new ObjectSet { Name = "Platform - Top - 2x2 - 1 Door", Ids = new string[] { "58033" } },
            new ObjectSet { Name = "Platform 1x1x0.5", Ids = new string[] { "72019" } },
            new ObjectSet { Name = "Platform 1x3 - With 1x1 Floor", Ids = new string[] { "58045" } },
            new ObjectSet { Name = "Platform 3x3x1 - Bridges", Ids = new string[] { "58032" } },
            new ObjectSet { Name = "Portcullis", Ids = new string[] { "60506" } },
            new ObjectSet { Name = "Ramp - Big", Ids = new string[] { "59006" } },
            new ObjectSet { Name = "Ramp - Small", Ids = new string[] { "59008", "59009" } },
            new ObjectSet { Name = "Secret Wall Block", Ids = new string[] { "61025" } },
            new ObjectSet { Name = "Stairs", Ids = new string[] { "59000", "59001", "59002", "59003", "59007", "59012", "59017" } },
            new ObjectSet { Name = "Stairs - Corner", Ids = new string[] { "59004", "59005" } },
            new ObjectSet { Name = "Switch - Lever Base", Ids = new string[] { "61026" } },
            new ObjectSet { Name = "Switch - Levers", Ids = new string[] { "61027", "61028" } },
            new ObjectSet { Name = "Switch - Wheel", Ids = new string[] { "61032" } },
            new ObjectSet { Name = "Trapdoor", Ids = new string[] { "54001" } },
        };

        public static ObjectSet[] dungeonPartsCaves = new ObjectSet[]
        {
            new ObjectSet { Name = "Cave - 2 Way", Ids = new string[] { "60100", "60101", "60102", "60103", "60107", "60108", "60109" } },
            new ObjectSet { Name = "Cave - 3 Way", Ids = new string[] { "60106" } },
            new ObjectSet { Name = "Cave - 4 Way", Ids = new string[] { "60111" } },
            new ObjectSet { Name = "Cave - Corner", Ids = new string[] { "60104", "60105", "60110" } },
            new ObjectSet { Name = "Cave - Dead End", Ids = new string[] { "60113" } },
            new ObjectSet { Name = "Cave - Ramp", Ids = new string[] { "60112" } },
            new ObjectSet { Name = "Cave - Room - 1 Way", Ids = new string[] { "60200" } },
            new ObjectSet { Name = "Cave - Room - 2 Way", Ids = new string[] { "60201" } },
            new ObjectSet { Name = "Cave - Room - 2 Way - No Ceiling", Ids = new string[] { "60202" } },
            new ObjectSet { Name = "Chasm - 2 Way", Ids = new string[] { "62014", "62015" } },
            new ObjectSet { Name = "Chasm - 2 Way - Bridge Gap", Ids = new string[] { "62013" } },
            new ObjectSet { Name = "Chasm - 2 Way - Pathway Up", Ids = new string[] { "62026", "74112" } },
            new ObjectSet { Name = "Chasm - 3 Way", Ids = new string[] { "60203" } },
            new ObjectSet { Name = "Chasm - Ceiling", Ids = new string[] { "62017", "62018", "62021", "74111" } },
            new ObjectSet { Name = "Chasm - Corner", Ids = new string[] { "62019" } },
            new ObjectSet { Name = "Chasm - Dead End", Ids = new string[] { "62016" } },
            new ObjectSet { Name = "Chasm - Dead End - 1 Way", Ids = new string[] { "62020" } },
            new ObjectSet { Name = "Chasm - Ledge - 2 Way", Ids = new string[] { "62023", "62123", "62223" } },
            new ObjectSet { Name = "Chasm - Ledge - 2 Way - 1 Arched Door", Ids = new string[] { "62022", "62122", "62222" } },
            new ObjectSet { Name = "Chasm - Ledge - 2 Way - Gap", Ids = new string[] { "62030", "62130", "62230" } },
            new ObjectSet { Name = "Chasm - Ledge - Dead End", Ids = new string[] { "62024", "62025", "62124", "62125", "62224", "62225" } },
            new ObjectSet { Name = "Chasm - Ledge - Wall", Ids = new string[] { "62027", "62127", "62227" } },
            new ObjectSet { Name = "Chasm - Ramp", Ids = new string[] { "74110" } },
        };

        public static ObjectSet[] dungeonPartsDoors = new ObjectSet[]
        {
            new ObjectSet { Name = "Door", Ids = new string[] { "55000", "55005", "9003", "9004" } },
            new ObjectSet { Name = "Entrance/Exit - Standalone", Ids = new string[] { "70300" } },
            new ObjectSet { Name = "Entrance/Exit - Crypt Entry Room", Ids = new string[] { "58051" } },
            new ObjectSet { Name = "Red Brick Door", Ids = new string[] { "72100" } },
            new ObjectSet { Name = "Secret Door - Hexagonal", Ids = new string[] { "55007", "55018", "55024" } },
            new ObjectSet { Name = "Secret Door - Hexagonal - Large", Ids = new string[] { "55019", "55025" } },
            new ObjectSet { Name = "Secret Door - Large", Ids = new string[] { "55010", "55021", "55027", "55032", "55011", "55022", "55028" } },
            new ObjectSet { Name = "Secret Door - Narrow", Ids = new string[] { "55008" } },
            new ObjectSet { Name = "Secret Door - Standard", Ids = new string[] { "55006", "55009", "55012", "55017", "55020", "55023", "55026", "55029", "55030", "55031" } },
        };

        public static ObjectSet[] interiorParts = houseParts
            .Concat(dungeonPartsRooms)
            .Concat(dungeonPartsCorridors)
            .Concat(dungeonPartsMisc)
            .Concat(dungeonPartsCaves)
            .Concat(dungeonPartsDoors)
            .OrderBy(set => set.Name)
            .ToArray();

        public static ObjectSet[] objects = models
            .Concat(billboards)
            .Concat(interiorParts)
            .OrderBy(set => set.Name)
            .ToArray();

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

                tokens = tokens.Select(token => token.Trim('"')).ToArray();

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
            CultureInfo cultureInfo = new CultureInfo("en-US");

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
                    writer.WriteLine("\t\t<rotW>" + inst.rot.w.ToString(cultureInfo) + "</rotW>");
                    writer.WriteLine("\t\t<rotX>" + inst.rot.x.ToString(cultureInfo) + "</rotX>");
                    writer.WriteLine("\t\t<rotY>" + inst.rot.y.ToString(cultureInfo) + "</rotY>");
                    writer.WriteLine("\t\t<rotZ>" + inst.rot.z.ToString(cultureInfo) + "</rotZ>");
                }
                if(inst.heightOffset != 0f)
                {
                    writer.WriteLine("\t\t<heightOffset>" + inst.heightOffset.ToString(cultureInfo) + "</heightOffset>");
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
            CultureInfo cultureInfo = new CultureInfo("en-US");

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

                writer.WriteLine("\t\t<posX>" + obj.pos.x.ToString(cultureInfo) + "</posX>");
                writer.WriteLine("\t\t<posY>" + obj.pos.y.ToString(cultureInfo) + "</posY>");
                writer.WriteLine("\t\t<posZ>" + obj.pos.z.ToString(cultureInfo) + "</posZ>");

                writer.WriteLine("\t\t<scaleX>" + obj.scale.x.ToString(cultureInfo) + "</scaleX>");
                writer.WriteLine("\t\t<scaleY>" + obj.scale.y.ToString(cultureInfo) + "</scaleY>");
                writer.WriteLine("\t\t<scaleZ>" + obj.scale.z.ToString(cultureInfo) + "</scaleZ>");

                if (!string.IsNullOrEmpty(obj.extraData))
                {
                    writer.WriteLine("\t\t<extraData>" + obj.extraData + "</extraData>");
                }

                if (obj.type == 0)
                {
                    writer.WriteLine("\t\t<rotW>" + obj.rot.w.ToString(cultureInfo) + "</rotW>");
                    writer.WriteLine("\t\t<rotX>" + obj.rot.x.ToString(cultureInfo) + "</rotX>");
                    writer.WriteLine("\t\t<rotY>" + obj.rot.y.ToString(cultureInfo) + "</rotY>");
                    writer.WriteLine("\t\t<rotZ>" + obj.rot.z.ToString(cultureInfo) + "</rotZ>");
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