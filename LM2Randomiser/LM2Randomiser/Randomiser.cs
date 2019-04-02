using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using LM2Randomiser.RuleParsing;
using LM2Randomiser.Utils;
using LM2Randomiser.Logging;

namespace LM2Randomiser
{
    public class Randomiser
    {
        PlayerState state;
        Settings settings;

        Dictionary<string, Area> areas;
        Dictionary<string, Location> locations;
        
        Random random;
        string seed;

        public Randomiser(Settings settings, string seedInput)
        {
            this.settings = settings;
            areas = new Dictionary<string, Area>();
            locations = new Dictionary<string, Location>();

            seed = seedInput;
            if (String.IsNullOrEmpty(seed))
            {
                seed = DateTime.Now.ToString();
            }
            random = new Random(seed.GetHashCode());

            state = new PlayerState(this);
        }

        public Random Random {
            get { return random; }
        }

        public string Seed {
            get { return seed; }
        }

        public bool SetupWorld()
        {
            const string rules = "Data\\rules.json";
            string path = Path.Combine(Directory.GetCurrentDirectory(), rules);

            List<AreaRuleInfo> areaRules = null;
            
            //maybe move this to file utils
            try
            {
                using (StreamReader sr = File.OpenText(path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    areaRules = (List<AreaRuleInfo>)serializer.Deserialize(sr, typeof(List<AreaRuleInfo>));
                }
            }
            catch(Exception ex)
            {
                Logger.GetLogger.Log("Tried to deserialise rules.json, Error: {1}", ex.Message);
                return false;
            }

            foreach (AreaRuleInfo info in areaRules)
            {
                Area area = new Area(info.areaName);

                //adding area locations to the area and world lists
                foreach (var l in info.locations)
                {
                    string[] locationInfo = l.Split(':');

                    if (locationInfo.Length == 2)
                    {
                        //this will get changed when more than just shop locations matter
                        LocationType locationType = LocationType.Default;
                        if (locationInfo[0].Contains("Shop"))
                        {
                            locationType = LocationType.Shop;
                        }

                        Location location = new Location(locationInfo[0], area, locationType);
                        location.ruleTree = RuleTree.ParseAndBuildRules(locationInfo[1]);
                        area.locations.Add(location);
                         
                        if (!locations.ContainsKey(location.name))
                        {
                            locations.Add(location.name, location);
                        }
                        else
                        {
                            Logger.GetLogger.Log("Location already exists {0} in area {1}.", location.name, area.name);
                            return false;
                        }
                    }
                    else
                    {
                        Logger.GetLogger.Log("Area {0} contains an invlaid location string: {1}", area.name, l);
                        return false;
                    }
                }
                //adding area exits to the area and world lists
                foreach (var e in info.exits)
                {
                    string[] exitInfo = e.Split(':');

                    if (exitInfo.Length == 2)
                    {
                        Connection exit = new Connection(exitInfo[0], area);
                        exit.ruleTree = RuleTree.ParseAndBuildRules(exitInfo[1]);
                        area.exits.Add(exit);
                    }
                    else
                    {
                        Logger.GetLogger.Log("Area {0} contains an invlaid exit string: {1}", area.name, e);
                        return false;
                    }
                }

                if (!areas.ContainsKey(area.name))
                {
                    areas.Add(area.name, area);
                }
                else
                {
                    Logger.GetLogger.Log("Area already exists {0}.", area.name);
                    return false;
                }
            }

            //Add entrances to areas so that when we try to see if wee can reach an area when can check its entraces rules 
            foreach (var area in areas)
            {
                foreach (var exit in area.Value.exits)
                {
                    Area connectingArea;
                    if(areas.TryGetValue(exit.connectingAreaName, out connectingArea))
                    {
                        exit.connectingArea = connectingArea;
                        connectingArea.entrances.Add(exit);
                    }
                    else
                    {
                        Logger.GetLogger.Log("Tried to add entrances to an area that doesn't exist {0}, from exit {1}", exit.connectingAreaName, exit.name);
                        return false;
                    }

                }
            }

            return true;
        }

        //places mostly fake items that the player won't collect but are used by the access rules
        public bool PlaceNonRandomItems()
        {
            //TODO: probably turn these into a method, also turn thses data files to json to
            //

            
            //Place Mantras, only exists since mantras currently cant be randomised
            List<string[]> data;
            if(FileUtils.GetData("Data\\murals.txt", 2, out data)) {
                foreach(var info in data)
                {
                    PlaceItem(info[0], new Item(info[1], ItemID.Default));
                }
            }
            else
            {
                return false;
            }

            //Place items to allow for checking of puzzle completion, fairies, dissonance
            if (FileUtils.GetData("Data\\nonrandom.txt", 2, out data))
            {
                foreach (var info in data)
                {
                    PlaceItem(info[0], new Item(info[1], ItemID.Default));
                }
            }
            else
            {
                return false;
            }

            //Place enemy items allow for checks to see if certain Boss/miniboss is dead
            if (FileUtils.GetData("Data\\enemies.txt", 1, out data))
            {
                foreach (var info in data)
                {
                    PlaceItem(info[0], new Item(info[0], ItemID.Default));
                }
            }
            else
            {
                return false;
            }

            //Place Item to Check if the game has been beaten
            PlaceItem("9th Child",new Item("Winner", ItemID.Default));

            return true;
        }

        public bool PlaceRandomItems()
        {
            string currentDir = Directory.GetCurrentDirectory();

            //Note: Changed this to to create the items from the data here rather than later

            //get shop only items
            if (!FileUtils.GetItemsFromJson("Data\\shopitems.json", out List<Item> shopItems))
            {
                return false;
            }

            //get required items
            if (!FileUtils.GetItemsFromJson("Data\\reqitems.json", out List<Item> requiredItems))
            {
                return false;
            }

            //get unrequired items
            if (!FileUtils.GetItemsFromJson("Data\\unreqitems.json", out List<Item> unrequiredItems))
            {
                return false;
            }
            
            //NOTE: when more options get add move these to a seperate method or something
            if (!settings.randomiseGrail)
            {
                PlaceItem("Holy Grail Chest", ItemPool.GetAndRemove(ItemID.HolyGrail, requiredItems));
            }

            if (!settings.randomiseScanner)
            {
                PlaceItem("Sidro Shop 1", ItemPool.GetAndRemove(ItemID.HandScanner, requiredItems));
            }

            //Shuffle all the item pools once
            shopItems = Shuffle.FisherYates(shopItems, this);
            requiredItems = Shuffle.FisherYates(requiredItems, this);
            unrequiredItems = Shuffle.FisherYates(unrequiredItems, this);

            //Places weights at a starting shop since they are needed for alot of early items
            //this means that player will not have to rely on drops or weights from pots
            PlaceItem("Nebur Shop 1", ItemPool.GetAndRemove(ItemID.Weights, shopItems));

            //ammo can't be placed here since there is an second item that takes this slot after 
            //the first is purchased 
            GetLocation("Hiner Shop 3").isLocked = true;

            //get list of shop locations as weights and ammo items can only be placed here
            List <Location> shopLocations = GetUnplacedShopLocations();
            
            //place shop only items
            ItemRandomisation.RandomiseSpecificItems(this, shopLocations, shopItems, requiredItems);

            //items can be placed here now
            GetLocation("Hiner Shop 3").isLocked = false;

            //lock locations that currently can't be randomised
            GetLocation("Funeral Item").isLocked = true;
            GetLocation("Mulbruk Item").isLocked = true;
            
            //Get all unplaced locations as required items can go anywhere aslong as it can be reached
            List<Location> unplacedLocations = GetUnplacedLocations();
            //place required items
            ItemRandomisation.RandomiseRequiredItems(this, unplacedLocations, requiredItems);

            //unlock the locked locations now since any item that is not required can go there
            GetLocation("Funeral Item").isLocked = false;
            GetLocation("Mulbruk Item").isLocked = false;

            //Get unplaced locations after the required items have been placed
            unplacedLocations = GetUnplacedLocations();
            
            //places no requires items
            ItemRandomisation.RandomiseUnrequiredItems(this, unplacedLocations, unrequiredItems);

            return true;
        }

        public bool CanBeatGame()
        {
            return state.CanBeatGame(GetPlacedRequiredItemLocations());
        }
        
        public void ClearItemsAndState()
        {
            foreach(var location in locations)
            {
                location.Value.item = null;
            }
            state = new PlayerState(this);
        }

        public void PlaceItem(string locationName, Item item)
        {
            locations[locationName].item = item;
        }

        public void PlaceItem(Location location, Item item)
        {
            location.item = item;
        }

        public Area GetArea(string areaName)
        {
            return areas[areaName];
        }

        public Location GetLocation(string locationName)
        {
            return locations[locationName];
        }
        
        public List<Location> GetPlacedLocations()
        {
            List<Location> placedLocations = new List<Location>();
            foreach(var location in locations)
            {
                if(location.Value.item != null)
                {
                    placedLocations.Add(location.Value);
                }
            }

            return placedLocations;
        }

        public List<Location> GetUnplacedLocations()
        {
            List<Location> placedLocations = new List<Location>();
            foreach (var location in locations)
            {
                if (location.Value.item == null && !location.Value.isLocked)
                {
                    placedLocations.Add(location.Value);
                }
            }

            return placedLocations;
        }

        public List<Location> GetPlacedRequiredItemLocations()
        {
            List<Location> placedLocations = new List<Location>();
            foreach (var location in locations)
            {
                if (location.Value.item != null && location.Value.item.isRequired)
                {
                    placedLocations.Add(location.Value);
                }
            }

            return placedLocations;
        }

        public List<Location> GetUnplacedShopLocations()
        {
            List<Location> placedLocations = new List<Location>();
            foreach (var location in locations)
            {
                if (location.Value.item == null && location.Value.locationType == LocationType.Shop)
                {
                    placedLocations.Add(location.Value);
                }
            }

            return placedLocations;
        }

        private void RemoveItemFromList(string name, List<string[]> list)
        {
            string[] itemToRemove = null;
            foreach(var item in list)
            {
                if(item[0].Equals(name))
                {
                    itemToRemove = item;
                }
            }
            list.Remove(itemToRemove);
        }

        private class AreaRuleInfo
        {
            public string areaName;
            public List<string> locations = new List<string>();
            public List<string> exits = new List<string>();
        }
    }
}
