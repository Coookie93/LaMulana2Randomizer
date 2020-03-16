using System;

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
        private int _seed;
        public int Seed 
        {
            get => _seed;
            set => Set(ref _seed, value);
        }

        private bool _randomGrail;
        public bool RandomGrail { 
            get=>_randomGrail; 
            set =>Set(ref _randomGrail, value); 
        }

        private bool _randomScanner;
        public bool RandomScanner { 
            get=>_randomScanner; 
            set=>Set(ref _randomScanner, value); 
        }

        private bool _randomCodices;
        public bool RandomCodices { 
            get=>_randomCodices; 
            set=>Set(ref _randomCodices, value); 
        }

        private bool _randomFDC;
        public bool RandomFDC {
            get => _randomFDC;
            set => Set(ref _randomFDC, value);
        }

        private bool _fDCForBacksides;
        public bool FDCForBacksides { 
            get=>_fDCForBacksides; 
            set=>Set(ref _fDCForBacksides, value); 
        }

        private bool _randomCurses;
        public bool RandomCurses {
            get => _randomCurses;
            set => Set(ref _randomCurses, value);
        }

        private bool _hardBosses;
        public bool HardBosses { 
            get=>_hardBosses; 
            set=>Set(ref _hardBosses, value); 
        }

        private bool _autScanTablets;
        public bool AutScanTablets {
            get => _autScanTablets;
            set => Set(ref _autScanTablets, value);
        }

        private MantraPlacement _mantraPlacement;
        public MantraPlacement MantraPlacement { 
            get=>_mantraPlacement; 
            set=>Set(ref _mantraPlacement, value); 
        }

        private ShopPlacement _shopPlacement;
        public ShopPlacement ShopPlacement { 
            get=>_shopPlacement;
            set 
            {
                Set(ref _shopPlacement, value);
                if(value == ShopPlacement.Original)
                {
                    RandomScanner = false;
                    RandomCodices = false;
                    RandomFDC = false;
                    FDCForBacksides = false;
                }
             }
        }

        public Settings()
        {
            Seed = new Random().Next(int.MinValue, int.MaxValue);
            RandomGrail = false;
            RandomScanner = false;
            RandomCodices = true;
            RandomFDC = true;
            FDCForBacksides = false;
            HardBosses = false;
            AutScanTablets = false;
            MantraPlacement = MantraPlacement.Original;
            ShopPlacement = ShopPlacement.Original;
        }
    }
}
