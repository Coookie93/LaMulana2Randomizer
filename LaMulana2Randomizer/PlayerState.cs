using System.Collections.Generic;
using System.Linq;
using LM2RandomizerShared;
using LM2Randomizer.RuleParsing;

namespace LM2Randomizer
{
    public class PlayerState
    {
        readonly string[] bossNames = {"Fafnir", "Surtr", "Vritra", "Kujata", "Aten-Ra", "Jormangund", "Anu", "Echidna", "Hel"};
        
        public Dictionary<string, bool> areaChecks;
        public Dictionary<string, bool> entraceChecks;
        public Dictionary<string, bool> collectedLocations;
        public Dictionary<string, int> collectedItems;
        public Randomiser Randomiser;

        public PlayerState(Randomiser randomiser)
        {
            Randomiser = randomiser;
            areaChecks =  new Dictionary<string, bool>();
            entraceChecks = new Dictionary<string, bool>();
            collectedLocations = new Dictionary<string, bool>();
            collectedItems = new Dictionary<string, int>();
        }
        
        public static PlayerState GetStateWithItems(Randomiser randomiser, List<Item> currentItems)
        {
            PlayerState state = new PlayerState(randomiser);
            
            foreach (Item item in currentItems)
            {
                state.CollectItem(item);
            }

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

        public bool CanBeatGame(List<Location> requiredLocations)
        {
            List<Location> reachableLocations;
            do
            {
                reachableLocations = GetReachableLocations(requiredLocations);
                foreach (Location location in reachableLocations)
                {
                    CollectItem(location.Item);
                    collectedLocations.Add(location.Name, true);
                }

                ResetCheckedAreasAndEntrances();

            } while (reachableLocations.Count > 0);
            
            return HasItem("Winner");
        }

        public bool CanReach(string areaName)
        {
            Area area = Randomiser.GetArea(areaName);
            if (area.Name.Equals("Village of Departure"))
            {
                return true;
            }

            if (areaChecks.TryGetValue(area.Name, out bool cached))
            {
                return cached;
            }
            else
            {
                if (area.Checking)
                {
                    return false;
                }
                area.Checking = true;
                bool canReach = area.CanReach(this);
                area.Checking = false;
                areaChecks.Add(area.Name, canReach);

                return canReach;
            }
        }

        public bool CanReach(Connection entrance)
        {
            if (entraceChecks.TryGetValue(entrance.Name, out bool cached))
            {
                return cached;
            }
            else
            {
                if (entrance.Checking)
                {
                    return false;
                }
                entrance.Checking = true;
                bool canReach = entrance.CanReach(this);
                entrance.Checking = false;
                entraceChecks.Add(entrance.Name, canReach);

                return canReach;
            }
        }

        public void CollectItem(Item item)
        {
            if (collectedItems.ContainsKey(item.name))
            {
                collectedItems[item.name]++;
            }
            else
            {
                collectedItems.Add(item.name, 1);
            }

            if (bossNames.Contains(item.name))
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
                    return CanSpinCorridor(); 

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

                case RuleType.HasWeaponUpgrade:
                    return HasItem("Chain Whip") || HasItem("Flail Whip") || HasItem("Axe") || HasItem("Axe");

                case RuleType.True:
                    return true;

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
            return collectedItems.ContainsKey(itemName) ? true : false;
        } 
        
        private bool CanChant(string mantra)
        {
            return HasItem("Djed Pillar") && HasItem("Mantra") && HasItem(mantra);
        }

        private bool CanUse(string subWeapon)
        {
            string[] subWeaponNames = { "Shuriken", "Rolling Shuriken", "Caltrops", "Chakram", "Earth Spear", "Flare", "Bomb", "Pistol",
                                            "Buckler", "Silver Shield", "Angel Shield", "Ankh Jewel"};

            if (subWeapon.Equals("Claydoll Suit"))
            {
                if (!HasItem(subWeapon))
                {
                    return false;
                }
                
                foreach(string subWeaponName in subWeaponNames)
                {
                    if (HasItem(subWeaponName))
                    {
                        return true;
                    }
                }

                return false;
            }
            else
            {
                return HasItem(subWeapon) && HasItem(subWeapon + " Ammo");
            }
        }
        
        private bool CanSpinCorridor()
        {
            return HasItem("Beherit") && Dissonance(1);
        }

        private bool GuardianKills(int count)
        {
            if (collectedItems.TryGetValue("Guardians", out int num))
            {
                return num >= count;
            }
            return false;
        }

        private bool OrbCount(int count)
        {
            if (collectedItems.TryGetValue("Sacred Orb", out int num))
            {
                return num >= count;
            }
            return false;
        }

        private bool AnkhCount(int count)
        {
            if (collectedItems.TryGetValue("Ankh Jewel", out int num))
            {
                return num >= count;
            }
            return false;
        }

        private bool SkullCount(int count)
        {
            if (collectedItems.TryGetValue("Crystal Skull", out int num))
            {
                return num >= count;
            }
            return false;
        }

        private bool Dissonance(int count)
        {
            if (collectedItems.TryGetValue("Dissonance", out int num))
            {
                return num >= count;
            }
            return false;
        }
    }
}
