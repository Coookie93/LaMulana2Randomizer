using Newtonsoft.Json;
using System.Collections.Generic;

namespace LaMulana2Randomizer
{
    public enum ItemPlacement
    {
        Random,
        AvailableAtStart,
        Starting
    }

    public enum ShopPlacement
    {
        Random,
        AtLeastOne,
        Original
    }

    public enum MantraPlacement
    {
        Random,
        OnlyMurals,
        Original
    }

    public enum ChestColour
    {
        Blue,
        Turquise,
        Red,
        Pink,
        Yellow
    }

    public class Settings : BindableBase {
        private int seed;
        [JsonIgnore]
        public int Seed 
        {
            get => seed;
            set => Set(ref seed, value);
        }

        //ITEMS
        private ItemPlacement randomGrail;
        public ItemPlacement RandomGrail { 
            get=>randomGrail; 
            set =>Set(ref randomGrail, value); 
        }

        private ItemPlacement randomScanner;
        public ItemPlacement RandomScanner { 
            get=>randomScanner; 
            set=>Set(ref randomScanner, value); 
        }

        private ItemPlacement randomCodices;
        public ItemPlacement RandomCodices { 
            get=>randomCodices; 
            set=>Set(ref randomCodices, value); 
        }

        private ItemPlacement randomFDC;
        public ItemPlacement RandomFDC {
            get => randomFDC;
            set => Set(ref randomFDC, value);
        }

        private ItemPlacement randomRing;
        public ItemPlacement RandomRing {
            get => randomRing;
            set => Set(ref randomRing, value);
        }

        private ItemPlacement randomShellHorn;
        public ItemPlacement RandomShellHorn {
            get => randomShellHorn;
            set => Set(ref randomShellHorn, value);
        }

        private bool randomResearch;
        public bool RandomResearch {
            get => randomResearch;
            set { 
                Set(ref randomResearch, value);
                if (!value)
                    RemoveResearch = false;
            }
        }

        private bool removeResearch;
        public bool RemoveResearch {
            get => removeResearch;
            set => Set(ref removeResearch, value);
        }

        private bool removeMaps;
        public bool RemoveMaps {
            get => removeMaps;
            set => Set(ref removeMaps, value);
        }

        private bool removeSkulls;
        public bool RemoveSkulls {
            get => removeSkulls;
            set => Set(ref removeSkulls, value);
        }

        private MantraPlacement mantraPlacement;
        public MantraPlacement MantraPlacement {
            get => mantraPlacement;
            set => Set(ref mantraPlacement, value);
        }

        private ShopPlacement shopPlacement;
        public ShopPlacement ShopPlacement {
            get => shopPlacement;
            set {
                Set(ref shopPlacement, value);
                if (value == ShopPlacement.Original)
                {
                    RandomScanner = ItemPlacement.Random;
                    RandomFDC = ItemPlacement.Random;
                    RandomCodices = ItemPlacement.Random;
                    FDCForBacksides = false;
                }
            }
        }

        //LOGIC
        private bool fDCForBacksides;
        public bool FDCForBacksides { 
            get=>fDCForBacksides; 
            set=>Set(ref fDCForBacksides, value); 
        }

        private bool lifeForHoM;
        public bool LifeForHoM {
            get => lifeForHoM;
            set => Set(ref lifeForHoM, value);
        }

        private bool randomCurses;
        public bool RandomCurses {
            get => randomCurses;
            set => Set(ref randomCurses, value);
        }

        private int totalCursedChests;
        public int TotalCursedChests {
            get => totalCursedChests;
            set => Set(ref totalCursedChests, value);
        }

        private bool removeITStatue;
        public bool RemoveITStatue {
            get => removeITStatue;
            set => Set(ref removeITStatue, value);
        }

        private int requiredSkulls;
        public int RequiredSkulls {
            get => requiredSkulls;
            set => Set(ref requiredSkulls, value);
        }

        private bool randomDissonance;
        public bool RandomDissonance {
            get => randomDissonance;
            set => Set(ref randomDissonance, value);
        }


        private int requiredGuardians;
        public int RequiredGuardians {
            get => requiredGuardians;
            set => Set(ref requiredGuardians, value);
        }

        //LAYOUT
        private bool randomHorizontalEntrances;
        public bool RandomHorizontalEntrances {
            get => randomHorizontalEntrances;
            set {
                Set(ref randomHorizontalEntrances, value);
                if(!value)
                {
                    if (!RandomGateEntrances && !RandomLadderEntrances)
                        FullRandomEntrances = false;
                }
            }
        }

        private bool randomLadderEntrances;
        public bool RandomLadderEntrances {
            get => randomLadderEntrances;
            set {
                Set(ref randomLadderEntrances, value);
                if(!value)
                {
                    if (!RandomGateEntrances && !RandomHorizontalEntrances)
                        FullRandomEntrances = false;

                    if(!RandomGateEntrances)
                        IcefireStart = false;
                }
            }
        }

        private bool randomGateEntrances;
        public bool RandomGateEntrances {
            get => randomGateEntrances;
            set { 
                Set(ref randomGateEntrances, value); 
                if(!value)
                {
                    if (!RandomLadderEntrances && !RandomHorizontalEntrances)
                        FullRandomEntrances = false;

                    if (!RandomLadderEntrances)
                        IcefireStart = false;

                    DivineStart = false;
                    ValhallaStart = false;
                    DarkStarStart = false;
                    AncientStart = false;
                }
            }
        }

        private bool randomSoulGateEntrances;
        public bool RandomSoulGateEntrances {
            get => randomSoulGateEntrances;
            set { 
                Set(ref randomSoulGateEntrances, value); 
                if(!value)
                {
                    IncludeNineGates = false;
                    RandomSoulPairs = false;
                }
            }
        }

        private bool includeNineGates;
        public bool IncludeNineGates {
            get => includeNineGates;
            set => Set(ref includeNineGates, value);
        }

        private bool randomSoulPairs;
        public bool RandomSoulPairs {
            get => randomSoulPairs;
            set => Set(ref randomSoulPairs, value);
        }

        private bool fullRandomEntrances;
        public bool FullRandomEntrances {
            get => fullRandomEntrances;
            set {
                Set(ref fullRandomEntrances, value);
                if (!value)
                {
                    IncludeUniqueTransitions = false;
                }
            }
        }

        private bool includeUniqueTransitions;
        public bool IncludeUniqueTransitions {
            get => includeUniqueTransitions;
            set => Set(ref includeUniqueTransitions, value); 
        }

        private bool reduceDeadEndStarts;
        public bool ReduceDeadEndStarts {
            get => reduceDeadEndStarts;
            set => Set(ref reduceDeadEndStarts, value);
        }

        private bool rootsStart;
        public bool RootsStart {
            get => rootsStart;
            set { 
                Set(ref rootsStart, value);
                if (!value && NoStartsSelected)
                    VillageStart = true;
            }
        }

        private bool annwfnStart;
        public bool AnnwfnStart {
            get => annwfnStart;
            set {
                Set(ref annwfnStart, value);
                if (!value && NoStartsSelected)
                    VillageStart = true;
            }
        }

        private bool immortalStart;
        public bool ImmortalStart {
            get => immortalStart;
            set { 
                Set(ref immortalStart, value);
                if (!value && NoStartsSelected)
                    VillageStart = true;
            }
        }

        private bool icefireStart;
        public bool IcefireStart {
            get => icefireStart;
            set {
                Set(ref icefireStart, value);
                if (!value && NoStartsSelected)
                    VillageStart = true;
            }
        }

        private bool divineStart;
        public bool DivineStart {
            get => divineStart;
            set {
                Set(ref divineStart, value);
                if (!value && NoStartsSelected)
                    VillageStart = true;
            }
        }

        private bool valhallaStart;
        public bool ValhallaStart {
            get => valhallaStart;
            set { 
                Set(ref valhallaStart, value); 
                if (!value && NoStartsSelected)
                    VillageStart = true;
            }
        }

        private bool darkStarStart;
        public bool DarkStarStart {
            get => darkStarStart;
            set { 
                Set(ref darkStarStart, value);
                if (!value && NoStartsSelected)
                    VillageStart = true;
            }
        }

        private bool ancientStart;
        public bool AncientStart {
            get => ancientStart;
            set { 
                Set(ref ancientStart, value);
                if (!value && NoStartsSelected)
                    VillageStart = true;
            }
        }

        private bool villageStart;
        public bool VillageStart {
            get => villageStart;
            set => Set(ref villageStart, value);
        }


        //COMBAT
        private bool knife;
        public bool Knife {
            get => knife;
            set {
                Set(ref knife, value);
                if(!value && NoWeaponsSelected)
                    Whip = true;
            }
        }

        private bool rapier;
        public bool Rapier {
            get => rapier;
            set {
                Set(ref rapier, value);
                if (!value && NoWeaponsSelected)
                    Whip = true;
            }
        }

        private bool axe;
        public bool Axe {
            get => axe;
            set {
                Set(ref axe, value);
                if (!value && NoWeaponsSelected)
                    Whip = true;
            }
        }

        private bool katana;
        public bool Katana {
            get => katana;
            set {
                Set(ref katana, value);
                if (!value && NoWeaponsSelected)
                    Whip = true;
            }
        }

        private bool shuriken;
        public bool Shuriken {
            get => shuriken;
            set {
                Set(ref shuriken, value);
                if (!value && NoWeaponsSelected)
                    Whip = true;
            }
        }

        private bool rollingShuriken;
        public bool RollingShuriken {
            get => rollingShuriken;
            set {
                Set(ref rollingShuriken, value);
                if (!value && NoWeaponsSelected)
                    Whip = true;
            }
        }

        private bool earthSpear;
        public bool EarthSpear {
            get => earthSpear;
            set {
                Set(ref earthSpear, value);
                if (!value && NoWeaponsSelected)
                    Whip = true;
            }
        }

        private bool flare;
        public bool Flare {
            get => flare;
            set {
                Set(ref flare, value);
                if (!value && NoWeaponsSelected)
                    Whip = true;
            }
        }

        private bool caltrop;
        public bool Caltrops {
            get => caltrop;
            set {
                Set(ref caltrop, value);
                if (!value && NoWeaponsSelected)
                    Whip = true;
            }
        }

        private bool chakram;
        public bool Chakrams {
            get => chakram;
            set {
                Set(ref chakram, value);
                if (!value && NoWeaponsSelected)
                    Whip = true;
            }
        }

        private bool bomb;
        public bool Bomb {
            get => bomb;
            set {
                Set(ref bomb, value);
                if (!value && NoWeaponsSelected)
                    Whip = true;
            }
        }

        private bool pistol;
        public bool Pistol {
            get => pistol;
            set {
                Set(ref pistol, value);
                if (!value && NoWeaponsSelected)
                    Whip = true;
            }
        }

        private bool whip;
        public bool Whip {
            get => whip;
            set => Set(ref whip, value);
        }

        private bool hardBosses;
        public bool HardBosses { 
            get=>hardBosses; 
            set=>Set(ref hardBosses, value); 
        }

        private bool easyEchidna;
        public bool EasyEchidna {
            get => easyEchidna;
            set => Set(ref easyEchidna, value);
        }

        //OTHER
        private bool autoScanTablets;
        public bool AutoScanTablets {
            get => autoScanTablets;
            set => Set(ref autoScanTablets, value);
        }

        private bool autoPlaceSkulls;
        public bool AutoPlaceSkulls {
            get => autoPlaceSkulls;
            set => Set(ref autoPlaceSkulls, value);
        }

        private int startingMoney;
        public int StartingMoney {
            get => startingMoney;
            set => Set(ref startingMoney, value);
        }

        private int startingWeights;
        public int StartingWeights {
            get => startingWeights;
            set => Set(ref startingWeights, value);
        }

        private ChestColour itemChestColour;
        public ChestColour ItemChestColour {
            get => itemChestColour;
            set => Set(ref itemChestColour, value);
        }

        private ChestColour weightChestColour;
        public ChestColour WeightChestColour {
            get => weightChestColour;
            set => Set(ref weightChestColour, value);
        }

        private bool NoStartsSelected {
            get => !VillageStart && !RootsStart && !AnnwfnStart && !ImmortalStart && !IcefireStart && 
                        !DivineStart && !ValhallaStart && !DarkStarStart && !AncientStart;
        }

        private bool NoWeaponsSelected {
            get => !Whip && !Knife && !Rapier && !Axe && !Katana && !Shuriken && !RollingShuriken && 
                        !EarthSpear && !Flare && !Chakrams && !Caltrops && !Bomb && !Pistol;
        }

        public Settings()
        {
            randomGrail = ItemPlacement.Random;
            randomScanner = ItemPlacement.Random;
            randomCodices = ItemPlacement.Random;
            randomFDC = ItemPlacement.Random;
            randomRing = ItemPlacement.Random;
            randomShellHorn = ItemPlacement.Random;
            randomResearch = false;
            mantraPlacement = MantraPlacement.Original;
            shopPlacement = ShopPlacement.Original;

            fDCForBacksides = false;
            lifeForHoM = false;
            randomCurses = false;
            totalCursedChests = 4;
            removeITStatue = false;
            requiredSkulls = 12;
            randomDissonance = false;
            requiredGuardians = 5;

            whip = true;
            knife = false;
            rapier = false;
            axe = false;
            katana = false;
            shuriken = false;
            rollingShuriken = false;
            earthSpear = false;
            flare = false;
            caltrop = false;
            chakram = false;
            bomb = false;
            pistol = false;

            hardBosses = false;
            easyEchidna = false;

            randomHorizontalEntrances = false;
            randomLadderEntrances = false;
            randomGateEntrances = false;
            fullRandomEntrances = false;
            includeUniqueTransitions = false;
            randomSoulGateEntrances = false;
            includeNineGates = false;
            randomSoulPairs = false;
            reduceDeadEndStarts = false;

            villageStart = true;
            rootsStart = false;
            annwfnStart = false;
            immortalStart = false;
            icefireStart = false;
            divineStart = false;
            valhallaStart = false;
            darkStarStart = false;
            ancientStart = false;

            autoScanTablets = false;
            autoPlaceSkulls = false;
            startingMoney = 100;
            startingWeights = 20;

            itemChestColour = ChestColour.Blue;
            weightChestColour = ChestColour.Blue;
        }

        public List<bool> GetWeaponChoices()
        {
            return new List<bool>() { Whip, Knife, Rapier, Axe, Katana, Shuriken, 
                RollingShuriken, EarthSpear, Flare, Caltrops, Chakrams, Bomb, Pistol };
        }

        public List<bool> GetStartingAreaChoices()
        {
            return new List<bool>() { VillageStart, RootsStart, AnnwfnStart, ImmortalStart, IcefireStart, 
                                        DivineStart, ValhallaStart, DarkStarStart, AncientStart };
        }
    }
}
