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
        private readonly Dictionary<string, Area> areas;
        private readonly Dictionary<string, Location> locations;
        private readonly List<Exit> exits;

        private List<JsonArea> worldData;
        private bool villageDeadEnd = false;

        public ItemID StartingWeaponID { get; private set; }
        public Item StartingWeapon { get; private set; }
        public List<Location> CursedLocations { get; private set; }
        public List<string> HorizontalPairs { get; private set; }
        public List<string> LadderPairs { get; private set; }
        public List<string> GatePairs { get; private set; }
        public List<string> EntrancePairs { get; private set; }
        public List<(ExitID, ExitID)> ExitPairs { get; private set; }
        public List<(Exit, Exit, int)> SoulGatePairs { get; private set; }
        public Settings Settings { get; private set; }

        public Randomiser(Settings settings)
        {
            Settings = settings;
            random = new Random(settings.Seed);
            areas = new Dictionary<string, Area>();
            locations = new Dictionary<string, Location>();
            exits = new List<Exit>();
            HorizontalPairs = new List<string>();
            LadderPairs = new List<string>();
            GatePairs = new List<string>();
            EntrancePairs = new List<string>();
            ExitPairs = new List<(ExitID, ExitID)>();
            SoulGatePairs = new List<(Exit, Exit, int)>();
        }
        
        public void Setup()
        {
            worldData = FileUtils.GetWorldData();

            foreach (JsonArea areaData in worldData)
            {
                Area area = new Area(areaData);

                foreach (JsonLocation locationData in areaData.Locations)
                {
                    Location location = new Location(locationData, area.Name);

                    if (Settings.HardBosses && (location.LocationType == LocationType.Guardian 
                        || location.LocationType == LocationType.Miniboss))
                    {
                        location.UseHardRules();
                    }

                    location.BuildLogicTree();
                    locations.Add(location.Name, location);
                }
                areas.Add(area.Name, area); 
            }

            RandomiseCurses();
        }

        public void PlaceEntrances()
        {
            foreach (JsonArea areaData in worldData)
            {
                Area area = GetArea(areaData.Name);
                foreach (JsonExit exitData in areaData.Exits)
                {
                    Exit exit = new Exit(exitData, area.Name);

                    exit.BuildLogicTree();
                    area.Exits.Add(exit);
                    exits.Add(exit);
                }
            }

            if (!Settings.FullRandomEntrances)
            {
                if (Settings.RandomHorizontalEntraces)
                    RandomiseHorizontalEntrances();

                if (Settings.RandomLadderEntraces)
                    RandomiseLadderEntrances();

                if (Settings.RandomGateEntraces)
                    RandomiseGateEntrances();
            }
            else
            {
                FullRandomEntrances();
            }

            if (Settings.RandomSoulGateEntraces)
                RandomiseSoulGateEntrances();

            foreach (Area area in areas.Values)
            {
                foreach (Exit exit in area.Exits)
                {
                    Area connectingArea = GetArea(exit.ConnectingAreaName);
                    connectingArea.Entrances.Add(exit);
                }
            }
        }

        public void FixAnkhLogic()
        {
            List<List<Location>> guardianGroups = new List<List<Location>>();

            PlayerState state = new PlayerState(this);
            foreach (Item item in FileUtils.GetItemsFromJson())
                state.CollectItem(item);

            List<Location> requiredLocations = GetPlacedRequiredItemLocations();

            for (int i = 0; i < 9; i++)
            {
                List<Location> reachableLocations;
                List<Location> guardians = new List<Location>();
                do
                {
                    reachableLocations = state.GetReachableLocations(requiredLocations);
                    foreach (Location location in reachableLocations)
                    {
                        if (location.LocationType == LocationType.Guardian)
                        {
                            guardians.Add(location);
                            state.CollectLocation(location);
                        }
                        else
                        {
                            state.CollectItem(location.Item);
                            state.CollectLocation(location);
                        }
                    }

                    state.RemoveFalseCheckedAreasAndEntrances();

                } while (reachableLocations.Count > 0);

                if(guardians.Count > 0)
                    guardianGroups.Add(guardians);

                foreach(Location guardian in guardians)
                    state.CollectItem(guardian.Item.Name);

                state.CollectItem("Guardians");
            }

            int ankhs = 0;
            foreach(var group in guardianGroups)
            {
                ankhs += group.Count();
                foreach(Location guardian in group)
                {
                    guardian.AppendLogicString($" and AnkhCount({ankhs})");
                    guardian.BuildLogicTree();
                }
            }
        }

        public void FixFDCLogic()
        {
            if (Settings.FDCForBacksides)
            {
                foreach(Exit exit in exits)
                {
                    if (exit.ExitType != ExitType.Internal && GetArea(exit.ConnectingAreaName).IsBackside)
                    {
                        exit.AppendLogicString(" and Has(Future Development Company)");
                        exit.BuildLogicTree();
                    }
                }
            }
        }

        public void PickStartingWeapon()
        {
            ItemID[] weapons = new ItemID[]{ ItemID.Whip, ItemID.Knife, ItemID.Rapier, ItemID.Axe, ItemID.Katana, ItemID.Shuriken,
                ItemID.RollingShuriken, ItemID.EarthSpear, ItemID.Flare, ItemID.Caltrops, ItemID.Chakram, ItemID.Bomb, ItemID.Pistol };

            List<ItemID> selectedWeapons = weapons.Zip(Settings.Weapons, Tuple.Create).Where(w => w.Item2).Select(w => w.Item1).ToList();
            StartingWeaponID = selectedWeapons[random.Next(selectedWeapons.Count)];
        }

        public void PlaceItems()
        {
            List<Item> items = FileUtils.GetItemsFromJson();

            StartingWeapon = ItemPool.GetAndRemove(StartingWeaponID, items);

            //Places weights at a starting shop so the player can buy them at the start of the game
            GetLocation("Nebur Shop 1").PlaceItem(ItemPool.GetAndRemove(ItemID.Weights, items));

            //if we have a subweapon need to give the player ammo at a shop
            if (StartingWeaponID > ItemID.Katana)
                GetLocation("Nebur Shop 2").PlaceItem(ItemPool.GetAndRemove(GetAmmoItemID(StartingWeaponID), items));

            if (Settings.ShopPlacement != ShopPlacement.Original)
            {
                //these locations cant be included properly atm since the reason the shop switches is unknown
                GetLocation("Hiner Shop 3").PlaceItem(ItemPool.GetAndRemove(ItemID.Map1, items));
                GetLocation("Hiner Shop 4").PlaceItem(ItemPool.GetAndRemove(ItemID.Map2, items));
            }

            List<Item> shopOnlyItems = Shuffle.FisherYates(ItemPool.GetAndRemoveShopOnlyItems(items), random);
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

            //place mantras if they are not fully randomised
            PlaceMantras(items);

            //split the remaining item it required/non required
            List<Item> requiredItems = Shuffle.FisherYates(ItemPool.GetRequiredItems(items), random);
            List<Item> nonRequiredItems = Shuffle.FisherYates(ItemPool.GetNonRequiredItems(items), random);

            //place required items
            RandomiseAssumedFill(GetUnplacedLocations(), requiredItems);

            //place non requires items
            RandomiseWithoutChecks(GetUnplacedLocations(), nonRequiredItems);
        }

        public void ClearEntrances()
        {
            foreach (Area area in areas.Values)
            {
                area.Exits.Clear();
                area.Entrances.Clear();
            }
            exits.Clear();
            HorizontalPairs.Clear();
            LadderPairs.Clear();
            GatePairs.Clear();
            EntrancePairs.Clear();
            ExitPairs.Clear();
            SoulGatePairs.Clear();
        }

        public void ClearPlacedItems()
        {
            foreach(Location location in locations.Values)
            {
                if (location.Item != null && location.Item.ID != ItemID.None)
                    location.PlaceItem(null);
            }
        }

        public bool CanBeatGame()
        {
            return PlayerState.CanBeatGame(this) && PlayerState.AnkhSoftlockCheck(this);
        }
        
        public bool EntranceCheck()
        {
            return PlayerState.EntrancePlacementCheck(this);
        }

        public void AdjustShopPrices()
        {
            PlayerState playthrough = new PlayerState(this)
            {
                IgnoreFalseChecks = true
            };
            playthrough.CollectItem(StartingWeapon);

            List<Location> reachableLocations;
            for(int i = 0; i < 5; i++)
            {
                int multiplier = 5 + i;
                reachableLocations = playthrough.GetReachableLocations(GetPlacedRequiredItemLocations());
                foreach (Location location in reachableLocations)
                {
                    Item item = location.Item;
                    playthrough.CollectItem(item);
                    playthrough.CollectLocation(location);

                    if (item.IsRequired && item.ID < ItemID.ShurikenAmmo)
                        item.PriceMultiplier = multiplier;

                }
                playthrough.RemoveFalseCheckedAreasAndEntrances();
            }
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

        public List<Location> GetLocationsOfType(LocationType type)
        {
            return locations.Values.Where(location => location.LocationType == type).ToList();
        }

        public List<Location> GetPlacedLocations()
        {
            return locations.Values.Where(location => location.Item != null).ToList();
        }

        public List<Location> GetPlacedLocationsOfType(LocationType type)
        {
            return locations.Values.Where(location => location.Item != null && location.LocationType == type).ToList();
        }

        public List<Location> GetUnplacedLocations()
        {
           return locations.Values.Where(location => location.Item == null && !location.IsLocked).ToList();
        }

        public List<Location> GetUnplacedLocationsOfType(LocationType type)
        {
            return locations.Values.Where(location => location.Item == null && !location.IsLocked && location.LocationType == type).ToList();
        }

        public List<Location> GetPlacedRequiredItemLocations()
        {
            return locations.Values.Where(location => location.Item != null && location.Item.IsRequired).ToList();
        }

        private ItemID GetAmmoItemID(ItemID itemID)
        {
            switch (itemID)
            {
                case ItemID.Shuriken: return ItemID.ShurikenAmmo;
                case ItemID.RollingShuriken: return ItemID.RollingShurikenAmmo;
                case ItemID.EarthSpear: return ItemID.EarthSpearAmmo;
                case ItemID.Flare: return ItemID.FlareAmmo;
                case ItemID.Caltrops: return ItemID.CaltropsAmmo;
                case ItemID.Chakram: return ItemID.ChakramAmmo;
                case ItemID.Bomb: return ItemID.BombAmmo;
                case ItemID.Pistol: return ItemID.PistolAmmo;
                default: return ItemID.None;
            }
        }

        private void PlaceShopItems(List<Item> shopItems, List<Item> items)
        {
            if(Settings.ShopPlacement == ShopPlacement.Random)
            {
                RandomiseWithChecks(GetUnplacedLocationsOfType(LocationType.Shop), shopItems, items);
            }
            else if(Settings.ShopPlacement == ShopPlacement.AtLeastOne)
            {
                //lock the third slot of each shop so there will be atleast one item in to buy in each shop
                foreach(var location in GetUnplacedLocationsOfType(LocationType.Shop)) {
                    if (location.Name.Contains("3"))
                        location.IsLocked = true;
                }

                RandomiseWithChecks(GetUnplacedLocationsOfType(LocationType.Shop), shopItems, items);

                //now unlock all the shop slots that were locked
                foreach (var location in GetLocationsOfType(LocationType.Shop))
                        location.IsLocked = false;
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

        private void PlaceMantras(List<Item> items)
        {
            if(Settings.MantraPlacement == MantraPlacement.Original)
            {
                //put the mantras where they are originally if they arent randomised
                GetLocation("Heaven Mantra Mural").PlaceItem(ItemPool.GetAndRemove(ItemID.Heaven, items));
                GetLocation("Earth Mantra Mural").PlaceItem(ItemPool.GetAndRemove(ItemID.Earth, items));
                GetLocation("Sun Mantra Mural").PlaceItem(ItemPool.GetAndRemove(ItemID.Sun, items));
                GetLocation("Moon Mantra Mural").PlaceItem(ItemPool.GetAndRemove(ItemID.Moon, items));
                GetLocation("Sea Mantra Mural").PlaceItem(ItemPool.GetAndRemove(ItemID.Sea, items));
                GetLocation("Fire Mantra Mural").PlaceItem(ItemPool.GetAndRemove(ItemID.Fire, items));
                GetLocation("Wind Mantra Mural").PlaceItem(ItemPool.GetAndRemove(ItemID.Wind, items));
                GetLocation("Mother Mantra Mural").PlaceItem(ItemPool.GetAndRemove(ItemID.Mother, items));
                GetLocation("Child Mantra Mural").PlaceItem(ItemPool.GetAndRemove(ItemID.Child, items));
                GetLocation("Night Mantra Mural").PlaceItem(ItemPool.GetAndRemove(ItemID.Night, items));
            }
            else if(Settings.MantraPlacement == MantraPlacement.OnlyMurals)
            {
                List<Item> mantras = Shuffle.FisherYates(ItemPool.GetAndRemoveMantras(items), random);
                RandomiseWithChecks(GetUnplacedLocationsOfType(LocationType.Mural), mantras, items);
            }
        }

        private void RandomiseAssumedFill(List<Location> locations, List<Item> itemsToPlace)
        {
            PlayerState state;

            while (itemsToPlace.Count > 0)
            {
                Item item = itemsToPlace.Last();
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
                    Logger.Log($"Failed to place item {item.Name}.");
                    Logger.Log($"Total items left to place {itemsToPlace.Count}.");
                    break;
                }
            }
        }

        private void RandomiseWithChecks(List<Location> locations, List<Item> itemsToPlace, List<Item> currentItems)
        {
            PlayerState state;

            while (itemsToPlace.Count > 0)
            {
                Item item = itemsToPlace.Last();
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
                    Logger.Log($"Failed to place item {item.Name}.");
                    Logger.Log($"Total items left to place {itemsToPlace.Count}.");
                    break;
                }
            }
        }

        private void RandomiseWithoutChecks(List<Location> locations, List<Item> itemsToPlace)
        {
            locations = Shuffle.FisherYates(locations, random);

            while (itemsToPlace.Count > 0)
            {
                Item item = itemsToPlace.Last();
                Location location = locations.Last();

                itemsToPlace.Remove(item);
                locations.Remove(location);
                location.PlaceItem(item);
            }
        }

        private void RandomiseCurses()
        {
            CursedLocations = new List<Location>();
            if (Settings.RandomCurses)
            {
                var chestLocations = Shuffle.FisherYates(GetUnplacedLocationsOfType(LocationType.Chest), random);
                for (int i = 0; i < 4; i++)
                {
                    Location location = chestLocations[i];
                    location.AppendLogicString("and Has(Mulana Talisman)");
                    location.BuildLogicTree();
                    CursedLocations.Add(location);
                }
            }
            else
            {
                var defaultLocations = new List<string> { "Flame Torc Chest", "Giants Flutes Chest", "Destiny Tablet Chest", "Power Band Chest" };
                foreach (string locationName in defaultLocations)
                {
                    Location location = GetLocation(locationName);
                    location.AppendLogicString("and Has(Mulana Talisman)");
                    location.BuildLogicTree();
                    CursedLocations.Add(location);
                }
            }
        }

        private List<Exit> GetConnectionsOfType(ExitType type)
        {
            return exits.Where(connection => connection.ExitType == type).ToList();
        }

        private void RandomiseHorizontalEntrances()
        {
            List<Exit> leftDoors = Shuffle.FisherYates(GetConnectionsOfType(ExitType.LeftDoor), random);
            List<Exit> rightDoors = Shuffle.FisherYates(GetConnectionsOfType(ExitType.RightDoor), random);

            //place the cliff first to avoid cliff to village if needed, then place one of the cavern sides to stop it
            //from looping on itself
            List<Exit> priorityLeftDoors = new List<Exit>
            {
                leftDoors.Find(x => x.ID == ExitID.fP02Left),
                leftDoors.Find(x => x.ID == ExitID.fP00Left)
            };

            bool cavernToCliff = false;
            bool cavernToVillage = false;
            Exit leftDoor = null;
            Exit rightDoor = null;

            while (leftDoors.Count > 0)
            {
                if (priorityLeftDoors.Count > 0)
                {
                    leftDoor = priorityLeftDoors.First();
                    priorityLeftDoors.Remove(leftDoor);
                }
                else
                {
                    leftDoor = leftDoors[random.Next(leftDoors.Count)];
                }

                if (leftDoor.ID == ExitID.fP02Left)
                {
                    do
                    {
                        rightDoor = rightDoors[random.Next(rightDoors.Count)];
                    } while ((rightDoor.ID == ExitID.f01Right && (!Settings.RandomLadderEntraces || Settings.ReduceDeadEndStarts)) || rightDoor.ID == ExitID.fL08Right);
                }
                else if (leftDoor.ID == ExitID.fP00Left)
                {
                    do
                    {
                        rightDoor = rightDoors[random.Next(rightDoors.Count)];
                    } while (rightDoor.ID == ExitID.fP00Right || (((rightDoor.ID == ExitID.f01Right && (!Settings.RandomLadderEntraces || Settings.ReduceDeadEndStarts))
                                || rightDoor.ID == ExitID.fL08Right) && cavernToCliff));
                }
                else
                {
                    rightDoor = rightDoors[random.Next(rightDoors.Count)];
                }

                if (leftDoor.ID == ExitID.fP02Left && rightDoor.ID == ExitID.fP00Right)
                    cavernToCliff = true;

                if (rightDoor.ID == ExitID.f01Right && (leftDoor.ID == ExitID.fP00Left && !cavernToCliff))
                    cavernToVillage = true;

                if (rightDoor.ID == ExitID.f01Right && (leftDoor.ID == ExitID.fP02Left || (leftDoor.ID == ExitID.fP00Left && cavernToCliff)))
                    villageDeadEnd = true;

                if (leftDoor.ID == ExitID.fL02Left)
                {
                    if (rightDoor.ID == ExitID.f01Right || (rightDoor.ID == ExitID.fP00Right && cavernToVillage))
                    {
                        rightDoor.AppendLogicString(" and CanWarp");
                        rightDoor.BuildLogicTree();
                    }
                    else
                    {
                        rightDoor.AppendLogicString(" and (CanWarp or CanReach(Annwfn Main))");
                        rightDoor.BuildLogicTree();
                    }
                }

                leftDoor.ConnectingAreaName = rightDoor.ParentAreaName;
                rightDoor.ConnectingAreaName = leftDoor.ParentAreaName;

                ExitPairs.Add((leftDoor.ID, rightDoor.ID));
                HorizontalPairs.Add($"    {leftDoor.Name} - {rightDoor.Name}");
                leftDoors.Remove(leftDoor);
                rightDoors.Remove(rightDoor);
            }
        }

        private void RandomiseLadderEntrances()
        {
            List<Exit> downLadders = Shuffle.FisherYates(GetConnectionsOfType(ExitType.DownLadder), random);
            List<Exit> upLadders = Shuffle.FisherYates(GetConnectionsOfType(ExitType.UpLadder), random);

            //these are one way down ladders so they need to be placed first to avoid being placed with inferno cavern
            List<Exit> priorityDownLadders = new List<Exit>()
            {
                downLadders.Find(x => x.ID == ExitID.f02Down),
                downLadders.Find(x => x.ID == ExitID.f03Down2)
            };

            //if the village door leads to the cliff then the ladder can't lead to inferno cavern
            if (villageDeadEnd)
                priorityDownLadders.Add(downLadders.Find(x => x.ID == ExitID.f01Down));

            Exit downLadder = null;
            Exit upLadder = null;

            while (upLadders.Count > 0)
            {
                if (priorityDownLadders.Count > 0)
                {
                    downLadder = priorityDownLadders.First();
                    priorityDownLadders.Remove(downLadder);
                }
                else
                {
                    downLadder = downLadders[random.Next(downLadders.Count)];
                }

                if (downLadder.ID == ExitID.f02Down || downLadder.ID == ExitID.f03Down2)
                {
                    do
                    {
                        upLadder = upLadders[random.Next(upLadders.Count)];
                    } while (upLadder.ID == ExitID.fL05Up);
                }
                else if (downLadder.ID == ExitID.f01Down)
                {
                    do
                    {
                        upLadder = upLadders[random.Next(upLadders.Count)];
                    } while ((upLadder.ID == ExitID.fL05Up || upLadder.ID == ExitID.f04Up3) && villageDeadEnd);
                }
                else
                {
                    upLadder = upLadders[random.Next(upLadders.Count)];
                }

                if (downLadder.ID == ExitID.f02Down)
                {
                    upLadder.AppendLogicString(" and (CanReach(Annwfn Main) or CanWarp)");
                    upLadder.BuildLogicTree();
                }
                else if (downLadder.ID == ExitID.f03Down2)
                {
                    upLadder.AppendLogicString(" and Has(Life Sigil) and (CanWarp or CanReach(Immortal Battlefield Dinosaur))");
                    upLadder.BuildLogicTree();
                }
                else if (downLadder.ID == ExitID.f02Down)
                {
                    upLadder.AppendLogicString(" and HorizontalAttack");
                    upLadder.BuildLogicTree();
                }

                if (upLadder.ID == ExitID.f03Up)
                {
                    downLadder.AppendLogicString(" and (CanWarp or CanKill(Cetus) or CanReach(Immortal Battlefield Main))");
                    downLadder.BuildLogicTree();
                }
                else if(upLadder.ID == ExitID.f04Up3)
                {
                    downLadder.AppendLogicString(" and False");
                    downLadder.BuildLogicTree();
                }

                upLadder.ConnectingAreaName = downLadder.ParentAreaName;
                downLadder.ConnectingAreaName = upLadder.ParentAreaName;

                ExitPairs.Add((upLadder.ID, downLadder.ID));
                LadderPairs.Add($"    {upLadder.Name} - {downLadder.Name}");
                upLadders.Remove(upLadder);
                downLadders.Remove(downLadder);
            }
        }

        private void RandomiseGateEntrances()
        {
            List<Exit> gates = Shuffle.FisherYates(GetConnectionsOfType(ExitType.Gate), random);
            
            Exit gate1 = null;
            Exit gate2 = null;

            //place one side of illusion first to avoid making it loop on itself
            gate1 = gates.Find(x => x.ID == ExitID.fL11GateN);
            gates.Remove(gate1);
            do
            {
                gate2 = gates[random.Next(gates.Count)];
            } while (gate2.ID == ExitID.fL11GateY0);
            gates.Remove(gate2);

            FixGateLogic(gate1, gate2);
            FixGateLogic(gate2, gate1);

            gate1.ConnectingAreaName = gate2.ParentAreaName;
            gate2.ConnectingAreaName = gate1.ParentAreaName;

            ExitPairs.Add((gate1.ID, gate2.ID));
            GatePairs.Add($"    {gate1.Name} - {gate2.Name}");
            
            //these are all inaccessible unless you come through the gate itself so to they can't be placed together
            List<Exit> priorityGates = new List<Exit>()
            {
                gates.Find(x => x.ID == ExitID.f02GateYA),
                gates.Find(x => x.ID == ExitID.f03GateYC),
                gates.Find(x => x.ID == ExitID.f06GateP0),
                gates.Find(x => x.ID == ExitID.f12GateP0),
                gates.Find(x => x.ID == ExitID.f13GateP0)
            };

            foreach (Exit gate in priorityGates)
                gates.Remove(gate);

            while (gates.Count > 0)
            {
                if (priorityGates.Count > 0)
                {
                    gate1 = priorityGates.First();
                    priorityGates.Remove(gate1);
                }
                else
                {
                    gate1 = gates[random.Next(gates.Count)];
                    gates.Remove(gate1);
                }

                gate2 = gates[random.Next(gates.Count)];
                gates.Remove(gate2);

                FixGateLogic(gate1, gate2);
                FixGateLogic(gate2, gate1);

                gate1.ConnectingAreaName = gate2.ParentAreaName;
                gate2.ConnectingAreaName = gate1.ParentAreaName;

                ExitPairs.Add((gate1.ID, gate2.ID));
                GatePairs.Add($"    {gate1.Name} - {gate2.Name}");
            }
        }

        private void FixGateLogic(Exit gate1, Exit gate2)
        {
            switch (gate1.ID)
            {
                case ExitID.f00GateYA:
                {
                    gate2.AppendLogicString(" and False");
                    gate2.BuildLogicTree();
                    break;
                }
                case ExitID.f00GateYB:
                {
                    gate2.AppendLogicString(" and (CanWarp or CanKill(Nidhogg))");
                    gate2.BuildLogicTree();
                    break;
                }
                case ExitID.f00GateYC:
                {
                    gate2.AppendLogicString(" and (CanWarp or Has(Birth Sigil))");
                    gate2.BuildLogicTree();
                    break;
                }
                case ExitID.f06_2GateP0:
                {
                    gate2.AppendLogicString(" and (CanKill(Tezcatlipoca) and (CanWarp or Has(Grapple Claw)))");
                    gate2.BuildLogicTree();
                    break;
                }
                case ExitID.f07GateP0:
                {
                    gate2.AppendLogicString(" and (CanWarp or (Has(Pepper) and Has(Birth Sigil) and CanChant(Sun) and CanKill(Unicorn)))");
                    gate2.BuildLogicTree();
                    break;
                }
                case ExitID.f09GateP0:
                {
                    gate2.AppendLogicString(" and CanWarp");
                    gate2.BuildLogicTree();
                    break;
                }
            }
        }

        private void FullRandomEntrances()
        {
            List<Exit> entrances = new List<Exit>();
            List<Exit> priorityEntrances = new List<Exit>();
            if (Settings.RandomHorizontalEntraces)
            {
                entrances.AddRange(GetConnectionsOfType(ExitType.LeftDoor));
                entrances.AddRange(GetConnectionsOfType(ExitType.RightDoor));
            }

            if (Settings.RandomLadderEntraces)
            {
                entrances.AddRange(GetConnectionsOfType(ExitType.DownLadder));
                entrances.AddRange(GetConnectionsOfType(ExitType.UpLadder));
            }

            if (Settings.RandomGateEntraces)
            {
                entrances.AddRange(GetConnectionsOfType(ExitType.Gate));
            }

            Exit entrance1 = null;
            Exit entrance2 = null;

            bool cavernToCliff = false;
            bool illusionToCliff = false;

            //place the cliff  first just to see if gets placed anywhere that affects any other placement logic and then
            //place one of the cavern entrances to stop it looping on itself
            if (Settings.RandomHorizontalEntraces)
            {
                entrance1 = entrances.Find(x => x.ID == ExitID.fP02Left);
                entrances.Remove(entrance1);
                do
                {
                    entrance2 = entrances[random.Next(entrances.Count)];
                } while ((entrance2.ID == ExitID.f01Right && (!Settings.RandomLadderEntraces || Settings.ReduceDeadEndStarts)) || entrance2.IsInaccessible() 
                            || entrance2.ID == ExitID.f06_2GateP0);   
                
                entrances.Remove(entrance2);

                if (entrance2.ID == ExitID.fP00Right || entrance2.ID == ExitID.fP00Left)
                {
                    cavernToCliff = true;
                }
                else if (entrance2.ID == ExitID.fL11GateN || entrance2.ID == ExitID.fL11GateY0)
                {
                    illusionToCliff = true;
                }

                if (entrance2.ID == ExitID.f01Right)
                    villageDeadEnd = true;

                FixFullRandomEntranceLogic(entrance1, entrance2);
                FixFullRandomEntranceLogic(entrance2, entrance1);

                entrance1.ConnectingAreaName = entrance2.ParentAreaName;
                entrance2.ConnectingAreaName = entrance1.ParentAreaName;

                ExitPairs.Add((entrance1.ID, entrance2.ID));
                EntrancePairs.Add($"    {entrance1.Name} - {entrance2.Name}");

                entrance1 = entrances.Find(x => x.ID == ExitID.fP00Left);
                if(entrance1 != null)
                {
                    entrances.Remove(entrance1);
                    do
                    {
                        entrance2 = entrances[random.Next(entrances.Count)];
                    } while (entrance2.ID == ExitID.fP00Right || (entrance2.ID == ExitID.f01Right && cavernToCliff && (!Settings.RandomLadderEntraces || Settings.ReduceDeadEndStarts)));
                    entrances.Remove(entrance2);

                    if (entrance2.ID == ExitID.f01Right && cavernToCliff)
                        villageDeadEnd = true;

                    FixFullRandomEntranceLogic(entrance1, entrance2);
                    FixFullRandomEntranceLogic(entrance2, entrance1);

                    entrance1.ConnectingAreaName = entrance2.ParentAreaName;
                    entrance2.ConnectingAreaName = entrance1.ParentAreaName;

                    ExitPairs.Add((entrance1.ID, entrance2.ID));
                    EntrancePairs.Add($"    {entrance1.Name} - {entrance2.Name}");
                }

                if (Settings.ReduceDeadEndStarts)
                {
                    entrance1 = entrances.Find(x => x.ID == ExitID.f01Right);
                    if (entrance1 != null)
                    {
                        entrances.Remove(entrance1);
                        do
                        {
                            entrance2 = entrances[random.Next(entrances.Count)];
                        } while (entrance2.ID == ExitID.fL08Right || entrance2.ID == ExitID.fL05Up || entrance2.ID == ExitID.fLGate || entrance2.ID == ExitID.f00GateYA
                                    || entrance2.ID == ExitID.f04Up3 || entrance2.ID == ExitID.f06_2GateP0 || entrance2.ID == ExitID.f03Down3 || entrance2.ID == ExitID.f00Down
                                    || entrance2.ID == ExitID.f03Down1 || ((entrance2.ID == ExitID.fL11GateN || entrance2.ID == ExitID.fL11GateY0) && illusionToCliff));
                        entrances.Remove(entrance2);

                        FixFullRandomEntranceLogic(entrance1, entrance2);
                        FixFullRandomEntranceLogic(entrance2, entrance1);

                        entrance1.ConnectingAreaName = entrance2.ParentAreaName;
                        entrance2.ConnectingAreaName = entrance1.ParentAreaName;

                        ExitPairs.Add((entrance1.ID, entrance2.ID));
                        EntrancePairs.Add($"    {entrance1.Name} - {entrance2.Name}");
                    }
                }
            }

            //try to place an illusion entrance to stop it looping on itself if its already placed it doesn't matter
            if (Settings.RandomGateEntraces)
            {
                entrance1 = entrances.Find(x => x.ID == ExitID.fL11GateN);
                if (entrance1 != null)
                {
                    entrances.Remove(entrance1);
                    do
                    {
                        entrance2 = entrances[random.Next(entrances.Count)];
                    } while (entrance2.ID == ExitID.fL11GateY0 || (entrance2.ID == ExitID.f01Right && illusionToCliff && (!Settings.RandomLadderEntraces || Settings.ReduceDeadEndStarts)));

                    entrances.Remove(entrance2);

                    FixFullRandomEntranceLogic(entrance1, entrance2);
                    FixFullRandomEntranceLogic(entrance2, entrance1);

                    entrance1.ConnectingAreaName = entrance2.ParentAreaName;
                    entrance2.ConnectingAreaName = entrance1.ParentAreaName;

                    ExitPairs.Add((entrance1.ID, entrance2.ID));
                    EntrancePairs.Add($"    {entrance1.Name} - {entrance2.Name}");
                }
            }

            if (Settings.RandomHorizontalEntraces)
            {
                priorityEntrances.Add(entrances.Find(x => x.ID == ExitID.fL08Right));
            }

            if (Settings.RandomLadderEntraces)
            {
                if(villageDeadEnd)
                    priorityEntrances.Add(entrances.Find(x => x.ID == ExitID.f01Down));

                priorityEntrances.Add(entrances.Find(x => x.ID == ExitID.f02Down));
                priorityEntrances.Add(entrances.Find(x => x.ID == ExitID.f03Down2));
                priorityEntrances.Add(entrances.Find(x => x.ID == ExitID.fL05Up));
            }

            if (Settings.RandomGateEntraces)
            {
                priorityEntrances.Add(entrances.Find(x => x.ID == ExitID.f02GateYA));
                priorityEntrances.Add(entrances.Find(x => x.ID == ExitID.f06GateP0));
                priorityEntrances.Add(entrances.Find(x => x.ID == ExitID.f12GateP0));
                priorityEntrances.Add(entrances.Find(x => x.ID == ExitID.f13GateP0));
                priorityEntrances.Add(entrances.Find(x => x.ID == ExitID.f03GateYC));
            }

            priorityEntrances.RemoveAll(x => x == null);

            foreach (Exit entrance in priorityEntrances)
                entrances.Remove(entrance);

            while (entrances.Count > 0)
            {
                if (priorityEntrances.Count > 0)
                {
                    entrance1 = priorityEntrances.First();
                    priorityEntrances.Remove(entrance1);
                }
                else
                {
                    entrance1 = entrances[random.Next(entrances.Count)];
                    entrances.Remove(entrance1);
                }

                if (entrance1.ID == ExitID.f01Down)
                {
                    do
                    {
                        entrance2 = entrances[random.Next(entrances.Count)];
                    } while ((entrance2.ID == ExitID.fL08Right || entrance2.ID == ExitID.fL05Up || entrance2.ID == ExitID.fLGate || entrance2.ID == ExitID.f00GateYA) && villageDeadEnd);
                }
                else
                {
                    do
                    {
                        entrance2 = entrances[random.Next(entrances.Count)];
                    } while (entrance1.ID == entrance2.ID);
                }

                entrances.Remove(entrance2);

                if ((entrance1.ID == ExitID.f01Right && (entrance2.ID == ExitID.fL08Right || entrance2.ID == ExitID.fL05Up)) ||
                    (entrance2.ID == ExitID.f01Right && (entrance1.ID == ExitID.fL08Right || entrance1.ID == ExitID.fL05Up)))
                    villageDeadEnd = true;

                FixFullRandomEntranceLogic(entrance1, entrance2);
                FixFullRandomEntranceLogic(entrance2, entrance1);

                entrance1.ConnectingAreaName = entrance2.ParentAreaName;
                entrance2.ConnectingAreaName = entrance1.ParentAreaName;

                ExitPairs.Add((entrance1.ID, entrance2.ID));
                EntrancePairs.Add($"    {entrance1.Name} - {entrance2.Name}");
            }
        }

        private void FixFullRandomEntranceLogic(Exit entrance1, Exit entrance2)
        {
            switch (entrance1.ID)
            {
                case ExitID.fL02Left:
                {
                    entrance2.AppendLogicString(" and (CanWarp or CanReach(Annwfn Main))");
                    entrance2.BuildLogicTree();
                    break;
                }
                case ExitID.f03Right:
                {
                    entrance2.AppendLogicString(" and (Has(Feather) or Has(Grapple Claw))");
                    entrance2.BuildLogicTree();
                    break;
                }
                case ExitID.f02Down:
                {
                    entrance2.AppendLogicString(" and CanWarp and HorizontalAttack");
                    entrance2.BuildLogicTree();
                    break;
                }
                case ExitID.f03Down2:
                {
                    entrance2.AppendLogicString(" and Has(Life Sigil) and (CanWarp or CanReach(Immortal Battlefield Dinosaur))");
                    entrance2.BuildLogicTree();
                    break;
                }

                case ExitID.f03Up:
                {
                    entrance2.AppendLogicString(" and (CanWarp or CanKill(Cetus) or CanReach(Immortal Battlefield Main))");
                    entrance2.BuildLogicTree();
                    break;
                }
                case ExitID.f04Up3:
                {
                    entrance2.AppendLogicString(" and False");
                    entrance2.BuildLogicTree();
                    break;
                }
                case ExitID.f00GateYA:
                {
                    entrance2.AppendLogicString(" and False");
                    entrance2.BuildLogicTree();
                    break;
                }
                case ExitID.f00GateYB:
                {
                    entrance2.AppendLogicString(" and (CanWarp or CanKill(Nidhogg))");
                    entrance2.BuildLogicTree();
                    break;
                }
                case ExitID.f00GateYC:
                {
                    entrance2.AppendLogicString(" and (CanWarp or Has(Birth Sigil))");
                    entrance2.BuildLogicTree();
                    break;
                }
                case ExitID.f06_2GateP0:
                {
                    entrance2.AppendLogicString(" and (CanKill(Tezcatlipoca) and (CanWarp or Has(Grapple Claw)))");
                    entrance2.BuildLogicTree();
                    break;
                }
                case ExitID.f07GateP0:
                {
                    entrance2.AppendLogicString(" and (CanWarp or (Has(Pepper) and Has(Birth Sigil) and CanChant(Sun) and CanKill(Unicorn)))");
                    entrance2.BuildLogicTree();
                    break;
                }
                case ExitID.f09GateP0:
                {
                    entrance2.AppendLogicString(" and CanWarp");
                    entrance2.BuildLogicTree();
                    break;
                }
            }
        }

        private void RandomiseSoulGateEntrances()
        {
            List<Exit> gates = Shuffle.FisherYates(GetConnectionsOfType(ExitType.SoulGate), random);
            List<int> soulAmounts = new List<int>() { 1, 2, 2, 3, 3, 5, 5, 5 };
            List<Exit> priorityGates = new List<Exit>()
            {
                gates.Find(x => x.ID == ExitID.f12GateN8)
            };

            Exit gate1 = null;
            Exit gate2 = null;

            if (Settings.IncludeNineGates)
            {
                gates.Find(x => x.ID == ExitID.f03GateN9);
                gates.Find(x => x.ID == ExitID.f08GateN8);
                soulAmounts.Add(9);
            }
            else
            {
                gate1 = gates.Find(x => x.ID == ExitID.f03GateN9);
                gate2 = gates.Find(x => x.ID == ExitID.f13GateN9);
                SoulGatePairs.Add((gate1, gate2, 9));
                gates.Remove(gate1);
                gates.Remove(gate2);
            }

            soulAmounts = Shuffle.FisherYates(soulAmounts, random);
            foreach (Exit exit in priorityGates)
                gates.Remove(exit);

            while (gates.Count > 0)
            {
                if(priorityGates.Count > 0)
                {
                    gate1 = priorityGates[random.Next(priorityGates.Count)];
                    priorityGates.Remove(gate1);
                }
                else
                {
                    gate1 = gates[random.Next(gates.Count)];
                    gates.Remove(gate1);
                }

                if (gate1.ID == ExitID.f12GateN8) 
                {
                    do
                    {
                        gate2 = gates[random.Next(gates.Count)];
                    } while (gate2.ID == ExitID.f03GateN9);
                }
                else
                {
                    gate2 = gates[random.Next(gates.Count)];
                }
                gates.Remove(gate2);

                int soulAmount = soulAmounts[random.Next(soulAmounts.Count)];
                soulAmounts.Remove(soulAmount);

                gate1.AppendLogicString($" and GuardianKills({soulAmount})");
                gate2.AppendLogicString($" and GuardianKills({soulAmount})");

                FixSoulGateLogic(gate1, gate2);
                FixSoulGateLogic(gate2, gate1);

                gate1.BuildLogicTree();
                gate2.BuildLogicTree();

                gate1.ConnectingAreaName = gate2.ParentAreaName;
                gate2.ConnectingAreaName = gate1.ParentAreaName;

                SoulGatePairs.Add((gate1, gate2, soulAmount));

                if (gate1.ID == ExitID.f14GateN6 || gate2.ID == ExitID.f14GateN6)
                {
                    Exit toHel = exits.Find(exit => exit.ConnectingAreaName.Equals("Eternal Prison Doom Hel"));
                    if (gate1.ID == ExitID.f04GateN6 || gate2.ID == ExitID.f04GateN6)
                    {
                        toHel.AppendLogicString($" and IsDead(Vidofnir) and GuardianKills({ soulAmount})");
                    }
                    else
                    {
                        toHel.AppendLogicString($" and GuardianKills({ soulAmount})");
                    }
                    toHel.BuildLogicTree();
                }
            }
        }

        private void FixSoulGateLogic(Exit gate1, Exit gate2)
        {
            if (gate1.ID == ExitID.f14GateN6)
            {
                gate2.AppendLogicString(" and CanWarp");
            }
            else if (gate1.ID == ExitID.f06GateN7)
            {
                gate2.AppendLogicString(" and Has(Feather) and Has(Claydoll Suit)");
            }
            else if (gate1.ID == ExitID.f12GateN8)
            {
                gate2.AppendLogicString("and (CanWarp or Has(Feather))");
            }
            else if(gate1.ID == ExitID.f13GateN9)
            {
                gate2.AppendLogicString("and False");
            }
        }


        private void VanillaItemPlacement(List<Item> items)
        {
            GetLocation("Xelpud Item").PlaceItem(ItemPool.GetAndRemove(ItemID.Xelputter, items));
            GetLocation("Nebur Item").PlaceItem(ItemPool.GetAndRemove(ItemID.Map16, items));
            GetLocation("Alsedana Item").PlaceItem(ItemPool.GetAndRemove(ItemID.Beherit, items));
            GetLocation("Giltoriyo Item").PlaceItem(ItemPool.GetAndRemove(ItemID.MulanaTalisman, items));
            GetLocation("Freyas Item").PlaceItem(ItemPool.GetAndRemove(ItemID.FreyasPendant, items));
            GetLocation("Fobos Item").PlaceItem(ItemPool.GetAndRemove(ItemID.RuinsEncylopedia, items));
            GetLocation("Fobos Item 2").PlaceItem(ItemPool.GetAndRemove(ItemID.SkullReader, items));
            GetLocation("Mulbruk Item").PlaceItem(ItemPool.GetAndRemove(ItemID.SnowShoes, items));
            GetLocation("Osiris Item").PlaceItem(ItemPool.GetAndRemove(ItemID.LightScythe, items));

            GetLocation("Djed Pillar Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.DjedPillar, items));
            GetLocation("Mjolnir Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.Mjolnir, items));
            GetLocation("Battery Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.AncientBattery, items));
            GetLocation("Lamp of Time Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.LampofTime, items));
            GetLocation("Pochette Key Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.PochetteKey, items));
            GetLocation("Pyramid Crystal Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.PyramidCrystal, items));
            GetLocation("Vessel Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.Vessel, items));
            GetLocation("Egg of Creation Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.EggofCreation, items));
            GetLocation("Giants Flutes Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.GiantsFlute, items));
            GetLocation("Cog of Antiquity Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.CogofAntiquity, items));

            GetLocation("Mobile Super X3 Item").PlaceItem(ItemPool.GetAndRemove(ItemID.MobileSuperx3P, items));
            GetLocation("Shell Horn Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.ShellHorn, items));
            GetLocation("Holy Grail Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.HolyGrail, items));
            GetLocation("Fairy Guild Pass Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.FairyPass, items));
            GetLocation("Glove Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.Gloves, items));
            GetLocation("Dinosaur Figure Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.DinosaurFigure, items));
            GetLocation("Gale Fibula Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.GaleFibula, items));
            GetLocation("Flame Torc Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.FlameTorc, items));
            GetLocation("Vajra Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.Vajra, items));
            GetLocation("Power Band Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.PowerBand, items));
            GetLocation("Bronze Mirror Spot").PlaceItem(ItemPool.GetAndRemove(ItemID.BronzeMirror, items));
            GetLocation("Perfume Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.Perfume, items));
            GetLocation("Ice Cloak Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.IceCloak, items));
            GetLocation("Nemean Fur Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.NemeanFur, items));
            GetLocation("Gauntlet Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.Gauntlet, items));
            GetLocation("Anchor Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.Anchor, items));
            GetLocation("Totem Pole Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.TotemPole, items));
            GetLocation("Grapple Claw Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.GrappleClaw, items));
            GetLocation("Spaulder Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.Spaulder, items));
            GetLocation("Scalesphere Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.Scalesphere, items));
            GetLocation("Crucifix Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.Crucifix, items));
            GetLocation("Maats Feather Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.MaatsFeather, items));
            GetLocation("Ring Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.Ring, items));
            GetLocation("Feather Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.Feather, items));
            GetLocation("Scriptures Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.Scriptures, items));
            GetLocation("Frey Ship").PlaceItem(ItemPool.GetAndRemove(ItemID.FreysShip, items));
            GetLocation("Book of the Dead Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.BookoftheDead, items));
            GetLocation("Destiny Tablet Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.DestinyTablet, items));
            GetLocation("Secret Treasure of Life Item").PlaceItem(ItemPool.GetAndRemove(ItemID.SecretTreasureofLife, items));
            GetLocation("Origin Seal Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.OriginSigil, items));
            GetLocation("Birth Sigil Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.BirthSigil, items));
            GetLocation("Life Sigil Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.LifeSigil, items));
            GetLocation("Death Sigil Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.DeathSigil, items));
            GetLocation("Claydoll Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.ClaydollSuit, items));

            GetLocation("Knife Puzzle Reward").PlaceItem(ItemPool.GetAndRemove(ItemID.Knife, items));
            GetLocation("Rapier Puzzle Reward").PlaceItem(ItemPool.GetAndRemove(ItemID.Rapier, items));
            GetLocation("Axe Puzzle Reward").PlaceItem(ItemPool.GetAndRemove(ItemID.Axe, items));
            GetLocation("Katana Puzzle Reward").PlaceItem(ItemPool.GetAndRemove(ItemID.Katana, items));
            GetLocation("Shuriken Puzzle Reward").PlaceItem(ItemPool.GetAndRemove(ItemID.Shuriken, items));
            GetLocation("Rolling Shuriken Puzzle Reward").PlaceItem(ItemPool.GetAndRemove(ItemID.RollingShuriken, items));
            GetLocation("Earth Spear Puzzle Reward").PlaceItem(ItemPool.GetAndRemove(ItemID.EarthSpear, items));
            GetLocation("Flare Puzzle Reward").PlaceItem(ItemPool.GetAndRemove(ItemID.Flare, items));
            GetLocation("Bomb Puzzle Reward").PlaceItem(ItemPool.GetAndRemove(ItemID.Bomb, items));
            GetLocation("Chakram Puzzle Reward").PlaceItem(ItemPool.GetAndRemove(ItemID.Chakram, items));
            GetLocation("Caltrop Puzzle Reward").PlaceItem(ItemPool.GetAndRemove(ItemID.Caltrops, items));

            GetLocation("Yagoo Map Street Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.YagooMapStreet, items));
            GetLocation("Mantra Mural").PlaceItem(ItemPool.GetAndRemove(ItemID.Mantra, items));
            GetLocation("Beo Eglana Mural").PlaceItem(ItemPool.GetAndRemove(ItemID.BeoEglana, items));
            GetLocation("Death Village Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.DeathVillage, items));
            GetLocation("Miracle Witch Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.MiracleWitch, items));
            GetLocation("La Mulana Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.LaMulana, items));
            GetLocation("La Mulana 2 Chest").PlaceItem(ItemPool.GetAndRemove(ItemID.LaMulana2, items));

            GetLocation("Sacred Orb VoD").PlaceItem(ItemPool.GetAndRemove(ItemID.SacredOrb0, items));
            GetLocation("Sacred Orb Chest RoY").PlaceItem(ItemPool.GetAndRemove(ItemID.SacredOrb1, items));
            GetLocation("Sacred Orb Chest Annwfn").PlaceItem(ItemPool.GetAndRemove(ItemID.SacredOrb2, items));
            GetLocation("Sacred Orb Chest IB").PlaceItem(ItemPool.GetAndRemove(ItemID.SacredOrb3, items));
            GetLocation("Sacred Orb Chest IT").PlaceItem(ItemPool.GetAndRemove(ItemID.SacredOrb4, items));
            GetLocation("Sacred Orb Chest DF").PlaceItem(ItemPool.GetAndRemove(ItemID.SacredOrb5, items));
            GetLocation("Sacred Orb Chest SotFG").PlaceItem(ItemPool.GetAndRemove(ItemID.SacredOrb6, items));
            GetLocation("Sacred Orb Chest GotD").PlaceItem(ItemPool.GetAndRemove(ItemID.SacredOrb7, items));
            GetLocation("Sacred Orb Chest TS").PlaceItem(ItemPool.GetAndRemove(ItemID.SacredOrb8, items));
            GetLocation("Sacred Orb Chest HL").PlaceItem(ItemPool.GetAndRemove(ItemID.SacredOrb9, items));

            GetLocation("Map Chest RoY").PlaceItem(ItemPool.GetAndRemove(ItemID.Map1, items));
            GetLocation("Map Chest Annwfn").PlaceItem(ItemPool.GetAndRemove(ItemID.Map2, items));
            GetLocation("Map Chest IB").PlaceItem(ItemPool.GetAndRemove(ItemID.Map3, items));
            GetLocation("Map Chest IT").PlaceItem(ItemPool.GetAndRemove(ItemID.Map4, items));
            GetLocation("Map Chest DF").PlaceItem(ItemPool.GetAndRemove(ItemID.Map5, items));
            GetLocation("Map Chest SotFG").PlaceItem(ItemPool.GetAndRemove(ItemID.Map6, items));
            GetLocation("Map Chest GotD").PlaceItem(ItemPool.GetAndRemove(ItemID.Map7, items));
            GetLocation("Map Chest TS").PlaceItem(ItemPool.GetAndRemove(ItemID.Map8, items));
            GetLocation("Map Chest HL").PlaceItem(ItemPool.GetAndRemove(ItemID.Map9, items));
            GetLocation("Map Chest Valhalla").PlaceItem(ItemPool.GetAndRemove(ItemID.Map10, items));
            GetLocation("Map Chest DLM").PlaceItem(ItemPool.GetAndRemove(ItemID.Map11, items));
            GetLocation("Map Chest AC").PlaceItem(ItemPool.GetAndRemove(ItemID.Map12, items));
            GetLocation("Map Chest HoM").PlaceItem(ItemPool.GetAndRemove(ItemID.Map13, items));
            GetLocation("Map Chest EPG").PlaceItem(ItemPool.GetAndRemove(ItemID.Map14, items));
            GetLocation("Map Chest EPD").PlaceItem(ItemPool.GetAndRemove(ItemID.Map15, items));

            GetLocation("Ankh Chest RoY").PlaceItem(ItemPool.GetAndRemove(ItemID.AnkhJewel2, items));
            GetLocation("Ankh Chest DF").PlaceItem(ItemPool.GetAndRemove(ItemID.AnkhJewel3, items));
            GetLocation("Ankh Chest IT").PlaceItem(ItemPool.GetAndRemove(ItemID.AnkhJewel4, items));
            GetLocation("Ankh Chest SotFG").PlaceItem(ItemPool.GetAndRemove(ItemID.AnkhJewel5, items));
            GetLocation("Ankh Chest DLM").PlaceItem(ItemPool.GetAndRemove(ItemID.AnkhJewel6, items));
            GetLocation("Ankh Chest AC").PlaceItem(ItemPool.GetAndRemove(ItemID.AnkhJewel7, items));
            GetLocation("Ankh Jewel").PlaceItem(ItemPool.GetAndRemove(ItemID.AnkhJewel9, items));

            GetLocation("Crystal Skull Chest RoY").PlaceItem(ItemPool.GetAndRemove(ItemID.CrystalSkull1, items));
            GetLocation("Crystal Skull Chest Annwfn").PlaceItem(ItemPool.GetAndRemove(ItemID.CrystalSkull2, items));
            GetLocation("Crystal Skull IB").PlaceItem(ItemPool.GetAndRemove(ItemID.CrystalSkull3, items));
            GetLocation("Crystal Skull Chest IT").PlaceItem(ItemPool.GetAndRemove(ItemID.CrystalSkull4, items));
            GetLocation("Crystal Skull Chest Valhalla").PlaceItem(ItemPool.GetAndRemove(ItemID.CrystalSkull5, items));
            GetLocation("Crystal Skull Chest GotD").PlaceItem(ItemPool.GetAndRemove(ItemID.CrystalSkull6, items));
            GetLocation("Crystal Skull Chest TS").PlaceItem(ItemPool.GetAndRemove(ItemID.CrystalSkull7, items));
            GetLocation("Crystal Skull Chest DLM").PlaceItem(ItemPool.GetAndRemove(ItemID.CrystalSkull8, items));
            GetLocation("Crystal Skull Chest AC").PlaceItem(ItemPool.GetAndRemove(ItemID.CrystalSkull9, items));
            GetLocation("Crystal Skull Chest HL").PlaceItem(ItemPool.GetAndRemove(ItemID.CrystalSkull10, items));
            GetLocation("Crystal Skull Chest HoM").PlaceItem(ItemPool.GetAndRemove(ItemID.CrystalSkull11, items));
            GetLocation("Crystal Skull Chest EPD").PlaceItem(ItemPool.GetAndRemove(ItemID.CrystalSkull12, items));

            GetLocation("Chain Whip Puzzle Reward").PlaceItem(ItemPool.GetAndRemove(ItemID.ChainWhip, items));
            GetLocation("Flail Whip Puzzle Reward").PlaceItem(ItemPool.GetAndRemove(ItemID.FlailWhip, items));
            GetLocation("Silver Shield Puzzle Reward").PlaceItem(ItemPool.GetAndRemove(ItemID.SilverShield, items));
        }
    }
}
