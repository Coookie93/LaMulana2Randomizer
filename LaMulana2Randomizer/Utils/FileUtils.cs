using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using LM2Randomizer.Logging;
using LM2RandomizerShared;

namespace LM2Randomizer.Utils
{
    public abstract class FileUtils
    {
        public static bool GetWorldData(out List<JsonArea> areas)
        {
            try
            {
                using (StreamReader sr = File.OpenText("Data\\World.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    areas = (List<JsonArea>)serializer.Deserialize(sr, typeof(List<JsonArea>));
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to deserialise World.json, {ex.Message}");
                areas = null;
                return false;
            }
        }

        public static bool GetItemsFromJson(string filePath, out List<Item> itemPool)
        {
            try
            {
                using (StreamReader sr = File.OpenText(filePath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    itemPool = (List<Item>)serializer.Deserialize(sr, typeof(List<Item>));
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to deserialise {filePath}, {ex.Message}");
                itemPool = null;
                return false;
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
                Logger.Log($"Failed to write spoiler log, {ex.Message}");
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
                }
                Logger.Log($"Total items randomised {temp.Count}");
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to write seed file, {ex.Message}");
                return false;
            }
            return true;
        }
    }
}
