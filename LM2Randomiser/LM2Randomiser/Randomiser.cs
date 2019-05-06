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
        private PlayerState state;
        private Settings settings;
        
        private Dictionary<string, Area> areas;
        private Dictionary<string, Location> locations;

        readonly Random random;

        public Randomiser(Settings settings, string seedInput)
        {
            this.settings = settings;

            Seed = seedInput;
            if (String.IsNullOrEmpty(Seed))
            {
                Seed = DateTime.Now.ToString();
            }
            random = new Random(Seed.GetHashCode());
            
            state = new PlayerState(this);
            locations = new Dictionary<string, Location>();
        }

        public Random Random {
            get { return random; }
        }

        public string Seed {
            get;
        }

        public bool SetupWorld()
        {
            string[] backsideAreas = { "Valhalla Main", "Dark Lords Mausoleum Main", "Hall of Malice", "Ancient Chaos Bottom", "Eternal Prison Gloom" };

            if(!FileUtils.GetWorldData(out areas))
            {
                return false;
            }

            foreach (Area area in areas.Values)
            {
                foreach (Location location in area.locations)
                {
                    location.parentArea = area;
                    location.ruleTree = RuleTree.ParseAndBuildRules(location.ruleString);
                    locations.Add(location.name, location);
                }

                foreach (Connection exit in area.exits)
                {
                    exit.parentArea = area;

                    if (settings.requireMirai && backsideAreas.Contains(exit.connectingAreaName))
                    {
                        exit.AppendRuleString(" and Has(Future Development Company)");
                    }

                    exit.ruleTree = RuleTree.ParseAndBuildRules(exit.ruleString);

                    //Add entrances to areas so that when we try to see if we can reach an area when can check its entraces rules 
                    Area connectingArea = GetArea(exit.connectingAreaName);
                    exit.connectingArea = connectingArea;
                    connectingArea.entrances.Add(exit);
                }


            }
            return true;
        }
        
        public bool PlaceRandomItems()
        {
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

            //get skulls
            if (!FileUtils.GetItemsFromJson("Data\\skulls.json", out List<Item> skulls))
            {
                return false;
            }

            //get unrequired items
            if (!FileUtils.GetItemsFromJson("Data\\unreqitems.json", out List<Item> unrequiredItems))
            {
                return false;
            }

            //NOTE: when more options get add move these to a seperate method or something
            if (settings.requireMirai)
            {
                requiredItems.Add(ItemPool.GetAndRemove(ItemID.FutureDevelopmentCompany, unrequiredItems));
            }

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
            GetLocation("Hiner Shop 4").isLocked = true;
            GetLocation("Fobos Skull Item").isLocked = true;
            
            //Get all unplaced locations as required items can go anywhere aslong as it can be reached
            List<Location> unplacedLocations = GetUnplacedLocations();
            //place required items
            ItemRandomisation.RandomiseRequiredItems(this, unplacedLocations, requiredItems);

            //unlock the locked locations now since any item that is not required can go there
            GetLocation("Funeral Item").isLocked = false;
            GetLocation("Mulbruk Item").isLocked = false;

            //randomise all skulls bar one seperately since its seem better for generation, needs testing 
            unplacedLocations = GetUnplacedLocations();
            ItemRandomisation.RandomiseSpecificItems(this, unplacedLocations, skulls, null);

            //anything left can go here
            GetLocation("Hiner Shop 4").isLocked = false;
            GetLocation("Fobos Skull Item").isLocked = false;

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
            foreach(Location location in locations.Values)
            {
                if (location.item != null && location.item.id != ItemID.Default)
                {
                    location.item = null;
                }
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
            foreach(Location location in locations.Values)
            {
                if(location.item != null)
                {
                    placedLocations.Add(location);
                }
            }

            return placedLocations;
        }

        public List<Location> GetUnplacedLocations()
        {
            List<Location> unplacedLocations = new List<Location>();
            foreach (Location location in locations.Values)
            {
                if (location.item == null && !location.isLocked)
                {
                    unplacedLocations.Add(location);
                }
            }

            return unplacedLocations;
        }

        public List<Location> GetUnplacedShopLocations()
        {
            List<Location> unplacedLocations = new List<Location>();
            foreach (Location location in locations.Values)
            {
                if (location.item == null && !location.isLocked && location.locationType == LocationType.Shop)
                {
                    unplacedLocations.Add(location);
                }
            }

            return unplacedLocations;
        }

        public List<Location> GetPlacedRequiredItemLocations()
        {
            List<Location> placedLocations = new List<Location>();
            foreach (Location location in locations.Values)
            {
                if (location.item != null && location.item.isRequired)
                {
                    placedLocations.Add(location);
                }
            }

            return placedLocations;
        }
    }
}
