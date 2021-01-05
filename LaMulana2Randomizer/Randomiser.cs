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
            worldData = FileUtils.LoadWorldData();

            foreach (JsonArea areaData in worldData)
            {
                Area area = new Area(areaData);

                foreach (JsonLocation locationData in areaData.Locations)
                {
                    Location location = new Location(locationData, area.Name);

                    if (Settings.HardBosses && (location.LocationType == LocationType.Guardian || 
                                                location.LocationType == LocationType.Miniboss))
                        location.UseHardRules();

                    location.BuildLogicTree();
                    locations.Add(location.Name, location);
                }
                areas.Add(area.Name, area); 
            }

            RandomiseCurses();

            //Set the amount of Skull required for Nibiru Dissonance
            var nibiruDiss = GetLocation("Dissonance (Nibiru)");
            nibiruDiss.AppendLogicString($" and SkullCount({Settings.RequiredSkulls})");
            nibiruDiss.BuildLogicTree();
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

            if (Settings.FullRandomEntrances)
            {
                FullRandomEntrances();
            }
            else
            {
                if (Settings.RandomHorizontalEntraces)
                    RandomiseHorizontalEntrances();

                if (Settings.RandomLadderEntraces)
                    RandomiseLadderEntrances();

                if (Settings.RandomGateEntraces)
                    RandomiseGateEntrances();
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
            ItemPool itemPool = new ItemPool(FileUtils.LoadItemFile());

            if (Settings.ShopPlacement == ShopPlacement.Original)
                PlaceShopItems(itemPool);

            if (Settings.MantraPlacement == MantraPlacement.Original)
                PlaceMantras(itemPool);

            foreach (Item item in itemPool)
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

        public void ChooseStartingWeapon()
        {
            ItemID[] weapons = new ItemID[]{ ItemID.Whip1, ItemID.Knife, ItemID.Rapier, ItemID.Axe, ItemID.Katana, ItemID.Shuriken,
                ItemID.RollingShuriken, ItemID.EarthSpear, ItemID.Flare, ItemID.Caltrops, ItemID.Chakram, ItemID.Bomb, ItemID.Pistol };

            List<ItemID> selectedWeapons = weapons.Zip(Settings.GetWeaponChoices(), Tuple.Create).Where(w => w.Item2).Select(w => w.Item1).ToList();

            if (selectedWeapons.Count > 0)
                StartingWeaponID = selectedWeapons[random.Next(selectedWeapons.Count)];
            else
                StartingWeaponID = ItemID.None;
        }

        public bool PlaceItems()
        {
            ItemPool itemPool = new ItemPool(FileUtils.LoadItemFile());

            //remove the starting weapon from the item pool
            if (StartingWeaponID != ItemID.None)
                StartingWeapon = itemPool.GetAndRemove(StartingWeaponID);
            else
                StartingWeapon = new Item("No Weapon", ItemID.None, false);

            //Places weights at a starting shop so the player can buy them at the start of the game
            GetLocation("Nebur Shop 1").PlaceItem(itemPool.GetAndRemove(ItemID.Weights));

            //if we have a subweapon need to give the player ammo at a shop
            if (StartingWeaponID > ItemID.Katana)
                GetLocation("Nebur Shop 2").PlaceItem(itemPool.GetAndRemove(GetAmmoItemID(StartingWeaponID)));

            //place the weights and ammo in shops first since they can only be in shops
            PlaceShopItems(itemPool);

            //create a list of the items we want to place that are accessible from the start
            ItemPool earlyItems = new ItemPool();
            if (!Settings.RandomGrail) 
                earlyItems.Add(itemPool.GetAndRemove(ItemID.HolyGrail));
            if (!Settings.RandomScanner && Settings.ShopPlacement != ShopPlacement.Original) 
                earlyItems.Add(itemPool.GetAndRemove(ItemID.HandScanner));
            if (!Settings.RandomCodices && Settings.ShopPlacement != ShopPlacement.Original) 
                earlyItems.Add(itemPool.GetAndRemove(ItemID.Codices));
            if (!Settings.RandomFDC && Settings.ShopPlacement != ShopPlacement.Original) 
                earlyItems.Add(itemPool.GetAndRemove(ItemID.FutureDevelopmentCompany));

            //place these items now before anythting else
            RandomiseWithChecks(GetUnplacedLocations(), earlyItems, new ItemPool());

            //place mantras if they are not fully randomised
            if (!PlaceMantras(itemPool))
                return false;

            //place research if it is not randomised
            PlaceResearch(itemPool);

            //split the remaining items into required/non required
            ItemPool requiredItems = new ItemPool(itemPool.GetandRemoveRequiredItems());
            ItemPool nonRequiredItems = new ItemPool(itemPool.GetandRemoveNonRequiredItems());

            //place required items
            if (!RandomiseAssumedFill(GetUnplacedLocations(), requiredItems))
                return false;

            //place non requires items
            RandomiseWithoutChecks(GetUnplacedLocations(), nonRequiredItems);

            return true;
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

        public List<Location> GetLocations()
        {
            return locations.Values.ToList();
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

        public void PlaceShopItems(ItemPool itemPool)
        {
            if(Settings.ShopPlacement == ShopPlacement.Random)
            {
                //remove all the weights and ammo since a pool of those is randomly generated
                itemPool.GetAndRemoveShopOnlyItems();

                //lock this shop only items can go here since there is a forth slot
                GetLocation("Hiner Shop 3").IsLocked = true;

                RandomiseWithChecks(GetUnplacedLocationsOfType(LocationType.Shop), ItemPool.CreateRandomShopPool(random, StartingWeaponID > ItemID.Katana), itemPool);

                GetLocation("Hiner Shop 3").IsLocked = false;
            }
            else if(Settings.ShopPlacement == ShopPlacement.AtLeastOne)
            {
                //remove all the weights and ammo since a pool of those is randomly generated
                itemPool.GetAndRemoveShopOnlyItems();

                //lock the third slot of each shop so there will be atleast one item in to buy in each shop
                foreach (var location in GetUnplacedLocationsOfType(LocationType.Shop)) {
                    if (location.Name.Contains("3"))
                        location.IsLocked = true;
                }

                RandomiseWithChecks(GetUnplacedLocationsOfType(LocationType.Shop), ItemPool.CreateRandomShopPool(random, StartingWeaponID > ItemID.Katana), itemPool);

                //now unlock all the shop slots that were locked
                foreach (var location in GetLocationsOfType(LocationType.Shop))
                    location.IsLocked = false;
            }
            else 
            {
                //place items how they are originally
                //if we start with a subweapon this item has to go somewhere else since ammo is placed in slot 2
                if (StartingWeaponID <= ItemID.Katana)
                    GetLocation("Nebur Shop 2").PlaceItem(itemPool.GetAndRemove(ItemID.YagooMapReader));

                GetLocation("Nebur Shop 3").PlaceItem(itemPool.GetAndRemove(ItemID.TextTrax));

                GetLocation("Modro Shop 1").PlaceItem(itemPool.GetAndRemove(ItemID.Shield1));
                GetLocation("Modro Shop 2").PlaceItem(itemPool.GetAndRemove(ItemID.Pistol));
                GetLocation("Modro Shop 3").PlaceItem(itemPool.GetAndRemove(ItemID.PistolAmmo));

                GetLocation("Sidro Shop 1").PlaceItem(itemPool.GetAndRemove(ItemID.HandScanner));
                GetLocation("Sidro Shop 2").PlaceItem(itemPool.GetAndRemove(ItemID.ShurikenAmmo));
                GetLocation("Sidro Shop 3").PlaceItem(itemPool.GetAndRemove(ItemID.Pepper));

                GetLocation("Hiner Shop 1").PlaceItem(itemPool.GetAndRemove(ItemID.Weights));
                GetLocation("Hiner Shop 2").PlaceItem(itemPool.GetAndRemove(ItemID.Codices));
                GetLocation("Hiner Shop 3").PlaceItem(itemPool.GetAndRemove(ItemID.AnkhJewel1));
                GetLocation("Hiner Shop 4").PlaceItem(itemPool.GetAndRemove(ItemID.AnkhJewel8));

                GetLocation("Korobok Shop 1").PlaceItem(itemPool.GetAndRemove(ItemID.Weights));
                GetLocation("Korobok Shop 2").PlaceItem(itemPool.GetAndRemove(ItemID.ShurikenAmmo));
                GetLocation("Korobok Shop 3").PlaceItem(itemPool.GetAndRemove(ItemID.Guild));

                GetLocation("Shuhoka Shop 1").PlaceItem(itemPool.GetAndRemove(ItemID.Weights));
                GetLocation("Shuhoka Shop 2").PlaceItem(itemPool.GetAndRemove(ItemID.ShurikenAmmo));
                GetLocation("Shuhoka Shop 3").PlaceItem(itemPool.GetAndRemove(ItemID.Alert));

                GetLocation("Pym Shop 1").PlaceItem(itemPool.GetAndRemove(ItemID.Weights));
                GetLocation("Pym Shop 2").PlaceItem(itemPool.GetAndRemove(ItemID.RollingShurikenAmmo));
                GetLocation("Pym Shop 3").PlaceItem(itemPool.GetAndRemove(ItemID.Snapshot));

                GetLocation("Btk Shop 1").PlaceItem(itemPool.GetAndRemove(ItemID.Weights));
                GetLocation("Btk Shop 2").PlaceItem(itemPool.GetAndRemove(ItemID.CaltropsAmmo));
                GetLocation("Btk Shop 3").PlaceItem(itemPool.GetAndRemove(ItemID.EngaMusica));

                GetLocation("Mino Shop 1").PlaceItem(itemPool.GetAndRemove(ItemID.Weights));
                GetLocation("Mino Shop 2").PlaceItem(itemPool.GetAndRemove(ItemID.BombAmmo));
                GetLocation("Mino Shop 3").PlaceItem(itemPool.GetAndRemove(ItemID.LonelyHouseMoving));

                GetLocation("Bargain Duck Shop 1").PlaceItem(itemPool.GetAndRemove(ItemID.Weights));
                GetLocation("Bargain Duck Shop 2").PlaceItem(itemPool.GetAndRemove(ItemID.CaltropsAmmo));
                GetLocation("Bargain Duck Shop 3").PlaceItem(itemPool.GetAndRemove(ItemID.FutureDevelopmentCompany));

                GetLocation("Peibalusa Shop 1").PlaceItem(itemPool.GetAndRemove(ItemID.Weights));
                GetLocation("Peibalusa Shop 2").PlaceItem(itemPool.GetAndRemove(ItemID.EarthSpearAmmo));
                GetLocation("Peibalusa Shop 3").PlaceItem(itemPool.GetAndRemove(ItemID.RaceScanner));

                GetLocation("Hiro Roderick Shop 1").PlaceItem(itemPool.GetAndRemove(ItemID.Weights));
                GetLocation("Hiro Roderick Shop 2").PlaceItem(itemPool.GetAndRemove(ItemID.FlareAmmo));
                GetLocation("Hiro Roderick Shop 3").PlaceItem(itemPool.GetAndRemove(ItemID.Harp));

                GetLocation("Hydlit Shop 1").PlaceItem(itemPool.GetAndRemove(ItemID.Weights));
                GetLocation("Hydlit Shop 2").PlaceItem(itemPool.GetAndRemove(ItemID.BombAmmo));
                GetLocation("Hydlit Shop 3").PlaceItem(itemPool.GetAndRemove(ItemID.GaneshaTalisman));

                GetLocation("Aytum Shop 1").PlaceItem(itemPool.GetAndRemove(ItemID.Weights));
                GetLocation("Aytum Shop 2").PlaceItem(itemPool.GetAndRemove(ItemID.ShurikenAmmo));
                GetLocation("Aytum Shop 3").PlaceItem(itemPool.GetAndRemove(ItemID.BounceShot));

                GetLocation("Kero Shop 1").PlaceItem(itemPool.GetAndRemove(ItemID.Weights));
                GetLocation("Kero Shop 2").PlaceItem(itemPool.GetAndRemove(ItemID.PistolAmmo));
                GetLocation("Kero Shop 3").PlaceItem(itemPool.GetAndRemove(ItemID.RoseandCamelia));

                GetLocation("Ash Geen Shop 1").PlaceItem(itemPool.GetAndRemove(ItemID.Weights));
                GetLocation("Ash Geen Shop 2").PlaceItem(itemPool.GetAndRemove(ItemID.FlareAmmo));
                GetLocation("Ash Geen Shop 3").PlaceItem(itemPool.GetAndRemove(ItemID.MekuriMaster));

                GetLocation("Venom Shop 1").PlaceItem(itemPool.GetAndRemove(ItemID.Weights));
                GetLocation("Venom Shop 2").PlaceItem(itemPool.GetAndRemove(ItemID.CaltropsAmmo));
                GetLocation("Venom Shop 3").PlaceItem(itemPool.GetAndRemove(ItemID.SpaceCapstarII));

                GetLocation("Megarock Shop 1").PlaceItem(itemPool.GetAndRemove(ItemID.Weights));
                GetLocation("Megarock Shop 2").PlaceItem(itemPool.GetAndRemove(ItemID.EarthSpearAmmo));
                GetLocation("Megarock Shop 3").PlaceItem(itemPool.GetAndRemove(ItemID.Bracelet));

                GetLocation("FairyLan Shop 1").PlaceItem(itemPool.GetAndRemove(ItemID.Weights));
                GetLocation("FairyLan Shop 2").PlaceItem(itemPool.GetAndRemove(ItemID.ChakramAmmo));
                GetLocation("FairyLan Shop 3").PlaceItem(itemPool.GetAndRemove(ItemID.Shield3));
            }
        }

        public bool PlaceMantras(ItemPool itemPool)
        {
            if(Settings.MantraPlacement == MantraPlacement.Original)
            {
                //put the mantras where they are originally if they arent randomised
                GetLocation("Heaven Mantra Mural").PlaceItem(itemPool.GetAndRemove(ItemID.Heaven));
                GetLocation("Earth Mantra Mural").PlaceItem(itemPool.GetAndRemove(ItemID.Earth));
                GetLocation("Sun Mantra Mural").PlaceItem(itemPool.GetAndRemove(ItemID.Sun));
                GetLocation("Moon Mantra Mural").PlaceItem(itemPool.GetAndRemove(ItemID.Moon));
                GetLocation("Sea Mantra Mural").PlaceItem(itemPool.GetAndRemove(ItemID.Sea));
                GetLocation("Fire Mantra Mural").PlaceItem(itemPool.GetAndRemove(ItemID.Fire));
                GetLocation("Wind Mantra Mural").PlaceItem(itemPool.GetAndRemove(ItemID.Wind));
                GetLocation("Mother Mantra Mural").PlaceItem(itemPool.GetAndRemove(ItemID.Mother));
                GetLocation("Child Mantra Mural").PlaceItem(itemPool.GetAndRemove(ItemID.Child));
                GetLocation("Night Mantra Mural").PlaceItem(itemPool.GetAndRemove(ItemID.Night));
            }
            else if(Settings.MantraPlacement == MantraPlacement.OnlyMurals)
            {
                return RandomiseWithChecks(GetUnplacedLocationsOfType(LocationType.Mural), new ItemPool(itemPool.GetAndRemoveMantras()), itemPool);
            }

            return true;
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

        private void PlaceResearch(ItemPool itemPool)
        {
            if (!Settings.RandomResearch)
            {
                GetLocation("Research Annwfn").PlaceItem(itemPool.GetAndRemove(ItemID.Research1));
                GetLocation("Research IB Top Left").PlaceItem(itemPool.GetAndRemove(ItemID.Research2));
                GetLocation("Research IB Top Right").PlaceItem(itemPool.GetAndRemove(ItemID.Research3));
                GetLocation("Research IB Tent 1").PlaceItem(itemPool.GetAndRemove(ItemID.Research4));
                GetLocation("Research IB Tent 2").PlaceItem(itemPool.GetAndRemove(ItemID.Research5));
                GetLocation("Research IB Tent 3").PlaceItem(itemPool.GetAndRemove(ItemID.Research6));
                GetLocation("Research IB Pit").PlaceItem(itemPool.GetAndRemove(ItemID.Research7));
                GetLocation("Research IB Left").PlaceItem(itemPool.GetAndRemove(ItemID.Research8));
                GetLocation("Research IT").PlaceItem(itemPool.GetAndRemove(ItemID.Research9));
                GetLocation("Research DSLM").PlaceItem(itemPool.GetAndRemove(ItemID.Research10));
            }
        }

        private bool RandomiseAssumedFill(List<Location> locations, ItemPool itemsToPlace)
        {
            PlayerState state;

            while (itemsToPlace.ItemCount > 0)
            {
                Item item = itemsToPlace.RandomGetAndRemove(random);
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
                    Logger.Log($"Total items left to place {itemsToPlace.ItemCount}.");
                    return false;
                }
            }

            return true;
        }

        private bool RandomiseWithChecks(List<Location> locations, ItemPool itemsToPlace, ItemPool currentItems)
        {
            PlayerState state;

            while (itemsToPlace.ItemCount > 0)
            {
                Item item = itemsToPlace.RandomGetAndRemove(random);
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
                    Logger.Log($"Total items left to place {itemsToPlace.ItemCount}.");
                    return false;
                }
            }

            return true;
        }

        private void RandomiseWithoutChecks(List<Location> locations, ItemPool itemsToPlace)
        {
            locations = Shuffle.FisherYates(locations, random);

            while (locations.Count > 0)
            {
                Item item = itemsToPlace.RandomGetAndRemove(random);
                Location location = locations.Last();

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

            priorityGates.RemoveAll(x => x == null);
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
                entrances.AddRange(GetConnectionsOfType(ExitType.Gate));

            if (Settings.IncludeUniqueTransitions)
            {
                entrances.AddRange(GetConnectionsOfType(ExitType.OneWay));
                entrances.AddRange(GetConnectionsOfType(ExitType.Pyramid));
            }

            Exit entrance1 = null;
            Exit entrance2 = null;

            bool cavernToCliff = false;
            bool illusionToCliff = false;

            if (Settings.RandomHorizontalEntraces)
            {
                //place the cliff  first just to see if gets placed anywhere that affects any other placement logic and then
                entrance1 = entrances.Find(x => x.ID == ExitID.fP02Left);
                entrances.Remove(entrance1);
                do
                {
                    entrance2 = entrances[random.Next(entrances.Count)];
                } while ((entrance2.ID == ExitID.f01Right && (!Settings.RandomLadderEntraces || Settings.ReduceDeadEndStarts)) || entrance2.IsInaccessible() 
                            || entrance2.ID == ExitID.f11Pyramid || entrance2.ID == ExitID.f09In);   
                
                entrances.Remove(entrance2);

                if (entrance2.ID == ExitID.fP00Right || entrance2.ID == ExitID.fP00Left)
                    cavernToCliff = true;
                else if (entrance2.ID == ExitID.fL11GateN || entrance2.ID == ExitID.fL11GateY0)
                    illusionToCliff = true;

                if (entrance2.ID == ExitID.f01Right)
                    villageDeadEnd = true;

                FixFullRandomEntranceLogic(entrance1, entrance2);
                FixFullRandomEntranceLogic(entrance2, entrance1);

                entrance1.ConnectingAreaName = entrance2.ParentAreaName;
                entrance2.ConnectingAreaName = entrance1.ParentAreaName;

                ExitPairs.Add((entrance1.ID, entrance2.ID));
                EntrancePairs.Add($"    {entrance1.Name} - {entrance2.Name}");

                //place one of the cavern entrances to stop it looping on itself
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

                //place main village entrance now if reduce dead end starts is on
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
                priorityEntrances.Add(entrances.Find(x => x.ID == ExitID.fL08Right));

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

            if (Settings.IncludeUniqueTransitions)
            {
                priorityEntrances.Add(entrances.Find(x => x.ID == ExitID.f03In));
                priorityEntrances.Add(entrances.Find(x => x.ID == ExitID.f09In));
                priorityEntrances.Add(entrances.Find(x => x.ID == ExitID.fNibiru));
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

                entrance2 = entrances[random.Next(entrances.Count)];
                entrances.Remove(entrance2);


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
                case ExitID.f03In:
                {
                    entrance2.AppendLogicString(" and CanWarp");
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
                case ExitID.f08Neck:
                {
                    entrance2.AppendLogicString("and (CanWarp or (CanChant(Heaven) and CanChant(Earth) and CanChant(Sea) and CanChant(Fire) and CanChant(Wind)))");
                    entrance2.BuildLogicTree();
                    break;
                }
                case ExitID.f09GateP0:
                {
                    entrance2.AppendLogicString(" and CanWarp");
                    entrance2.BuildLogicTree();
                    break;
                }
                case ExitID.f11Pyramid:
                {
                    entrance2.AppendLogicString(" and False");
                    entrance2.BuildLogicTree();
                    break;
                }
            }
        }

        private void RandomiseSoulGateEntrances()
        {
            List<Exit> gates = Shuffle.FisherYates(GetConnectionsOfType(ExitType.SoulGate), random);
            List<int> soulAmounts;

            //if we are doing random souls pairs we only need one of each soul value amount as we will just pick from this each time but never remove
            //otherwise we'll use the vanilla soul value amounts and remove after each one is picked
            if (Settings.RandomSoulPairs)
                soulAmounts = new List<int>() { 1, 2, 3, 5 };
            else
                soulAmounts = new List<int>() { 1, 2, 2, 3, 3, 5, 5, 5 };

            //the ancient chaos soul gate can't lead to the spiral boat so it has to be placed first to avoid this
            List<Exit> priorityGates = new List<Exit>()
            {
                gates.Find(x => x.ID == ExitID.f12GateN8)
            };

            Exit gate1 = null;
            Exit gate2 = null;

            //including the nine gates means we need to add the 9 into the list of soul amounts otherwise we'll place the nine gates like its vanilla
            if (Settings.IncludeNineGates)
            {
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

                gate2 = gates[random.Next(gates.Count)];
                gates.Remove(gate2);

                int soulAmount = soulAmounts[random.Next(soulAmounts.Count)];
                //only remove if we are using the vanilla list of soul pair values
                if(!Settings.RandomSoulPairs)
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

        private void VanillaItemPlacement(ItemPool itemPool)
        {
            GetLocation("Xelpud Item").PlaceItem(itemPool.GetAndRemove(ItemID.Xelputter));
            GetLocation("Nebur Item").PlaceItem(itemPool.GetAndRemove(ItemID.Map16));
            GetLocation("Alsedana Item").PlaceItem(itemPool.GetAndRemove(ItemID.Beherit));
            GetLocation("Giltoriyo Item").PlaceItem(itemPool.GetAndRemove(ItemID.MulanaTalisman));
            GetLocation("Freyas Item").PlaceItem(itemPool.GetAndRemove(ItemID.FreyasPendant));
            GetLocation("Fobos Item").PlaceItem(itemPool.GetAndRemove(ItemID.RuinsEncylopedia));
            GetLocation("Fobos Item 2").PlaceItem(itemPool.GetAndRemove(ItemID.SkullReader));
            GetLocation("Mulbruk Item").PlaceItem(itemPool.GetAndRemove(ItemID.SnowShoes));
            GetLocation("Osiris Item").PlaceItem(itemPool.GetAndRemove(ItemID.LightScythe));

            GetLocation("Djed Pillar Chest").PlaceItem(itemPool.GetAndRemove(ItemID.DjedPillar));
            GetLocation("Mjolnir Chest").PlaceItem(itemPool.GetAndRemove(ItemID.Mjolnir));
            GetLocation("Battery Chest").PlaceItem(itemPool.GetAndRemove(ItemID.AncientBattery));
            GetLocation("Lamp of Time Chest").PlaceItem(itemPool.GetAndRemove(ItemID.LampofTime));
            GetLocation("Pochette Key Chest").PlaceItem(itemPool.GetAndRemove(ItemID.PochetteKey));
            GetLocation("Pyramid Crystal Chest").PlaceItem(itemPool.GetAndRemove(ItemID.PyramidCrystal));
            GetLocation("Vessel Chest").PlaceItem(itemPool.GetAndRemove(ItemID.Vessel));
            GetLocation("Egg of Creation Chest").PlaceItem(itemPool.GetAndRemove(ItemID.EggofCreation));
            GetLocation("Giants Flutes Chest").PlaceItem(itemPool.GetAndRemove(ItemID.GiantsFlute));
            GetLocation("Cog of Antiquity Chest").PlaceItem(itemPool.GetAndRemove(ItemID.CogofAntiquity));

            GetLocation("Mobile Super X3 Item").PlaceItem(itemPool.GetAndRemove(ItemID.MobileSuperx3P));
            GetLocation("Shell Horn Chest").PlaceItem(itemPool.GetAndRemove(ItemID.ShellHorn));
            GetLocation("Holy Grail Chest").PlaceItem(itemPool.GetAndRemove(ItemID.HolyGrail));
            GetLocation("Fairy Guild Pass Chest").PlaceItem(itemPool.GetAndRemove(ItemID.FairyPass));
            GetLocation("Glove Chest").PlaceItem(itemPool.GetAndRemove(ItemID.Gloves));
            GetLocation("Dinosaur Figure Chest").PlaceItem(itemPool.GetAndRemove(ItemID.DinosaurFigure));
            GetLocation("Gale Fibula Chest").PlaceItem(itemPool.GetAndRemove(ItemID.GaleFibula));
            GetLocation("Flame Torc Chest").PlaceItem(itemPool.GetAndRemove(ItemID.FlameTorc));
            GetLocation("Vajra Chest").PlaceItem(itemPool.GetAndRemove(ItemID.Vajra));
            GetLocation("Power Band Chest").PlaceItem(itemPool.GetAndRemove(ItemID.PowerBand));
            GetLocation("Bronze Mirror Spot").PlaceItem(itemPool.GetAndRemove(ItemID.BronzeMirror));
            GetLocation("Perfume Chest").PlaceItem(itemPool.GetAndRemove(ItemID.Perfume));
            GetLocation("Ice Cloak Chest").PlaceItem(itemPool.GetAndRemove(ItemID.IceCloak));
            GetLocation("Nemean Fur Chest").PlaceItem(itemPool.GetAndRemove(ItemID.NemeanFur));
            GetLocation("Gauntlet Chest").PlaceItem(itemPool.GetAndRemove(ItemID.Gauntlet));
            GetLocation("Anchor Chest").PlaceItem(itemPool.GetAndRemove(ItemID.Anchor));
            GetLocation("Totem Pole Chest").PlaceItem(itemPool.GetAndRemove(ItemID.TotemPole));
            GetLocation("Grapple Claw Chest").PlaceItem(itemPool.GetAndRemove(ItemID.GrappleClaw));
            GetLocation("Spaulder Chest").PlaceItem(itemPool.GetAndRemove(ItemID.Spaulder));
            GetLocation("Scalesphere Chest").PlaceItem(itemPool.GetAndRemove(ItemID.Scalesphere));
            GetLocation("Crucifix Chest").PlaceItem(itemPool.GetAndRemove(ItemID.Crucifix));
            GetLocation("Maats Feather Chest").PlaceItem(itemPool.GetAndRemove(ItemID.MaatsFeather));
            GetLocation("Ring Chest").PlaceItem(itemPool.GetAndRemove(ItemID.Ring));
            GetLocation("Feather Chest").PlaceItem(itemPool.GetAndRemove(ItemID.Feather));
            GetLocation("Scriptures Chest").PlaceItem(itemPool.GetAndRemove(ItemID.Scriptures));
            GetLocation("Frey Ship").PlaceItem(itemPool.GetAndRemove(ItemID.FreysShip));
            GetLocation("Book of the Dead Chest").PlaceItem(itemPool.GetAndRemove(ItemID.BookoftheDead));
            GetLocation("Destiny Tablet Chest").PlaceItem(itemPool.GetAndRemove(ItemID.DestinyTablet));
            GetLocation("Secret Treasure of Life Item").PlaceItem(itemPool.GetAndRemove(ItemID.SecretTreasureofLife));
            GetLocation("Origin Seal Chest").PlaceItem(itemPool.GetAndRemove(ItemID.OriginSigil));
            GetLocation("Birth Sigil Chest").PlaceItem(itemPool.GetAndRemove(ItemID.BirthSigil));
            GetLocation("Life Sigil Chest").PlaceItem(itemPool.GetAndRemove(ItemID.LifeSigil));
            GetLocation("Death Sigil Chest").PlaceItem(itemPool.GetAndRemove(ItemID.DeathSigil));
            GetLocation("Claydoll Chest").PlaceItem(itemPool.GetAndRemove(ItemID.ClaydollSuit));

            GetLocation("Knife Puzzle Reward").PlaceItem(itemPool.GetAndRemove(ItemID.Knife));
            GetLocation("Rapier Puzzle Reward").PlaceItem(itemPool.GetAndRemove(ItemID.Rapier));
            GetLocation("Axe Puzzle Reward").PlaceItem(itemPool.GetAndRemove(ItemID.Axe));
            GetLocation("Katana Puzzle Reward").PlaceItem(itemPool.GetAndRemove(ItemID.Katana));
            GetLocation("Shuriken Puzzle Reward").PlaceItem(itemPool.GetAndRemove(ItemID.Shuriken));
            GetLocation("Rolling Shuriken Puzzle Reward").PlaceItem(itemPool.GetAndRemove(ItemID.RollingShuriken));
            GetLocation("Earth Spear Puzzle Reward").PlaceItem(itemPool.GetAndRemove(ItemID.EarthSpear));
            GetLocation("Flare Puzzle Reward").PlaceItem(itemPool.GetAndRemove(ItemID.Flare));
            GetLocation("Bomb Puzzle Reward").PlaceItem(itemPool.GetAndRemove(ItemID.Bomb));
            GetLocation("Chakram Puzzle Reward").PlaceItem(itemPool.GetAndRemove(ItemID.Chakram));
            GetLocation("Caltrop Puzzle Reward").PlaceItem(itemPool.GetAndRemove(ItemID.Caltrops));

            GetLocation("Yagoo Map Street Chest").PlaceItem(itemPool.GetAndRemove(ItemID.YagooMapStreet));
            GetLocation("Mantra Mural").PlaceItem(itemPool.GetAndRemove(ItemID.Mantra));
            GetLocation("Beo Eglana Mural").PlaceItem(itemPool.GetAndRemove(ItemID.BeoEglana));
            GetLocation("Death Village Chest").PlaceItem(itemPool.GetAndRemove(ItemID.DeathVillage));
            GetLocation("Miracle Witch Chest").PlaceItem(itemPool.GetAndRemove(ItemID.MiracleWitch));
            GetLocation("La Mulana Chest").PlaceItem(itemPool.GetAndRemove(ItemID.LaMulana));
            GetLocation("La Mulana 2 Chest").PlaceItem(itemPool.GetAndRemove(ItemID.LaMulana2));

            GetLocation("Sacred Orb VoD").PlaceItem(itemPool.GetAndRemove(ItemID.SacredOrb0));
            GetLocation("Sacred Orb Chest RoY").PlaceItem(itemPool.GetAndRemove(ItemID.SacredOrb1));
            GetLocation("Sacred Orb Chest Annwfn").PlaceItem(itemPool.GetAndRemove(ItemID.SacredOrb2));
            GetLocation("Sacred Orb Chest IB").PlaceItem(itemPool.GetAndRemove(ItemID.SacredOrb3));
            GetLocation("Sacred Orb Chest IT").PlaceItem(itemPool.GetAndRemove(ItemID.SacredOrb4));
            GetLocation("Sacred Orb Chest DF").PlaceItem(itemPool.GetAndRemove(ItemID.SacredOrb5));
            GetLocation("Sacred Orb Chest SotFG").PlaceItem(itemPool.GetAndRemove(ItemID.SacredOrb6));
            GetLocation("Sacred Orb Chest GotD").PlaceItem(itemPool.GetAndRemove(ItemID.SacredOrb7));
            GetLocation("Sacred Orb Chest TS").PlaceItem(itemPool.GetAndRemove(ItemID.SacredOrb8));
            GetLocation("Sacred Orb Chest HL").PlaceItem(itemPool.GetAndRemove(ItemID.SacredOrb9));

            GetLocation("Map Chest RoY").PlaceItem(itemPool.GetAndRemove(ItemID.Map1));
            GetLocation("Map Chest Annwfn").PlaceItem(itemPool.GetAndRemove(ItemID.Map2));
            GetLocation("Map Chest IB").PlaceItem(itemPool.GetAndRemove(ItemID.Map3));
            GetLocation("Map Chest IT").PlaceItem(itemPool.GetAndRemove(ItemID.Map4));
            GetLocation("Map Chest DF").PlaceItem(itemPool.GetAndRemove(ItemID.Map5));
            GetLocation("Map Chest SotFG").PlaceItem(itemPool.GetAndRemove(ItemID.Map6));
            GetLocation("Map Chest GotD").PlaceItem(itemPool.GetAndRemove(ItemID.Map7));
            GetLocation("Map Chest TS").PlaceItem(itemPool.GetAndRemove(ItemID.Map8));
            GetLocation("Map Chest HL").PlaceItem(itemPool.GetAndRemove(ItemID.Map9));
            GetLocation("Map Chest Valhalla").PlaceItem(itemPool.GetAndRemove(ItemID.Map10));
            GetLocation("Map Chest DLM").PlaceItem(itemPool.GetAndRemove(ItemID.Map11));
            GetLocation("Map Chest AC").PlaceItem(itemPool.GetAndRemove(ItemID.Map12));
            GetLocation("Map Chest HoM").PlaceItem(itemPool.GetAndRemove(ItemID.Map13));
            GetLocation("Map Chest EPG").PlaceItem(itemPool.GetAndRemove(ItemID.Map14));
            GetLocation("Map Chest EPD").PlaceItem(itemPool.GetAndRemove(ItemID.Map15));

            GetLocation("Ankh Chest RoY").PlaceItem(itemPool.GetAndRemove(ItemID.AnkhJewel2));
            GetLocation("Ankh Chest DF").PlaceItem(itemPool.GetAndRemove(ItemID.AnkhJewel3));
            GetLocation("Ankh Chest IT").PlaceItem(itemPool.GetAndRemove(ItemID.AnkhJewel4));
            GetLocation("Ankh Chest SotFG").PlaceItem(itemPool.GetAndRemove(ItemID.AnkhJewel5));
            GetLocation("Ankh Chest DLM").PlaceItem(itemPool.GetAndRemove(ItemID.AnkhJewel6));
            GetLocation("Ankh Chest AC").PlaceItem(itemPool.GetAndRemove(ItemID.AnkhJewel7));
            GetLocation("Ankh Jewel").PlaceItem(itemPool.GetAndRemove(ItemID.AnkhJewel9));

            GetLocation("Crystal Skull Chest RoY").PlaceItem(itemPool.GetAndRemove(ItemID.CrystalSkull1));
            GetLocation("Crystal Skull Chest Annwfn").PlaceItem(itemPool.GetAndRemove(ItemID.CrystalSkull2));
            GetLocation("Crystal Skull IB").PlaceItem(itemPool.GetAndRemove(ItemID.CrystalSkull3));
            GetLocation("Crystal Skull Chest IT").PlaceItem(itemPool.GetAndRemove(ItemID.CrystalSkull4));
            GetLocation("Crystal Skull Chest Valhalla").PlaceItem(itemPool.GetAndRemove(ItemID.CrystalSkull5));
            GetLocation("Crystal Skull Chest GotD").PlaceItem(itemPool.GetAndRemove(ItemID.CrystalSkull6));
            GetLocation("Crystal Skull Chest TS").PlaceItem(itemPool.GetAndRemove(ItemID.CrystalSkull7));
            GetLocation("Crystal Skull Chest DLM").PlaceItem(itemPool.GetAndRemove(ItemID.CrystalSkull8));
            GetLocation("Crystal Skull Chest AC").PlaceItem(itemPool.GetAndRemove(ItemID.CrystalSkull9));
            GetLocation("Crystal Skull Chest HL").PlaceItem(itemPool.GetAndRemove(ItemID.CrystalSkull10));
            GetLocation("Crystal Skull Chest HoM").PlaceItem(itemPool.GetAndRemove(ItemID.CrystalSkull11));
            GetLocation("Crystal Skull Chest EPD").PlaceItem(itemPool.GetAndRemove(ItemID.CrystalSkull12));

            GetLocation("Chain Whip Puzzle Reward").PlaceItem(itemPool.GetAndRemove(ItemID.Whip2));
            GetLocation("Flail Whip Puzzle Reward").PlaceItem(itemPool.GetAndRemove(ItemID.Whip3));
            GetLocation("Silver Shield Puzzle Reward").PlaceItem(itemPool.GetAndRemove(ItemID.Shield2));
        }
    }
}
