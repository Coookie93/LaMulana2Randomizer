using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using LaMulana2RandomizerShared;

namespace LaMulana2Randomizer.Utils
{
    public abstract class FileUtils
    {
        public static List<JsonArea> GetWorldData()
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

        public static void GetItemsFromJson(string filePath, out List<Item> itemPool)
        {
            try
            {
                using (StreamReader sr = File.OpenText(filePath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    itemPool = (List<Item>)serializer.Deserialize(sr, typeof(List<Item>));
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to deserialise {filePath}.\n{ex.Message}");
                throw new RandomiserException($"Failed to parse {filePath}.");
            }
        }
        
        public static bool WriteSpoilers(Randomiser randomiser)
        {
            try
            {
                using (StreamWriter sr = File.CreateText("Seed\\spoilers.txt"))
                {
                    sr.WriteLine($"Seed: {randomiser.Settings.Seed}");
                    sr.WriteLine();

                    sr.WriteLine($"Starting Weapon: {randomiser.StartingWeapon.name}");
                    sr.WriteLine();

                    if (randomiser.Settings.RandomCurses)
                    {
                        sr.WriteLine("Curse Locations:");
                        foreach(Location location in randomiser.CursedLocations)
                        {
                            sr.WriteLine($"  {location.Name}");
                        }
                        sr.WriteLine();
                    }

                    foreach (LocationID id in Enum.GetValues(typeof(LocationID)))
                    {
                        foreach (Location location in randomiser.GetPlacedLocations())
                        {
                            if (id != LocationID.None && id == location.Id)
                            {
                                sr.WriteLine($"{location.Name} -> {location.Item.name}");
                            }
                        }
                    }
                    sr.WriteLine();
                    sr.WriteLine("Expected Playthrough");

                    PlayerState playthrough = new PlayerState(randomiser);
                    List<Location> reachableLocations;

                    do
                    {
                        reachableLocations = playthrough.GetReachableLocations(randomiser.GetPlacedRequiredItemLocations());
                        sr.WriteLine("{");
                        foreach (Location location in reachableLocations)
                        {
                            playthrough.CollectItem(location.Item);
                            playthrough.collectedLocations.Add(location.Name, true);
                            sr.WriteLine($"  {location.Name} -> {location.Item.name}");
                        }
                        sr.WriteLine("}");

                        if (playthrough.CanBeatGame(reachableLocations))
                        {
                            break;
                        }

                        playthrough.ResetCheckedAreasAndEntrances();
                        
                    } while (reachableLocations.Count > 0);
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
            List<Tuple<LocationID, ItemID>> temp = new List<Tuple<LocationID, ItemID>>();
            foreach (LocationID id in Enum.GetValues(typeof(LocationID)))
            {
                foreach (Location location in randomiser.GetPlacedLocations())
                {
                    if (id != LocationID.None && id == location.Id)
                    {
                        temp.Add(new Tuple<LocationID, ItemID>(location.Id, location.Item.Id));
                    }
                }
            }

            try
            {   using (BinaryWriter br = new BinaryWriter(File.Open("Seed\\seed.lm2r", FileMode.Create)))
                {
                    br.Write(randomiser.Settings.AutScanTablets);
                    br.Write(temp.Count);
                    foreach(var p in temp)
                    {
                        br.Write((int)p.Item1);
                        br.Write((int)p.Item2);
                    }
                    foreach(Location location in randomiser.CursedLocations)
                    {
                        br.Write((int)location.Id);
                    }
                }
                Logger.Log($"Total items randomised {temp.Count}");
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
