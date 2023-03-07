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
        public List<(Exit, Exit)> EntrancePairs { get; private set; }
        public List<(Exit, Exit, int)> SoulGatePairs { get; private set; }
        public Settings Settings { get; private set; }
        public int Seed { get; private set; }

        public Randomiser(Settings settings, int seed)
        {
            Settings = settings;
            Seed = seed;
            random = new Random(seed);
            StartingItems = new List<Item>();
            CursedLocations = new List<Location>();
            areas = new Dictionary<AreaID, Area>();
            locations = new Dictionary<LocationID, Location>();
            exits = new List<Exit>();
            EntrancePairs = new List<(Exit, Exit)>();
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
            PlaceOriginalResearch();
            PlaceOriginalDissonance();
            RemoveItems();

            FixNibiruLogic();
            ChooseEchidnaType();
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

            //change the exit to spiral gates logic depending on whether dissonance is randomised
            var spiralGate = GetExitsOfType(ExitType.SpiralGate).First();
            if(Settings.RandomDissonance)
                spiralGate.AppendLogicString($" and GuardianKills(5)");
            else
                spiralGate.AppendLogicString($" and IsDead(Anu)");
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
                    guardian.AppendLogicString($" and AnkhCount({ankhs})");
            }
        }

        public void FixFDCLogic()
        {
            if (Settings.FDCForBacksides)
            {
                foreach(Exit exit in exits)
                {
                    if (exit.ExitType != ExitType.Internal && GetArea(exit.ConnectingAreaID).IsBackside)
                        exit.AppendLogicString(" and Has(Future Development Company)");
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
            EntrancePairs.Clear();
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
            ItemID scanItem = ItemID.FakeScan01;
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
                    case LocationType.Chest:
                    case LocationType.Dissonance:
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
            int freeSlots = 24;
            if (StartingWeapon.ID > ItemID.Katana) freeSlots -= 1;
            if (StartingArea.ID != AreaID.VoD) freeSlots += 3;

            if (Settings.ShopPlacement == ShopPlacement.Random)
            {
                //lock this shop only items can go here since the item needs to be purchased to see the forth slot
                GetLocation(LocationID.HinerShop3).IsLocked = true;
                RandomiseWithChecks(GetUnplacedLocationsOfType(LocationType.Shop), items.CreateRandomShopPool(freeSlots, random), items);
                GetLocation(LocationID.HinerShop3).IsLocked = false;
            }
            else if(Settings.ShopPlacement == ShopPlacement.AtLeastOne)
            {
                //lock the third slot of each shop so there will be atleast one item in to buy in each shop
                foreach (var location in GetUnplacedLocationsOfType(LocationType.Shop)) {
                    if (location.Name.Contains("3"))
                        location.IsLocked = true;
                }
                RandomiseWithChecks(GetUnplacedLocationsOfType(LocationType.Shop), items.CreateRandomShopPool(freeSlots, random), items);

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

                GetLocation(LocationID.KorobockShop1).PlaceItem(Items.Get(ItemID.Weights), false);
                GetLocation(LocationID.KorobockShop2).PlaceItem(Items.Get(ItemID.ShurikenAmmo), false);
                GetLocation(LocationID.KorobockShop3).PlaceItem(Items.GetAndRemove(ItemID.Guild), false);

                GetLocation(LocationID.ShuhokaShop1).PlaceItem(Items.Get(ItemID.Weights), false);
                GetLocation(LocationID.ShuhokaShop2).PlaceItem(Items.Get(ItemID.ShurikenAmmo), false);
                GetLocation(LocationID.ShuhokaShop3).PlaceItem(Items.GetAndRemove(ItemID.Alert), false);

                GetLocation(LocationID.PymShop1).PlaceItem(Items.Get(ItemID.Weights), false);
                GetLocation(LocationID.PymShop2).PlaceItem(Items.Get(ItemID.RollingShurikenAmmo), false);
                GetLocation(LocationID.PymShop3).PlaceItem(Items.GetAndRemove(ItemID.Snapshot), false);

                GetLocation(LocationID.BTKShop1).PlaceItem(Items.Get(ItemID.Weights), false);
                GetLocation(LocationID.BTKShop2).PlaceItem(Items.Get(ItemID.CaltropsAmmo), false);
                GetLocation(LocationID.BTKShop3).PlaceItem(Items.GetAndRemove(ItemID.EngaMusica), false);

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

                GetLocation(LocationID.VenumShop1).PlaceItem(Items.Get(ItemID.Weights), false);
                GetLocation(LocationID.VenumShop2).PlaceItem(Items.Get(ItemID.CaltropsAmmo), false);
                GetLocation(LocationID.VenumShop3).PlaceItem(Items.GetAndRemove(ItemID.SpaceCapstarII), false);

                GetLocation(LocationID.MegarockShop1).PlaceItem(Items.Get(ItemID.Weights), false);
                GetLocation(LocationID.MegarockShop2).PlaceItem(Items.Get(ItemID.EarthSpearAmmo), false);
                GetLocation(LocationID.MegarockShop3).PlaceItem(Items.GetAndRemove(ItemID.Bracelet), false);

                GetLocation(LocationID.FairylanShop1).PlaceItem(Items.Get(ItemID.Weights), false);
                GetLocation(LocationID.FairylanShop2).PlaceItem(Items.Get(ItemID.ChakramAmmo), false);
                GetLocation(LocationID.FairylanShop3).PlaceItem(Items.GetAndRemove(ItemID.Shield3), false);

                if (StartingArea.ID != AreaID.VoD)
                {
                    if (StartingWeapon.ID <= ItemID.Katana)
                        GetLocation(LocationID.StartingShop2).PlaceItem(Items.Get(ItemID.Weights), false);

                    GetLocation(LocationID.StartingShop3).PlaceItem(Items.Get(ItemID.Weights), false);
                }

                //after placing the weights/ammo remove them as they arent needed anymore
                Items.GetAndRemoveShopOnlyItems();
            }
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

        private bool PlaceMantras(ItemPool items)
        {
            if (Settings.MantraPlacement == MantraPlacement.OnlyMurals)
                return RandomiseWithChecks(GetUnplacedLocationsOfType(LocationType.Mural), new ItemPool(items.GetAndRemoveMantras()), items);

            return true;
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

        private void PlaceOriginalResearch()
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

        private void PlaceOriginalDissonance()
        {
            if (Settings.RandomDissonance)
            {
                Items.Add(new Item("Progressive Beherit", ItemID.ProgressiveBeherit2, true));
                Items.Add(new Item("Progressive Beherit", ItemID.ProgressiveBeherit3, true));
                Items.Add(new Item("Progressive Beherit", ItemID.ProgressiveBeherit4, true));
                Items.Add(new Item("Progressive Beherit", ItemID.ProgressiveBeherit5, true));
                Items.Add(new Item("Progressive Beherit", ItemID.ProgressiveBeherit6, true));
                Items.Add(new Item("Progressive Beherit", ItemID.ProgressiveBeherit7, true));
            }
            else
            {
                GetLocation(LocationID.DissonanceDSLM).PlaceItem(new Item("Dissonance", ItemID.None, true), false);
                GetLocation(LocationID.DissonanceEPG).PlaceItem(new Item("Dissonance", ItemID.None, true), false);
                GetLocation(LocationID.DissonanceHL).PlaceItem(new Item("Dissonance", ItemID.None, true), false);
                GetLocation(LocationID.DissonanceMoG).PlaceItem(new Item("Dissonance", ItemID.None, true), false);
                GetLocation(LocationID.DissonanceNibiru).PlaceItem(new Item("Dissonance", ItemID.None, true), false);
                GetLocation(LocationID.DissonanceValhalla).PlaceItem(new Item("Dissonance", ItemID.None, true), false);
            }
        }

        private void ChooseStartingWeapon()
        {
            ItemID[] weapons = new ItemID[]{ ItemID.Whip1, ItemID.Knife, ItemID.Rapier, ItemID.Axe, ItemID.Katana, ItemID.Shuriken, 
                                                ItemID.RollingShuriken, ItemID.EarthSpear, ItemID.Flare, ItemID.Caltrops, ItemID.Chakram, 
                                                ItemID.Bomb, ItemID.Pistol, ItemID.ClaydollSuit};

            List<ItemID> selectedWeapons = weapons.Zip(Settings.GetWeaponChoices(), Tuple.Create).Where(w => w.Item2).Select(w => w.Item1).ToList();

            //remove the starting weapon from the item pool
            StartingWeapon = Items.GetAndRemove(selectedWeapons[random.Next(selectedWeapons.Count)]);
        }

        private void ChooseStartingArea()
        {
            //choose starting area from areas which where chosen
            AreaID[] areas = new AreaID[] { AreaID.VoD, AreaID.RoY, AreaID.AnnwfnMain, AreaID.IBMain, AreaID.ITLeft, AreaID.DFMain, AreaID.SotFGGrail,
                                                AreaID.TSLeft, AreaID.ValhallaMain, AreaID.DSLMMain, AreaID.ACTablet, AreaID.HoMTop};
            List<AreaID> selectedAreas = areas.Zip(Settings.GetStartingAreaChoices(), Tuple.Create).Where(a => a.Item2).Select(a => a.Item1).ToList();
            StartingArea = GetArea(selectedAreas[random.Next(selectedAreas.Count)]);

            //if we arent starting in the village a shop needs to be added
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
                    if (Settings.RandomGateEntrances) entrances.Add(ExitID.f01Right);
                    if (Settings.IncludeUniqueTransitions) entrances.Add(ExitID.f01Start);
                    break;
                }
                case AreaID.RoY:
                {
                    entrances.Add(ExitID.f00GateY0);
                    break;
                }
                case AreaID.AnnwfnMain:
                {
                    if (Settings.RandomLadderEntrances) entrances.Add(ExitID.f02Up);
                    if (Settings.IncludeUniqueTransitions) entrances.Add(ExitID.f02Down);
                    break;
                }
                case AreaID.IBMain:
                {
                    entrances.Add(ExitID.f03Right);
                    break;
                }
                case AreaID.ITLeft:
                {
                    entrances.Add(ExitID.f04Up);
                    break;
                }
                case AreaID.DFMain:
                {
                    entrances.Add(ExitID.f05GateP1);
                    break;
                }
                case AreaID.SotFGGrail:
                {
                    entrances.Add(ExitID.f06GateP0);
                    break;
                }
                case AreaID.TSLeft:
                {
                    entrances.Add(ExitID.f08GateP0);
                    break;
                }
                case AreaID.ValhallaMain:
                {
                    entrances.Add(ExitID.f10GateP0);
                    break;
                }
                case AreaID.DSLMMain:
                {
                    entrances.Add(ExitID.f11GateP0);
                    break;
                }
                case AreaID.ACTablet:
                {
                    entrances.Add(ExitID.f12GateP0);
                    break;
                }
                case AreaID.HoMTop:
                {
                    entrances.Add(ExitID.f13GateP0);
                    break;
                }
            }

            startingEntrance = entrances.Count > 0 ? entrances[random.Next(entrances.Count)] : ExitID.None;
        }

        private void CheckStartingItems()
        {
            if (Settings.RandomGrail == ItemPlacement.Starting) StartingItems.Add(Items.GetAndRemove(ItemID.HolyGrail));
            if (Settings.RandomScanner == ItemPlacement.Starting) StartingItems.Add(Items.GetAndRemove(ItemID.HandScanner));
            if (Settings.RandomCodices == ItemPlacement.Starting) StartingItems.Add(Items.GetAndRemove(ItemID.Codices));
            if (Settings.RandomFDC == ItemPlacement.Starting) StartingItems.Add(Items.GetAndRemove(ItemID.FutureDevelopmentCompany));
            if (Settings.RandomRing == ItemPlacement.Starting) StartingItems.Add(Items.GetAndRemove(ItemID.Ring));
            if (Settings.RandomShellHorn == ItemPlacement.Starting) StartingItems.Add(Items.GetAndRemove(ItemID.ShellHorn));
            if (Settings.RandomMapping == ItemPlacement.Starting)
            {
                StartingItems.Add(Items.GetAndRemove(ItemID.YagooMapReader));
                StartingItems.Add(Items.GetAndRemove(ItemID.YagooMapStreet));
                for(ItemID id = ItemID.Map1; id <= ItemID.Map16; id++)
                    StartingItems.Add(Items.GetAndRemove(id));
            }
        }

        private void PlaceAvailableAtStart(ItemPool items)
        {
            //create a list of the items we want to place that are accessible from the start
            ItemPool earlyItems = new ItemPool();
            if (Settings.RandomGrail == ItemPlacement.AvailableAtStart) earlyItems.Add(items.GetAndRemove(ItemID.HolyGrail));
            if (Settings.RandomScanner == ItemPlacement.AvailableAtStart) earlyItems.Add(items.GetAndRemove(ItemID.HandScanner));
            if (Settings.RandomCodices == ItemPlacement.AvailableAtStart) earlyItems.Add(items.GetAndRemove(ItemID.Codices));
            if (Settings.RandomFDC == ItemPlacement.AvailableAtStart) earlyItems.Add(items.GetAndRemove(ItemID.FutureDevelopmentCompany));
            if (Settings.RandomRing == ItemPlacement.AvailableAtStart) earlyItems.Add(items.GetAndRemove(ItemID.Ring));
            if (Settings.RandomShellHorn == ItemPlacement.AvailableAtStart) earlyItems.Add(items.GetAndRemove(ItemID.ShellHorn));

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
                for (ItemID id = ItemID.Map1; id <= ItemID.Map16; id++)
                    Items.Remove(id);
            }

            if (Settings.RemoveResearch)
            {
                for (ItemID id = ItemID.Research1; id <= ItemID.Research10; id++)
                    Items.Remove(id);
            }

            if (Settings.RemoveSkulls)
            {
                int skullsToRemove = 12 - Settings.RequiredSkulls;
                for (ItemID id = ItemID.CrystalSkull1; id < ItemID.CrystalSkull1 + skullsToRemove; id++)
                    Items.Remove(id);
            }
        }

        private void FixNibiruLogic()
        {
            //fix some logic based on settings chosen
            Location nibiruDiss = GetLocation(LocationID.DissonanceNibiru);
            nibiruDiss.AppendLogicString($" and SkullCount({Settings.RequiredSkulls})");
        }

        private void ChooseEchidnaType()
        {
            if (Settings.Echidna == EchidnaType.Random)
                Settings.ChosenEchidna = (EchidnaType)random.Next(0, 4);
            else
                Settings.ChosenEchidna = Settings.Echidna;
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

            //Hiner Shop 3 always needs an item, so place one first if its empty
            Location hinerShop3 = locations.Find(x => x.ID == LocationID.HinerShop3);
            if (hinerShop3 != null) {
                hinerShop3.PlaceItem(items.RandomGetAndRemove(random), true);
                locations.Remove(hinerShop3);
            }

            while (items.ItemCount > 0)
            {
                Location location = locations.Last();
                location.PlaceItem(items.RandomGetAndRemove(random), true);
                locations.Remove(location);
            }
        }

        private void RandomiseCurses()
        {
            if (Settings.RandomCurses)
            {
                var chestLocations = Shuffle.FisherYates(GetUnplacedLocationsOfType(LocationType.Chest), random);
                for (int i = 0; i < Settings.TotalCursedChests; i++)
                {
                    Location location = chestLocations[i];
                    location.AppendLogicString("and Has(Mulana Talisman)");
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

            List<Exit> priorityLeftDoors = new List<Exit>
            {
                leftDoors.Find(x => x.ID == ExitID.fP02Left),
                leftDoors.Find(x => x.ID == ExitID.fP00Left)
            };

            foreach (Exit exit in priorityLeftDoors)
                leftDoors.Remove(exit);

            bool cavernToCliff = false;
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

                leftDoor.ConnectingAreaID = rightDoor.ParentAreaID;
                rightDoor.ConnectingAreaID = leftDoor.ParentAreaID;

                EntrancePairs.Add((leftDoor, rightDoor));
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
                } while (StartEntranceLoopCheck(downLadder) || downLadder.IsDeadEnd);
                downLadders.Remove(downLadder);

                upLadder.ConnectingAreaID = downLadder.ParentAreaID;
                downLadder.ConnectingAreaID = upLadder.ParentAreaID;

                EntrancePairs.Add((upLadder, downLadder));
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

                upLadder.ConnectingAreaID = downLadder.ParentAreaID;
                downLadder.ConnectingAreaID = upLadder.ParentAreaID;

                EntrancePairs.Add((upLadder, downLadder));
            }
        }

        private void RandomiseGateEntrances()
        {
            List<Exit> gates = Shuffle.FisherYates(GetExitsOfType(ExitType.Gate), random);

            Exit gate1 = null;
            Exit gate2 = null;

            if (Settings.ReduceDeadEndStarts && (startingEntrance == ExitID.f00GateY0 || startingEntrance == ExitID.f04GateYB || startingEntrance == ExitID.f05GateP1))
            {
                gate1 = gates.Find(x => x.ID == startingEntrance);
                gates.Remove(gate1);

                do
                {
                    gate2 = gates[random.Next(gates.Count)];
                } while (StartEntranceLoopCheck(gate2) || gate2.IsDeadEnd);
                gates.Remove(gate2);

                gate2.ConnectingAreaID = gate1.ParentAreaID;
                gate1.ConnectingAreaID = gate2.ParentAreaID;

                EntrancePairs.Add((gate1, gate2));
            }

            //stop illusion looping on itself if it can
            if (Settings.AllAccessible)
            {
                gate1 = gates.Find(x => x.ID == ExitID.fL11GateN);
                if (gate1 != null)
                {
                    gates.Remove(gate1);
                    do
                    {
                        gate2 = gates[random.Next(gates.Count)];
                    } while (gate2.ID == ExitID.fL11GateY0);
                    gates.Remove(gate2);

                    gate2.ConnectingAreaID = gate1.ParentAreaID;
                    gate1.ConnectingAreaID = gate2.ParentAreaID;

                    EntrancePairs.Add((gate1, gate2));
                }
            }

            List<Exit> priorityGates = new List<Exit>();
            if (Settings.AllAccessible)
                priorityGates.AddRange(gates.Where(x => x.IsInaccessible));

            if (Settings.CostumeClip)
                priorityGates.Remove(gates.Find(x => x.ID == ExitID.f12GateP0));

            priorityGates.RemoveAll(x => x == null);
            foreach (Exit gate in priorityGates)
                gates.Remove(gate);

            while (gates.Count > 0)
            {
                if (priorityGates.Count > 0)
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

                gate1.ConnectingAreaID = gate2.ParentAreaID;
                gate2.ConnectingAreaID = gate1.ParentAreaID;

                EntrancePairs.Add((gate1, gate2));
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
                entrances.AddRange(GetExitsOfType(ExitType.Altar));
            }

            Exit entrance1 = null;
            Exit entrance2 = null;

            if (Settings.RandomHorizontalEntrances)
            {
                //place the cliff first just to see if gets placed anywhere that affects any other placement logic
                entrance1 = entrances.Find(x => x.ID == ExitID.fP02Left);
                entrances.Remove(entrance1);
                do
                {
                    entrance2 = entrances[random.Next(entrances.Count)];
                } while ((entrance2.ID ==  startingEntrance && Settings.ReduceDeadEndStarts) || 
                         (entrance2.ID == ExitID.f12GateP0 && !Settings.CostumeClip) || 
                          entrance2.IsOneWay || entrance2.IsInaccessible);   
                
                entrances.Remove(entrance2);

                entrance1.ConnectingAreaID = entrance2.ParentAreaID;
                entrance2.ConnectingAreaID = entrance1.ParentAreaID;

                EntrancePairs.Add((entrance1, entrance2));

                //place one of the cavern entrances to stop it looping on itself
                if (Settings.AllAccessible)
                {
                    entrance1 = entrances.Find(x => x.ID == ExitID.fP00Left);
                    if (entrance1 != null)
                    {
                        entrances.Remove(entrance1);
                        do
                        {
                            entrance2 = entrances[random.Next(entrances.Count)];
                        } while (entrance2.ID == ExitID.fP00Right);
                        entrances.Remove(entrance2);

                        entrance1.ConnectingAreaID = entrance2.ParentAreaID;
                        entrance2.ConnectingAreaID = entrance1.ParentAreaID;

                        EntrancePairs.Add((entrance1, entrance2));
                    }
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
                    } while (StartEntranceLoopCheck(entrance2) || entrance2.IsDeadEnd);
                    entrances.Remove(entrance2);

                    entrance1.ConnectingAreaID = entrance2.ParentAreaID;
                    entrance2.ConnectingAreaID = entrance1.ParentAreaID;

                    EntrancePairs.Add((entrance1, entrance2));
                }
            }

            //try to place an illusion entrance to stop it looping on itself if its already placed it doesn't matter
            if (Settings.RandomGateEntrances && Settings.AllAccessible)
            {
                entrance1 = entrances.Find(x => x.ID == ExitID.fL11GateN);
                if (entrance1 != null)
                {
                    entrances.Remove(entrance1);
                    do
                    {
                        entrance2 = entrances[random.Next(entrances.Count)];
                    } while (entrance2.ID == ExitID.fL11GateY0);

                    entrances.Remove(entrance2);

                    entrance1.ConnectingAreaID = entrance2.ParentAreaID;
                    entrance2.ConnectingAreaID = entrance1.ParentAreaID;

                    EntrancePairs.Add((entrance1, entrance2));
                }
            }

            //try to place an altar entrance to stop it looping on itself if its already placed it doesn't matter
            if (Settings.IncludeUniqueTransitions && Settings.AllAccessible)
            {
                entrance1 = entrances.Find(x => x.ID == ExitID.fP01Left);
                if (entrance1 != null)
                {
                    entrances.Remove(entrance1);
                    do
                    {
                        entrance2 = entrances[random.Next(entrances.Count)];
                    } while (entrance2.ID == ExitID.fP01Right);

                    entrances.Remove(entrance2);

                    entrance1.ConnectingAreaID = entrance2.ParentAreaID;
                    entrance2.ConnectingAreaID = entrance1.ParentAreaID;

                    EntrancePairs.Add((entrance1, entrance2));
                }
            }

            if (Settings.AllAccessible)
                priorityEntrances.AddRange(entrances.Where(x => x.IsInaccessible));

            if (Settings.CostumeClip)
                priorityEntrances.Remove(entrances.Find(x => x.ID == ExitID.f12GateP0));

            priorityEntrances.RemoveAll(x => x == null);
            foreach (Exit entrance in priorityEntrances)
                entrances.Remove(entrance);

            while (entrances.Count > 0)
            {
                if (priorityEntrances.Count > 0)
                {
                    entrance1 = priorityEntrances[random.Next(priorityEntrances.Count)];
                    priorityEntrances.Remove(entrance1);
                }
                else
                {
                    entrance1 = entrances[random.Next(entrances.Count)];
                    entrances.Remove(entrance1);
                }

                entrance2 = entrances[random.Next(entrances.Count)];
                entrances.Remove(entrance2);

                entrance1.ConnectingAreaID = entrance2.ParentAreaID;
                entrance2.ConnectingAreaID = entrance1.ParentAreaID;

                EntrancePairs.Add((entrance1, entrance2));
            }
        }

        private bool StartEntranceLoopCheck(Exit entrance)
        {
            switch (startingEntrance)
            {
                case ExitID.f00GateY0:
                    return entrance.ID == ExitID.f00GateYA && entrance.ID == ExitID.f00GateYB && entrance.ID == ExitID.f00GateYC && 
                        entrance.ID == ExitID.f00Down;
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

                //the spirit bote soul gate can't go to belial or EPG with dissonance rando
                do
                {
                    gate2 = gates[random.Next(gates.Count)];
                } while (gate1.ID == ExitID.f03GateN9 && (gate2.ID == ExitID.f08GateN8 || (gate2.ID == ExitID.f14GateN6 && Settings.RandomDissonance)));
                gates.Remove(gate2);

                int soulAmount;

                //the EPG soul gate can't be 9 because its needed to beat a guardian
                do
                {
                    soulAmount = soulAmounts[random.Next(soulAmounts.Count)];
                } while ((Settings.AllAccessible || !Settings.RandomDissonance) && (gate1.ID == ExitID.f14GateN6 || gate2.ID == ExitID.f14GateN6) && soulAmount == 9);

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
        }
    }
}
