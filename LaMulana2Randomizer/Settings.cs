using Newtonsoft.Json;
using System.Collections.ObjectModel;

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

        private bool hardBosses;
        public bool HardBosses { 
            get=>hardBosses; 
            set=>Set(ref hardBosses, value); 
        }

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

        private bool moneyStart;
        public bool MoneyStart {
            get => moneyStart;
            set => Set(ref moneyStart, value);
        }

        private bool weightStart;
        public bool WeightStart {
            get => weightStart;
            set => Set(ref weightStart, value);
        }

        private MantraPlacement mantraPlacement;
        public MantraPlacement MantraPlacement { 
            get=>mantraPlacement; 
            set=>Set(ref mantraPlacement, value); 
        }

        private ShopPlacement shopPlacement;
        public ShopPlacement ShopPlacement { 
            get=>shopPlacement;
            set 
            {
                Set(ref shopPlacement, value);
                if(value == ShopPlacement.Original)
                {
                    RandomScanner = false;
                    RandomCodices = false;
                    RandomFDC = false;
                    FDCForBacksides = false;
                }
             }
        }

        private ObservableCollection<bool> weapons;
        public ObservableCollection<bool> Weapons {
            get => weapons;
            set => Set(ref weapons, value);
        }

        public Settings()
        {
            RandomGrail = false;
            RandomScanner = false;
            RandomCodices = true;
            RandomFDC = true;
            randomResearch = false;
            FDCForBacksides = false;
            LifeForHoM = false;
            RandomCurses = false;
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
            HardBosses = false;
            AutoScanTablets = false;
            AutoPlaceSkulls = false;
            MantraPlacement = MantraPlacement.Original;
            ShopPlacement = ShopPlacement.Original;
            Weapons = new ObservableCollection<bool>()
            {
                true, false, false, false, false,
                false, false, false, false, false, false, false, false
            };
        }

        [JsonConstructor]
        public Settings(ObservableCollection<bool> weapons)
        {
            Weapons = weapons;
            if (Weapons == null)
            {
                Weapons = new ObservableCollection<bool>
                {
                    true
                };
            }

            while (weapons.Count < 13)
                Weapons.Add(false);
        }
    }
}
