using System;
using System.Linq;
using System.Collections.Generic;
using LaMulana2Randomizer.Utils;
using LaMulana2Randomizer.ExtensionMethods;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

    public enum EchidnaType
    {
        Child,
        Teenager,
        YoungAdult,
        Adult,
        Random,
        Normal
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
        //ITEMS
        private ItemPlacement randomGrail;
        [JsonConverter(typeof(StringEnumConverter))]
        public ItemPlacement RandomGrail { 
            get=>randomGrail; 
            set =>Set(ref randomGrail, value); 
        }

        private ItemPlacement randomScanner;
        [JsonConverter(typeof(StringEnumConverter))]
        public ItemPlacement RandomScanner { 
            get=>randomScanner; 
            set=>Set(ref randomScanner, value); 
        }

        private ItemPlacement randomCodices;
        [JsonConverter(typeof(StringEnumConverter))]
        public ItemPlacement RandomCodices { 
            get=>randomCodices; 
            set=>Set(ref randomCodices, value); 
        }

        private ItemPlacement randomFDC;
        [JsonConverter(typeof(StringEnumConverter))]
        public ItemPlacement RandomFDC {
            get => randomFDC;
            set => Set(ref randomFDC, value);
        }

        private ItemPlacement randomRing;
        [JsonConverter(typeof(StringEnumConverter))]
        public ItemPlacement RandomRing {
            get => randomRing;
            set => Set(ref randomRing, value);
        }

        private ItemPlacement randomShellHorn;
        [JsonConverter(typeof(StringEnumConverter))]
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
        [JsonConverter(typeof(StringEnumConverter))]
        public MantraPlacement MantraPlacement {
            get => mantraPlacement;
            set => Set(ref mantraPlacement, value);
        }

        private ShopPlacement shopPlacement;
        [JsonConverter(typeof(StringEnumConverter))]
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

        private bool costumeClip;
        public bool CostumeClip {
            get => costumeClip;
            set => Set(ref costumeClip, value);
        }

        private bool allAccessible;
        public bool AllAccessible {
            get => allAccessible;
            set => Set(ref allAccessible, value);
        }

        private bool dlcItem;
        public bool DLCItem {
            get => dlcItem;
            set => Set(ref dlcItem, value);
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

                    DivineStart = false;
                    FrostGiantsStart = false;
                    TakaStart = false;
                    ValhallaStart = false;
                    DarkStarStart = false;
                    AncientStart = false;
                    MaliceStart = false;
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

        private bool frostGiantsStart;
        public bool FrostGiantsStart {
            get => frostGiantsStart;
            set {
                Set(ref frostGiantsStart, value);
                if (!value && NoStartsSelected)
                    VillageStart = true;
            }
        }

        private bool takaStart;
        public bool TakaStart {
            get => takaStart;
            set {
                Set(ref takaStart, value);
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

        private bool maliceStart;
        public bool MaliceStart {
            get => maliceStart;
            set {
                Set(ref maliceStart, value);
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


        private bool clayDoll;
        public bool ClayDoll {
            get => clayDoll;
            set {
                Set(ref clayDoll, value);
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

        private EchidnaType echidna;
        [JsonConverter(typeof(StringEnumConverter))]
        public EchidnaType Echidna {
            get => echidna;
            set => Set(ref echidna, value);
        }

        [JsonIgnore]
        public EchidnaType ChosenEchidna;

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
        [JsonConverter(typeof(StringEnumConverter))]
        public ChestColour ItemChestColour {
            get => itemChestColour;
            set => Set(ref itemChestColour, value);
        }

        private ChestColour weightChestColour;
        [JsonConverter(typeof(StringEnumConverter))]
        public ChestColour WeightChestColour {
            get => weightChestColour;
            set => Set(ref weightChestColour, value);
        }

        private bool NoStartsSelected {
            get => !VillageStart && !RootsStart && !AnnwfnStart && !ImmortalStart && !IcefireStart && !DivineStart 
                    && !FrostGiantsStart && !TakaStart && !ValhallaStart && !DarkStarStart && !AncientStart && !MaliceStart;
        }

        private bool NoWeaponsSelected {
            get => !Whip && !Knife && !Rapier && !Axe && !Katana && !Shuriken && !RollingShuriken && 
                        !EarthSpear && !Flare && !Chakrams && !Caltrops && !Bomb && !Pistol && !ClayDoll;
        }

        public Settings()
        {
            randomGrail = ItemPlacement.Random;
            randomScanner = ItemPlacement.Random;
            randomCodices = ItemPlacement.Random;
            randomFDC = ItemPlacement.Random;
            randomRing = ItemPlacement.Random;
            randomShellHorn = ItemPlacement.Random;
            mantraPlacement = MantraPlacement.Original;
            shopPlacement = ShopPlacement.Original;
            randomResearch = false;
            removeResearch = false;
            removeMaps = false;
            removeSkulls = false;

            fDCForBacksides = false;
            lifeForHoM = false;
            randomCurses = false;
            totalCursedChests = 4;
            removeITStatue = false;
            requiredSkulls = 12;
            randomDissonance = false;
            requiredGuardians = 5;
            costumeClip = false;
            allAccessible = true;

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
            clayDoll = false;

            hardBosses = false;
            echidna = EchidnaType.Normal;

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
            return new List<bool>() { Whip, Knife, Rapier, Axe, Katana, Shuriken, RollingShuriken, 
                                        EarthSpear, Flare, Caltrops, Chakrams, Bomb, Pistol, clayDoll};
        }

        public List<bool> GetStartingAreaChoices()
        {
            return new List<bool>() { VillageStart, RootsStart, AnnwfnStart, ImmortalStart, IcefireStart, DivineStart,
                                       FrostGiantsStart, TakaStart, ValhallaStart, DarkStarStart, AncientStart, MaliceStart };
        }

        public string GenerateSettingsString()
        {
            ulong BoolToUlong(bool value) { return value ? 1ul : 0ul; }
            void AddFlag(ulong flag, int positon, ref ulong value) { value |= flag << positon; }

            ulong part1 = 0;
            AddFlag(BoolToUlong(randomResearch), 0, ref part1);
            AddFlag(BoolToUlong(removeResearch), 1, ref part1);
            AddFlag(BoolToUlong(removeMaps), 2, ref part1);
            AddFlag(BoolToUlong(removeSkulls), 3, ref part1);
            AddFlag(BoolToUlong(fDCForBacksides), 4, ref part1);
            AddFlag(BoolToUlong(lifeForHoM), 5, ref part1);
            AddFlag(BoolToUlong(randomCurses), 6, ref part1);
            AddFlag(BoolToUlong(removeITStatue), 7, ref part1);
            AddFlag(BoolToUlong(randomDissonance), 8, ref part1);
            AddFlag(BoolToUlong(costumeClip), 9, ref part1);
            //AddFlag(BoolToUlong(crouchJump), 10, ref part1);
            AddFlag(BoolToUlong(whip), 11, ref part1);
            AddFlag(BoolToUlong(knife), 12, ref part1);
            AddFlag(BoolToUlong(rapier), 13, ref part1);
            AddFlag(BoolToUlong(axe), 14, ref part1);
            AddFlag(BoolToUlong(katana), 15, ref part1);
            AddFlag(BoolToUlong(shuriken), 16, ref part1);
            AddFlag(BoolToUlong(rollingShuriken), 17, ref part1);
            AddFlag(BoolToUlong(earthSpear), 18, ref part1);
            AddFlag(BoolToUlong(flare), 19, ref part1);
            AddFlag(BoolToUlong(caltrop), 20, ref part1);
            AddFlag(BoolToUlong(chakram), 21, ref part1);
            AddFlag(BoolToUlong(bomb), 22, ref part1);
            AddFlag(BoolToUlong(pistol), 23, ref part1);
            AddFlag(BoolToUlong(clayDoll), 24, ref part1);
            AddFlag(BoolToUlong(hardBosses), 25, ref part1);
            AddFlag(BoolToUlong(randomHorizontalEntrances), 26, ref part1);
            AddFlag(BoolToUlong(randomLadderEntrances), 27, ref part1);
            AddFlag(BoolToUlong(randomGateEntrances), 28, ref part1);
            AddFlag(BoolToUlong(fullRandomEntrances), 29, ref part1);
            AddFlag(BoolToUlong(includeUniqueTransitions), 30, ref part1);
            AddFlag(BoolToUlong(randomSoulGateEntrances), 31, ref part1);
            AddFlag(BoolToUlong(includeNineGates), 32, ref part1);
            AddFlag(BoolToUlong(randomSoulPairs), 33, ref part1);
            AddFlag(BoolToUlong(reduceDeadEndStarts), 34, ref part1);
            AddFlag(BoolToUlong(villageStart), 35, ref part1);
            AddFlag(BoolToUlong(rootsStart), 36, ref part1);
            AddFlag(BoolToUlong(annwfnStart), 37, ref part1);
            AddFlag(BoolToUlong(immortalStart), 38, ref part1);
            AddFlag(BoolToUlong(icefireStart), 39, ref part1);
            AddFlag(BoolToUlong(divineStart), 40, ref part1);
            AddFlag(BoolToUlong(valhallaStart),41, ref part1);
            AddFlag(BoolToUlong(darkStarStart), 42, ref part1);
            AddFlag(BoolToUlong(ancientStart), 43, ref part1);
            AddFlag(BoolToUlong(autoScanTablets), 44, ref part1);
            AddFlag(BoolToUlong(autoPlaceSkulls), 45, ref part1);
            AddFlag(BoolToUlong(frostGiantsStart), 46, ref part1);
            AddFlag(BoolToUlong(takaStart), 47, ref part1);
            AddFlag(BoolToUlong(maliceStart), 48, ref part1);
            AddFlag(BoolToUlong(allAccessible), 49, ref part1);

            ulong part2 = 0;
            AddFlag((ulong)randomGrail, 0, ref part2);
            AddFlag((ulong)randomScanner, 2, ref part2);
            AddFlag((ulong)randomCodices, 4, ref part2);
            AddFlag((ulong)randomFDC, 6, ref part2);
            AddFlag((ulong)randomRing, 8, ref part2);
            AddFlag((ulong)randomShellHorn, 10, ref part2);
            AddFlag((ulong)mantraPlacement, 12, ref part2);
            AddFlag((ulong)shopPlacement, 14, ref part2);
            AddFlag((ulong)totalCursedChests, 16, ref part2);
            AddFlag((ulong)requiredSkulls, 23, ref part2);
            AddFlag((ulong)requiredGuardians, 27, ref part2);
            AddFlag((ulong)echidna, 31, ref part2);
            AddFlag((ulong)itemChestColour, 34, ref part2);
            AddFlag((ulong)weightChestColour, 36, ref part2);
            AddFlag((ulong)startingMoney, 38, ref part2);
            AddFlag((ulong)startingWeights, 48, ref part2);

            return $"{part1:X16}{part2:X16}";
        }

        public void ApplySettingsString(string settingString)
        {
            List<ulong> parts = null;
            try
            {
                parts = settingString.Chunk(16).Select(x => ulong.Parse(x, System.Globalization.NumberStyles.HexNumber)).ToList();
            }
            catch(Exception e)
            {
                Logger.Log($"Failed to chunk settings string: {e.ToString()}");
                throw new RandomiserException("Invalid settings string");
            }

            if (parts.Count < 2)
                throw new RandomiserException("Invalid settings string");

            bool UintToBool(ulong value) { return (value & 1ul) == 1; }
            ulong GetFlag(int position, ulong value, ulong mask = 1ul) { return (value >> position) & mask; }

            RandomResearch = UintToBool(GetFlag(0, parts[0]));
            RemoveResearch = UintToBool(GetFlag(1, parts[0]));
            RemoveMaps = UintToBool(GetFlag(2, parts[0]));
            RemoveSkulls = UintToBool(GetFlag(3, parts[0]));
            FDCForBacksides = UintToBool(GetFlag(4, parts[0]));
            LifeForHoM = UintToBool(GetFlag(5, parts[0]));
            RandomCurses = UintToBool(GetFlag(6, parts[0]));
            RemoveITStatue = UintToBool(GetFlag(7, parts[0]));
            RandomDissonance = UintToBool(GetFlag(8, parts[0]));
            CostumeClip = UintToBool(GetFlag(9, parts[0]));
            //CrouchJump  = UintToBool(GetFlag(10, parts[0]));
            Whip = UintToBool(GetFlag(11, parts[0]));
            Knife = UintToBool(GetFlag(12, parts[0]));
            Rapier = UintToBool(GetFlag(13, parts[0]));
            Axe = UintToBool(GetFlag(14, parts[0]));
            Katana = UintToBool(GetFlag(15, parts[0]));
            Shuriken = UintToBool(GetFlag(16, parts[0]));
            RollingShuriken = UintToBool(GetFlag(17, parts[0]));
            EarthSpear = UintToBool(GetFlag(18, parts[0]));
            Flare = UintToBool(GetFlag(19, parts[0]));
            Caltrops = UintToBool(GetFlag(20, parts[0]));
            Chakrams = UintToBool(GetFlag(21, parts[0]));
            Bomb = UintToBool(GetFlag(22, parts[0]));
            Pistol = UintToBool(GetFlag(23, parts[0]));
            ClayDoll = UintToBool(GetFlag(24, parts[0]));
            HardBosses = UintToBool(GetFlag(25, parts[0]));
            RandomHorizontalEntrances = UintToBool(GetFlag(26, parts[0]));
            RandomLadderEntrances = UintToBool(GetFlag(27, parts[0]));
            RandomGateEntrances = UintToBool(GetFlag(28, parts[0]));
            FullRandomEntrances = UintToBool(GetFlag(29, parts[0]));
            IncludeUniqueTransitions = UintToBool(GetFlag(30, parts[0]));
            RandomSoulGateEntrances = UintToBool(GetFlag(31, parts[0]));
            IncludeNineGates = UintToBool(GetFlag(32, parts[0]));
            RandomSoulPairs = UintToBool(GetFlag(33, parts[0]));
            ReduceDeadEndStarts = UintToBool(GetFlag(34, parts[0]));
            VillageStart = UintToBool(GetFlag(35, parts[0]));
            RootsStart = UintToBool(GetFlag(36, parts[0]));;
            AnnwfnStart = UintToBool(GetFlag(37, parts[0]));;
            ImmortalStart = UintToBool(GetFlag(38, parts[0]));;
            IcefireStart = UintToBool(GetFlag(39, parts[0]));;
            DivineStart = UintToBool(GetFlag(40, parts[0]));;
            ValhallaStart = UintToBool(GetFlag(41, parts[0]));;
            DarkStarStart = UintToBool(GetFlag(42, parts[0]));;
            AncientStart = UintToBool(GetFlag(43, parts[0]));;
            AutoScanTablets = UintToBool(GetFlag(44, parts[0]));
            AutoPlaceSkulls = UintToBool(GetFlag(45, parts[0]));
            FrostGiantsStart = UintToBool(GetFlag(46, parts[0]));
            TakaStart = UintToBool(GetFlag(47, parts[0]));
            MaliceStart = UintToBool(GetFlag(48, parts[0]));
            AllAccessible = UintToBool(GetFlag(49, parts[0]));

            RandomGrail = (ItemPlacement)GetFlag(0, parts[1], 3ul);
            RandomScanner = (ItemPlacement)GetFlag(2, parts[1], 3ul);
            RandomCodices = (ItemPlacement)GetFlag(4, parts[1], 3ul);
            RandomFDC = (ItemPlacement)GetFlag(6, parts[1], 3ul);
            RandomRing = (ItemPlacement)GetFlag(8, parts[1], 3ul);
            RandomShellHorn = (ItemPlacement)GetFlag(10, parts[1], 3ul);
            MantraPlacement = (MantraPlacement)GetFlag(12, parts[1], 3ul);
            ShopPlacement = (ShopPlacement)GetFlag(14, parts[1], 3ul);
            TotalCursedChests = (int)GetFlag(16, parts[1], 127ul);
            RequiredSkulls = (int)GetFlag(23, parts[1], 15ul);
            RequiredGuardians = (int)GetFlag(27, parts[1], 15ul);
            Echidna = (EchidnaType)GetFlag(31, parts[1], 7ul);
            ItemChestColour = (ChestColour)GetFlag(34, parts[1], 7ul);
            WeightChestColour = (ChestColour)GetFlag(36, parts[1], 7ul);
            StartingMoney = (int)GetFlag(38, parts[1], 1023ul);
            StartingWeights = (int)GetFlag(48, parts[1], 127ul);
        }
    }
}
