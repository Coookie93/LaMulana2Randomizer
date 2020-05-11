using System;
using Newtonsoft.Json;
using LaMulana2RandomizerShared;

namespace LaMulana2Randomizer
{
    public class Item
    {
        public string Name { get; private set; }
        public int Price { get; private set; }
        public bool IsRequired { get; private set; }

        [JsonIgnore]
        public ItemID ID { get; private set; }

        [JsonConstructor]
        public Item(string name, ItemID id, int price, bool isRequired = true)
        {
            Name = name;
            ID = id;
            Price = price;
            IsRequired = isRequired;
        }

        public void AdjustPrice(float multiplier)
        {
            Price = (int)Math.Round(Price * multiplier);
        }
    }
}
