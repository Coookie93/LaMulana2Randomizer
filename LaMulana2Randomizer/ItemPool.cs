using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using LaMulana2RandomizerShared;

namespace LaMulana2Randomizer
{
    public class ItemPool : IEnumerable
    {
        private readonly List<Item> items;

        public int ItemCount {
            get => items.Count;
        }

        public ItemPool() 
        {
            items = new List<Item>();
        }

        public ItemPool(List<Item> items)
        {
            this.items = items;
        }

        public void Add(Item item)
        {
            items.Add(item);
        }

        public Item Get(ItemID id)
        {
            return items.Find(i => i.ID == id);
        }

        public Item GetAndRemove(ItemID id)
        {
            Item item = Get(id);
            items.Remove(item);
            return item;
        }

        public Item RandomGetAndRemove(Random random)
        {
            Item item = items[random.Next(items.Count)];
            items.Remove(item);
            return item;
        }

        public List<Item> GetandRemoveRequiredItems()
        {
            List<Item> required = items.Where(item => item.IsRequired).ToList();
            foreach (Item item in required)
                items.Remove(item);

            return required;
        }

        public List<Item> GetandRemoveNonRequiredItems()
        {
            List<Item> nonRequired = items.Where(item => !item.IsRequired).ToList();
            foreach (Item item in nonRequired)
                items.Remove(item);

            return nonRequired;
        }

        public List<Item> GetAndRemoveShopOnlyItems()
        {
            List<Item> shopItems = items.Where(item => item.ID >= ItemID.ShurikenAmmo && item.ID <= ItemID.Weights).ToList();
            foreach (Item item in shopItems)
                items.Remove(item);

            return shopItems;
        }

        public List<Item> GetAndRemoveMantras()
        {
            var mantras = items.Where(item => item.ID >= ItemID.Heaven && item.ID <= ItemID.Night).ToList();
            foreach (Item mantra in mantras)
                items.Remove(mantra);

            return mantras;
        }


        public IEnumerator GetEnumerator()
        {
            foreach (Item item in items)
                yield return item;
        }

        public static ItemPool CreateRandomShopPool(Random random, bool subWeaponStart)
        {
            List<Item> shopItems = new List<Item>()
            {
                new Item("Weights", ItemID.Weights, true),
                new Item("Shuriken Ammo", ItemID.ShurikenAmmo, true),
                new Item("Rolling Shuriken Ammo", ItemID.RollingShurikenAmmo, true),
                new Item("Earth Spear Ammo", ItemID.EarthSpearAmmo, true),
                new Item("Flare Ammo", ItemID.FlareAmmo, true),
                new Item("Caltrops Ammo", ItemID.CaltropsAmmo, true),
                new Item("Chakram Ammo", ItemID.ChakramAmmo, true),
                new Item("Bomb Ammo", ItemID.BombAmmo, true),
                new Item("Pistol Ammo", ItemID.PistolAmmo, true)
            };

            ItemPool itemPool = new ItemPool(shopItems);

            int remainingItems = subWeaponStart ? 23 : 24;

            for (; remainingItems > 0; remainingItems--)
                itemPool.Add(shopItems[random.Next(shopItems.Count)]);

            return itemPool;
        }
    }
}
