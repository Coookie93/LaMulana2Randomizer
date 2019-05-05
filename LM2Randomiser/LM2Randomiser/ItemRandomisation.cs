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
        public static void RandomiseSpecificItems(Randomiser randomiser, List<Location> locations, List<Item> itemsToPlace, List<Item> currentItems)
        {
            PlayerState state;
            
            while(itemsToPlace.Count > 0)
            {
                Item item = itemsToPlace[itemsToPlace.Count - 1];
                itemsToPlace.Remove(item);
                locations = Shuffle.FisherYates(locations, randomiser);

                state = PlayerState.GetStateWithItems(randomiser, currentItems);

                Location locationToPlaceAt = null;
                foreach(Location location in locations)
                {
                    if (location.CanReach(state))
                    {
                        locationToPlaceAt = location;
                        break;
                    }
                }

                if(locationToPlaceAt != null)
                {
                    randomiser.PlaceItem(locationToPlaceAt.name, item);
                    locations.Remove(locationToPlaceAt);
                   //Logger.GetLogger.Log("Placed Item {0} at location {1}", item[0], locationToPlaceAt.name);
                }
                else
                {
                    Logger.GetLogger.Log("Failed to place item {0}", item.name);
                }
            }

        }

        public static void RandomiseRequiredItems(Randomiser world, List<Location> locations, List<Item> itemsToPlace)
        {
            PlayerState state;

            while (itemsToPlace.Count > 0)
            {
                Item item = itemsToPlace[itemsToPlace.Count - 1];
                itemsToPlace.Remove(item);
                locations = Shuffle.FisherYates(locations, world);

                state = PlayerState.GetStateWithItems(world, itemsToPlace);

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
                    world.PlaceItem(locationToPlaceAt.name, item);
                    locations.Remove(locationToPlaceAt);
                    //Logger.GetLogger.Log("Placed Item {0} at location {1}", item[0], locationToPlaceAt.name);
                }
                else
                {
                    Logger.GetLogger.Log("Failed to place item {0}", item.name);
                }
            }
        }

        public static void RandomiseUnrequiredItems(Randomiser world, List<Location> locations, List<Item> itemsToPlace)
        {
            locations = Shuffle.FisherYates(locations, world);
            int index = (itemsToPlace.Count - 1);

            while (index >= 0)
            {
                Item item = itemsToPlace[index];
                itemsToPlace.Remove(item);

                Location location = locations[index];
                locations.Remove(location);
                
                world.PlaceItem(location.name, item);
                //Logger.GetLogger.Log("Placed Item {0} at location {1}", item[0], location.name);

                index--;
            }
        }
    }
}
