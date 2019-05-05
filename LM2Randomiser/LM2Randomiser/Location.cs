using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using LM2Randomiser.RuleParsing;
using ExtensionMethods;

namespace LM2Randomiser
{
    public class Location
    {
        public string name;

        [JsonConverter(typeof(StringEnumConverter))]
        public LocationType locationType;

        public string ruleString;
        public Item item;
        
        [JsonIgnore]
        public LocationID id;
        
        [JsonIgnore]
        public Area parentArea;

        [JsonIgnore]
        public BinaryNode ruleTree;

        [JsonIgnore]
        public bool isLocked = false;
        
        [JsonConstructor]
        public Location(string name)
        {
            this.name = name;
            Enum.TryParse(name.RemoveWhitespace(), out id);
        }
        
        public bool CanReach(PlayerState state)
        {
            return ruleTree.Evaluate(state) && state.CanReach(parentArea);
        }
    }

    public enum LocationType
    {
        Default,
        Chest,
        FreeStanding,
        Shop,
        Dialogue,
        Mural
    }

    //the item chests and free standing items map directly too their enums in the lm2 item database
    public enum LocationID
    {
        None = 0,

        DjedPillarChest = 1,
        MjolnirChest = 2,
        BatteryChest = 4,
        LampofTimeChest = 5,
        PochetteKeyChest = 6,
        PyramidCrystalChest = 7,
        VesselChest = 9,
        EggofCreationChest = 11,
        GiantsFlutesChest = 12,
        CogofAntiquityChest = 13,
        
        MobileSuperX3Item = 15,
        ShellHornChest = 16,
        HolyGrailChest = 17,
        FairyGuildPassChest = 18,
        GloveChest = 19,
        DinosaurFigureChest = 20,
        GaleFibulaChest = 21,
        FlameTorcChest = 22,
        VajraChest = 23,
        PowerBandChest = 24,
        BronzeMirrorSpot = 25,
        PerfumeChest = 26,
        IceCloakChest = 27,
        NemeanFur = 28,
        GauntletChest = 29,
        AnchorChest = 30,
        TotemPoleChest = 32,
        GrappleClawChest = 33,
        SpaulderChest = 34,
        ScalesphereChest = 35,
        CrucifixChest = 36,
        MaatsFeatherChest = 38,
        RingChest = 39,
        FeatherChest = 41,
        ScripturesChest = 42,
        FreyShip = 43,
        BookoftheDeadChest = 47,
        DestinyTabletChest = 49,
        SecretTreasureofLifeItem = 50,

        OriginSealChest = 51,
        BirthSigilChest = 52,
        LifeSigilChest = 53,
        DeathSigilChest = 54,

        ClaydollChest = 56,
        
        KnifePuzzleReward = 63,
        RapierPuzzleReward = 64,
        AxePuzzleReward = 65,
        KatanaPuzzleReward = 66,
        ShurikenPuzzleReward = 67,
        RollingShurikenPuzzleReward = 68,
        EarthSpearPuzzleReward = 69,
        FlarePuzzleReward = 70,
        BombPuzzleReward = 71,
        ChakramPuzzleReward = 72,
        CaltropPuzzleReward = 73,

        YagooMapStreetChest = 79,
        DeathVillageChest = 91,
        MiracleWitchChest = 97,
        LaMulanaChest = 99,
        LaMulana2Chest = 100,
        

        SacredOrbVoD = 101,
        SacredOrbChestRoY = 102,
        SacredOrbChestAnnwfn = 103,
        SacredOrbChestIB = 104,
        SacredOrbChestIT = 105,
        SacredOrbChestDF = 106,
        SacredOrbChestSotFG = 107,
        SacredOrbChestGotD = 108,
        SacredOrbChestTS = 109,
        SacredOrbChestHL = 110,

        MapChestRoY = 111,
        MapChestAnnwfn = 112,
        MapChestIB = 113,
        MapChestIT = 114,
        MapChestDF = 115,
        MapChestSotFG = 116,
        MapChestGotD = 117,
        MapChestTS = 118,
        MapChestHL = 119,
        MapChestValhalla = 120,
        MapChestDLM = 121,
        MapChestAC = 122,
        MapChestHoM = 123,
        MapChestEPG = 124,
        MapChestEPD = 125,

        AnkhChestRoY = 132,
        AnkhChestDF = 133,
        AnkhChestIT = 134,
        AnkhChestSotFG = 135,
        AnkhChestDLM = 136,
        AnkhChestAC = 137,
        AnkhJewel = 139,

        CrystalSkullChestRoY = 140,
        CrystalSkullChestAnnwfn = 141,
        CrystalSkullIB = 142,
        CrystalSkullChestIT = 143,
        CrystalSkullChestValhalla = 144,
        CrystalSkullChestGotD = 145,
        CrystalSkullChestTS = 146,
        CrystalSkullChestDLM = 147,
        CrystalSkullChestAC = 148,
        CrystalSkullChestHL = 149,
        CrystalSkullChestHoM = 150,
        CrystalSkullChestEPD = 151,

        ChainWhipPuzzleReward = 153,
        FlailWhipPuzzleReward = 154,
        SilverShieldPuzzleReward = 156,
        
        NeburShop1 = 256,
        NeburShop2,
        NeburShop3,
        ModroShop1,
        ModroShop2,
        ModroShop3,
        SidroShop1,
        SidroShop2,
        SidroShop3,
        HinerShop1,
        HinerShop2,
        HinerShop3,
        HinerShop4,
        KorobokShop1,
        KorobokShop2,
        KorobokShop3,
        ShuhokaShop1,
        ShuhokaShop2,
        ShuhokaShop3,
        PymShop1,
        PymShop2,
        PymShop3,
        BtkShop1,
        BtkShop2,
        BtkShop3,
        MinoShop1,
        MinoShop2,
        MinoShop3,
        BargainDuckShop1,
        BargainDuckShop2,
        BargainDuckShop3,
        VenomShop1,
        VenomShop2,
        VenomShop3,
        PiebalusaShop1,
        PiebalusaShop2,
        PiebalusaShop3,
        HiroRoderickShop1,
        HiroRoderickShop2,
        HiroRoderickShop3,
        HydlitShop1,
        HydlitShop2,
        HydlitShop3,
        AytumShop1,
        AytumShop2,
        AytumShop3,
        KeroShop1,
        KeroShop2,
        KeroShop3,
        AshGeenShop1,
        AshGeenShop2,
        AshGeenShop3,
        FairyLanShop1,
        FairyLanShop2,
        FairyLanShop3,
        MegarockShop1,
        MegarockShop2,
        MegarockShop3,
        AlsedanaItem,
        FuneralItem,
        XelpudItem,
        MapfromNebur,
        FreyasItem,
        FobosItem,
        FobosSkullItem,
        MulbrukItem,
        LightScytheItem
    }
}
