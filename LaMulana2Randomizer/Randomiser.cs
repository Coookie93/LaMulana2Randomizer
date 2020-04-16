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
        private List<Connection> exits;

        private bool villageDeadEnd = false;

        public Item StartingWeapon { get; private set; }
        public List<Location> CursedLocations { get; private set; }
        public List<(Connection, Connection)> LeftRightPairs { get; private set; }
        public List<(Connection, Connection)> DownUpLadderPairs { get; private set; }
        public List<(Connection, Connection)> GatePairs { get; private set; }
        public Settings Settings { get; private set; }

        public Randomiser(Settings settings)
        {
            Settings = settings;
            random = new Random(settings.Seed);

            areas = new Dictionary<string, Area>();
            locations = new Dictionary<string, Location>();
            exits = new List<Connection>();
            LeftRightPairs = new List<(Connection, Connection)>();
            DownUpLadderPairs = new List<(Connection, Connection)>();
            GatePairs = new List<(Connection, Connection)>();
        }
        
        public void Setup()
        {
            List<JsonArea> worldData = FileUtils.GetWorldData();

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
                        exit.AppendLogicString(" and Has(Future Development Company)");

                    exit.BuildLogicTree();
                    area.Exits.Add(exit);
                    exits.Add(exit);
                }
                areas.Add(area.Name, area); 
            }

            if(Settings.RandomHorizontalEntraces)
                RandomiseHorizontalEntrances();
                
            if(Settings.RandomLadderEntraces)
                RandomiseVerticalEntrances();

            if (Settings.RandomGateEntraces)
                RandomiseGateEntrances();

            foreach (Area area in areas.Values)
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
            List<Item> items = FileUtils.GetItemsFromJson();

            StartingWeapon = ItemPool.GetAndRemove(ItemID.Whip, items);

            //Places weights at a starting shop so the player can buy them at the start of the game
            GetLocation("Nebur Shop 1").PlaceItem(ItemPool.GetAndRemove(ItemID.Weights, items));

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

        public void RandomiseCurses()
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

        public void Clear()
        {
            areas.Clear();
            locations.Clear();
            exits.Clear();
            LeftRightPairs.Clear();
            DownUpLadderPairs.Clear();
            GatePairs.Clear();
        }

        public void ClearPlacedItems()
        {
            foreach(Location location in locations.Values)
            {
                if (location.Item != null && location.Item.Id != ItemID.None)
                    location.PlaceItem(null);
            }
        }

        public void RandomiseHorizontalEntrances()
        {
            List<Connection> leftDoors = Shuffle.FisherYates(GetConnectionsOfType(ConnectionType.LeftDoor), random);
            List<Connection> rightDoors = Shuffle.FisherYates(GetConnectionsOfType(ConnectionType.RightDoor), random);
            List<Connection> priorityLeftDoors = new List<Connection>
            {
                leftDoors.Find(x => x.Name.Equals("Cliff A1")),
                leftDoors.Find(x => x.Name.Equals("Cavern A1"))
            };

            bool cavernToCliff = false;
            bool cavernToVillage = false;
            Connection leftDoor = null;
            Connection rightDoor = null;

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

                if (leftDoor.Name.Equals("Cliff A1"))
                {
                    do
                    {
                        rightDoor = rightDoors[random.Next(rightDoors.Count)];
                    } while ((rightDoor.Name.Equals("Village of Departure F5") && !Settings.RandomLadderEntraces) || rightDoor.Name.Equals("Endless Corridor C1"));
                }
                else if (leftDoor.Name.Equals("Cavern A1"))
                {
                    do
                    {
                        rightDoor = rightDoors[random.Next(rightDoors.Count)];
                    } while (rightDoor.Name.Equals("Cavern D1") || (((rightDoor.Name.Equals("Village of Departure F5") && !Settings.RandomLadderEntraces)
                                || rightDoor.Name.Equals("Endless Corridor C1")) && cavernToCliff));
                }
                else
                {
                    rightDoor = rightDoors[random.Next(rightDoors.Count)];
                }

                if (leftDoor.Name.Equals("Cliff A1") && rightDoor.Name.Equals("Cavern D1"))
                    cavernToCliff = true;

                if (rightDoor.Name.Equals("Village of Departure F5") && (leftDoor.Name.Equals("Cavern A1") && !cavernToCliff))
                    cavernToVillage = true;

                if (rightDoor.Name.Equals("Village of Departure F5") && (leftDoor.Name.Equals("Cliff A1") || (leftDoor.Name.Equals("Cavern A1") && cavernToCliff)))
                    villageDeadEnd = true;

                if (leftDoor.Name.Equals("Mausoleum of Giants A5"))
                {
                    if (rightDoor.Name.Equals("Village of Departure") || (rightDoor.Name.Equals("Cavern D1") && cavernToVillage))
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

                LeftRightPairs.Add((leftDoor, rightDoor));
                leftDoors.Remove(leftDoor);
                rightDoors.Remove(rightDoor);
            }
        }

        public void RandomiseVerticalEntrances()
        {
            List<Connection> downLadders = Shuffle.FisherYates(GetConnectionsOfType(ConnectionType.DownLadder), random);
            List<Connection> upLadders = Shuffle.FisherYates(GetConnectionsOfType(ConnectionType.UpLadder), random);
            List<Connection> priorityDownLadders = new List<Connection>()
            {
                downLadders.Find(x => x.Name.Equals("Annwfn E5")),
                downLadders.Find(x => x.Name.Equals("Immortal Battlefield G7(left)"))
            };
            if (villageDeadEnd)
                priorityDownLadders.Add(downLadders.Find(x => x.Name.Equals("Village of Departure F3")));

            Connection downLadder = null;
            Connection upLadder = null;

            while (upLadders.Count > 0)
            {
                if(priorityDownLadders.Count > 0)
                {
                    downLadder = priorityDownLadders.First();
                    priorityDownLadders.Remove(downLadder);
                }
                else
                {
                    downLadder = downLadders[random.Next(downLadders.Count)];
                }

                if(downLadder.Name.Equals("Annwfn E5") || downLadder.Name.Equals("Immortal Battlefield G7(left)"))
                {
                    do
                    {
                        upLadder = upLadders[random.Next(upLadders.Count)];
                    } while (upLadder.Name.Equals("Inferno Cavern B1"));
                }
                else if(downLadder.Name.Equals("Village of Departure F3"))
                {
                    do
                    {
                        upLadder = upLadders[random.Next(upLadders.Count)];
                    } while (upLadder.Name.Equals("Inferno Cavern B1") && villageDeadEnd);
                }
                else
                {
                    upLadder = upLadders[random.Next(upLadders.Count)];
                }

                if (downLadder.Name.Equals("Annwfn E5"))
                {
                    upLadder.AppendLogicString(" and (CanReach(Annwfn Main) or CanWarp)");
                    upLadder.BuildLogicTree();
                }
                else if(downLadder.Name.Equals("Immortal Battlefield G7(left)"))
                {
                    upLadder.AppendLogicString(" and Has(Life Sigil) and (CanWarp or CanReach(Immortal Battlefield Dinosaur))");
                    upLadder.BuildLogicTree();
                }
                
                if(upLadder.Name.Equals("Immortal Battlefield F1"))
                {
                    downLadder.AppendLogicString(" and (CanWarp or CanKill(Cetus) or CanReach(Immortal Battlefield Main))");
                    downLadder.BuildLogicTree();
                }

                upLadder.ConnectingAreaName = downLadder.ParentAreaName;
                downLadder.ConnectingAreaName = upLadder.ParentAreaName;

                DownUpLadderPairs.Add((upLadder, downLadder));
                upLadders.Remove(upLadder);
                downLadders.Remove(downLadder);
            }
        }

        public void RandomiseGateEntrances()
        {
            List<Connection> gates = Shuffle.FisherYates(GetConnectionsOfType(ConnectionType.Gate), random);
            List<Connection> priorityGates = new List<Connection>()
            {
                gates.Find(x => x.Name.Equals("Annwfn G4")),
                gates.Find(x => x.Name.Equals("Shrine of the Frost Giants B4")),
                gates.Find(x => x.Name.Equals("Ancient Chaos D6")),
                gates.Find(x => x.Name.Equals("Hall of Malice C1")),
                gates.Find(x => x.Name.Equals("Gate of Illusion A1"))
            };

            foreach (Connection gate in priorityGates)
                gates.Remove(gate);

            Connection gate1 = null;
            Connection gate2 = null;

            while(gates.Count > 0)
            {
                if (priorityGates.Count > 0)
                {
                    gate1 = priorityGates.First();
                    priorityGates.Remove(gate1);
                }
                else
                {
                    gate1 = gates[random.Next(gates.Count)];
                }

                if (gate1.Name.Equals("Gate of Illusion A1"))
                {
                    do
                    {
                        gate2 = gates[random.Next(gates.Count)];
                    } while (gate2.Name.Equals("Gate of Illusion C1"));
                }
                else if (gate1.Name.Equals("Annwfn G4") || gate1.Name.Equals("Shrine of the Frost Giants B4") || 
                            gate1.Name.Equals("Ancient Chaos D6") || gate1.Name.Equals("Hall of Malice C1"))
                {
                    do
                    {
                        gate2 = gates[random.Next(gates.Count)];
                    } while (gate2.Name.Equals("Dark Star Lord's Mausoleum D7") || gate2.Name.Equals("Icefire Treetop D3") ||
                                gate1.Name.Equals("mmortal Battlefield A6"));
                }
                else
                {
                    do
                    {
                        gate2 = gates[random.Next(gates.Count)];
                    } while (gate1.Equals(gate2));
                }

                if (gate1.Name.Equals("Gate of the Dead F5") && !gate2.Name.Equals("Heavens Labyrinth D1"))
                {
                    gate2.AppendLogicString(" and False");
                    gate2.BuildLogicTree();
                }
                else if (gate1.Name.Equals("Heavens Labyrinth D1") && !gate2.Name.Equals("Gate of the Dead F5"))
                {
                    gate2.AppendLogicString(" and False");
                    gate2.BuildLogicTree();
                }

                if (gate2.Name.Equals("Gate of the Dead F5") && !gate1.Name.Equals("Heavens Labyrinth D1"))
                {
                    gate1.AppendLogicString(" and False");
                    gate1.BuildLogicTree();
                }
                else if (gate2.Name.Equals("Heavens Labyrinth D1") && !gate1.Name.Equals("Gate of the Dead F5"))
                {
                    gate1.AppendLogicString(" and False");
                    gate1.BuildLogicTree();
                }

                gate1.ConnectingAreaName = gate2.ParentAreaName;
                gate2.ConnectingAreaName = gate1.ParentAreaName;

                GatePairs.Add((gate1, gate2));
                gates.Remove(gate1);
                gates.Remove(gate2);
            }
        }

        public bool CanBeatGame()
        {
            if (PlayerState.CanBeatGame(this,GetPlacedRequiredItemLocations()))
            {
                foreach (Location guardian in GetPlacedLocationsOfType(LocationType.Guardian))
                {
                    if (!PlayerState.SoftlockCheck(this, GetPlacedRequiredItemLocations(), guardian))
                        return false;
                }
                return true;
            }
            return false;
        }
        
        public bool EntranceCheck()
        {
            return PlayerState.EntrancePlacementCheck(this, GetPlacedRequiredItemLocations(), FileUtils.GetItemsFromJson());
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
            else if(Settings.MantraPlacement== MantraPlacement.OnlyMurals)
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
                    Logger.Log($"Failed to place item {item.name}");
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
                    Logger.Log($"Failed to place item {item.name}");
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

        private List<Connection> GetConnectionsOfType(ConnectionType type)
        {
            var connections = from connection in exits
                        where connection.ConnectionType == type
                        select connection;

            return connections.ToList();
        }
    }
}
