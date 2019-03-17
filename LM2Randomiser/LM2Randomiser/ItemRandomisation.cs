using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LM2Randomiser.Logging;
using LM2Randomiser.Utils;

namespace LM2Randomiser
{
    public abstract class ItemRandomisation
    {
        public static void RandomiseSpecificItems(Randomiser world, List<Location> locations, List<string[]> itemsToPlace, List<string[]> currentItems)
        {
            PlayerState state;
            
            while(itemsToPlace.Count > 0)
            {
                string[] item = itemsToPlace[itemsToPlace.Count - 1];
                itemsToPlace.Remove(item);
                locations = Shuffle.FisherYates(locations, world);

                state = PlayerState.GetStateWithItems(world, currentItems);

                Location locationToPlaceAt = null;
                foreach(var location in locations)
                {
                    if (location.CanReach(state))
                    {
                        locationToPlaceAt = location;
                        break;
                    }
                }

                if(locationToPlaceAt != null)
                {
                    world.PlaceItem(locationToPlaceAt.name, new Item(item[0], item[1], true));
                    locations.Remove(locationToPlaceAt);
                   //Logger.GetLogger.Log("Placed Item {0} at location {1}", item[0], locationToPlaceAt.name);
                }
                else
                {
                    Logger.GetLogger.Log("Failed to place item {0}", item[0]);
                }
            }

        }

        public static void RandomiseRequiredItems(Randomiser world, List<Location> locations, List<string[]> itemsToPlace)
        {
            PlayerState state;

            while (itemsToPlace.Count > 0)
            {
                string[] item = itemsToPlace[itemsToPlace.Count - 1];
                itemsToPlace.Remove(item);
                locations = Shuffle.FisherYates(locations, world);

                state = PlayerState.GetStateWithItems(world, itemsToPlace);

                Location locationToPlaceAt = null;
                foreach (var location in locations)
                {
                    if (location.CanReach(state))
                    {
                        locationToPlaceAt = location;
                        break;
                    }
                }

                if (locationToPlaceAt != null)
                {
                    world.PlaceItem(locationToPlaceAt.name, new Item(item[0], item[1], true));
                    locations.Remove(locationToPlaceAt);
                    //Logger.GetLogger.Log("Placed Item {0} at location {1}", item[0], locationToPlaceAt.name);
                }
                else
                {
                    Logger.GetLogger.Log("Failed to place item {0}", item[0]);
                }
            }
        }

        public static void RandomiseUnrequiredItems(Randomiser world, List<Location> locations, List<string[]> itemsToPlace)
        {
            locations = Shuffle.FisherYates(locations, world);
            int index = (itemsToPlace.Count - 1);

            while (index >= 0)
            {
                string[] item = itemsToPlace[index];
                itemsToPlace.Remove(item);

                Location location = locations[index];
                locations.Remove(location);
                
                world.PlaceItem(location.name, new Item(item[0], item[1], true));
                //Logger.GetLogger.Log("Placed Item {0} at location {1}", item[0], location.name);

                index--;
            }
        }
    }
}
