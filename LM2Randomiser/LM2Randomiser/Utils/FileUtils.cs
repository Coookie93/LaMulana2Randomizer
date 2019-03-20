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
        public static bool GetData(string filePath, int columns, out List<string[]> data)
        {
            string currentDir = Directory.GetCurrentDirectory();

            List<string[]> temp = new List<string[]>();
            try
            {
                using (StreamReader sr = File.OpenText(Path.Combine(currentDir, filePath)))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] info = line.Split(',');
                        if (info.Length == columns)
                        {
                            temp.Add(info);
                        }
                        else
                        {
                            Logger.GetLogger.Log("Error in line: {0} of file {1}, column count did not match.", line, filePath);
                            data = null;
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger.Log("Error: {1}", ex.Message);
                data = null;
                return false;
            }
            data = temp;
            return true;
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
                        foreach (var location in placedLocations)
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
                        foreach (var location in reachableLocations)
                        {
                            playthrough.CollectItem(location.item);
                            playthrough.collectedLocations.Add(location.name, true);
                            if ((int)location.item.id < 155)
                            {
                                sr.WriteLine("  {0} -> {1}", location.name, location.item.name);
                            }
                        }
                        sr.WriteLine("}");
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
                foreach (var location in placedLocations)
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
