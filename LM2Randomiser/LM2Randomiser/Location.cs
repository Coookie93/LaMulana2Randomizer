using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LM2Randomiser.RuleParsing;
using ExtensionMethods;

namespace LM2Randomiser
{
    public class Location
    {
        //TODO?? maybe have a location id enum like the items 

        public string name;
        public Area parentArea;
        public BinaryNode ruleTree;
        public Item item;
        public LocationType locationType;
        public LocationID id;
        public bool isLocked;

        public Location(string name, Area parent, LocationType locationType)
        {
            this.name = name;
            this.parentArea = parent;
            this.locationType = locationType;

            Enum.TryParse(name.RemoveWhitespace(), out id);
            isLocked = false;
        }

        public bool CanReach(PlayerState state)
        {
            return ruleTree.Evaluate(state) && state.CanReach(parentArea);
        }
    }

    public enum LocationType
    {
        Shop,
        Default
    }

    //this is pretty bad but it works for now, probably should change this to use descriptions
    public enum LocationID
    {
        None = 0,
        NeburShop1,
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

        SacredOrbVoD,
        SacredOrbChestRoY,
        SacredOrbChestAnnwfn,
        SacredOrbChestIB,
        SacredOrbChestIT,
        SacredOrbChestDF,
        SacredOrbChestSotFG,
        SacredOrbChestGotD,
        SacredOrbChestTS,
        SacredOrbChestHL,

        MapChestRoY,
        MapChestAnnwfn,
        MapChestIB,
        MapChestIT,
        MapChestDF,
        MapChestSotFG,
        MapChestGotD,
        MapChestTS,
        MapChestHL,
        MapChestValhalla,
        MapChestDLM,
        MapChestAC,
        MapChestHoM,
        MapChestEPG,
        MapChestEPD,
        
        CrystalSkullChestRoY,
        CrystalSkullChestAnnwfn,
        CrystalSkullIB,
        CrystalSkullChestIT,
        CrystalSkullChestValhalla,
        CrystalSkullChestGotD,
        CrystalSkullChestTS,
        CrystalSkullChestDLM,
        CrystalSkullChestAC,
        CrystalSkullChestHL,
        CrystalSkullChestHoM,
        CrystalSkullChestEPD,

        AnkhChestRoY,
        AnkhChestDF,
        AnkhChestIT,
        AnkhChestSotFG,
        AnkhChestDLM,
        AnkhChestAC,
        AnkhJewel,

        AlsedanaItem,
        FuneralItem,
        XelpudItem,
        MapfromNebur,
        ShellHornChest,
        HolyGrailChest,
        FreyasItem,
        FobosItem,
        BronzeMirrorSpot,
        PyramidCrystalChest,
        ShurikenPuzzleReward,
        KnifePuzzleReward,
        OriginSealChest,
        FreyShip,
        DeathVillageChest,
        GloveChest,
        RollingShurikenPuzzleReward,
        SilverShieldPuzzleReward,
        DjedPillarChest,
        GrappleClawChest,
        LifeSigilChest,
        FlameTorcChest,
        FairyGuildPassChest,
        ScalesphereChest,
        CaltropPuzzleReward,
        FlailWhipPuzzleReward,
        GaleFibulaChest,
        EarthSpearPuzzleReward,
        MulbrukItem,
        IceCloakChest,
        TotemPoleChest,
        LampofTimeChest,
        MjolnirChest,
        ChainWhipPuzzleReward,
        ChakramPuzzleReward,
        BatteryChest,
        DinosaurFigureChest,
        BirthSigilChest,
        PochetteKeyChest,
        RapierPuzzleReward,
        ClaydollChest,
        GauntletChest,
        YagooMapStreetChest,
        VajraChest,
        AnchorChest,
        KatanaPuzzleReward,
        FeatherChest,
        LaMulanaChest,
        VesselChest,
        CrucifixChest,
        MaatsFeatherChest,
        RingChest,
        EggofCreation,
        FlarePuzzleReward,
        PowerBandChest,
        DestinyTabletChest,
        Scriptures,
        NemeanFur,
        GiantsFlutesChest,
        MiracleWitchChest,
        PerfumeChest,
        AxePuzzleReward,
        MobileSuperX3Item,
        CogofAntiquityChest,
        LaMulana2,
        SpaulderChest,
        LightScytheItem,
        BookoftheDeadChest,
        DeathSigilChest,
        BombPuzzleReward,
        SecretTreasureofLifeItem
    }
}
