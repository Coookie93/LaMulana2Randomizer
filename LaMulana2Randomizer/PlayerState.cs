using System.Linq;
using System.Collections.Generic;
using LaMulana2Randomizer.Utils;
using LaMulana2Randomizer.LogicParsing;

namespace LaMulana2Randomizer
{
    public class PlayerState
    {
        private readonly string[] bossNames = {"Fafnir", "Surtr", "Vritra", "Kujata", "Aten-Ra", "Jormangund", "Anu", "Echidna", "Hel"};

        private readonly Randomiser randomiser;
        private readonly bool softlockCheck;
        private readonly bool ignoreFalseChecks;
        private readonly Dictionary<string, bool> collectedLocations;
        private readonly Dictionary<string, int> collectedItems;
        
        private Dictionary<string, bool> areaChecks;
        private Dictionary<string, bool> entraceChecks;

        public PlayerState(Randomiser randomiser, bool softlockCheck = false, bool ignoreFalseChecks = false)
        {
            this.randomiser = randomiser;
            this.softlockCheck = softlockCheck;
            this.ignoreFalseChecks = ignoreFalseChecks;
            areaChecks =  new Dictionary<string, bool>();
            entraceChecks = new Dictionary<string, bool>();
            collectedLocations = new Dictionary<string, bool>();
            collectedItems = new Dictionary<string, int>();
        }
        
        public static PlayerState GetStateWithItems(Randomiser randomiser, List<Item> currentItems)
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

                state.ResetCheckedAreasAndEntrances();

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

                state.ResetCheckedAreasAndEntrances();

            } while (reachableLocations.Count > 0);

            return state.CanBeatGame();
        }

        public static bool AnkhSoftlockCheck(Randomiser randomiser)
        {
            foreach (Location guardianToSkip in randomiser.GetPlacedLocationsOfType(LocationType.Guardian))
            {
                PlayerState state = new PlayerState(randomiser, true);
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

                    state.ResetCheckedAreasAndEntrances();

                } while (reachableLocations.Count > 0);

                if (guardiansEncountered >= state.collectedItems["Ankh Jewel"])
                    return false;
            }
            return true;
        }

        public static bool EntrancePlacementCheck(Randomiser randomiser)
        {
            PlayerState state = new PlayerState(randomiser);
            foreach (Item item in FileUtils.GetItemsFromJson())
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

                state.ResetCheckedAreasAndEntrances();

            } while (reachableLocations.Count > 0);

            return state.CanBeatGame();
        }

        public bool CanBeatGame()
        {
            return HasItem("Winner");
        }

        public bool CanReach(string areaName)
        {
            Area area = randomiser.GetArea(areaName);
            if (area.Equals(randomiser.GetArea("Village of Departure")))
                return true;

            if (areaChecks.TryGetValue(area.Name, out bool cached))
                return cached;

            if (area.Checking)
            {
                return false;
            }
            area.Checking = true;
            bool canReach = area.CanReach(this);
            area.Checking = false;

            //when writing the spoiler log playthrough only care about caching areas that can be reached, caching areas that can't
            //be reached sometimes leads to the playthorugh being incorrect
            if (ignoreFalseChecks && canReach)
            {
                areaChecks.Add(area.Name, canReach);
            }
            else
            {
                areaChecks.Add(area.Name, canReach);
            }

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
            if (ignoreFalseChecks && canReach)
            {
                entraceChecks.Add(entrance.Name, canReach);
            }
            else
            {
                entraceChecks.Add(entrance.Name, canReach);
            }

            return canReach;
        }

        public void CollectItem(Item item)
        {
            if (collectedItems.ContainsKey(item.Name))
            {
                collectedItems[item.Name]++;
            }
            else
            {
                collectedItems.Add(item.Name, 1);
            }

            if (bossNames.Contains(item.Name))
            {
                if (collectedItems.ContainsKey("Guardians"))
                {
                    collectedItems["Guardians"]++;
                }
                else
                {
                    collectedItems.Add("Guardians", 1);
                }
            }
        }

        public void CollectItem(string itemName)
        {
            if (collectedItems.ContainsKey(itemName))
            {
                collectedItems[itemName]++;
            }
            else
            {
                collectedItems.Add(itemName, 1);
            }
        }

        public void CollectLocation(Location location)
        {
            collectedLocations.Add(location.Name, true);
        }

        public bool Evaluate(Logic rule)
        {
            switch (rule.logicType)
            {
                case LogicType.CanReach:
                    return CanReach(rule.value);

                case LogicType.CanChant:
                    return CanChant(rule.value);

                case LogicType.CanWarp:
                    return HasItem("Holy Grail");

                case LogicType.CanSpinCorridor:
                    return CanSpinCorridor();

                case LogicType.CanStopTime:
                    return CanStopTime();

                case LogicType.Has:
                    return HasItem(rule.value);

                case LogicType.CanUse:
                    return CanUse(rule.value);

                case LogicType.IsDead:
                    return HasItem(rule.value);

                case LogicType.CanKill:
                    return CanKill(rule.value);

                case LogicType.OrbCount:
                    return OrbCount(int.Parse(rule.value));

                case LogicType.GuardianKills:
                    return GuardianKills(int.Parse(rule.value));

                case LogicType.PuzzleFinished:
                    return HasItem(rule.value);

                case LogicType.AnkhCount:
                    return softlockCheck ? AnkhCountSoftLock() : AnkhCount(int.Parse(rule.value));

                case LogicType.Dissonance:
                    return Dissonance(int.Parse(rule.value));

                case LogicType.SkullCount:
                    return SkullCount(int.Parse(rule.value));

                case LogicType.Setting:
                    return Settings(rule.value);

                case LogicType.True:
                    return true;

                case LogicType.False:
                    return false;

                default:
                    return false;
            }
        }
        
        public void ResetCheckedAreasAndEntrances()
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

        public List<Location> GetReachableLocations(List<Location> requiredLocations)
        {
            List<Location> locations = new List<Location>();

            foreach(Location location in requiredLocations)
            {
                if(!collectedLocations.ContainsKey(location.Name) && location.CanReach(this))
                {
                    locations.Add(location);
                }
            }
            return locations;
        }
        
        private bool HasItem(string itemName)
        {
            return collectedItems.ContainsKey(itemName);
        } 
        
        private bool CanChant(string mantra)
        {
            return HasItem("Djed Pillar") && HasItem("Mantra") && HasItem(mantra);
        }

        private bool CanUse(string subWeapon)
        {
            return HasItem(subWeapon) && HasItem(subWeapon + " Ammo");
        }
        
        private bool CanSpinCorridor()
        {
            return HasItem("Beherit") && Dissonance(1);
        }

        private bool CanStopTime()
        {
            if(collectedItems.ContainsKey("Lamp of Time"))
            {
                return CanReach("Roots of Yggdrasil") || CanReach("Immortal Battlefield Main") 
                        || CanReach("Icefire Treetop Left") || CanReach("Dark Lords Mausoleum Main");
            }
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
                case "AutoScan":
                    return randomiser.Settings.AutoScanTablets;

                case "Random Ladders":
                    return randomiser.Settings.RandomLadderEntraces;

                case "Non Random Ladders":
                    return !randomiser.Settings.RandomLadderEntraces;

                case "Random Gates":
                    return randomiser.Settings.RandomGateEntraces;

                case "Non Random Gates":
                    return !randomiser.Settings.RandomGateEntraces;

                case "Random Soul Gates":
                    return randomiser.Settings.RandomSoulGateEntraces;

                case "Non Random Soul Gates":
                    return !randomiser.Settings.RandomSoulGateEntraces;

                default:
                    return false;
            }
        }
    }
}
