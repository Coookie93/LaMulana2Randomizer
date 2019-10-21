using System.Collections.Generic;
using System.Linq;
using LM2RandomizerShared;

namespace LM2Randomizer
{
    public class ItemPool
    {
        public static Item Get(ItemID id, List<Item> itemPool)
        {
            return itemPool.First(i => i.id == id);
        }

        public static Item GetAndRemove(ItemID id, List<Item> itemPool)
        {
            Item item = itemPool.First(i => i.id == id);
            itemPool.Remove(item);
            return item;
        }

        public static List<Item> GetRequiredItems(List<Item> itemPool)
        {
            var requiredItems = from item in itemPool
                                where item.isRequired
                                select item;

            return requiredItems.ToList();
        }

        public static List<Item> GetNonRequiredItems(List<Item> itemPool)
        {
            var nonRequiredItems = from item in itemPool
                                   where !item.isRequired
                                   select item;

            return nonRequiredItems.ToList();
        }
    }
}
