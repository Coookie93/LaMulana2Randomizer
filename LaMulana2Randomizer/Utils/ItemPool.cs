using System.Collections.Generic;
using System.Linq;
using LaMulana2RandomizerShared;

namespace LaMulana2Randomizer
{
    public class ItemPool
    {
        public static Item Get(ItemID id, List<Item> itemPool)
        {
            return itemPool.First(i => i.ID == id);
        }

        public static Item GetAndRemove(ItemID id, List<Item> itemPool)
        {
            Item item = itemPool.First(i => i.ID == id);
            itemPool.Remove(item);
            return item;
        }

        public static List<Item> GetRequiredItems(List<Item> itemPool)
        {
            return itemPool.Where(item => item.IsRequired).ToList();
        }

        public static List<Item> GetNonRequiredItems(List<Item> itemPool)
        {
            return itemPool.Where(item => !item.IsRequired).ToList();
        }

        public static List<Item> GetAndRemoveShopOnlyItems(List<Item> itemPool)
        {
            var shopItems = itemPool.Where(item => item.ID >= ItemID.ShurikenAmmo && item.ID <= ItemID.Weights).ToList();
            foreach (Item item in shopItems)
                itemPool.Remove(item);

            return shopItems;
        }

        public static List<Item> GetAndRemoveMantras(List<Item> itemPool)
        {
            var mantras = itemPool.Where(item => item.ID >= ItemID.Heaven && item.ID <= ItemID.Night).ToList();
            foreach (Item mantra in mantras)
                itemPool.Remove(mantra);

            return mantras;
        }
    }
}
