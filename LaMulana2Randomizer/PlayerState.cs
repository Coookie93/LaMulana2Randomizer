using System.Linq;
using System.Collections.Generic;
using LaMulana2Randomizer.Utils;
using LaMulana2Randomizer.LogicParsing;
using LaMulana2RandomizerShared;

namespace LaMulana2Randomizer
{
    public class PlayerState
    {
        private readonly string[] bossNames = {"Fafnir", "Surtr", "Vritra", "Kujata", "Aten-Ra", "Jormangund", "Anu", "Echidna", "Hel"};

        private readonly Randomiser randomiser;
        private readonly Dictionary<string, bool> collectedLocations;
        private readonly Dictionary<string, int> collectedItems;
        
        private Dictionary<string, bool> areaChecks;
        private Dictionary<string, bool> entraceChecks;

        public bool SoftlockCheck;
        public bool IgnoreFalseChecks;
        public bool EscapeCheck;
        public Area StartingArea;

        public PlayerState(Randomiser randomiser)
        {
            this.randomiser = randomiser;
            StartingArea = randomiser.GetArea("Village of Departure");
            areaChecks =  new Dictionary<string, bool>();
            entraceChecks = new Dictionary<string, bool>();
            collectedLocations = new Dictionary<string, bool>();
            collectedItems = new Dictionary<string, int>();
        }
        
        public static PlayerState GetStateWithItems(Randomiser randomiser, ItemPool currentItems)
        {
            PlayerState state = new PlayerState(randomiser);
            state.CollectItem(randomiser.StartingWeapon);

            foreach (Item item in currentItems)
                state.CollectItem(item);

            List<Location> requiredLocations = randomiser.GetPlacedRequiredItemLocations();
            List<Location> reachableLocations;
            do
            {
                reachableLocations = state.GetReachableLocations(requiredLocations);
                foreach (Location location in reachableLocations)
                {
                    state.CollectItem(location.Item);
                    state.collectedLocations.Add(location.Name, true);
                }

                state.RemoveFalseCheckedAreasAndEntrances();

            } while (reachableLocations.Count > 0);

            return state;
        }

        public static bool CanBeatGame(Randomiser randomiser)
        {
            PlayerState state = new PlayerState(randomiser);
            state.CollectItem(randomiser.StartingWeapon);

            List<Location> requiredLocations = randomiser.GetPlacedRequiredItemLocations();
            List<Location> reachableLocations;
            do
            {
                reachableLocations = state.GetReachableLocations(requiredLocations);
                foreach (Location location in reachableLocations)
                {
                    state.CollectItem(location.Item);
                    state.collectedLocations.Add(location.Name, true);
                }

                state.RemoveFalseCheckedAreasAndEntrances();

            } while (reachableLocations.Count > 0);

            return state.CanBeatGame();
        }

        public static bool AnkhSoftlockCheck(Randomiser randomiser)
        {
            foreach (Location guardianToSkip in randomiser.GetPlacedLocationsOfType(LocationType.Guardian))
            {
                PlayerState state = new PlayerState(randomiser)
                {
                    SoftlockCheck = true
                };
                state.CollectItem(randomiser.StartingWeapon);

                int guardiansEncountered = 0;
                List<Location> requiredLocations = randomiser.GetPlacedRequiredItemLocations();

                List<Location> reachableLocations;
                do
                {
                    reachableLocations = state.GetReachableLocations(requiredLocations);
                    foreach (Location location in reachableLocations)
                    {
                        state.collectedLocations.Add(location.Name, true);
                        if (location.LocationType == LocationType.Guardian)
                        {
                            if (!location.Equals(guardianToSkip))
                            {
                                state.CollectItem(location.Item);
                                guardiansEncountered++;
                            }
                        }
                        else
                        {
                            state.CollectItem(location.Item);
                        }
                    }

                    state.RemoveFalseCheckedAreasAndEntrances();

                } while (reachableLocations.Count > 0);

                if (guardiansEncountered >= state.collectedItems["Ankh Jewel"])
                    return false;
            }
            return true;
        }

        public static bool EntrancePlacementCheck(Randomiser randomiser)
        {
            PlayerState state = new PlayerState(randomiser);
            ItemPool itemPool = new ItemPool(FileUtils.LoadItemFile());

            if (randomiser.Settings.ShopPlacement == ShopPlacement.Original)
                randomiser.PlaceShopItems(itemPool);

            if (randomiser.Settings.MantraPlacement == MantraPlacement.Original)
                randomiser.PlaceMantras(itemPool);

            foreach (Item item in itemPool)
                state.CollectItem(item);

            List<Location> requiredLocations = randomiser.GetPlacedLocations();
            List<Location> reachableLocations;
            do
            {
                reachableLocations = state.GetReachableLocations(requiredLocations);
                foreach (Location location in reachableLocations)
                {
                    state.CollectItem(location.Item);
                    state.collectedLocations.Add(location.Name, true);
                }

                state.RemoveFalseCheckedAreasAndEntrances();

            } while (reachableLocations.Count > 0);

            //check to see if its possible to beat the game with this configuration
            if (!state.CanBeatGame())
                return false;

            //check to see if all locations are accessable
            foreach (Location location in randomiser.GetLocations())
            {
                if (!location.CanReach(state))
                    return false;
            }

            //check to see if its possible to actually escape
            state.EscapeCheck = true;
            state.StartingArea = randomiser.GetArea("Immortal Battlefield Main");
            List<string> exitNames = new List<string>() { "Cliff", "Gate of Guidance", "Mausoleum of Giants", "Village of Departure", "Gate of Illusion", "Nibiru"};
            foreach(string exitName in exitNames)
            {
                state.ClearCheckedAreasAndEntrances();
                if (state.CanReach(exitName))
                    return true;
            }
            return false;
        }

        public bool CanBeatGame()
        {
            return HasItem("Winner") && CanReach("Cliff");
        }

        public bool CanReach(string areaName)
        {
            Area area = randomiser.GetArea(areaName);
            if (area.Equals(StartingArea))
                return true;

            if (areaChecks.TryGetValue(area.Name, out bool cached))
                return cached;

            if (area.Checking)
                return false;

            area.Checking = true;
            bool canReach = area.CanReach(this);
            area.Checking = false;

            //when writing the spoiler log playthrough only care about caching areas that can be reached, caching areas that can't
            //be reached sometimes leads to the playthorugh being incorrect
            if (IgnoreFalseChecks && canReach)
                areaChecks.Add(area.Name, canReach);
            else
                areaChecks.Add(area.Name, canReach);

            return canReach;
        }

        public bool CanReach(Exit entrance)
        {
            if (entraceChecks.TryGetValue(entrance.Name, out bool cached))
                return cached;

            if (entrance.Checking)
                return false;

            entrance.Checking = true;
            bool canReach = entrance.CanReach(this);
            entrance.Checking = false;

            //when writing the spoiler log playthrough only care about caching entrances that can be reached, caching entrances that can't
            //be reached sometimes leads to the playthorugh being incorrect
            if (IgnoreFalseChecks && canReach)
                entraceChecks.Add(entrance.Name, canReach);
            else
                entraceChecks.Add(entrance.Name, canReach);

            return canReach;
        }

        public void CollectItem(Item item)
        {
            if (item == null)
                return;

            if (collectedItems.ContainsKey(item.Name))
                collectedItems[item.Name]++;
            else
                collectedItems.Add(item.Name, 1);

            if (bossNames.Contains(item.Name))
            {
                if (collectedItems.ContainsKey("Guardians"))
                    collectedItems["Guardians"]++;
                else
                    collectedItems.Add("Guardians", 1);
            }
        }

        public void CollectItem(string itemName)
        {
            if (collectedItems.ContainsKey(itemName))
                collectedItems[itemName]++;
            else
                collectedItems.Add(itemName, 1);
        }

        public void CollectLocation(Location location)
        {
            collectedLocations.Add(location.Name, true);
        }

        public bool Evaluate(Logic rule)
        {
            switch (rule.logicType)
            {
                case LogicType.CanReach: return CanReach(rule.value);
                case LogicType.CanChant: return CanChant(rule.value);
                case LogicType.CanWarp: return HasItem("Holy Grail");
                case LogicType.CanSpinCorridor: return CanSpinCorridor();
                case LogicType.CanStopTime: return CanStopTime();
                case LogicType.Has: return HasItem(rule.value);
                case LogicType.CanUse: return CanUse(rule.value);
                case LogicType.MeleeAttack: return MeleeAttack();
                case LogicType.HorizontalAttack: return HorizontalAttack();
                case LogicType.IsDead: return HasItem(rule.value);
                case LogicType.CanKill: return CanKill(rule.value);
                case LogicType.OrbCount: return OrbCount(int.Parse(rule.value));
                case LogicType.GuardianKills: return GuardianKills(int.Parse(rule.value));
                case LogicType.PuzzleFinished: return HasItem(rule.value);
                case LogicType.AnkhCount: return SoftlockCheck ? AnkhCountSoftLock() : AnkhCount(int.Parse(rule.value));
                case LogicType.Dissonance: return Dissonance(int.Parse(rule.value));
                case LogicType.SkullCount: return SkullCount(int.Parse(rule.value));
                case LogicType.Setting: return Settings(rule.value);
                case LogicType.True: return true;
                case LogicType.False: return false;
                default: return false;
            }
        }

        public void RemoveFalseCheckedAreasAndEntrances()
        {
            //reset areas
            Dictionary<string, bool> temp = new Dictionary<string, bool>();
            foreach (var area in areaChecks)
            {
                if (area.Value)
                    temp.Add(area.Key, area.Value);
            }
            areaChecks = temp;

            //reset entrances
            temp = new Dictionary<string, bool>();
            foreach (var entrance in entraceChecks)
            {
                if (entrance.Value)
                    temp.Add(entrance.Key, entrance.Value);
            }
            entraceChecks = temp;
        }

        public void ClearCheckedAreasAndEntrances()
        {
            areaChecks.Clear();
            entraceChecks.Clear();
        }

        public List<Location> GetReachableLocations(List<Location> requiredLocations)
        {
            List<Location> locations = new List<Location>();

            foreach(Location location in requiredLocations)
            {
                if(!collectedLocations.ContainsKey(location.Name) && location.CanReach(this))
                    locations.Add(location);
            }
            return locations;
        }
        
        private bool HasItem(string itemName)
        {
            if (itemName.Contains("Whip"))
            {
                if(collectedItems.TryGetValue("Progressive Whip", out int value))
                {
                    if (itemName.Equals("Leather Whip")) return value >= 1;
                    else if (itemName.Equals("Chain Whip")) return value >= 2;
                    else if (itemName.Equals("Flail Whip")) return value >= 3;
                }
                return false;
            }
            else if (itemName.Contains("Shield") || itemName.Equals("Buckler"))
            {
                if (collectedItems.TryGetValue("Progressive Shield", out int value))
                {
                    if (itemName.Equals("Buckler")) return value >= 1;
                    else if (itemName.Equals("Silver Shield")) return value >= 2;
                    else if (itemName.Equals("Angel Shield")) return value >= 3;
                }
                return false;
            }
            else
            {
                return collectedItems.ContainsKey(itemName);
            }
        } 
        
        private bool CanChant(string mantra)
        {
            return HasItem("Djed Pillar") && HasItem("Mantra") && HasItem(mantra);
        }

        private bool CanUse(string subWeapon)
        {
            if (subWeapon.Equals("Pistol"))
                return HasItem(subWeapon) && HasItem(subWeapon + " Ammo") && (HasItem("Money Fairy") || randomiser.StartingWeaponID == ItemID.Pistol);
            else
                return HasItem(subWeapon) && HasItem(subWeapon + " Ammo");
        }

        private bool MeleeAttack()
        {
            return HasItem("Leather Whip") || HasItem("Chain Whip") || HasItem("Flail Whip") || HasItem("Knife") 
                    || HasItem("Rapier") || HasItem("Axe") || HasItem("Katana");
        }

        private bool HorizontalAttack()
        {
            return HasItem("Leather Whip") || HasItem("Chain Whip") || HasItem("Flail Whip") || HasItem("Knife") || HasItem("Rapier") || HasItem("Axe") 
                    || HasItem("Katana") || CanUse("Shuriken") || CanUse("Rolling Shuriken") || CanUse("Earth Spear") || CanUse("Caltrops") || CanUse("Chakram") 
                    || CanUse("Bomb") || CanUse("Pistol") || HasItem("Claydoll Suit");
        }

        private bool CanSpinCorridor()
        {
            return HasItem("Beherit") && Dissonance(1);
        }

        private bool CanStopTime()
        {
            if(collectedItems.ContainsKey("Lamp of Time"))
                return CanReach("Roots of Yggdrasil") || CanReach("Immortal Battlefield Main") || CanReach("Icefire Treetop Left") || CanReach("Dark Lords Mausoleum Main");
            
            return false;
        }

        private bool CanKill(string enemy)
        {
            return randomiser.GetLocation(enemy).LogicTree.Evaluate(this);
        }

        private bool GuardianKills(int count)
        {
            if (collectedItems.TryGetValue("Guardians", out int num))
                return num >= count;
            
            return false;
        }

        private bool OrbCount(int count)
        {
            if (collectedItems.TryGetValue("Sacred Orb", out int num))
                return num >= count;
            
            return false;
        }

        private bool AnkhCount(int count)
        {
            if (collectedItems.TryGetValue("Ankh Jewel", out int num))
                return num >= count;
            
            return false;
        }

        private bool AnkhCountSoftLock()
        {
            return collectedItems.ContainsKey("Ankh Jewel");
        }

        private bool SkullCount(int count)
        {
            if (collectedItems.TryGetValue("Crystal Skull", out int num))
                return num >= count;
            
            return false;
        }

        private bool Dissonance(int count)
        {
            if (collectedItems.TryGetValue("Dissonance", out int num))
                return num >= count;
            
            return false;
        }

        private bool Settings(string settingName)
        {
            switch (settingName)
            {
                case "AutoScan": return randomiser.Settings.AutoScanTablets;
                case "Random Ladders": return randomiser.Settings.RandomLadderEntraces;
                case "Non Random Ladders": return !randomiser.Settings.RandomLadderEntraces;
                case "Random Gates": return randomiser.Settings.RandomGateEntraces;
                case "Non Random Gates": return !randomiser.Settings.RandomGateEntraces;
                case "Random Soul Gates": return randomiser.Settings.RandomSoulGateEntraces;
                case "Non Random Soul Gates": return !randomiser.Settings.RandomSoulGateEntraces;
                case "Remove IT Statue": return randomiser.Settings.RemoveITStatue;
                case "Not Life for HoM": return !randomiser.Settings.LifeForHoM;
                default: return false;
            }
        }
    }
}
