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
            catch (Exception ex)
            {
                Logger.Log($"Failed to deserialise Settings.json.\n{ex.ToString()}");
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
            catch (Exception ex)
            {
                Logger.Log($"Failed to serialise Settings.json.\n{ex.ToString()}");
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
                Logger.Log($"Failed to deserialise World.json.\n{ex.ToString()}");
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
                Logger.Log($"Failed to deserialise Data\\items.json.\n{ex.ToString()}");
                throw new RandomiserException($"Failed to parse Data\\items.json.");
            }
        }
        
        public static void WriteSpoilerLog(Randomiser randomiser)
        {
            SpoilerLog spoilerLog = new SpoilerLog
            {
                Seed = randomiser.Seed,
                SettingsString = randomiser.Settings.GenerateSettingsString(),
                StartingArea = randomiser.StartingArea.Name,
                StartingWeapon = randomiser.StartingWeapon?.Name,
                StartingItems = randomiser.StartingItems.Select(item => item.Name).ToList(),
                Settings = randomiser.Settings
            };

            SortedList<string, string> entrances = new SortedList<string, string>();
            foreach (var pair in randomiser.EntrancePairs)
            {
                entrances.Add(pair.Item1.Name, pair.Item2.Name);
                entrances.Add(pair.Item2.Name, pair.Item1.Name);
            }

            spoilerLog.Entrances = entrances;

            SortedList<int, SortedList<string, string>> soulGates = new SortedList<int, SortedList<string, string>>();
            foreach (var group in randomiser.SoulGatePairs.GroupBy(x => x.Item3))
            {
                SortedList<string, string> pairs = new SortedList<string, string>();
                foreach(var item in group)
                {
                    pairs.Add(item.Item1.Name, item.Item2.Name);
                    pairs.Add(item.Item2.Name, item.Item1.Name);
                }
                soulGates.Add(group.Key, pairs);
            }
            spoilerLog.SoulGates = soulGates;

            Dictionary<string, string> locations = new Dictionary<string, string>();
            foreach (Location location in randomiser.GetPlacedLocations().OrderBy(x => x.ID))
            {
                if (location.ID < LocationID.Ratatoskr1 && location.ID != LocationID.None)
                    locations.Add(location.Name, location.Item.Name);
            }
            spoilerLog.Locations = locations;

            PlayerState state = new PlayerState(randomiser)
            {
                IgnoreFalseChecks = true
            };

            List<Location> reachableLocations;
            Dictionary<int, Dictionary<string, string>> playthrough = new Dictionary<int, Dictionary<string, string>>();
            int sphere = 0;
            do
            {
                Dictionary<string, string> spoilerSphere = new Dictionary<string, string>();
                reachableLocations = state.GetReachableLocations(randomiser.GetPlacedRequiredItemLocations());
                foreach (Location location in reachableLocations.OrderBy(x => x.ID))
                {
                    state.CollectItem(location.Item);
                    state.CollectLocation(location);
                    spoilerSphere.Add(location.Name,location.Item.Name);
                }

                playthrough.Add(sphere++, spoilerSphere);

                if (state.CanBeatGame())
                    break;

                state.RemoveFalseCheckedAreasAndEntrances();

            } while (reachableLocations.Count > 0);

            spoilerLog.Playthrough = playthrough;

            try
            {
                using (StreamWriter sw = new StreamWriter("Seed\\Spoilers.json"))
                using (JsonWriter jw = new JsonTextWriter(sw))
                {
                    JsonSerializer serializer = new JsonSerializer
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        Formatting = Formatting.Indented
                    };

                    serializer.Serialize(jw, spoilerLog);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to write spoiler log.\n {ex.ToString()}");
                throw new RandomiserException("Failed to write spoiler log.");
            }
        }

        public static void WriteSeedFile(Randomiser randomiser)
        {
            List<(LocationID, ItemID)> items = new List<(LocationID, ItemID)>();
            List<(LocationID, ItemID, int)> shopItems = new List<(LocationID, ItemID, int)>();
            foreach (Location location in randomiser.GetPlacedLocations())
            {
                if (location.ID < LocationID.Ratatoskr1 && location.ID != LocationID.None && location.Item.ID != ItemID.None)
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
                    br.Write(randomiser.Settings.RandomDissonance);
                    br.Write(randomiser.Settings.RequiredGuardians);
                    br.Write(randomiser.Settings.RequiredSkulls);
                    br.Write(randomiser.Settings.RemoveITStatue);
                    br.Write((int)randomiser.Settings.ChosenEchidna);
                    br.Write(randomiser.Settings.AutoScanTablets);
                    br.Write(randomiser.Settings.AutoPlaceSkulls);
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

                    br.Write(randomiser.CursedLocations.Count);
                    foreach (Location location in randomiser.CursedLocations)
                        br.Write((int)location.ID);

                    br.Write(randomiser.EntrancePairs.Count);
                    foreach(var d in randomiser.EntrancePairs)
                    {
                        br.Write((int)d.Item1.ID);
                        br.Write((int)d.Item2.ID);
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
                Logger.Log($"Failed to write seed file.\n {ex.ToString()}");
                throw new RandomiserException("Failed to write seed file.");
            }
        }
    }
}
