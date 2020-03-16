using System;
using System.Linq;
using System.Collections.Generic;
using LaMulana2Randomizer.Utils;
using LaMulana2RandomizerShared;

namespace LaMulana2Randomizer
{
    public class Randomiser
    {
        private readonly Random random;
        private Dictionary<string, Area> areas;
        private Dictionary<string, Location> locations;
        
        public Settings Settings { get; }

        public Randomiser(Settings settings)
        {
            Settings = settings;
            random = new Random(settings.Seed);
        }
        
        public void Setup()
        {
            areas = new Dictionary<string, Area>();
            locations = new Dictionary<string, Location>();

            FileUtils.GetWorldData(out List<JsonArea> worldData);

            foreach (JsonArea areaData in worldData)
            {
                Area area = new Area(areaData.Name);

                foreach (JsonLocation locationData in areaData.Locations)
                {
                    Location location = new Location(locationData)
                    {
                        ParentAreaName = area.Name
                    };

                    if (Settings.HardBosses && (location.LocationType == LocationType.Guardian 
                        || location.LocationType == LocationType.Miniboss))
                    {
                        location.UseHardRules();
                    }

                    location.BuildLogicTree();
                    locations.Add(location.Name, location);
                }
                foreach (JsonConnection exitData in areaData.Exits)
                {
                    Connection exit = new Connection(exitData, area.Name);

                    if (Settings.FDCForBacksides && exit.IsBackside)
                    {
                        exit.AppendRuleString(" and Has(Future Development Company)");
                    }

                    exit.BuildLogicTree();
                    area.Exits.Add(exit);
                }
                areas.Add(area.Name, area);
            }

            foreach(Area area in areas.Values)
            {
                foreach(Connection exit in area.Exits)
                {
                    Area connectingArea = GetArea(exit.ConnectingAreaName);
                    connectingArea.Entrances.Add(exit);
                }
            }
        }
        
        public void PlaceItems()
        {
            FileUtils.GetItemsFromJson("Data//Items.json", out List<Item> items);
            FileUtils.GetItemsFromJson("Data//Mantras.json", out List<Item> mantras);
            FileUtils.GetItemsFromJson("Data//ShopOnlyItems.json", out List<Item> shopOnlyItems);

            //Places weights at a starting shop since they are needed for alot of early items
            //this means that player will not have to rely on drops or weights from pots
            GetLocation("Nebur Shop 1").PlaceItem(ItemPool.GetAndRemove(ItemID.Weights, shopOnlyItems));

            if (Settings.ShopPlacement != ShopPlacement.Original)
            {
                //these locations cant be included properly atm since the reason the shop switches is unknown
                GetLocation("Hiner Shop 3").PlaceItem(ItemPool.GetAndRemove(ItemID.Map1, items));
                GetLocation("Hiner Shop 4").PlaceItem(ItemPool.GetAndRemove(ItemID.Map2, items));
            }

            shopOnlyItems = Shuffle.FisherYates(shopOnlyItems, random);
            //place the weights and ammo in shops first since they can only be in shops
            PlaceShopItems(shopOnlyItems, items);

            //create a list of the items we want to place that are accessible from the start
            List<Item> earlyItems = new List<Item>();
            if (!Settings.RandomGrail) 
                earlyItems.Add(ItemPool.GetAndRemove(ItemID.HolyGrail, items));
            if (!Settings.RandomScanner && Settings.ShopPlacement != ShopPlacement.Original) 
                earlyItems.Add(ItemPool.GetAndRemove(ItemID.HandScanner, items));
            if (!Settings.RandomCodices && Settings.ShopPlacement != ShopPlacement.Original) 
                earlyItems.Add(ItemPool.GetAndRemove(ItemID.Codices, items));
            if (!Settings.RandomFDC && Settings.ShopPlacement != ShopPlacement.Original) 
                earlyItems.Add(ItemPool.GetAndRemove(ItemID.FutureDevelopmentCompany, items));

            earlyItems = Shuffle.FisherYates(earlyItems, random);
            RandomiseWithChecks(GetUnplacedLocations(), earlyItems, new List<Item>());

            //split the remaining item it required/non required
            List<Item> requiredItems = ItemPool.GetRequiredItems(items);
            List<Item> nonRequiredItems = ItemPool.GetNonRequiredItems(items);

            requiredItems = Shuffle.FisherYates(requiredItems, random);
            nonRequiredItems = Shuffle.FisherYates(nonRequiredItems, random);

            mantras = Shuffle.FisherYates(mantras, random);
            //place mantras if they are not fully randomised
            PlaceMantras(mantras, requiredItems);

            //place required items
            RandomiseAssumedFill(GetUnplacedLocations(), requiredItems);

            //place non requires items
            RandomiseWithoutChecks(GetUnplacedLocations(), nonRequiredItems);
        }

        public bool CanBeatGame()
        {
            foreach (Location guardian in GetPlacedLocationsOfType(LocationType.Guardian))
            {
                if (!new PlayerState(this, true).SoftlockCheck(GetPlacedRequiredItemLocations(), guardian))
                    return false;
            }
            return new PlayerState(this).CanBeatGame(GetPlacedRequiredItemLocations());
        }
        
        public Area GetArea(string areaName)
        {
            if(!areas.TryGetValue(areaName, out Area area))
                throw new InvalidAreaException($"Area does not exist: {areaName}");

            return area;
        }

        public Location GetLocation(string locationName)
        {
            if (!locations.TryGetValue(locationName, out Location location))
                throw new InvalidLocationException($"Location does not exist: {locationName}");

            return location;
        }
        
        public List<Location> GetPlacedLocations()
        {
            var placedLocations = from location in locations.Values
                                  where location.Item != null
                                  select location;

            return placedLocations.ToList();
        }

        public List<Location> GetPlacedLocationsOfType(LocationType type)
        {
            var placedLocations = from location in locations.Values
                                    where location.Item != null && location.LocationType == type
                                    select location;

            return placedLocations.ToList();
        }

        public List<Location> GetUnplacedLocations()
        {
            var unplacedLocations = from location in locations.Values
                                    where location.Item == null && !location.IsLocked
                                    select location;

            return unplacedLocations.ToList();
        }

        public List<Location> GetUnplacedLocationsOfType(LocationType type)
        {
            var unplacedLocations = from location in locations.Values
                                    where location.Item == null && !location.IsLocked && 
                                          location.LocationType == type
                                    select location;

            return unplacedLocations.ToList();
        }

        public List<Location> GetPlacedRequiredItemLocations()
        {
            var placedLocations = from location in locations.Values
                                  where location.Item != null && location.Item.isRequired
                                  select location;

            return placedLocations.ToList();
        }

        private void PlaceShopItems(List<Item> shopItems, List<Item> items)
        {
            if(Settings.ShopPlacement == ShopPlacement.Random)
            {
                RandomiseWithChecks(GetUnplacedLocationsOfType(LocationType.Shop), shopItems, items);
            }
            else if(Settings.ShopPlacement == ShopPlacement.AtLeastOne)
            {
                //lock the first slot of each shop so there will be atleast one item in to buy in each shop
                GetLocation("Nebur Shop 2").IsLocked = true;
                GetLocation("Modro Shop 1").IsLocked = true;
                GetLocation("Sidro Shop 1").IsLocked = true;
                GetLocation("Hiner Shop 1").IsLocked = true;
                GetLocation("Korobok Shop 1").IsLocked = true;
                GetLocation("Shuhoka Shop 1").IsLocked = true;
                GetLocation("Pym Shop 1").IsLocked = true;
                GetLocation("Btk Shop 1").IsLocked = true;
                GetLocation("Mino Shop 1").IsLocked = true;
                GetLocation("Bargain Duck Shop 1").IsLocked = true;
                GetLocation("Peibalusa Shop 1").IsLocked = true;
                GetLocation("Hiro Roderick Shop 1").IsLocked = true;
                GetLocation("Hydlit Shop 1").IsLocked = true;
                GetLocation("Aytum Shop 1").IsLocked = true;
                GetLocation("Kero Shop 1").IsLocked = true;
                GetLocation("Ash Geen Shop 1").IsLocked = true;
                GetLocation("Venom Shop 1").IsLocked = true;
                GetLocation("Megarock Shop 1").IsLocked = true;
                GetLocation("FairyLan Shop 1").IsLocked = true;

                RandomiseWithChecks(GetUnplacedLocationsOfType(LocationType.Shop), shopItems, items);

                //now unlock all the shop slots that were locked
                GetLocation("Nebur Shop 2").IsLocked = false;
                GetLocation("Modro Shop 1").IsLocked = false;
                GetLocation("Sidro Shop 1").IsLocked = false;
                GetLocation("Hiner Shop 1").IsLocked = false;
                GetLocation("Korobok Shop 1").IsLocked = false;
                GetLocation("Shuhoka Shop 1").IsLocked = false;
                GetLocation("Pym Shop 1").IsLocked = false;
                GetLocation("Btk Shop 1").IsLocked = false;
                GetLocation("Mino Shop 1").IsLocked = false;
                GetLocation("Bargain Duck Shop 1").IsLocked = false;
                GetLocation("Peibalusa Shop 1").IsLocked = false;
                GetLocation("Hiro Roderick Shop 1").IsLocked = false;
                GetLocation("Hydlit Shop 1").IsLocked = false;
                GetLocation("Aytum Shop 1").IsLocked = false;
                GetLocation("Kero Shop 1").IsLocked = false;
                GetLocation("Ash Geen Shop 1").IsLocked = false;
                GetLocation("Venom Shop 1").IsLocked = false;
                GetLocation("Megarock Shop 1").IsLocked = false;
                GetLocation("FairyLan Shop 1").IsLocked = false;
            }
            else 
            {
                //otherwise place items how they are originally
                GetLocation("Nebur Shop 2").PlaceItem(ItemPool.GetAndRemove(ItemID.YagooMapReader, items));
                GetLocation("Nebur Shop 3").PlaceItem(ItemPool.GetAndRemove(ItemID.TextTrax, items));

                GetLocation("Modro Shop 1").PlaceItem(ItemPool.GetAndRemove(ItemID.Buckler, items));
                GetLocation("Modro Shop 2").PlaceItem(ItemPool.GetAndRemove(ItemID.Pistol, items));
                GetLocation("Modro Shop 3").PlaceItem(ItemPool.GetAndRemove(ItemID.PistolAmmo, shopItems));

                GetLocation("Sidro Shop 1").PlaceItem(ItemPool.GetAndRemove(ItemID.HandScanner, items));
                GetLocation("Sidro Shop 2").PlaceItem(ItemPool.GetAndRemove(ItemID.ShurikenAmmo, shopItems));
                GetLocation("Sidro Shop 3").PlaceItem(ItemPool.GetAndRemove(ItemID.Pepper, items));

                GetLocation("Hiner Shop 1").PlaceItem(ItemPool.GetAndRemove(ItemID.Weights, shopItems));
                GetLocation("Hiner Shop 2").PlaceItem(ItemPool.GetAndRemove(ItemID.Codices, items));
                GetLocation("Hiner Shop 3").PlaceItem(ItemPool.GetAndRemove(ItemID.AnkhJewel1, items));
                GetLocation("Hiner Shop 4").PlaceItem(ItemPool.GetAndRemove(ItemID.AnkhJewel8, items));

                GetLocation("Korobok Shop 1").PlaceItem(ItemPool.GetAndRemove(ItemID.Weights, shopItems));
                GetLocation("Korobok Shop 2").PlaceItem(ItemPool.GetAndRemove(ItemID.ShurikenAmmo, shopItems));
                GetLocation("Korobok Shop 3").PlaceItem(ItemPool.GetAndRemove(ItemID.Guild, items));

                GetLocation("Shuhoka Shop 1").PlaceItem(ItemPool.GetAndRemove(ItemID.Weights, shopItems));
                GetLocation("Shuhoka Shop 2").PlaceItem(ItemPool.GetAndRemove(ItemID.ShurikenAmmo, shopItems));
                GetLocation("Shuhoka Shop 3").PlaceItem(ItemPool.GetAndRemove(ItemID.Alert, items));

                GetLocation("Pym Shop 1").PlaceItem(ItemPool.GetAndRemove(ItemID.Weights, shopItems));
                GetLocation("Pym Shop 2").PlaceItem(ItemPool.GetAndRemove(ItemID.RollingShurikenAmmo, shopItems));
                GetLocation("Pym Shop 3").PlaceItem(ItemPool.GetAndRemove(ItemID.Snapshot, items));

                GetLocation("Btk Shop 1").PlaceItem(ItemPool.GetAndRemove(ItemID.Weights, shopItems));
                GetLocation("Btk Shop 2").PlaceItem(ItemPool.GetAndRemove(ItemID.CaltropsAmmo, shopItems));
                GetLocation("Btk Shop 3").PlaceItem(ItemPool.GetAndRemove(ItemID.EngaMusica, items));

                GetLocation("Mino Shop 1").PlaceItem(ItemPool.GetAndRemove(ItemID.Weights, shopItems));
                GetLocation("Mino Shop 2").PlaceItem(ItemPool.GetAndRemove(ItemID.BombAmmo, shopItems));
                GetLocation("Mino Shop 3").PlaceItem(ItemPool.GetAndRemove(ItemID.LonelyHouseMoving, items));

                GetLocation("Bargain Duck Shop 1").PlaceItem(ItemPool.GetAndRemove(ItemID.Weights, shopItems));
                GetLocation("Bargain Duck Shop 2").PlaceItem(ItemPool.GetAndRemove(ItemID.CaltropsAmmo, shopItems));
                GetLocation("Bargain Duck Shop 3").PlaceItem(ItemPool.GetAndRemove(ItemID.FutureDevelopmentCompany, items));

                GetLocation("Peibalusa Shop 1").PlaceItem(ItemPool.GetAndRemove(ItemID.Weights, shopItems));
                GetLocation("Peibalusa Shop 2").PlaceItem(ItemPool.GetAndRemove(ItemID.EarthSpearAmmo, shopItems));
                GetLocation("Peibalusa Shop 3").PlaceItem(ItemPool.GetAndRemove(ItemID.RaceScanner, items));

                GetLocation("Hiro Roderick Shop 1").PlaceItem(ItemPool.GetAndRemove(ItemID.Weights, shopItems));
                GetLocation("Hiro Roderick Shop 2").PlaceItem(ItemPool.GetAndRemove(ItemID.FlareAmmo, shopItems));
                GetLocation("Hiro Roderick Shop 3").PlaceItem(ItemPool.GetAndRemove(ItemID.Harp, items));

                GetLocation("Hydlit Shop 1").PlaceItem(ItemPool.GetAndRemove(ItemID.Weights, shopItems));
                GetLocation("Hydlit Shop 2").PlaceItem(ItemPool.GetAndRemove(ItemID.BombAmmo, shopItems));
                GetLocation("Hydlit Shop 3").PlaceItem(ItemPool.GetAndRemove(ItemID.GaneshaTalisman, items));

                GetLocation("Aytum Shop 1").PlaceItem(ItemPool.GetAndRemove(ItemID.Weights, shopItems));
                GetLocation("Aytum Shop 2").PlaceItem(ItemPool.GetAndRemove(ItemID.ShurikenAmmo, shopItems));
                GetLocation("Aytum Shop 3").PlaceItem(ItemPool.GetAndRemove(ItemID.BounceShot, items));

                GetLocation("Kero Shop 1").PlaceItem(ItemPool.GetAndRemove(ItemID.Weights, shopItems));
                GetLocation("Kero Shop 2").PlaceItem(ItemPool.GetAndRemove(ItemID.PistolAmmo, shopItems));
                GetLocation("Kero Shop 3").PlaceItem(ItemPool.GetAndRemove(ItemID.RoseandCamelia, items));

                GetLocation("Ash Geen Shop 1").PlaceItem(ItemPool.GetAndRemove(ItemID.Weights, shopItems));
                GetLocation("Ash Geen Shop 2").PlaceItem(ItemPool.GetAndRemove(ItemID.FlareAmmo, shopItems));
                GetLocation("Ash Geen Shop 3").PlaceItem(ItemPool.GetAndRemove(ItemID.MekuriMaster, items));

                GetLocation("Venom Shop 1").PlaceItem(ItemPool.GetAndRemove(ItemID.Weights, shopItems));
                GetLocation("Venom Shop 2").PlaceItem(ItemPool.GetAndRemove(ItemID.CaltropsAmmo, shopItems));
                GetLocation("Venom Shop 3").PlaceItem(ItemPool.GetAndRemove(ItemID.SpaceCapstarII, items));

                GetLocation("Megarock Shop 1").PlaceItem(ItemPool.GetAndRemove(ItemID.Weights, shopItems));
                GetLocation("Megarock Shop 2").PlaceItem(ItemPool.GetAndRemove(ItemID.EarthSpearAmmo, shopItems));
                GetLocation("Megarock Shop 3").PlaceItem(ItemPool.GetAndRemove(ItemID.Bracelet, items));

                GetLocation("FairyLan Shop 1").PlaceItem(ItemPool.GetAndRemove(ItemID.Weights, shopItems));
                GetLocation("FairyLan Shop 2").PlaceItem(ItemPool.GetAndRemove(ItemID.ChakramAmmo, shopItems));
                GetLocation("FairyLan Shop 3").PlaceItem(ItemPool.GetAndRemove(ItemID.AngelShield, items));
            }
        }

        private void PlaceMantras(List<Item> mantras, List<Item> requiredItems)
        {
            if(Settings.MantraPlacement == MantraPlacement.Original)
            {
                //put the mantras where they are originally if they arent randomised
                GetLocation("Heaven Mantra Mural").PlaceItem(ItemPool.GetAndRemove(ItemID.Heaven, mantras));
                GetLocation("Earth Mantra Mural").PlaceItem(ItemPool.GetAndRemove(ItemID.Earth, mantras));
                GetLocation("Sun Mantra Mural").PlaceItem(ItemPool.GetAndRemove(ItemID.Sun, mantras));
                GetLocation("Moon Mantra Mural").PlaceItem(ItemPool.GetAndRemove(ItemID.Moon, mantras));
                GetLocation("Sea Mantra Mural").PlaceItem(ItemPool.GetAndRemove(ItemID.Sea, mantras));
                GetLocation("Fire Mantra Mural").PlaceItem(ItemPool.GetAndRemove(ItemID.Fire, mantras));
                GetLocation("Wind Mantra Mural").PlaceItem(ItemPool.GetAndRemove(ItemID.Wind, mantras));
                GetLocation("Mother Mantra Mural").PlaceItem(ItemPool.GetAndRemove(ItemID.Mother, mantras));
                GetLocation("Child Mantra Mural").PlaceItem(ItemPool.GetAndRemove(ItemID.Child, mantras));
                GetLocation("Night Mantra Mural").PlaceItem(ItemPool.GetAndRemove(ItemID.Night, mantras));
            }
            else if(Settings.MantraPlacement== MantraPlacement.OnlyMurals)
            {
                RandomiseWithChecks(GetUnplacedLocationsOfType(LocationType.Mural), mantras, requiredItems);
            }
            else {
                //if they are fully random they will get randomised with the rest of the items
                requiredItems.AddRange(mantras);
            }
        }

        private void RandomiseAssumedFill(List<Location> locations, List<Item> itemsToPlace)
        {
            PlayerState state;

            while (itemsToPlace.Count > 0)
            {
                Item item = itemsToPlace[itemsToPlace.Count - 1];
                itemsToPlace.Remove(item);
                locations = Shuffle.FisherYates(locations, random);

                state = PlayerState.GetStateWithItems(this, itemsToPlace);

                Location locationToPlaceAt = null;
                foreach (Location location in locations)
                {
                    if (location.CanReach(state))
                    {
                        locationToPlaceAt = location;
                        break;
                    }
                }

                if (locationToPlaceAt != null)
                {
                    locationToPlaceAt.PlaceItem(item);
                    locations.Remove(locationToPlaceAt);
                }
                else
                {
                    Logger.Log($"Failed to place item {item.name}");
                }
            }
        }

        private void RandomiseWithChecks(List<Location> locations, List<Item> itemsToPlace, List<Item> currentItems)
        {
            PlayerState state;

            while (itemsToPlace.Count > 0)
            {
                Item item = itemsToPlace[itemsToPlace.Count - 1];
                itemsToPlace.Remove(item);
                locations = Shuffle.FisherYates(locations, random);

                state = PlayerState.GetStateWithItems(this, currentItems);

                Location locationToPlaceAt = null;
                foreach (Location location in locations)
                {
                    if (location.CanReach(state))
                    {
                        locationToPlaceAt = location;
                        break;
                    }
                }

                if (locationToPlaceAt != null)
                {
                    locationToPlaceAt.PlaceItem(item);
                    locations.Remove(locationToPlaceAt);
                }
                else
                {
                    Logger.Log($"Failed to place item {item.name}");
                }
            }

        }

        private void RandomiseWithoutChecks(List<Location> locations, List<Item> itemsToPlace)
        {
            locations = Shuffle.FisherYates(locations, random);
            int index = (itemsToPlace.Count - 1);

            while (index >= 0)
            {
                Item item = itemsToPlace[index];
                Location location = locations[index];

                itemsToPlace.Remove(item);
                locations.Remove(location);
                location.PlaceItem(item);
                index--;
            }
        }
    }
}
