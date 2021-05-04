using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using LaMulana2RandomizerShared;
using LaMulana2Randomizer.Utils;

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

        public void Remove(ItemID id)
        {
            items.Remove(Get(id));
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

        public ItemPool GetandRemoveRequiredItems()
        {
            List<Item> required = items.Where(item => item.IsRequired).ToList();
            foreach (Item item in required)
                items.Remove(item);

            return new ItemPool(required);
        }

        public ItemPool GetandRemoveNonRequiredItems()
        {
            List<Item> nonRequired = items.Where(item => !item.IsRequired).ToList();
            foreach (Item item in nonRequired)
                items.Remove(item);

            return new ItemPool(nonRequired);
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

        public ItemPool Copy()
        {
            List<Item> copies = new List<Item>();

            foreach (Item item in items)
                copies.Add(item.DeepCopy());

            return new ItemPool(copies);
        }

        public IEnumerator GetEnumerator()
        {
            foreach (Item item in items)
                yield return item;
        }

        //
        public ItemPool CreateRandomShopPool(int size, Random random)
        {
            if (size <= 0)
                throw new RandomiserException($"Can not create a random shop item pool with size {size}.");

            var shopItems = GetAndRemoveShopOnlyItems();
            if (shopItems.Count <= 0)
                throw new RandomiserException($"Can not create a random shop item pool from {size} shop items.");

            ItemPool itemPool = new ItemPool(shopItems);
            for (; size > 0; size--)
                itemPool.Add(shopItems[random.Next(shopItems.Count)]);

            return itemPool;
        }
    }
}
