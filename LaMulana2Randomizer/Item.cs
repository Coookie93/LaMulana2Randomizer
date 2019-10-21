using LM2RandomizerShared;
using Newtonsoft.Json;

namespace LM2Randomizer
{
    public class Item
    {
        public string name;
        public bool isRequired;

        [JsonIgnore]
        public ItemID id;
        
        [JsonConstructor]
        public Item(string name, ItemID id, bool isRequired = true)
        {
            this.name = name;
            this.id = id;
            this.isRequired = isRequired;
        }
    }
}
