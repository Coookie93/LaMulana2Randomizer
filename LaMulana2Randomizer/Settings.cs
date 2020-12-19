using Newtonsoft.Json;
using System.Collections.Generic;

namespace LaMulana2Randomizer
{
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

    public class Settings : BindableBase {
        private int seed;
        [JsonIgnore]
        public int Seed 
        {
            get => seed;
            set => Set(ref seed, value);
        }

        //ITEMS
        private bool randomGrail;
        public bool RandomGrail { 
            get=>randomGrail; 
            set =>Set(ref randomGrail, value); 
        }

        private bool randomScanner;
        public bool RandomScanner { 
            get=>randomScanner; 
            set=>Set(ref randomScanner, value); 
        }

        private bool randomCodices;
        public bool RandomCodices { 
            get=>randomCodices; 
            set=>Set(ref randomCodices, value); 
        }

        private bool randomFDC;
        public bool RandomFDC {
            get => randomFDC;
            set => Set(ref randomFDC, value);
        }

        private bool randomResearch;
        public bool RandomResearch {
            get => randomResearch;
            set => Set(ref randomResearch, value);
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
                    RandomScanner = false;
                    RandomCodices = false;
                    RandomFDC = false;
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

        private int requiredSkulls;
        public int RequiredSkulls {
            get => requiredSkulls;
            set => Set(ref requiredSkulls, value);
        }


        //ENTRANCES
        private bool randomHorizontalEntrances;
        public bool RandomHorizontalEntraces {
            get => randomHorizontalEntrances;
            set => Set(ref randomHorizontalEntrances, value);
        }

        private bool reduceDeadEndStarts;
        public bool ReduceDeadEndStarts {
            get => reduceDeadEndStarts;
            set => Set(ref reduceDeadEndStarts, value);
        }

        private bool randomLadderEntrances;
        public bool RandomLadderEntraces {
            get => randomLadderEntrances;
            set => Set(ref randomLadderEntrances, value);
        }

        private bool randomGateEntrances;
        public bool RandomGateEntraces {
            get => randomGateEntrances;
            set => Set(ref randomGateEntrances, value);
        }

        private bool removeITStatue;
        public bool RemoveITStatue {
            get => removeITStatue;
            set => Set(ref removeITStatue, value);
        }

        private bool fullRandomEntrances;
        public bool FullRandomEntrances {
            get => fullRandomEntrances;
            set => Set(ref fullRandomEntrances, value);
        }

        private bool includeOneWays;
        public bool IncludeUniqueTransitions {
            get => includeOneWays;
            set => Set(ref includeOneWays, value);
        }

        private bool randomSoulGateEntrances;
        public bool RandomSoulGateEntraces {
            get => randomSoulGateEntrances;
            set => Set(ref randomSoulGateEntrances, value);
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


        //COMBAT
        private bool whip;
        public bool Whip {
            get => whip;
            set => Set(ref whip, value);
        }

        private bool knife;
        public bool Knife {
            get => knife;
            set => Set(ref knife, value);
        }

        private bool rapier;
        public bool Rapier {
            get => rapier;
            set => Set(ref rapier, value);
        }

        private bool axe;
        public bool Axe {
            get => axe;
            set => Set(ref axe, value);
        }

        private bool katana;
        public bool Katana {
            get => katana;
            set => Set(ref katana, value);
        }

        private bool shuriken;
        public bool Shuriken {
            get => shuriken;
            set => Set(ref shuriken, value);
        }

        private bool rollingShuriken;
        public bool RollingShuriken {
            get => rollingShuriken;
            set => Set(ref rollingShuriken, value);
        }

        private bool earthSpear;
        public bool EarthSpear {
            get => earthSpear;
            set => Set(ref earthSpear, value);
        }

        private bool flare;
        public bool Flare {
            get => flare;
            set => Set(ref flare, value);
        }

        private bool caltrop;
        public bool Caltrops {
            get => caltrop;
            set => Set(ref caltrop, value);
        }

        private bool chakram;
        public bool Chakrams {
            get => chakram;
            set => Set(ref chakram, value);
        }

        private bool bomb;
        public bool Bomb {
            get => bomb;
            set => Set(ref bomb, value);
        }

        private bool pistol;
        public bool Pistol {
            get => pistol;
            set => Set(ref pistol, value);
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

        private bool fastCorridor;
        public bool FastCorridor {
            get => fastCorridor;
            set => Set(ref fastCorridor, value);
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

        private bool alwaysShellHorn;
        public bool AlwaysShellHorn {
            get => alwaysShellHorn;
            set => Set(ref alwaysShellHorn, value);
        }

        public Settings()
        {
            RandomGrail = false;
            RandomScanner = false;
            RandomCodices = true;
            RandomFDC = true;
            randomResearch = false;
            MantraPlacement = MantraPlacement.Original;
            ShopPlacement = ShopPlacement.Original;

            FDCForBacksides = false;
            LifeForHoM = false;
            RandomCurses = false;
            RequiredSkulls = 12;

            Whip = true;
            Knife = false;
            Rapier = false;
            Axe = false;
            Katana = false;
            Shuriken = false;
            RollingShuriken = false;
            EarthSpear = false;
            Flare = false;
            Caltrops = false;
            Chakrams = false;
            Bomb = false;
            Pistol = false;
            HardBosses = false;
            EasyEchidna = false;

            RandomHorizontalEntraces = false;
            ReduceDeadEndStarts = true;
            RandomLadderEntraces = false;
            RandomGateEntraces = false;
            RemoveITStatue = false;
            FullRandomEntrances = false;
            IncludeUniqueTransitions = false;
            RandomSoulGateEntraces = false;
            IncludeNineGates = false;
            RandomSoulPairs = false;

            AutoScanTablets = false;
            AutoPlaceSkulls = false;
            FastCorridor = false;
            StartingMoney = 0;
            StartingWeights = 0;
        }

        public List<bool> GetWeaponChoices()
        {
            return new List<bool>() { Whip, Knife, Rapier, Axe, Katana, Shuriken, 
                RollingShuriken, EarthSpear, Flare, Caltrops, Chakrams, Bomb, Pistol };
        }
    }
}
