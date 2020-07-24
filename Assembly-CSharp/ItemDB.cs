using System.Collections.Generic;
using LaMulana2RandomizerShared;

namespace LM2RandomiserMod
{
    public abstract class ItemDB
    {
        public static ItemInfo GetItemInfo(ItemID id)
        {
            itemData.TryGetValue(id, out ItemInfo result);
            return result;
        }
        
        private static readonly Dictionary<ItemID, ItemInfo> itemData = new Dictionary<ItemID, ItemInfo>
        {
            {ItemID.HandScanner,                new ItemInfo("H Scanner",        "H Scanner",         "item",        2,     0,        1,    1)},
            {ItemID.DjedPillar,                 new ItemInfo("Djed Pillar",      "Djed Pillar",       "item",        2,     1,       10,    1)},
            {ItemID.Mjolnir,                    new ItemInfo("Mjolnir",          "Mjolnir",           "item",        2,     2,       10,    1)},
            {ItemID.Beherit,                    new ItemInfo("Beherit",          "Beherit",           "item",        2,     3,       10,    1)},
            {ItemID.AncientBattery,             new ItemInfo("Battery",          "Battery",           "item",        2,     4,       10,    1)},
            {ItemID.LampofTime,                 new ItemInfo("Lamp",             "Lamp",              "item",        2,     5,       10,    1)},
            {ItemID.PochetteKey,                new ItemInfo("P Key",            "P Key",             "item",        2,     6,       10,    1)},
            {ItemID.PyramidCrystal,             new ItemInfo("Crystal P",        "Crystal P",         "item",        2,     7,       10,    1)},
            {ItemID.CrystalSkull,               new ItemInfo("Crystal S",        "Crystal S",         "item",        2,     8,        5,    1)},
            {ItemID.Vessel,                     new ItemInfo("Vessel",           "Vessel",            "item",        2,     9,       10,    1)},
            {ItemID.Pepper,                     new ItemInfo("Pepper",           "Pepper",            "item",        2,     10,       5,     1)},
            {ItemID.EggofCreation,              new ItemInfo("Egg",              "Egg",               "item",        2,     11,      10,    1)},
            {ItemID.GiantsFlute,                new ItemInfo("G Pipe",           "G Pipe",            "item",        2,     12,      15,    1)},
            {ItemID.CogofAntiquity,             new ItemInfo("Gear",             "Gear",              "item",        2,     13,      10,    1)},
            {ItemID.MulanaTalisman,             new ItemInfo("M Talisman",       "M Talisman",        "item",        2,     14,      10,    1)},
                                                                                                                              
            {ItemID.MobileSuperx3P,             new ItemInfo("MSX3p",            "MSX",               "item",        2,     15,      20,    2)},
            {ItemID.ShellHorn,                  new ItemInfo("Shell Horn",       "Shell Horn",        "item",        2,     16,       0,    1)},
            {ItemID.HolyGrail,                  new ItemInfo("Holy Grail",       "Holy Grail",        "item",        2,     17,       0,    1)},
            {ItemID.FairyPass,                  new ItemInfo("F Pass",           "F Pass",            "item",        2,     18,      10,    1)},
            {ItemID.Gloves,                     new ItemInfo("Glove",            "Glove",             "item",        2,     19,      10,    1)},
            {ItemID.DinosaurFigure,             new ItemInfo("D Figure",         "D Figure",          "item",        2,     20,      10,    1)},
            {ItemID.GaleFibula,                 new ItemInfo("G Band",           "G Band",            "item",        2,     21,      10,    1)},
            {ItemID.FlameTorc,                  new ItemInfo("F Torque",         "F Torque",          "item",        2,     22,      10,    1)},
            {ItemID.Vajra,                      new ItemInfo("Vajra",            "Vajra",             "item",        2,     23,      10,    1)},
            {ItemID.PowerBand,                  new ItemInfo("Power Band",       "Power Band",        "item",        2,     24,      10,    1)},
            {ItemID.BronzeMirror,               new ItemInfo("B Mirror",         "B Mirror",          "item",        2,     25,      10,    1)},
            {ItemID.Perfume,                    new ItemInfo("Perfume",          "Perfume",           "item",        2,     26,      10,    1)},
            {ItemID.IceCloak,                   new ItemInfo("Ice Cape",         "Ice Cape",          "item",        2,     27,      10,    1)},
            {ItemID.NemeanFur,                  new ItemInfo("Fur",              "Fur",               "item",        2,     28,      15,    1)},
            {ItemID.Gauntlet,                   new ItemInfo("Gauntlet",         "Gauntlet",          "item",        2,     29,      10,    1)},
            {ItemID.Anchor,                     new ItemInfo("Anchor",           "Anchor",            "item",        2,     30,      10,    1)},
            {ItemID.FreyasPendant,              new ItemInfo("F Pendant",        "F Pendant",         "item",        2,     31,       5,    1)},
            {ItemID.TotemPole,                  new ItemInfo("T Pole",           "T Pole",            "item",        2,     32,      10,    1)},
            {ItemID.GrappleClaw,                new ItemInfo("G Claw",           "G Claw",            "item",        2,     33,      10,    1)},
            {ItemID.Spaulder,                   new ItemInfo("Spaulder",         "Spaulder",          "item",        2,     34,      15,    1)},
            {ItemID.Scalesphere,                new ItemInfo("Scalesphere",      "Scalesphere",       "item",        2,     35,       5,    1)},
            {ItemID.Crucifix,                   new ItemInfo("Crucifix",         "Crucifix",          "item",        2,     36,      10,    1)},
            {ItemID.GaneshaTalisman,            new ItemInfo("Gold Bangle",      "Gold Bangle",       "item",        2,     37,      10,    1)},
            {ItemID.MaatsFeather,               new ItemInfo("M Feather",        "M Feather",         "item",        2,     38,      15,    1)},
            {ItemID.Ring,                       new ItemInfo("Ring",             "Ring",              "item",        2,     39,      10,    1)},
            {ItemID.Bracelet,                   new ItemInfo("Bracelet",         "Bracelet",          "item",        2,     40,      10,    1)},
            {ItemID.Feather,                    new ItemInfo("Feather",          "Feather",           "item",        2,     41,      10,    1)},
            {ItemID.Scriptures,                 new ItemInfo("Scriptures",       "Scriptures",        "item",        2,     42,      10,    1)},
            {ItemID.FreysShip,                  new ItemInfo("F Ship",           "F Ship",            "item",        2,     43,      10,    1)},
            {ItemID.Codices,                    new ItemInfo("Pandora Box",      "Pandora Box",       "item",        2,     44,       5,    1)},
            {ItemID.SnowShoes,                  new ItemInfo("Snow Shoes",       "Snow Shoes",        "item",        2,     45,       5,    1)},
            {ItemID.Harp,                       new ItemInfo("Harp",             "Harp",              "item",        2,     46,      10,    1)},
            {ItemID.BookoftheDead,              new ItemInfo("Book",             "Book",              "item",        2,     47,      10,    1)},
            {ItemID.LightScythe,                new ItemInfo("L Scythe",         "L Scythe",          "item",        2,     48,      10,    1)},
            {ItemID.DestinyTablet,              new ItemInfo("Destiny Tablet",   "Destiny Tablet",    "item",        2,     49,      15,    1)},
            {ItemID.SecretTreasureofLife,       new ItemInfo("Secret Treasure",  "Secret Treasure",   "item",        2,     50,      15,    1)},
                                                                                                                                           
            {ItemID.OriginSigil,                new ItemInfo("Origin Seal",      "Origin Seal",       "item",        2,     51,      10,    1)},
            {ItemID.BirthSigil,                 new ItemInfo("Birth Seal",       "Birth Seal",        "item",        2,     52,      10,    1)},
            {ItemID.LifeSigil,                  new ItemInfo("Life Seal",        "Life Seal",         "item",        2,     53,      10,    1)},
            {ItemID.DeathSigil,                 new ItemInfo("Death Seal",       "Death Seal",        "item",        2,     54,      10,    1)},
                                                                                                                                           
            {ItemID.ClaydollSuit,               new ItemInfo("Clay Doll",        "Clay Doll",         "fashion",     2,     56,      10,    1)},
            {ItemID.KimonoCowgirl,              new ItemInfo("Kimono Cowgirl",   "Kimono Cowgirl",    "fashion",     2,     57,       0,    1)},
            {ItemID.Valkyria,                   new ItemInfo("Valkyria",         "Valkyria",          "fashion",     2,     58,       0,    1)},
            {ItemID.LittleDevil,                new ItemInfo("Little Devil",     "Little Devil",      "fashion",     2,     59,       0,    1)},
            {ItemID.EasternEurope,              new ItemInfo("Eastern Europe",   "Eastern Europe",    "fashion",     2,     60,       0,    1)},
                                                                                                                                           
            {ItemID.Whip,                       new ItemInfo("Whip",             "Whip",              "weapon",      2,     62,       5,    1)},
            {ItemID.ChainWhip,                  new ItemInfo("Whip2",            "Whip2",             "weapon",      2,     62,      10,    1)},
            {ItemID.FlailWhip,                  new ItemInfo("Whip3",            "Whip3",             "weapon",      2,     62,      20,    1)},
                                                                                                                                           
            {ItemID.Knife,                      new ItemInfo("Knife",            "Knife",             "weapon",      2,     63,      10,    1)},
            {ItemID.Rapier,                     new ItemInfo("Rapier",           "Rapier",            "weapon",      2,     64,      10,    1)},
            {ItemID.Axe,                        new ItemInfo("Axe",              "Axe",               "weapon",      2,     65,      15,    1)},
            {ItemID.Katana,                     new ItemInfo("Katana",           "Katana",            "weapon",      2,     66,      15,    1)},
            {ItemID.Shuriken,                   new ItemInfo("Shuriken",         "Shuriken",          "weapon",      2,     67,       5,    1)},
            {ItemID.RollingShuriken,            new ItemInfo("R-Shuriken",       "R-Shuriken",        "weapon",      2,     68,       5,    1)},
            {ItemID.EarthSpear,                 new ItemInfo("E-Spear",          "E-Spear",           "weapon",      2,     69,       5,    1)},
            {ItemID.Flare,                      new ItemInfo("Flare Gun",        "Flare Gun",         "weapon",      2,     70,       5,    1)},
            {ItemID.Bomb,                       new ItemInfo("Bomb",             "Bomb",              "weapon",      2,     71,       5,    1)},
            {ItemID.Chakram,                    new ItemInfo("Chakram",          "Chakram",           "weapon",      2,     72,       5,    1)},
            {ItemID.Caltrops,                   new ItemInfo("Caltrops",         "Caltrops",          "weapon",      2,     73,       5,    1)},
            {ItemID.Pistol,                     new ItemInfo("Pistol",           "Pistol",            "weapon",      2,     74,      20,    1)},
                                                                                                                                           
            {ItemID.Buckler,                    new ItemInfo("Shield",           "Shield",            "weapon",      2,     75,       1,    1)},
            {ItemID.SilverShield,               new ItemInfo("Shield2",          "Shield2",           "weapon",      2,     75,      10,    1)},
            {ItemID.AngelShield,                new ItemInfo("Shield3",          "Shield3",           "weapon",      2,     75,      20,    1)},
                                                                                                                                           
            {ItemID.AnkhJewel,                  new ItemInfo("Ankh Jewel",       "Ankh Jewel",        "weapon",      2,     76,       5,    1)},
                                                                                                                                          
            {ItemID.Xelputter,                  new ItemInfo("Xelputter",        "Xelputter",         "soft",        2,     77,      15,    1)},
            {ItemID.YagooMapReader,             new ItemInfo("G Map",            "G Map",             "soft",        2,     78,      15,    1)},
            {ItemID.YagooMapStreet,             new ItemInfo("G Street",         "G Street",          "soft",        2,     79,      15,    1)},
            {ItemID.TextTrax,                   new ItemInfo("TextTrax",         "TextTrax",          "soft",        2,     80,      15,    1)},
            {ItemID.RuinsEncylopedia,           new ItemInfo("R Book",           "R Book",            "soft",        2,     81,      15,    1)},
            {ItemID.Mantra,                     new ItemInfo("Mantra",           "Mantra",            "soft",        2,     82,      10,    1)},
            {ItemID.Guild,                      new ItemInfo("Guild",            "Guild",             "soft",        2,     83,      15,    1)},
            {ItemID.Research,                   new ItemInfo("Research",         "Research",          "soft",        2,     84,      15,    1)},
            {ItemID.EngaMusica,                 new ItemInfo("Enga Musica",      "Enga Musica",       "soft",        2,     85,      10,    1)},
            {ItemID.BeoEglana,                  new ItemInfo("Beo Eg-Lana",      "Beo Eg-Lana",       "soft",        2,     86,      15,    1)},
            {ItemID.Alert,                      new ItemInfo("Alarm",            "Alarm",             "soft",        2,     87,      15,    1)},
            {ItemID.Snapshot,                   new ItemInfo("Snapshots",        "Snapshots",         "soft",        2,     88,      10,    1)},
            {ItemID.SkullReader,                new ItemInfo("Skull",            "Skull",             "soft",        2,     89,      15,    1)},
            {ItemID.RaceScanner,                new ItemInfo("Race Reader",      "Race Reader",       "soft",        2,     90,      15,    1)},
            {ItemID.DeathVillage,               new ItemInfo("Death Village",    "Death Village",     "soft",        2,     91,      15,    1)},
            {ItemID.RoseandCamelia,             new ItemInfo("R and C",          "R and C",           "soft",        2,     92,      15,    1)},
            {ItemID.SpaceCapstarII,             new ItemInfo("Capstar II",       "Capstar II",        "soft",        2,     93,      15,    1)},
            {ItemID.LonelyHouseMoving,          new ItemInfo("L House Moving",   "L House Moving",    "soft",        2,     94,      15,    1)},
            {ItemID.MekuriMaster,               new ItemInfo("Mekuri Master",    "Mekuri Master",     "soft",        2,     95,      15,    1)},
            {ItemID.BounceShot,                 new ItemInfo("Bounce Shot",      "Bounce Shot",       "soft",        2,     96,      15,    1)},
            {ItemID.MiracleWitch,               new ItemInfo("Miracle Witch",    "Miracle Witch",     "soft",        2,     97,      15,    1)},
            {ItemID.FutureDevelopmentCompany,   new ItemInfo("Future DC",        "Future DC",         "soft",        2,     98,       5,    1)},
            {ItemID.LaMulana,                   new ItemInfo("La-Mulana",        "La-Mulana",         "soft",        2,     99,      15,    1)},
            {ItemID.LaMulana2,                  new ItemInfo("La-Mulana2",       "La-Mulana2",        "soft",        2,     100,     15,    1)},
                                                                                                                                           
            {ItemID.SacredOrb0,                 new ItemInfo("Sacred Orb",       "Sacred Orb0",       "item",        2,     101,     12,    1)},
            {ItemID.SacredOrb1,                 new ItemInfo("Sacred Orb",       "Sacred Orb1",       "item",        2,     102,     12,    1)},
            {ItemID.SacredOrb2,                 new ItemInfo("Sacred Orb",       "Sacred Orb2",       "item",        2,     103,     12,    1)},
            {ItemID.SacredOrb3,                 new ItemInfo("Sacred Orb",       "Sacred Orb3",       "item",        2,     104,     12,    1)},
            {ItemID.SacredOrb4,                 new ItemInfo("Sacred Orb",       "Sacred Orb4",       "item",        2,     105,     12,    1)},
            {ItemID.SacredOrb5,                 new ItemInfo("Sacred Orb",       "Sacred Orb5",       "item",        2,     106,     12,    1)},
            {ItemID.SacredOrb6,                 new ItemInfo("Sacred Orb",       "Sacred Orb6",       "item",        2,     107,     12,    1)},
            {ItemID.SacredOrb7,                 new ItemInfo("Sacred Orb",       "Sacred Orb7",       "item",        2,     108,     12,    1)},
            {ItemID.SacredOrb8,                 new ItemInfo("Sacred Orb",       "Sacred Orb8",       "item",        2,     109,     12,    1)},
            {ItemID.SacredOrb9,                 new ItemInfo("Sacred Orb",       "Sacred Orb9",       "item",        2,     110,     12,    1)},
                                                                                                                                           
            {ItemID.Map1,                       new ItemInfo("Map",              "Map1",              "item",        2,     111,     10,    1)},
            {ItemID.Map2,                       new ItemInfo("Map",              "Map2",              "item",        2,     112,     10,    1)},
            {ItemID.Map3,                       new ItemInfo("Map",              "Map3",              "item",        2,     113,     10,    1)},
            {ItemID.Map4,                       new ItemInfo("Map",              "Map4",              "item",        2,     114,     10,    1)},
            {ItemID.Map5,                       new ItemInfo("Map",              "Map5",              "item",        2,     115,     10,    1)},
            {ItemID.Map6,                       new ItemInfo("Map",              "Map6",              "item",        2,     116,     10,    1)},
            {ItemID.Map7,                       new ItemInfo("Map",              "Map7",              "item",        2,     117,     10,    1)},
            {ItemID.Map8,                       new ItemInfo("Map",              "Map8",              "item",        2,     118,     10,    1)},
            {ItemID.Map9,                       new ItemInfo("Map",              "Map9",              "item",        2,     119,     10,    1)},
            {ItemID.Map10,                      new ItemInfo("Map",              "Map10",             "item",        2,     120,     10,    1)},
            {ItemID.Map11,                      new ItemInfo("Map",              "Map11",             "item",        2,     121,     10,    1)},
            {ItemID.Map12,                      new ItemInfo("Map",              "Map12",             "item",        2,     122,     10,    1)},
            {ItemID.Map13,                      new ItemInfo("Map",              "Map13",             "item",        2,     123,     10,    1)},
            {ItemID.Map14,                      new ItemInfo("Map",              "Map14",             "item",        2,     124,     10,    1)},
            {ItemID.Map15,                      new ItemInfo("Map",              "Map15",             "item",        2,     125,     10,    1)},
            {ItemID.Map16,                      new ItemInfo("Map",              "Map16",             "item",        2,     126,     10,    1)},
            {ItemID.Map17,                      new ItemInfo("Map",              "Map17",             "item",        2,     127,     10,    1)},
            {ItemID.Map18,                      new ItemInfo("Map",              "Map18",             "item",        2,     128,     10,    1)},
            {ItemID.Map19,                      new ItemInfo("Map",              "Map19",             "item",        2,     129,     10,    1)},
            {ItemID.Map20,                      new ItemInfo("Map",              "Map20",             "item",        2,     130,     10,    1)},
                                                                                                                                           
            {ItemID.AnkhJewel1,                 new ItemInfo("Ankh Jewel",       "Ankh Jewel1",       "weapon",      2,     131,     10,    1)},
            {ItemID.AnkhJewel2,                 new ItemInfo("Ankh Jewel",       "Ankh Jewel2",       "weapon",      2,     132,     10,    1)},
            {ItemID.AnkhJewel3,                 new ItemInfo("Ankh Jewel",       "Ankh Jewel3",       "weapon",      2,     133,     10,    1)},
            {ItemID.AnkhJewel4,                 new ItemInfo("Ankh Jewel",       "Ankh Jewel4",       "weapon",      2,     134,     10,    1)},
            {ItemID.AnkhJewel5,                 new ItemInfo("Ankh Jewel",       "Ankh Jewel5",       "weapon",      2,     135,     10,    1)},
            {ItemID.AnkhJewel6,                 new ItemInfo("Ankh Jewel",       "Ankh Jewel6",       "weapon",      2,     136,     10,    1)},
            {ItemID.AnkhJewel7,                 new ItemInfo("Ankh Jewel",       "Ankh Jewel7",       "weapon",      2,     137,     10,    1)},
            {ItemID.AnkhJewel8,                 new ItemInfo("Ankh Jewel",       "Ankh Jewel8",       "weapon",      2,     138,     10,    1)},
            {ItemID.AnkhJewel9,                 new ItemInfo("Ankh Jewel",       "Ankh Jewel9",       "weapon",      2,     139,     10,    1)},
                                                                                                                                        
            {ItemID.CrystalSkull1,              new ItemInfo("Crystal S",        "Crystal S1",        "item",        2,     140,     15,    1)},
            {ItemID.CrystalSkull2,              new ItemInfo("Crystal S",        "Crystal S2",        "item",        2,     141,     15,    1)},
            {ItemID.CrystalSkull3,              new ItemInfo("Crystal S",        "Crystal S3",        "item",        2,     142,     15,    1)},
            {ItemID.CrystalSkull4,              new ItemInfo("Crystal S",        "Crystal S4",        "item",        2,     143,     15,    1)},
            {ItemID.CrystalSkull5,              new ItemInfo("Crystal S",        "Crystal S5",        "item",        2,     144,     15,    1)},
            {ItemID.CrystalSkull6,              new ItemInfo("Crystal S",        "Crystal S6",        "item",        2,     145,     15,    1)},
            {ItemID.CrystalSkull7,              new ItemInfo("Crystal S",        "Crystal S7",        "item",        2,     146,     15,    1)},
            {ItemID.CrystalSkull8,              new ItemInfo("Crystal S",        "Crystal S8",        "item",        2,     147,     15,    1)},
            {ItemID.CrystalSkull9,              new ItemInfo("Crystal S",        "Crystal S9",        "item",        2,     148,     15,    1)},
            {ItemID.CrystalSkull10,             new ItemInfo("Crystal S",        "Crystal S10",       "item",        2,     149,     15,    1)},
            {ItemID.CrystalSkull11,             new ItemInfo("Crystal S",        "Crystal S11",       "item",        2,     150,     15,    1)},
            {ItemID.CrystalSkull12,             new ItemInfo("Crystal S",        "Crystal S12",       "item",        2,     151,     15,    1)},
                                                                                                                                          
            {ItemID.Heaven,                     new ItemInfo("Mantra1",          "Mantra1",           "menu",        2,     152,     10,    1)},
            {ItemID.Earth,                      new ItemInfo("Mantra2",          "Mantra2",           "menu",        2,     153,     10,    1)},
            {ItemID.Sun,                        new ItemInfo("Mantra3",          "Mantra3",           "menu",        2,     154,     10,    1)},
            {ItemID.Moon,                       new ItemInfo("Mantra4",          "Mantra4",           "menu",        2,     155,     10,    1)},
            {ItemID.Sea,                        new ItemInfo("Mantra5",          "Mantra5",           "menu",        2,     156,     10,    1)},
            {ItemID.Fire,                       new ItemInfo("Mantra6",          "Mantra6",           "menu",        2,     157,     10,    1)},
            {ItemID.Wind,                       new ItemInfo("Mantra7",          "Mantra7",           "menu",        2,     158,     10,    1)},
            {ItemID.Mother,                     new ItemInfo("Mantra8",          "Mantra8",           "menu",        2,     159,     10,    1)},
            {ItemID.Child,                      new ItemInfo("Mantra9",          "Mantra9",           "menu",        2,     160,     10,    1)},
            {ItemID.Night,                      new ItemInfo("Mantra10",         "Mantra10",          "menu",        2,     161,     10,    1)},
                                                                                                                                      
            {ItemID.ShurikenAmmo,               new ItemInfo("Shuriken-b",       "Shuriken-b",        "weapon",     -1,      -1,      1,    10,    200)},
            {ItemID.RollingShurikenAmmo,        new ItemInfo("R-Shuriken-b",     "R-Shuriken-b",      "weapon",     -1,      -1,      1,    10,    100)},
            {ItemID.EarthSpearAmmo,             new ItemInfo("E-Spear-b",        "E-Spear-b",         "weapon",     -1,      -1,      2,    10,    100)},
            {ItemID.FlareAmmo,                  new ItemInfo("Flare Gun-b",      "Flare Gun-b",       "weapon",     -1,      -1,      2,    10,    50)},
            {ItemID.BombAmmo,                   new ItemInfo("Bomb-b",           "Bomb-b",            "weapon",     -1,      -1,      3,    5,     30)},
            {ItemID.ChakramAmmo,                new ItemInfo("Chakram-b",        "Chakram-b",         "weapon",     -1,      -1,      2,    1,     12)},
            {ItemID.CaltropsAmmo,               new ItemInfo("Caltrops-b",       "Caltrops-b",        "weapon",     -1,      -1,      1,    10,    100)},
            {ItemID.PistolAmmo,                 new ItemInfo("Pistol-b",         "Pistol-b",          "weapon",     -1,      -1,     40,    1,     1)},
            {ItemID.Weights,                    new ItemInfo("Weight",           "Weight",            "item",       -1,      -1,      1,    5)},

            {ItemID.Research1,                  new ItemInfo("Research",         "Research",          "soft",     6,     43,     10,   1)},
            {ItemID.Research2,                  new ItemInfo("Research",         "Research",          "soft",     8,     45,     10,   1)},
            {ItemID.Research3,                  new ItemInfo("Research",         "Research",          "soft",     7,     78,     10,   1)},
            {ItemID.Research4,                  new ItemInfo("Research",         "Research",          "soft",     7,     79,     10,   1)},
            {ItemID.Research5,                  new ItemInfo("Research",         "Research",          "soft",     7,     80,     10,   1)},
            {ItemID.Research6,                  new ItemInfo("Research",         "Research",          "soft",     7,     81,     10,   1)},
            {ItemID.Research7,                  new ItemInfo("Research",         "Research",          "soft",     7,     83,     10,   1)},
            {ItemID.Research8,                  new ItemInfo("Research",         "Research",          "soft",     7,     85,     10,   1)},
            {ItemID.Research9,                  new ItemInfo("Research",         "Research",          "soft",     7,     86,     10,   1)},
            {ItemID.Research10,                 new ItemInfo("Research",         "Research",          "soft",     15,    44,     10,   1)},
        };                                                                                                               

    }

    public class ItemInfo
    {
        public string BoxName;
        public string ShopName;
        public string ShopType;
        public int ItemSheet;
        public int ItemFlag;
        public int ShopPrice;
        public int ShopAmount;
        public int MaxShopAmount;
        
        public ItemInfo(string boxName, string shopName, string type, int itemSheet, int itemFlag, int price, int amount, int maxAmount = -1)
        {
            BoxName = boxName;
            ShopName = shopName;
            ShopType = type;
            ItemSheet = itemSheet;
            ItemFlag = itemFlag;
            ShopPrice = price;
            ShopAmount = amount;
            MaxShopAmount = maxAmount;
        }
    }
}
