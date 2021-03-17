using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using LaMulana2RandomizerShared;

namespace LaMulana2Randomizer.Utils
{
    public abstract class FileUtils
    {
        public static Settings LoadSettings()
        {
            try
            {
                using (StreamReader sr = File.OpenText("Settings.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    return (Settings)serializer.Deserialize(sr, typeof(Settings));
                }
            }
            catch(Exception ex)
            {
                Logger.Log($"Failed to deserialise Settings.json.\n{ex.Message}");
            }
            return new Settings();
        }

        public static void SaveSettings(Settings settings)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter("Settings.json"))
                using (JsonWriter jw = new JsonTextWriter(sw))
                {
                    JsonSerializer serializer = new JsonSerializer
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        Formatting = Formatting.Indented
                    };

                    serializer.Serialize(jw, settings);
                }
            }
            catch(Exception ex)
            {
                Logger.Log($"Failed to serialise Settings.json.\n{ex.Message}");
            }
        }

        public static List<JsonArea> LoadWorldData()
        {
            try
            {
                using (StreamReader sr = File.OpenText("Data\\World.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    return (List<JsonArea>)serializer.Deserialize(sr, typeof(List<JsonArea>));
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to deserialise World.json.\n{ex.Message}");
                throw new RandomiserException("Failed to parse World.json.");
            }
        }

        public static List<Item> LoadItemFile(string name)
        {
            try
            {
                using (StreamReader sr = File.OpenText(Path.Combine("Data", name)))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    return (List<Item>)serializer.Deserialize(sr, typeof(List<Item>));
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to deserialise Data\\items.json.\n{ex.Message}");
                throw new RandomiserException($"Failed to parse Data\\items.json.");
            }
        }
        
        public static bool WriteSpoilerLog(Randomiser randomiser)
        {
            try
            {
                using (StreamWriter sw = File.CreateText("Seed\\spoilers.txt"))
                {
                    sw.WriteLine($"Seed: {randomiser.Settings.Seed}\n");

                    sw.WriteLine($"Starting Area: {randomiser.StartingArea.Name}\n");
                    sw.WriteLine($"Starting Weapon: {randomiser.StartingWeapon?.Name}\n");

                    if(randomiser.StartingItems.Count != 0)
                    {
                        sw.WriteLine("Starting Items: {");
                        foreach (Item item in randomiser.StartingItems)
                            sw.WriteLine($"  {item.Name}");

                        sw.WriteLine("}\n");
                    }

                    sw.WriteLine("Curse Locations: {");
                    foreach (Location location in randomiser.CursedLocations)
                        sw.WriteLine($"  {location.Name}");

                    sw.WriteLine("}\n");

                    sw.WriteLine("Entrance Placement: {");
                    if (!randomiser.Settings.FullRandomEntrances)
                    {
                        if (randomiser.Settings.RandomHorizontalEntrances)
                        {
                            sw.WriteLine("  Horizontal Entrances: {");
                            foreach (string pair in randomiser.HorizontalPairs)
                                sw.WriteLine($"    {pair}");

                            sw.WriteLine("  }");
                        }

                        if (randomiser.Settings.RandomLadderEntrances)
                        {
                            sw.WriteLine("  Ladders Entrances: {");
                            foreach (string pair in randomiser.LadderPairs)
                                sw.WriteLine($"    {pair}");

                            sw.WriteLine("  }");
                        }

                        if (randomiser.Settings.RandomGateEntrances)
                        {
                            sw.WriteLine("  Gate Entrances: {");
                            foreach (string pair in randomiser.GatePairs)
                                sw.WriteLine($"    {pair}");

                            sw.WriteLine("  }");
                        }
                    }
                    else
                    {
                        sw.WriteLine("  Entrances: {");
                        foreach (string pair in randomiser.EntrancePairs)
                            sw.WriteLine($"    {pair}");

                        sw.WriteLine("  }");
                    }

                    if (randomiser.Settings.RandomSoulGateEntrances)
                    {
                        sw.WriteLine("  Soul Gate Entrances: {");
                        foreach (var pair in randomiser.SoulGatePairs)
                            sw.WriteLine($"    {pair.Item1.Name} - {pair.Item2.Name}: Soul Amount {pair.Item3}");

                        sw.WriteLine("  }");
                    }
                    sw.WriteLine("}\n");

                    sw.WriteLine("Item Placement {");
                    foreach (Location location in randomiser.GetPlacedLocations().OrderBy(x => x.ID))
                    {
                        if (location.ID < LocationID.DissonanceValhalla && location.ID != LocationID.None)
                            sw.WriteLine($"  {location.Name} -> {location.Item.Name}");
                    }
                    sw.WriteLine("}\n");
                    sw.WriteLine("Expected Playthrough {");

                    PlayerState playthrough = new PlayerState(randomiser)
                    {
                        IgnoreFalseChecks = true
                    };

                    List<Location> reachableLocations;
                    int sphere = 0;
                    do
                    {
                        reachableLocations = playthrough.GetReachableLocations(randomiser.GetPlacedRequiredItemLocations());
                        sw.WriteLine($"  Sphere {sphere} {{");
                        foreach (Location location in reachableLocations.OrderBy(x => x.ID))
                        {
                            playthrough.CollectItem(location.Item);
                            playthrough.CollectLocation(location);
                            sw.WriteLine($"    {location.Name} -> {location.Item.Name}");
                        }
                        sw.WriteLine("  }");

                        if (playthrough.CanBeatGame())
                            break;

                        playthrough.RemoveFalseCheckedAreasAndEntrances();
                        sphere++;

                    } while (reachableLocations.Count > 0);
                    sw.WriteLine("}");
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to write spoiler log.\n {ex.Message}");
                return false;
            }
        }

        public static bool WriteSeedFile(Randomiser randomiser)
        {
            List<(LocationID, ItemID)> items = new List<(LocationID, ItemID)>();
            List<(LocationID, ItemID, int)> shopItems = new List<(LocationID, ItemID, int)>();
            foreach (Location location in randomiser.GetPlacedLocations())
            {
                if (location.ID < LocationID.DissonanceValhalla && location.ID != LocationID.None)
                {
                    if (location.LocationType == LocationType.Shop)
                    {
                        shopItems.Add((location.ID, location.Item.ID, location.Item.PriceMultiplier));
                    }
                    else
                    {
                        items.Add((location.ID, location.Item.ID));
                    }
                }
            }

            try
            {   
                using (BinaryWriter br = new BinaryWriter(File.Open("Seed\\seed.lm2r", FileMode.Create)))
                {
                    br.Write((int)randomiser.StartingWeapon.ID);
                    br.Write((int)randomiser.StartingArea.ID);
                    br.Write(randomiser.Settings.RequiredSkulls);
                    br.Write(randomiser.Settings.RemoveITStatue);
                    br.Write(randomiser.Settings.EasyEchidna);
                    br.Write(randomiser.Settings.AutoScanTablets);
                    br.Write(randomiser.Settings.AutoPlaceSkulls);
                    br.Write(randomiser.Settings.FastCorridor);
                    br.Write(randomiser.Settings.StartingMoney);
                    br.Write(randomiser.Settings.StartingWeights);
                    br.Write((int)randomiser.Settings.ItemChestColour);
                    br.Write((int)randomiser.Settings.WeightChestColour);

                    br.Write(randomiser.StartingItems.Count);
                    foreach (var item in randomiser.StartingItems)
                        br.Write((int)item.ID);

                    br.Write(items.Count);
                    foreach(var item in items)
                    {
                        br.Write((int)item.Item1);
                        br.Write((int)item.Item2);
                    }

                    br.Write(shopItems.Count);
                    foreach (var item in shopItems)
                    {
                        br.Write((int)item.Item1);
                        br.Write((int)item.Item2);
                        br.Write(item.Item3);
                    }

                    foreach (Location location in randomiser.CursedLocations)
                        br.Write((int)location.ID);

                    br.Write(randomiser.ExitPairs.Count);
                    foreach(var d in randomiser.ExitPairs)
                    {
                        br.Write((int)d.Item1);
                        br.Write((int)d.Item2);
                    }

                    br.Write(randomiser.SoulGatePairs.Count);
                    foreach (var s in randomiser.SoulGatePairs)
                    {
                        br.Write((int)s.Item1.ID);
                        br.Write((int)s.Item2.ID);
                        br.Write(s.Item3);
                    }
                }
                Logger.Log($"Total items randomised {items.Count + shopItems.Count}");
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to write seed file.\n {ex.Message}");
                return false;
            }
            return true;
        }
    }
}
