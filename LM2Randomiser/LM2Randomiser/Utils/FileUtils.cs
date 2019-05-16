using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using LM2Randomiser.Logging;
using LM2Randomiser;

namespace LM2Randomiser.Utils
{
    public abstract class FileUtils
    {
        public static bool GetWorldData(out Dictionary<string, Area> areas)
        {
            string currentDir = Directory.GetCurrentDirectory();
            try
            {
                using (StreamReader sr = File.OpenText(Path.Combine(currentDir, "Data\\world.json")))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    areas = (Dictionary<string, Area>)serializer.Deserialize(sr, typeof(Dictionary<string, Area>));
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger.Log("Tried to deserialise world.json, Error: {0}", ex.Message);
                areas = null;
                return false;
            }
        }

        public static bool GetItemsFromJson(string filePath, out List<Item> itemPool)
        {
            string currentDir = Directory.GetCurrentDirectory();
            try
            {
                using (StreamReader sr = File.OpenText(Path.Combine(currentDir, filePath)))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    itemPool = (List<Item>)serializer.Deserialize(sr, typeof(List<Item>));
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger.Log("Error: {0}", ex.Message);
                itemPool = null;
                return false;
            }

            return true;
        }

        public static bool GetHardRequirementsFromJson(out List<Location> hardreqs)
        {
            string currentDir = Directory.GetCurrentDirectory();
            try
            {
                using (StreamReader sr = File.OpenText(Path.Combine(currentDir, "Data\\hardreqs.json")))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    hardreqs = (List<Location>)serializer.Deserialize(sr, typeof(List<Location>));
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger.Log("Error: {0}", ex.Message);
                hardreqs = null;
                return false;
            }

            return true;
        }

        public static bool WriteSpoilers(Randomiser randomiser)
        {
            string currentDir = Directory.GetCurrentDirectory();
            List<Location> placedLocations = randomiser.GetPlacedLocations();

            try
            {
                using (StreamWriter sr = File.CreateText(Path.Combine(currentDir, "Seed\\spoilers.txt")))
                {

                    sr.WriteLine("Seed: {0}", randomiser.Seed);
                    sr.WriteLine();

                    foreach (LocationID id in Enum.GetValues(typeof(LocationID)))
                    {
                        foreach (Location location in placedLocations)
                        {
                            if (id == location.id && id != LocationID.None)
                            {
                                sr.WriteLine("{0} -> {1}", location.name, location.item.name);
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
                            playthrough.CollectItem(location.item);
                            playthrough.collectedLocations.Add(location.name, true);
                            if (location.item.id < ItemID.Weights)
                            {
                                sr.WriteLine("  {0} -> {1}", location.name, location.item.name);
                            }
                        }
                        sr.WriteLine("}");

                        if (playthrough.CanBeatGame(reachableLocations))
                        {
                            break;
                        }

                        playthrough.ResetCheckedAreasAndEntrances();
                        
                    } while (reachableLocations.Count > 0);
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger.Log("Error: {0}", ex.Message);
                return false;
            }

            return true;
        }
        public static bool WriteSeedFile(Randomiser randomiser)
        {
            string currentDir = Directory.GetCurrentDirectory();
            List<Location> placedLocations = randomiser.GetPlacedLocations();
            
            Dictionary<int, int> itemLocation = new Dictionary<int, int>();

            foreach (LocationID id in Enum.GetValues(typeof(LocationID)))
            {
                foreach (Location location in placedLocations)
                {
                    if (id == location.id && id != LocationID.None)
                    {
                        itemLocation.Add((int)location.id, (int)location.item.id);
                    }
                }
            }
                
            FileStream fs = new FileStream(Path.Combine(currentDir, "Seed\\seed.lm2"), FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, itemLocation);
            }
            catch (Exception ex)
            {
                Logger.GetLogger.Log("Error: {0}", ex.Message);
                return false;
            }
            finally
            {
                fs.Close();
            }
            return true;
        }
    }
}
