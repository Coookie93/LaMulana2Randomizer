using Newtonsoft.Json;
using LaMulana2RandomizerShared;

namespace LaMulana2Randomizer
{
    public class Item
    {
        public string Name { get; private set; }
        public bool IsRequired { get; private set; }

        [JsonIgnore]
        public ItemID ID { get; private set; }

        public int PriceMultiplier;

        [JsonConstructor]
        public Item(string name, ItemID id, bool isRequired = false)
        {
            Name = name;
            ID = id;
            IsRequired = isRequired;
            PriceMultiplier = 10;
        }
    }
}
