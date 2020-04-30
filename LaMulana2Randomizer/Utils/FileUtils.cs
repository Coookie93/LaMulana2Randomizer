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

        public static List<Item> GetItemsFromJson()
        {
            try
            {
                using (StreamReader sr = File.OpenText("Data//Items.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    return (List<Item>)serializer.Deserialize(sr, typeof(List<Item>));
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to deserialise Data//Items.json.\n{ex.Message}");
                throw new RandomiserException($"Failed to parse Data//Items.json.");
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

                    sr.WriteLine($"Starting Weapon: {randomiser.StartingWeapon.Name}");
                    sr.WriteLine();

                    sr.WriteLine("Curse Locations: {");
                    foreach (Location location in randomiser.CursedLocations)
                        sr.WriteLine($"  {location.Name}");

                    sr.WriteLine("}");
                    sr.WriteLine();

                    sr.WriteLine("Horizontal Entrances: {");
                    foreach (var pair in randomiser.DoorPairs)
                        sr.WriteLine($"  {pair.Item1.Name} - {pair.Item2.Name}");

                    sr.WriteLine("}");
                    sr.WriteLine();

                    sr.WriteLine("Ladder Entrances: {");
                    foreach (var pair in randomiser.LadderPairs)
                        sr.WriteLine($"  {pair.Item1.Name} - {pair.Item2.Name}");

                    sr.WriteLine("}");
                    sr.WriteLine();

                    sr.WriteLine("Gate Entrances: {");
                    foreach (var pair in randomiser.GatePairs)
                        sr.WriteLine($"  {pair.Item1.Name} - {pair.Item2.Name}");

                    sr.WriteLine("}");
                    sr.WriteLine();

                    sr.WriteLine("Soul Gate Entrances: {");
                    foreach (var pair in randomiser.SoulGatePairs)
                        sr.WriteLine($"  {pair.Item1.Name} - {pair.Item2.Name}: Soul Amount {pair.Item3}");

                    sr.WriteLine("}");
                    sr.WriteLine();

                    sr.WriteLine("Item Locations {");
                    foreach (LocationID id in Enum.GetValues(typeof(LocationID)))
                    {
                        foreach (Location location in randomiser.GetPlacedLocations())
                        {
                            if (id != LocationID.None && id == location.ID)
                                sr.WriteLine($"  {location.Name} -> {location.Item.Name}");
                        }
                    }
                    sr.WriteLine("}");
                    sr.WriteLine();
                    sr.WriteLine("Expected Playthrough {");

                    PlayerState playthrough = new PlayerState(randomiser);
                    playthrough.CollectItem(randomiser.StartingWeapon);

                    List<Location> reachableLocations;

                    int sphere = 0;
                    do
                    {
                        reachableLocations = playthrough.GetReachableLocations(randomiser.GetPlacedRequiredItemLocations());
                        sr.WriteLine($"  Sphere {sphere} {{");
                        foreach (Location location in reachableLocations)
                        {
                            playthrough.CollectItem(location.Item);
                            playthrough.CollectLocation(location);
                            sr.WriteLine($"    {location.Name} -> {location.Item.Name}");
                        }
                        sr.WriteLine("  }");

                        if (playthrough.CanBeatGame())
                            break;

                        playthrough.ResetCheckedAreasAndEntrances();
                        sphere++;

                    } while (reachableLocations.Count > 0);
                    sr.WriteLine("}");
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
                    if (id != LocationID.None && id == location.ID)
                    {
                        temp.Add(new Tuple<LocationID, ItemID>(location.ID, location.Item.ID));
                    }
                }
            }

            try
            {   using (BinaryWriter br = new BinaryWriter(File.Open("Seed\\seed.lm2r", FileMode.Create)))
                {
                    br.Write(randomiser.Settings.AutoScanTablets);
                    br.Write(randomiser.Settings.AutoPlaceSkulls);
                    br.Write(temp.Count);
                    foreach(var p in temp)
                    {
                        br.Write((int)p.Item1);
                        br.Write((int)p.Item2);
                    }
                    foreach(Location location in randomiser.CursedLocations)
                    {
                        br.Write((int)location.ID);
                    }
                    br.Write(randomiser.DoorPairs.Count);
                    foreach(var d in randomiser.DoorPairs)
                    {
                        br.Write((int)d.Item1.ID);
                        br.Write((int)d.Item2.ID);
                    }
                    br.Write(randomiser.LadderPairs.Count);
                    foreach(var l in randomiser.LadderPairs)
                    {
                        br.Write((int)l.Item1.ID);
                        br.Write((int)l.Item2.ID);
                    }
                    br.Write(randomiser.GatePairs.Count);
                    foreach (var l in randomiser.GatePairs)
                    {
                        br.Write((int)l.Item1.ID);
                        br.Write((int)l.Item2.ID);
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
