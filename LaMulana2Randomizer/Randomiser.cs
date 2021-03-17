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
        private readonly Dictionary<AreaID, Area> areas;
        private readonly Dictionary<LocationID, Location> locations;
        private readonly List<Exit> exits;

        private List<JsonArea> worldData;
        private ExitID startingEntrance;

        public ItemPool Items { get; private set; }
        public Item StartingWeapon { get; private set; }
        public Area StartingArea { get; private set; }
        public List<Item> StartingItems { get; private set; }
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
            StartingItems = new List<Item>();
            areas = new Dictionary<AreaID, Area>();
            locations = new Dictionary<LocationID, Location>();
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
            Items = new ItemPool(FileUtils.LoadItemFile("Items.json"));
            worldData = FileUtils.LoadWorldData();

            foreach (JsonArea areaData in worldData)
            {
                Area area = new Area(areaData);

                foreach (JsonLocation locationData in areaData.Locations)
                {
                    Location location = new Location(locationData, area.ID);

                    if (Settings.HardBosses && (location.LocationType == LocationType.Guardian || 
                                                location.LocationType == LocationType.Miniboss))
                        location.UseHardLogic();

                    location.BuildLogicTree();
                    locations.Add(location.ID, location);
                    area.Locations.Add(location);
                }
                areas.Add(area.ID, area); 
            }

            RandomiseCurses();
            ChooseStartingArea();
            ChooseStartingWeapon();

            //we can place these items now since they arent random placements
            CheckStartingItems();
            PlaceStartingShopItems();
            PlaceOriginalMantras();
            PlaceOriginalShops();
            PlaceResearch();
            RemoveItems();

            Items.Add(GetLocation(LocationID.DissonanceDSLM).Item);
            GetLocation(LocationID.DissonanceDSLM).PlaceItem(null, true);
            Items.Add(GetLocation(LocationID.DissonanceEPG).Item);
            GetLocation(LocationID.DissonanceEPG).PlaceItem(null, true);
            Items.Add(GetLocation(LocationID.DissonanceHL).Item);
            GetLocation(LocationID.DissonanceHL).PlaceItem(null, true);
            Items.Add(GetLocation(LocationID.DissonanceMoG).Item);
            GetLocation(LocationID.DissonanceMoG).PlaceItem(null, true);
            Items.Add(GetLocation(LocationID.DissonanceNibiru).Item);
            GetLocation(LocationID.DissonanceNibiru).PlaceItem(null, true);
            Items.Add(GetLocation(LocationID.DissonanceValhalla).Item);
            GetLocation(LocationID.DissonanceValhalla).PlaceItem(null, true);

            //Set the amount of Skull required for Nibiru Dissonance
            var nibiruDiss = GetLocation(LocationID.DissonanceNibiru);
            nibiruDiss.AppendLogicString($" and SkullCount({Settings.RequiredSkulls})");
            nibiruDiss.BuildLogicTree();
        }

        public void PlaceEntrances()
        {
            foreach (JsonArea areaData in worldData)
            {
                Area area = GetArea(areaData.ID);
                foreach (JsonExit exitData in areaData.Exits)
                {
                    Exit exit = new Exit(exitData, area.ID);

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
                if (Settings.RandomHorizontalEntrances)
                    RandomiseHorizontalEntrances();

                if (Settings.RandomLadderEntrances)
                    RandomiseLadderEntrances();

                if (Settings.RandomGateEntrances)
                    RandomiseGateEntrances();
            }

            if (Settings.RandomSoulGateEntrances)
                RandomiseSoulGateEntrances();

            foreach (Area area in areas.Values)
            {
                foreach (Exit exit in area.Exits)
                {
                    Area connectingArea = GetArea(exit.ConnectingAreaID);
                    connectingArea.Entrances.Add(exit);
                }
            }
        }

        public void FixAnkhLogic()
        {
            List<List<Location>> guardianGroups = new List<List<Location>>();

            PlayerState state = new PlayerState(this)
            {
                IgnoreGuardians = true
            };

            foreach (Item item in Items)
                state.CollectItem(item);

            for (int i = 0; i < 9; i++)
            {
                List<Location> reachableLocations;
                List<Location> guardians = new List<Location>();
                do
                {
                    reachableLocations = state.GetReachableLocations(GetPlacedRequiredItemLocations());
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
                    state.CollectItem(guardian.Item);

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
                    if (exit.ExitType != ExitType.Internal && GetArea(exit.ConnectingAreaID).IsBackside)
                    {
                        exit.AppendLogicString(" and Has(Future Development Company)");
                        exit.BuildLogicTree();
                    }
                }
            }
        }

        public bool PlaceItems()
        {
            var items = Items.Copy();

            //place the weights and ammo in shops first since they can only be in shops
            PlaceShopItems(items);

            //place these items now before anythting else
            PlaceAvailableAtStart(items);

            //place mantras if they are not fully randomised
            if (!PlaceMantras(items))
                return false;

            //split the remaining items into required/non required
            ItemPool requiredItems = items.GetandRemoveRequiredItems();
            ItemPool nonRequiredItems = items.GetandRemoveNonRequiredItems();

            //place required items
            if (!RandomiseAssumedFill(requiredItems))
                return false;

            //place non requires items
            RandomiseWithoutChecks(nonRequiredItems);

            Logger.Log($"Total unplaced items {nonRequiredItems.ItemCount}.");
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

        public void ClearRandomlyPlacedItems()
        {
            foreach(Location location in locations.Values)
            {
                if (location.Item != null && location.Item.ID != ItemID.None && location.RandomPlacement)
                    location.PlaceItem(null, true);
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
                IgnoreFalseChecks = false
            };

            List<Location> reachableLocations;
            for(int multiplier = 5; multiplier < 10; multiplier++)
            {
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

        public void FixEmptyLocations()
        {
            Item weight = new Item("Weights", ItemID.Weights, false);
            ItemID chestWeight = ItemID.ChestWeight01;
            ItemID fakeItem = ItemID.FakeItem01;
            ItemID scanItem = ItemID.NothingScan01;
            ItemID npcMoney = ItemID.NPCMoney01;
            foreach (Location location in GetUnplacedLocations())
            {
                switch (location.LocationType)
                {
                    case LocationType.Shop:
                    {
                        location.PlaceItem(weight, true);
                        break;
                    }
                    case LocationType.Dissonance:
                    case LocationType.Chest:
                    {
                        location.PlaceItem(new Item("Weight", chestWeight++, false), true);
                        break;
                    }
                    case LocationType.FreeStanding:
                    {
                        location.PlaceItem(new Item("Fake Item", fakeItem++, false), true);
                        break;
                    }
                    case LocationType.Mural:
                    {
                        location.PlaceItem(new Item("Nothing", scanItem++, false), true);
                        break;
                    }
                    case LocationType.Dialogue:
                    {
                        location.PlaceItem(new Item("Money", npcMoney++, false), true);
                        break;
                    }
                    default:
                        throw new RandomiserException($"Empty location of type {location.LocationType} can't exist");
                }
            }
        }

        public Area GetArea(AreaID areaID)
        {
            if(!areas.TryGetValue(areaID, out Area area))
                throw new InvalidAreaException($"Area does not exist: {areaID}");

            return area;
        }

        public Location GetLocation(LocationID locationID)
        {
            if (!locations.TryGetValue(locationID, out Location location))
                throw new InvalidLocationException($"Location does not exist: {locationID}");

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

        private void PlaceShopItems(ItemPool items)
        {
            int amount = 24;
            if (StartingWeapon.ID > ItemID.Katana) amount -= 1;
            if (StartingArea.ID != AreaID.VoD) amount += 3;

            if (Settings.ShopPlacement == ShopPlacement.Random)
            {
                //lock this shop only items can go here since there is a forth slot
                GetLocation(LocationID.HinerShop3).IsLocked = true;

                RandomiseWithChecks(GetUnplacedLocationsOfType(LocationType.Shop), items.CreateRandomShopPool(amount, random), items);

                GetLocation(LocationID.HinerShop3).IsLocked = false;
            }
            else if(Settings.ShopPlacement == ShopPlacement.AtLeastOne)
            {
                //lock the third slot of each shop so there will be atleast one item in to buy in each shop
                foreach (var location in GetUnplacedLocationsOfType(LocationType.Shop)) {
                    if (location.Name.Contains("3"))
                        location.IsLocked = true;
                }
                RandomiseWithChecks(GetUnplacedLocationsOfType(LocationType.Shop), items.CreateRandomShopPool(amount, random), items);

                //now unlock all the shop slots that were locked
                foreach (var location in GetLocationsOfType(LocationType.Shop))
                    location.IsLocked = false;
            }
        }

        private void PlaceOriginalShops()
        {
            if(Settings.ShopPlacement == ShopPlacement.Original)
            {
                //place items how they are originally
                GetLocation(LocationID.NeburShop1).PlaceItem(Items.Get(ItemID.Weights), false);

                //if we start with a subweapon and start in the village this item has to go somewhere else since ammo is placed in slot 2
                //if we dont place the item we have 1 too many items now so remove a map since they are the least useful items
                if (StartingWeapon.ID <= ItemID.Katana || StartingArea.ID != AreaID.VoD)
                    GetLocation(LocationID.NeburShop2).PlaceItem(Items.GetAndRemove(ItemID.YagooMapReader), false);
                else
                    Items.GetAndRemove(ItemID.Map1);

                GetLocation(LocationID.NeburShop3).PlaceItem(Items.GetAndRemove(ItemID.TextTrax), false);

                GetLocation(LocationID.ModroShop1).PlaceItem(Items.GetAndRemove(ItemID.Shield1), false);
                GetLocation(LocationID.ModroShop2).PlaceItem(Items.GetAndRemove(ItemID.Pistol), false);
                GetLocation(LocationID.ModroShop3).PlaceItem(Items.Get(ItemID.PistolAmmo), false);

                GetLocation(LocationID.SidroShop1).PlaceItem(Items.GetAndRemove(ItemID.HandScanner), false);
                GetLocation(LocationID.SidroShop2).PlaceItem(Items.Get(ItemID.ShurikenAmmo), false);
                GetLocation(LocationID.SidroShop3).PlaceItem(Items.GetAndRemove(ItemID.Pepper), false);

                GetLocation(LocationID.HinerShop1).PlaceItem(Items.Get(ItemID.Weights), false);
                GetLocation(LocationID.HinerShop2).PlaceItem(Items.GetAndRemove(ItemID.Codices), false);
                GetLocation(LocationID.HinerShop3).PlaceItem(Items.GetAndRemove(ItemID.AnkhJewel1), false);
                GetLocation(LocationID.HinerShop4).PlaceItem(Items.GetAndRemove(ItemID.AnkhJewel8), false);

                GetLocation(LocationID.KorobokShop1).PlaceItem(Items.Get(ItemID.Weights), false);
                GetLocation(LocationID.KorobokShop2).PlaceItem(Items.Get(ItemID.ShurikenAmmo), false);
                GetLocation(LocationID.KorobokShop3).PlaceItem(Items.GetAndRemove(ItemID.Guild), false);

                GetLocation(LocationID.ShuhokaShop1).PlaceItem(Items.Get(ItemID.Weights), false);
                GetLocation(LocationID.ShuhokaShop2).PlaceItem(Items.Get(ItemID.ShurikenAmmo), false);
                GetLocation(LocationID.ShuhokaShop3).PlaceItem(Items.GetAndRemove(ItemID.Alert), false);

                GetLocation(LocationID.PymShop1).PlaceItem(Items.Get(ItemID.Weights), false);
                GetLocation(LocationID.PymShop2).PlaceItem(Items.Get(ItemID.RollingShurikenAmmo), false);
                GetLocation(LocationID.PymShop3).PlaceItem(Items.GetAndRemove(ItemID.Snapshot), false);

                GetLocation(LocationID.BtkShop1).PlaceItem(Items.Get(ItemID.Weights), false);
                GetLocation(LocationID.BtkShop2).PlaceItem(Items.Get(ItemID.CaltropsAmmo), false);
                GetLocation(LocationID.BtkShop3).PlaceItem(Items.GetAndRemove(ItemID.EngaMusica), false);

                GetLocation(LocationID.MinoShop1).PlaceItem(Items.Get(ItemID.Weights), false);
                GetLocation(LocationID.MinoShop2).PlaceItem(Items.Get(ItemID.BombAmmo), false);
                GetLocation(LocationID.MinoShop3).PlaceItem(Items.GetAndRemove(ItemID.LonelyHouseMoving), false);

                GetLocation(LocationID.BargainDuckShop1).PlaceItem(Items.Get(ItemID.Weights), false);
                GetLocation(LocationID.BargainDuckShop2).PlaceItem(Items.Get(ItemID.CaltropsAmmo), false);
                GetLocation(LocationID.BargainDuckShop3).PlaceItem(Items.GetAndRemove(ItemID.FutureDevelopmentCompany), false);

                GetLocation(LocationID.PeibalusaShop1).PlaceItem(Items.Get(ItemID.Weights), false);
                GetLocation(LocationID.PeibalusaShop2).PlaceItem(Items.Get(ItemID.EarthSpearAmmo), false);
                GetLocation(LocationID.PeibalusaShop3).PlaceItem(Items.GetAndRemove(ItemID.RaceScanner), false);

                GetLocation(LocationID.HiroRoderickShop1).PlaceItem(Items.Get(ItemID.Weights), false);
                GetLocation(LocationID.HiroRoderickShop2).PlaceItem(Items.Get(ItemID.FlareAmmo), false);
                GetLocation(LocationID.HiroRoderickShop3).PlaceItem(Items.GetAndRemove(ItemID.Harp), false);

                GetLocation(LocationID.HydlitShop1).PlaceItem(Items.Get(ItemID.Weights), false);
                GetLocation(LocationID.HydlitShop2).PlaceItem(Items.Get(ItemID.BombAmmo), false);
                GetLocation(LocationID.HydlitShop3).PlaceItem(Items.GetAndRemove(ItemID.GaneshaTalisman), false);

                GetLocation(LocationID.AytumShop1).PlaceItem(Items.Get(ItemID.Weights), false);
                GetLocation(LocationID.AytumShop2).PlaceItem(Items.Get(ItemID.ShurikenAmmo), false);
                GetLocation(LocationID.AytumShop3).PlaceItem(Items.GetAndRemove(ItemID.BounceShot), false);

                GetLocation(LocationID.KeroShop1).PlaceItem(Items.Get(ItemID.Weights), false);
                GetLocation(LocationID.KeroShop2).PlaceItem(Items.Get(ItemID.PistolAmmo), false);
                GetLocation(LocationID.KeroShop3).PlaceItem(Items.GetAndRemove(ItemID.RoseandCamelia), false);

                GetLocation(LocationID.AshGeenShop1).PlaceItem(Items.Get(ItemID.Weights), false);
                GetLocation(LocationID.AshGeenShop2).PlaceItem(Items.Get(ItemID.FlareAmmo), false);
                GetLocation(LocationID.AshGeenShop3).PlaceItem(Items.GetAndRemove(ItemID.MekuriMaster), false);

                GetLocation(LocationID.VenomShop1).PlaceItem(Items.Get(ItemID.Weights), false);
                GetLocation(LocationID.VenomShop2).PlaceItem(Items.Get(ItemID.CaltropsAmmo), false);
                GetLocation(LocationID.VenomShop3).PlaceItem(Items.GetAndRemove(ItemID.SpaceCapstarII), false);

                GetLocation(LocationID.MegarockShop1).PlaceItem(Items.Get(ItemID.Weights), false);
                GetLocation(LocationID.MegarockShop2).PlaceItem(Items.Get(ItemID.EarthSpearAmmo), false);
                GetLocation(LocationID.MegarockShop3).PlaceItem(Items.GetAndRemove(ItemID.Bracelet), false);

                GetLocation(LocationID.FairyLanShop1).PlaceItem(Items.Get(ItemID.Weights), false);
                GetLocation(LocationID.FairyLanShop2).PlaceItem(Items.Get(ItemID.ChakramAmmo), false);
                GetLocation(LocationID.FairyLanShop3).PlaceItem(Items.GetAndRemove(ItemID.Shield3), false);

                if (StartingArea.ID != AreaID.VoD)
                {
                    //TODO:REMOVE THIS WHEN YOU FIX IT
                    if (StartingWeapon.ID <= ItemID.Katana)
                        GetLocation(LocationID.StartingShop2).PlaceItem(Items.Get(ItemID.Weights), false);

                    GetLocation(LocationID.StartingShop3).PlaceItem(Items.Get(ItemID.Weights), false);
                }

                //after placing the weights/ammo remove them as they arent needed anymore
                Items.GetAndRemoveShopOnlyItems();
            }
        }

        private bool PlaceMantras(ItemPool items)
        {
            if (Settings.MantraPlacement == MantraPlacement.OnlyMurals)
                return RandomiseWithChecks(GetUnplacedLocationsOfType(LocationType.Mural), new ItemPool(items.GetAndRemoveMantras()), items);

            return true;
        }

        private void PlaceOriginalMantras()
        {
            if (Settings.MantraPlacement == MantraPlacement.Original)
            {
                //put the mantras where they are originally if they arent randomised
                GetLocation(LocationID.HeavenMantraMural).PlaceItem(Items.GetAndRemove(ItemID.Heaven), false);
                GetLocation(LocationID.EarthMantraMural).PlaceItem(Items.GetAndRemove(ItemID.Earth), false);
                GetLocation(LocationID.SunMantraMural).PlaceItem(Items.GetAndRemove(ItemID.Sun), false);
                GetLocation(LocationID.MoonMantraMural).PlaceItem(Items.GetAndRemove(ItemID.Moon), false);
                GetLocation(LocationID.SeaMantraMural).PlaceItem(Items.GetAndRemove(ItemID.Sea), false);
                GetLocation(LocationID.FireMantraMural).PlaceItem(Items.GetAndRemove(ItemID.Fire), false);
                GetLocation(LocationID.WindMantraMural).PlaceItem(Items.GetAndRemove(ItemID.Wind), false);
                GetLocation(LocationID.MotherMantraMural).PlaceItem(Items.GetAndRemove(ItemID.Mother), false);
                GetLocation(LocationID.ChildMantraMural).PlaceItem(Items.GetAndRemove(ItemID.Child), false);
                GetLocation(LocationID.NightMantraMural).PlaceItem(Items.GetAndRemove(ItemID.Night), false);
            }
        }

        private void PlaceStartingShopItems()
        {
            //Places weights at a starting shop so the player can buy them at the start of the game
            //if we have a subweapon start need to give the player ammo at a shop
            if (StartingArea.ID == AreaID.VoD)
            {
                GetLocation(LocationID.NeburShop1).PlaceItem(Items.Get(ItemID.Weights), false);

                if (StartingWeapon.ID > ItemID.Katana)
                    GetLocation(LocationID.NeburShop2).PlaceItem(Items.Get(GetAmmoItemID(StartingWeapon.ID)), false);
            }
            else
            {
                GetLocation(LocationID.StartingShop1).PlaceItem(Items.Get(ItemID.Weights), false);

                if (StartingWeapon.ID > ItemID.Katana)
                    GetLocation(LocationID.StartingShop2).PlaceItem(Items.Get(GetAmmoItemID(StartingWeapon.ID)), false);
            }
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

        private void PlaceResearch()
        {
            if (!Settings.RandomResearch)
            {
                GetLocation(LocationID.ResearchAnnwfn).PlaceItem(Items.GetAndRemove(ItemID.Research1), false);
                GetLocation(LocationID.ResearchIBTopLeft).PlaceItem(Items.GetAndRemove(ItemID.Research2), false);
                GetLocation(LocationID.ResearchIBTopRight).PlaceItem(Items.GetAndRemove(ItemID.Research3), false);
                GetLocation(LocationID.ResearchIBTent1).PlaceItem(Items.GetAndRemove(ItemID.Research4), false);
                GetLocation(LocationID.ResearchIBTent2).PlaceItem(Items.GetAndRemove(ItemID.Research5), false);
                GetLocation(LocationID.ResearchIBTent3).PlaceItem(Items.GetAndRemove(ItemID.Research6), false);
                GetLocation(LocationID.ResearchIBPit).PlaceItem(Items.GetAndRemove(ItemID.Research7), false);
                GetLocation(LocationID.ResearchIBLeft).PlaceItem(Items.GetAndRemove(ItemID.Research8), false);
                GetLocation(LocationID.ResearchIT).PlaceItem(Items.GetAndRemove(ItemID.Research9), false);
                GetLocation(LocationID.ResearchDSLM).PlaceItem(Items.GetAndRemove(ItemID.Research10), false);
            }               
        }                   

        private void ChooseStartingWeapon()
        {
            ItemID[] weapons = new ItemID[]{ ItemID.Whip1, ItemID.Knife, ItemID.Rapier, ItemID.Axe, ItemID.Katana, ItemID.Shuriken,
                ItemID.RollingShuriken, ItemID.EarthSpear, ItemID.Flare, ItemID.Caltrops, ItemID.Chakram, ItemID.Bomb, ItemID.Pistol };

            List<ItemID> selectedWeapons = weapons.Zip(Settings.GetWeaponChoices(), Tuple.Create).Where(w => w.Item2).Select(w => w.Item1).ToList();

            //remove the starting weapon from the item pool
            StartingWeapon = Items.GetAndRemove(selectedWeapons[random.Next(selectedWeapons.Count)]);
        }

        private void ChooseStartingArea()
        {
            //choose starting area from areas which where chosen
            AreaID[] areas = new AreaID[] { AreaID.VoD, AreaID.RoY, AreaID.AnnwfnMain, AreaID.IBMain, AreaID.ITLeft, AreaID.DFMain, 
                                                AreaID.ValhallaMain, AreaID.DSLMMain, AreaID.ACTablet};
            List<AreaID> selectedAreas = areas.Zip(Settings.GetStartingAreaChoices(), Tuple.Create).Where(a => a.Item2).Select(a => a.Item1).ToList();
            StartingArea = GetArea(selectedAreas[random.Next(selectedAreas.Count)]);

            //if we arent starting in the village i shop needs to be added
            if(StartingArea.ID != AreaID.VoD)
            {
                Location location = new Location("Starting Shop 1", LocationID.StartingShop1, LocationType.Shop, "True", StartingArea.ID);
                location.BuildLogicTree();
                locations.Add(LocationID.StartingShop1, location);
                StartingArea.Locations.Add(location);

                location = new Location("Starting Shop 2", LocationID.StartingShop2, LocationType.Shop, "True", StartingArea.ID);
                location.BuildLogicTree();
                locations.Add(LocationID.StartingShop2, location);
                StartingArea.Locations.Add(location);

                location = new Location("Starting Shop 3", LocationID.StartingShop3, LocationType.Shop, "True", StartingArea.ID);
                location.BuildLogicTree();
                locations.Add(LocationID.StartingShop3, location);
                StartingArea.Locations.Add(location);
            }

            //work out a starting entrance for reduce dead end starts to use
            List<ExitID> entrances = new List<ExitID>();
            switch (StartingArea.ID)
            {
                case AreaID.VoD:
                {
                    if(Settings.RandomGateEntrances)
                        entrances.Add(ExitID.f01Right);

                    if(Settings.IncludeUniqueTransitions)
                        entrances.Add(ExitID.f01Start);
                    break;
                }
                case AreaID.RoY:
                {
                    if (Settings.RandomGateEntrances)
                        entrances.Add(ExitID.f00GateY0);
                    break;
                }
                case AreaID.AnnwfnMain:
                {
                    if(Settings.RandomLadderEntrances)
                        entrances.Add(ExitID.f02Up);
                    if (Settings.IncludeUniqueTransitions)
                        entrances.Add(ExitID.f02Down);
                    break;
                }
                case AreaID.IBMain:
                {
                    if(Settings.RandomHorizontalEntrances)
                        entrances.Add(ExitID.f03Right);
                    break;
                }
                case AreaID.ITLeft:
                {
                    if (Settings.RandomLadderEntrances)
                        entrances.Add(ExitID.f04Up);
                    if (Settings.RandomGateEntrances)
                        entrances.Add(ExitID.f04GateYB);

                    break;
                }
                case AreaID.DFMain:
                {
                    if (Settings.RandomGateEntrances)
                        entrances.Add(ExitID.f05GateP1);
                    break;
                }
                case AreaID.ValhallaMain:
                {
                    if (Settings.RandomGateEntrances)
                        entrances.Add(ExitID.f10GateP0);
                    break;
                }
                case AreaID.DSLMMain:
                {
                    if (Settings.RandomGateEntrances)
                        entrances.Add(ExitID.f11GateP0);
                    break;
                }
                case AreaID.ACTablet:
                {
                    if (Settings.RandomGateEntrances)
                        entrances.Add(ExitID.f12GateP0);
                    break;
                }
            }

            if (entrances.Count > 0)
                startingEntrance = entrances[random.Next(entrances.Count)];
            else
                startingEntrance = ExitID.None;
        }

        private void CheckStartingItems()
        {
            if (Settings.RandomGrail == ItemPlacement.Starting)
                StartingItems.Add(Items.GetAndRemove(ItemID.HolyGrail));
            if(Settings.RandomScanner == ItemPlacement.Starting)
                StartingItems.Add(Items.GetAndRemove(ItemID.HandScanner));
            if (Settings.RandomCodices == ItemPlacement.Starting)
                StartingItems.Add(Items.GetAndRemove(ItemID.Codices));
            if (Settings.RandomFDC == ItemPlacement.Starting)
                StartingItems.Add(Items.GetAndRemove(ItemID.FutureDevelopmentCompany));
            if(Settings.RandomRing == ItemPlacement.Starting)
                StartingItems.Add(Items.GetAndRemove(ItemID.Ring));
            if(Settings.RandomShellHorn == ItemPlacement.Starting)
                StartingItems.Add(Items.GetAndRemove(ItemID.ShellHorn));
        }

        private void PlaceAvailableAtStart(ItemPool items)
        {
            //create a list of the items we want to place that are accessible from the start
            ItemPool earlyItems = new ItemPool();
            if (Settings.RandomGrail == ItemPlacement.AvailableAtStart)
                earlyItems.Add(items.GetAndRemove(ItemID.HolyGrail));
            if (Settings.RandomScanner == ItemPlacement.AvailableAtStart)
                earlyItems.Add(items.GetAndRemove(ItemID.HandScanner));
            if (Settings.RandomCodices == ItemPlacement.AvailableAtStart)
                earlyItems.Add(items.GetAndRemove(ItemID.Codices));
            if (Settings.RandomFDC == ItemPlacement.AvailableAtStart)
                earlyItems.Add(items.GetAndRemove(ItemID.FutureDevelopmentCompany));
            if (Settings.RandomRing == ItemPlacement.AvailableAtStart)
                earlyItems.Add(items.GetAndRemove(ItemID.Ring));
            if (Settings.RandomShellHorn == ItemPlacement.AvailableAtStart)
                earlyItems.Add(items.GetAndRemove(ItemID.ShellHorn));

            PlayerState state = PlayerState.GetStateWithItems(this, new ItemPool());
            var locations = state.GetReachableLocations(GetUnplacedLocations());

            if (earlyItems.ItemCount > locations.Count)
                throw new RandomiserException("Unable to generate seed, not enough locations available to place items available at start.");

            locations = Shuffle.FisherYates(locations, random);
            foreach (Item item in earlyItems)
            {
                Location location = locations[random.Next(locations.Count)];
                location.PlaceItem(item, true);
                locations.Remove(location);
            }
        }

        private void RemoveItems()
        {
            if (Settings.RemoveMaps)
            {
                for (ItemID i = ItemID.Map1; i <= ItemID.Map16; i++)
                    Items.GetAndRemove(i);
            }

            if (Settings.RemoveResearch)
            {
                for (ItemID i = ItemID.Research1; i <= ItemID.Research10; i++)
                    Items.GetAndRemove(i);
            }

            if (Settings.RemoveSkulls)
            {
                int skullsToRemove = 12 - Settings.RequiredSkulls;
                for (ItemID i = ItemID.CrystalSkull1; i < ItemID.CrystalSkull1 + skullsToRemove; i++)
                    Items.GetAndRemove(i);
            }
        }

        private bool RandomiseAssumedFill(ItemPool itemsToPlace)
        {
            PlayerState state;
            List<Location> locations = GetUnplacedLocations();

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
                    locationToPlaceAt.PlaceItem(item, true);
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
                    locationToPlaceAt.PlaceItem(item, true);
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

        private void RandomiseWithoutChecks(ItemPool items)
        {
            var locations = Shuffle.FisherYates(GetUnplacedLocations(), random);
            while (items.ItemCount > 0)
            {
                Item item = items.RandomGetAndRemove(random);
                Location location = locations.Last();

                location.PlaceItem(item, true);
                locations.Remove(location);
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
                var defaultLocations = new List<LocationID> { LocationID.FlameTorcChest, LocationID.GiantsFluteChest, 
                                                                LocationID.DestinyTabletChest, LocationID.PowerBandChest };
                foreach (LocationID locationID in defaultLocations)
                {
                    Location location = GetLocation(locationID);
                    location.AppendLogicString("and Has(Mulana Talisman)");
                    location.BuildLogicTree();
                    CursedLocations.Add(location);
                }
            }
        }

        private List<Exit> GetExitsOfType(ExitType type)
        {
            return exits.Where(connection => connection.ExitType == type).ToList();
        }

        private void RandomiseHorizontalEntrances()
        {
            List<Exit> leftDoors = Shuffle.FisherYates(GetExitsOfType(ExitType.LeftDoor), random);
            List<Exit> rightDoors = Shuffle.FisherYates(GetExitsOfType(ExitType.RightDoor), random);

            //place the cliff first to avoid cliff to village if needed, then place one of the cavern sides to stop it
            //from looping on itself
            List<Exit> priorityLeftDoors = new List<Exit>
            {
                leftDoors.Find(x => x.ID == ExitID.fP02Left),
                leftDoors.Find(x => x.ID == ExitID.fP00Left)
            };

            foreach (Exit exit in priorityLeftDoors)
                leftDoors.Remove(exit);

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
                    leftDoors.Remove(leftDoor);
                }

                if (leftDoor.ID == ExitID.fP02Left)
                {
                    do
                    {
                        rightDoor = rightDoors[random.Next(rightDoors.Count)];
                    } while ((rightDoor.ID == ExitID.f01Right && StartingArea.ID == AreaID.VoD && (!Settings.RandomLadderEntrances || Settings.ReduceDeadEndStarts)) ||
                             (rightDoor.ID == ExitID.f03Right && StartingArea.ID == AreaID.IBMain && Settings.ReduceDeadEndStarts) ||
                              rightDoor.ID == ExitID.fL08Right);
                }
                else if (leftDoor.ID == ExitID.fP00Left)
                {
                    do
                    {
                        rightDoor = rightDoors[random.Next(rightDoors.Count)];
                    } while ((((rightDoor.ID == ExitID.f01Right && StartingArea.ID == AreaID.VoD && (!Settings.RandomLadderEntrances || Settings.ReduceDeadEndStarts)) ||
                               (rightDoor.ID == ExitID.f03Right && StartingArea.ID == AreaID.IBMain && Settings.ReduceDeadEndStarts) ||
                                rightDoor.ID == ExitID.fL08Right) && cavernToCliff) || rightDoor.ID == ExitID.fP00Right);
                }
                else
                {
                    rightDoor = rightDoors[random.Next(rightDoors.Count)];
                }

                rightDoors.Remove(rightDoor);

                if (leftDoor.ID == ExitID.fP02Left && rightDoor.ID == ExitID.fP00Right)
                    cavernToCliff = true;

                if (rightDoor.ID == ExitID.f01Right && (leftDoor.ID == ExitID.fP00Left && !cavernToCliff))
                    cavernToVillage = true;

                if (leftDoor.ID == ExitID.fL02Left)
                {
                    if (rightDoor.ID == ExitID.f01Right || (rightDoor.ID == ExitID.fP00Right && cavernToVillage))
                    {
                        rightDoor.AppendLogicString(" and CanWarp");
                        rightDoor.BuildLogicTree();
                    }
                    else
                    {
                        rightDoor.AppendLogicString($" and (CanWarp or CanReach({AreaID.AnnwfnMain}))");
                        rightDoor.BuildLogicTree();
                    }
                }

                leftDoor.ConnectingAreaID = rightDoor.ParentAreaID;
                rightDoor.ConnectingAreaID = leftDoor.ParentAreaID;

                ExitPairs.Add((leftDoor.ID, rightDoor.ID));
                HorizontalPairs.Add($"{leftDoor.Name} - {rightDoor.Name}");
            }
        }

        private void RandomiseLadderEntrances()
        {
            List<Exit> downLadders = Shuffle.FisherYates(GetExitsOfType(ExitType.DownLadder), random);
            List<Exit> upLadders = Shuffle.FisherYates(GetExitsOfType(ExitType.UpLadder), random);

            Exit downLadder = null;
            Exit upLadder = null;

            if (Settings.ReduceDeadEndStarts && (startingEntrance == ExitID.f04Up || startingEntrance == ExitID.f02Up))
            {
                upLadder = upLadders.Find(x => x.ID == startingEntrance);
                upLadders.Remove(upLadder);
                do
                {
                    downLadder = downLadders[random.Next(downLadders.Count)];
                } while (downLadder.ID != ExitID.f02Down && downLadder.ID != ExitID.fLDown);
                downLadders.Remove(downLadder);

                FixLadderLogic(downLadder, upLadder);

                upLadder.ConnectingAreaID = downLadder.ParentAreaID;
                downLadder.ConnectingAreaID = upLadder.ParentAreaID;

                ExitPairs.Add((upLadder.ID, downLadder.ID));
                LadderPairs.Add($"{upLadder.Name} - {downLadder.Name}");
            }

            //these are one way down ladders so they need to be placed first to avoid being placed with inferno cavern
            List<Exit> priorityDownLadders = new List<Exit>()
            {
                downLadders.Find(x => x.ID == ExitID.f02Down),
                downLadders.Find(x => x.ID == ExitID.f03Down2)
            };
            priorityDownLadders.RemoveAll(x => x == null);
            foreach (Exit exit in priorityDownLadders)
                downLadders.Remove(exit);

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
                    downLadders.Remove(downLadder);
                }

                if (downLadder.ID == ExitID.f02Down || downLadder.ID == ExitID.f03Down2)
                {
                    do
                    {
                        upLadder = upLadders[random.Next(upLadders.Count)];
                    } while (upLadder.ID == ExitID.fL05Up);
                }
                else
                {
                    upLadder = upLadders[random.Next(upLadders.Count)];
                }
                upLadders.Remove(upLadder);

                FixLadderLogic(downLadder, upLadder);

                upLadder.ConnectingAreaID = downLadder.ParentAreaID;
                downLadder.ConnectingAreaID = upLadder.ParentAreaID;

                ExitPairs.Add((upLadder.ID, downLadder.ID));
                LadderPairs.Add($"{upLadder.Name} - {downLadder.Name}");
            }
        }

        private void FixLadderLogic(Exit downLadder, Exit upLadder)
        {
            if (downLadder.ID == ExitID.f02Down)
            {
                upLadder.AppendLogicString($" and (CanReach({AreaID.AnnwfnMain}) or CanWarp)");
                upLadder.BuildLogicTree();
            }
            else if (downLadder.ID == ExitID.f03Down2)
            {
                upLadder.AppendLogicString($" and Has(Life Sigil) and (CanWarp or CanReach({AreaID.IBDinosaur}))");
                upLadder.BuildLogicTree();
            }
            else if (downLadder.ID == ExitID.f02Down)
            {
                upLadder.AppendLogicString(" and HorizontalAttack");
                upLadder.BuildLogicTree();
            }

            if (upLadder.ID == ExitID.f03Up)
            {
                downLadder.AppendLogicString($" and (CanWarp or CanKill(Cetus) or CanReach({AreaID.IBMain}))");
                downLadder.BuildLogicTree();
            }
            else if (upLadder.ID == ExitID.f04Up3)
            {
                downLadder.AppendLogicString(" and False");
                downLadder.BuildLogicTree();
            }
        }

        private void RandomiseGateEntrances()
        {
            List<Exit> gates = Shuffle.FisherYates(GetExitsOfType(ExitType.Gate), random);

            bool startToIllusion = false;
            Exit gate1 = null;
            Exit gate2 = null;

            if (Settings.ReduceDeadEndStarts && (startingEntrance == ExitID.f00GateY0 || startingEntrance == ExitID.f04GateYB || startingEntrance == ExitID.f05GateP1))
            {
                gate1 = gates.Find(x => x.ID == startingEntrance);
                gates.Remove(gate1);

                gate2 = gates[random.Next(gates.Count)];
                gates.Remove(gate2);

                if (gate2.ID == ExitID.fL11GateN || gate2.ID == ExitID.fL11GateY0)
                    startToIllusion = true;

                FixGateLogic(gate1, gate2);
                FixGateLogic(gate2, gate1);

                gate2.ConnectingAreaID = gate1.ParentAreaID;
                gate1.ConnectingAreaID = gate2.ParentAreaID;

                ExitPairs.Add((gate1.ID, gate2.ID));
                GatePairs.Add($"{gate1.Name} - {gate2.Name}");
            }

            //place one side of illusion to avoid making it loop on itself
            gate1 = gates.Find(x => x.ID == ExitID.fL11GateN);
            if(gate1 == null)
                gate1 = gates.Find(x => x.ID == ExitID.fL11GateY0);
            gates.Remove(gate1);
            do
            {
                gate2 = gates[random.Next(gates.Count)];
            } while (gate2.ID == ExitID.fL11GateY0 || (startToIllusion && gate2.IsInaccessible));
            gates.Remove(gate2);

            FixGateLogic(gate1, gate2);
            FixGateLogic(gate2, gate1);

            gate2.ConnectingAreaID = gate1.ParentAreaID;
            gate1.ConnectingAreaID = gate2.ParentAreaID;

            ExitPairs.Add((gate1.ID, gate2.ID));
            GatePairs.Add($"{gate1.Name} - {gate2.Name}");

            //these are all inaccessible unless you come through the gate itself so to they can't be placed together
            List<Exit> priorityGates = new List<Exit>();
            priorityGates.AddRange(gates.Where(x => x.IsInaccessible));
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

                gate1.ConnectingAreaID = gate2.ParentAreaID;
                gate2.ConnectingAreaID = gate1.ParentAreaID;

                ExitPairs.Add((gate1.ID, gate2.ID));
                GatePairs.Add($"{gate1.Name} - {gate2.Name}");
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
            if (Settings.RandomHorizontalEntrances)
            {
                entrances.AddRange(GetExitsOfType(ExitType.LeftDoor));
                entrances.AddRange(GetExitsOfType(ExitType.RightDoor));
            }

            if (Settings.RandomLadderEntrances)
            {
                entrances.AddRange(GetExitsOfType(ExitType.DownLadder));
                entrances.AddRange(GetExitsOfType(ExitType.UpLadder));
            }

            if (Settings.RandomGateEntrances)
                entrances.AddRange(GetExitsOfType(ExitType.Gate));

            if (Settings.IncludeUniqueTransitions)
            {
                entrances.AddRange(GetExitsOfType(ExitType.OneWay));
                entrances.AddRange(GetExitsOfType(ExitType.Pyramid));
                entrances.AddRange(GetExitsOfType(ExitType.Start));
            }

            Exit entrance1 = null;
            Exit entrance2 = null;

            bool cavernToDeadEnd = false;
            bool illusionToDeadEnd = false;

            if (Settings.RandomHorizontalEntrances)
            {
                //place the cliff  first just to see if gets placed anywhere that affects any other placement logic
                entrance1 = entrances.Find(x => x.ID == ExitID.fP02Left);
                entrances.Remove(entrance1);
                do
                {
                    entrance2 = entrances[random.Next(entrances.Count)];
                } while ((entrance2.ID ==  startingEntrance && Settings.ReduceDeadEndStarts) || entrance2.IsInaccessible);   
                
                entrances.Remove(entrance2);

                if (entrance2.ID == ExitID.fP00Right || entrance2.ID == ExitID.fP00Left)
                    cavernToDeadEnd = true;
                else if (entrance2.ID == ExitID.fL11GateN || entrance2.ID == ExitID.fL11GateY0)
                    illusionToDeadEnd = true;

                FixFullRandomEntranceLogic(entrance1, entrance2);
                FixFullRandomEntranceLogic(entrance2, entrance1);

                entrance1.ConnectingAreaID = entrance2.ParentAreaID;
                entrance2.ConnectingAreaID = entrance1.ParentAreaID;

                ExitPairs.Add((entrance1.ID, entrance2.ID));
                EntrancePairs.Add($"{entrance1.Name} - {entrance2.Name}");

                //place one of the cavern entrances to stop it looping on itself
                entrance1 = entrances.Find(x => x.ID == ExitID.fP00Left);
                if(entrance1 != null)
                {
                    entrances.Remove(entrance1);
                    do
                    {
                        entrance2 = entrances[random.Next(entrances.Count)];
                    } while (entrance2.ID == ExitID.fP00Right || (entrance2.ID == startingEntrance && Settings.ReduceDeadEndStarts && cavernToDeadEnd));
                    entrances.Remove(entrance2);

                    if (entrance2.IsInaccessible)
                        cavernToDeadEnd = true;

                    FixFullRandomEntranceLogic(entrance1, entrance2);
                    FixFullRandomEntranceLogic(entrance2, entrance1);

                    entrance1.ConnectingAreaID = entrance2.ParentAreaID;
                    entrance2.ConnectingAreaID = entrance1.ParentAreaID;

                    ExitPairs.Add((entrance1.ID, entrance2.ID));
                    EntrancePairs.Add($"{entrance1.Name} - {entrance2.Name}");
                }
            }

            //place start entrance now if reduce dead end starts is on
            if (Settings.ReduceDeadEndStarts)
            {
                entrance1 = entrances.Find(x => x.ID == startingEntrance);
                if (entrance1 != null)
                {
                    entrances.Remove(entrance1);
                    do
                    {
                        entrance2 = entrances[random.Next(entrances.Count)];
                    } while (StartEntranceLoopCheck(entrance2) || entrance2.IsDeadEnd ||
                                ((entrance2.ID == ExitID.fL11GateN || entrance2.ID == ExitID.fL11GateY0) && illusionToDeadEnd) || 
                                ((entrance2.ID == ExitID.fP00Right || entrance2.ID == ExitID.fP00Left) && cavernToDeadEnd));
                    entrances.Remove(entrance2);

                    FixFullRandomEntranceLogic(entrance1, entrance2);
                    FixFullRandomEntranceLogic(entrance2, entrance1);

                    entrance1.ConnectingAreaID = entrance2.ParentAreaID;
                    entrance2.ConnectingAreaID = entrance1.ParentAreaID;

                    ExitPairs.Add((entrance1.ID, entrance2.ID));
                    EntrancePairs.Add($"{entrance1.Name} - {entrance2.Name}");
                }
            }

            //try to place an illusion entrance to stop it looping on itself if its already placed it doesn't matter
            if (Settings.RandomGateEntrances)
            {
                entrance1 = entrances.Find(x => x.ID == ExitID.fL11GateN);
                if (entrance1 != null)
                {
                    entrances.Remove(entrance1);
                    do
                    {
                        entrance2 = entrances[random.Next(entrances.Count)];
                    } while (entrance2.ID == ExitID.fL11GateY0 || (entrance2.ID == startingEntrance && illusionToDeadEnd && Settings.ReduceDeadEndStarts));

                    entrances.Remove(entrance2);

                    FixFullRandomEntranceLogic(entrance1, entrance2);
                    FixFullRandomEntranceLogic(entrance2, entrance1);

                    entrance1.ConnectingAreaID = entrance2.ParentAreaID;
                    entrance2.ConnectingAreaID = entrance1.ParentAreaID;

                    ExitPairs.Add((entrance1.ID, entrance2.ID));
                    EntrancePairs.Add($"{entrance1.Name} - {entrance2.Name}");
                }
            }

            priorityEntrances.AddRange(entrances.Where(x => x.IsInaccessible));
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

                entrance1.ConnectingAreaID = entrance2.ParentAreaID;
                entrance2.ConnectingAreaID = entrance1.ParentAreaID;

                ExitPairs.Add((entrance1.ID, entrance2.ID));
                EntrancePairs.Add($"{entrance1.Name} - {entrance2.Name}");
            }
        }

        private bool StartEntranceLoopCheck(Exit entrance)
        {
            switch (startingEntrance)
            {
                case ExitID.f00GateY0:
                    return entrance.ID == ExitID.f00GateYA && entrance.ID == ExitID.f00GateYB && 
                            entrance.ID == ExitID.f00GateYC && entrance.ID == ExitID.f00Down;
                case ExitID.f01Right:
                    return entrance.ID == ExitID.f01Start;
                case ExitID.f01Start:
                    return entrance.ID == ExitID.f01Right;
                case ExitID.f02Up:
                    return entrance.ID == ExitID.f02Bifrost || entrance.ID == ExitID.f02Down || entrance.ID == ExitID.f02GateYA;
                case ExitID.f02Bifrost:
                    return entrance.ID == ExitID.f02Up || entrance.ID == ExitID.f02Down || entrance.ID == ExitID.f02GateYA;
                case ExitID.f03Right:
                    return entrance.ID == ExitID.f03Down1 || entrance.ID == ExitID.f03Down2 || entrance.ID == ExitID.f03Down3 || 
                            entrance.ID == ExitID.f03Up || entrance.ID == ExitID.f03GateYC || entrance.ID == ExitID.f03In;
                case ExitID.f04Up:
                    return entrance.ID == ExitID.f04Up2 || entrance.ID == ExitID.f04Up3 || entrance.ID == ExitID.f04GateYB;
                default:
                    return false;
            }
        }

        private void FixFullRandomEntranceLogic(Exit entrance1, Exit entrance2)
        {
            switch (entrance1.ID)
            {
                case ExitID.f00GateYA:
                {
                    entrance2.AppendLogicString(" and False");
                    break;
                }
                case ExitID.f00GateYB:
                {
                    entrance2.AppendLogicString(" and (CanWarp or CanKill(Nidhogg))");
                    break;
                }
                case ExitID.f00GateYC:
                {
                    entrance2.AppendLogicString(" and (CanWarp or Has(Birth Sigil))");
                    break;
                }
                case ExitID.fL02Left:
                {
                    entrance2.AppendLogicString($" and (CanWarp or CanReach({AreaID.AnnwfnMain}))");
                    break;
                }
                case ExitID.f02Down:
                {
                    entrance2.AppendLogicString(" and CanWarp and HorizontalAttack");
                    break;
                }
                case ExitID.f03Right:
                {
                    entrance2.AppendLogicString(" and (Has(Feather) or Has(Grapple Claw))");
                    break;
                }
                case ExitID.f03Down2:
                {
                    entrance2.AppendLogicString($" and Has(Life Sigil) and (CanWarp or CanReach({AreaID.IBDinosaur}))");
                    break;
                }
                case ExitID.f03Up:
                {
                    entrance2.AppendLogicString($" and (CanWarp or CanKill(Cetus) or CanReach({AreaID.IBMain}))");
                    break;
                }
                case ExitID.f03In:
                {
                    entrance2.AppendLogicString(" and CanWarp");
                    break;
                }
                case ExitID.f04Up3:
                {
                    entrance2.AppendLogicString(" and False");
                    break;
                }
                case ExitID.f06_2GateP0:
                {
                    entrance2.AppendLogicString(" and (CanKill(Tezcatlipoca) and (CanWarp or Has(Grapple Claw)))");
                    break;
                }
                case ExitID.f07GateP0:
                {
                    entrance2.AppendLogicString(" and (CanWarp or (Has(Pepper) and Has(Birth Sigil) and CanChant(Sun) and CanKill(Unicorn)))");
                    break;
                }
                case ExitID.f08Neck:
                {
                    entrance2.AppendLogicString("and (CanWarp or (CanChant(Heaven) and CanChant(Earth) and CanChant(Sea) and CanChant(Fire) and CanChant(Wind)))");
                    break;
                }
                case ExitID.f09GateP0:
                {
                    entrance2.AppendLogicString(" and CanWarp");
                    break;
                }
                case ExitID.f11Pyramid:
                {
                    entrance2.AppendLogicString(" and False");
                    break;
                }
            }

            entrance2.BuildLogicTree();
        }

        private void RandomiseSoulGateEntrances()
        {
            List<Exit> gates = Shuffle.FisherYates(GetExitsOfType(ExitType.SoulGate), random);
            List<int> soulAmounts;

            //if we are doing random souls pairs we only need one of each soul value amount as we will just pick from this each time but never remove
            //otherwise we'll use the vanilla soul value amounts and remove after each one is picked
            if (Settings.RandomSoulPairs)
                soulAmounts = new List<int>() { 1, 2, 3, 5 };
            else
                soulAmounts = new List<int>() { 1, 2, 2, 3, 3, 5, 5, 5 };

            List<Exit> priorityGates = new List<Exit>();

            Exit gate1 = null;
            Exit gate2 = null;

            //including the nine gates means we need to add the 9 into the list of soul amounts otherwise we'll place the nine gates like its vanilla
            if (Settings.IncludeNineGates)
            {
                soulAmounts.Add(9);
                //place the spiral boat soul gate first so it doesnt end up going somewhere it can't
                priorityGates.Add(gates.Find(x => x.ID == ExitID.f03GateN9));
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
            priorityGates.RemoveAll(x => x == null);
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

                do
                {
                    gate2 = gates[random.Next(gates.Count)];
                } while (gate1.ID == ExitID.f03GateN9 && gate2.ID == ExitID.f08GateN8);
                gates.Remove(gate2);

                int soulAmount = soulAmounts[random.Next(soulAmounts.Count)];
                //only remove if we are using the vanilla list of soul pair values
                if(!Settings.RandomSoulPairs)
                    soulAmounts.Remove(soulAmount);

                gate1.AppendLogicString($" and GuardianKills({soulAmount})");
                gate2.AppendLogicString($" and GuardianKills({soulAmount})");

                FixSoulGateLogic(gate1, gate2);
                FixSoulGateLogic(gate2, gate1);

                gate1.ConnectingAreaID = gate2.ParentAreaID;
                gate2.ConnectingAreaID = gate1.ParentAreaID;

                SoulGatePairs.Add((gate1, gate2, soulAmount));

                //if one of the soul gates is in EPG need to change the logic of the gates puzzle 
                if (gate1.ID == ExitID.f14GateN6 || gate2.ID == ExitID.f14GateN6)
                {
                    Exit toHel = exits.Find(exit => exit.ConnectingAreaID == AreaID.EPDHel);
                    if (gate1.ID == ExitID.f04GateN6 || gate2.ID == ExitID.f04GateN6)
                        toHel.AppendLogicString($" and IsDead(Vidofnir) and GuardianKills({ soulAmount})");
                    else
                        toHel.AppendLogicString($" and GuardianKills({ soulAmount})");
                    toHel.BuildLogicTree();
                }
            }
        }

        private void FixSoulGateLogic(Exit gate1, Exit gate2)
        {
            switch (gate1.ID)
            {
                case ExitID.f14GateN6: gate2.AppendLogicString(" and CanWarp"); break;
                case ExitID.f06GateN7: gate2.AppendLogicString(" and Has(Feather) and Has(Claydoll Suit)"); break;
                case ExitID.f12GateN8: gate2.AppendLogicString(" and (CanWarp or Has(Feather))"); break;
                case ExitID.f13GateN9: gate2.AppendLogicString(" and False"); break;
                default: break;
            }

            gate2.BuildLogicTree();
        }
    }
}
