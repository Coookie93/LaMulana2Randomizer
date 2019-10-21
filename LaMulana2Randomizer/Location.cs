using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using LM2Randomizer.RuleParsing;
using LM2Randomizer.ExtensionMethods;
using LM2RandomizerShared;

namespace LM2Randomizer
{
    public class JsonLocation
    {
        public string Name;

        [JsonConverter(typeof(StringEnumConverter))]
        public LocationType LocationType;

        public string RuleString;
        public string HardString;
        public Item Item;
    }

    public class Location
    {
        public string Name { get; private set; }
        public LocationType LocationType { get; private set; }
        public Item Item { get; private set; }
        public LocationID Id { get; private set; }

        public BinaryNode Rules;
        public string ParentAreaName;
        public bool IsLocked = false;

        private string ruleString;
        private readonly string hardRuleString;

        public Location(JsonLocation jsonLocation)
        {
            Name = jsonLocation.Name;
            LocationType = jsonLocation.LocationType;
            ruleString = jsonLocation.RuleString;
            hardRuleString = jsonLocation.HardString;
            Item = jsonLocation.Item;
            Enum.TryParse(Name.RemoveWhitespace(), out LocationID temp);
            Id = temp;
        }
        
        public bool CanReach(PlayerState state)
        {
            return Rules.Evaluate(state) && state.CanReach(ParentAreaName);
        }

        public void UseHardRules()
        {
            ruleString = hardRuleString;
        }

        public void PlaceItem(Item item)
        {
            Item = item;
        }
        public void BuildRuleTree()
        {
            Rules = RuleTree.ParseAndBuildRules(ruleString);
        }
    }

    public enum LocationType
    {
        Chest,
        FreeStanding,
        Shop,
        Dialogue,
        Mural,
        Miniboss,
        Guardian,
        Puzzle,
        Dissonance,
        Fairy
    }
}
