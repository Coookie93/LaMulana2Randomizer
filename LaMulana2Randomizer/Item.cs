using LaMulana2RandomizerShared;
using Newtonsoft.Json;

namespace LaMulana2Randomizer
{
    public class Item
    {
        public string Name { get; private set; }
        public bool IsRequired { get; private set; }

        [JsonIgnore]
        public ItemID ID { get; private set; }

        [JsonConstructor]
        public Item(string name, ItemID id, bool isRequired = true)
        {
            Name = name;
            ID = id;
            IsRequired = isRequired;
        }
    }
}
