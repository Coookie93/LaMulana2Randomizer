using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LM2Randomiser.RuleParsing;
using LM2Randomiser.Logging;

namespace LM2Randomiser
{
    public class PlayerState
    {
        readonly string[] bossNames = {"Fafnir", "Surtr", "Vritra", "Kujata", "Aten-Ra", "Jormangund", "Anu", "Echidna", "Hel"};

        Dictionary<string, bool> areaChecks;
        Dictionary<string, bool> entraceChecks;
        Dictionary<string, bool> collectedLocations;
        Dictionary<string, int> collectedItems;

        Randomiser World;

        public PlayerState(Randomiser world)
        {
            this.World = world;
            areaChecks =  new Dictionary<string, bool>();
            entraceChecks = new Dictionary<string, bool>();
            collectedLocations = new Dictionary<string, bool>();
            collectedItems = new Dictionary<string, int>();
        }
        
        public static PlayerState GetStateWithItems(Randomiser world, List<string[]> currentItems)
        {
            PlayerState state = new PlayerState(world);

            if (currentItems != null)
            {
                foreach (var item in currentItems)
                {
                    state.CollectItem(item[0]);
                }
            }

            List<Location> requiredLocations = world.GetPlacedRequiredItemLocations();

            List<Location> reachableLocations;

            do
            {
                reachableLocations = state.GetReachableLocations(requiredLocations);
                foreach (var location in reachableLocations)
                {
                    state.CollectItem(location.item.name);
                    state.collectedLocations.Add(location.name, true);
                }

                state.ResetCheckedAreasAndEntrances();

            } while (reachableLocations.Count > 0);

            return state;
        }

        public bool CanBeatGame(List<Location> requiredLocations)
        {
            List<Location> reachableLocations;
            do
            {
                reachableLocations = GetReachableLocations(requiredLocations);
                foreach (var location in reachableLocations)
                {
                    CollectItem(location.item.name);
                    collectedLocations.Add(location.name, true);
                }

                ResetCheckedAreasAndEntrances();

            } while (reachableLocations.Count > 0);

            if(!HasItem("Winner"))
            {
                Logger.GetLogger.Log("Failed to generate beatable seed, generating new seed.");
            }

            return HasItem("Winner");
        }

        public bool CanReach(Area area)
        {
            if (area.name.Equals("Village of Departure"))
            {
                return true;
            }

            bool cached;
            if (areaChecks.TryGetValue(area.name, out cached))
            {
                return cached;
            }
            else
            {
                if (area.checking)
                {
                    return false;
                }

                area.checking = true;
                bool canReach = area.CanReach(this);
                area.checking = false;

                areaChecks.Add(area.name, canReach);

                return canReach;
            }
        }

        public bool CanReach(Connection entrance)
        {
            bool cached;
            if (entraceChecks.TryGetValue(entrance.name, out cached))
            {
                return cached;
            }
            else
            {
                if (entrance.checking)
                {
                    return false;
                }

                entrance.checking = true;
                bool canReach = entrance.CanReach(this);
                entrance.checking = false;

                entraceChecks.Add(entrance.name, canReach);

                return canReach;
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

            if (bossNames.Contains(itemName))
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

        public bool Evaluate(Rule rule)
        {
            switch (rule.ruleType)
            {
                case RuleType.CanReach:
                    return CanReach(rule.value);

                case RuleType.CanChant:
                    return CanChant(rule.value);

                case RuleType.CanWarp:
                    return HasItem("Holy Grail");

                case RuleType.CanSpinCorridor:
                    return HasItem("Beherit");

                case RuleType.Has:
                    return HasItem(rule.value);

                case RuleType.CanUse:
                    return CanUse(rule.value);

                case RuleType.IsDead:
                    return HasItem(rule.value);

                case RuleType.OrbCount:
                    return OrbCount(int.Parse(rule.value));

                case RuleType.GuardianKills:
                    return GuardianKills(int.Parse(rule.value));

                case RuleType.PuzzleFinished:
                    return HasItem(rule.value);

                case RuleType.AnkhCount:
                    return AnkhCount(int.Parse(rule.value));

                case RuleType.Dissonance:
                    return Dissonance(int.Parse(rule.value));

                case RuleType.SkullCount:
                    return SkullCount(int.Parse(rule.value));

                case RuleType.True:
                    return true;

                default:
                    return false;
            }
        }

        private void ResetCheckedAreasAndEntrances()
        {
            //reset areas
            Dictionary<string, bool> temp = new Dictionary<string, bool>();
            foreach (var area in areaChecks)
            {
                if (area.Value)
                {
                    temp.Add(area.Key, area.Value);
                }
            }
            areaChecks = temp;

            //reset entrances
            temp = new Dictionary<string, bool>();
            foreach (var entrance in entraceChecks)
            {
                if (entrance.Value)
                {
                    temp.Add(entrance.Key, entrance.Value);
                }
            }
            entraceChecks = temp;
        }

        private List<Location> GetReachableLocations(List<Location> requiredLocations)
        {
            List<Location> locations = new List<Location>();

            foreach(var location in requiredLocations)
            {
                if(!collectedLocations.ContainsKey(location.name) && location.CanReach(this))
                {
                    locations.Add(location);
                }
            }

            return locations;
        }
        
        private bool HasItem(string itemName)
        {
            if(collectedItems.ContainsKey(itemName)) {
                return true;
            }

            return false;
        } 
        
        private bool CanChant(string mantra)
        {
            return HasItem("Djed Pillar") && HasItem("Mantra") && HasItem(mantra);
        }

        private bool CanUse(string subWeapon)
        {
            return HasItem(subWeapon) && HasItem(subWeapon + " Ammo");
        }
        
        private bool GuardianKills(int count)
        {
            int num;
            if(collectedItems.TryGetValue("Guardians", out num))
            {
                return num >= count;
            }

            return false;
        }

        private bool OrbCount(int count)
        {
            int num;
            if (collectedItems.TryGetValue("Sacred Orb", out num))
            {
                return num >= count;
            }

            return false;
        }

        private bool AnkhCount(int count)
        {
            int num;
            if (collectedItems.TryGetValue("Ankh Jewel", out num))
            {
                return num >= count;
            }

            return false;
        }

        private bool SkullCount(int count)
        {
            int num;
            if (collectedItems.TryGetValue("Crystal Skull", out num))
            {
                return num >= count;
            }

            return false;
        }

        private bool Dissonance(int count)
        {
            int num;
            if (collectedItems.TryGetValue("Dissonance", out num))
            {
                return num >= count;
            }

            return false;
        }

        private bool CanReach(string areaName)
        {
            return CanReach(World.GetArea(areaName));
        }
    }
}
