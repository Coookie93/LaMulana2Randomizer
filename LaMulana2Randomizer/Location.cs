using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using LaMulana2Randomizer.LogicParsing;
using LaMulana2Randomizer.ExtensionMethods;
using LaMulana2RandomizerShared;

namespace LaMulana2Randomizer
{
    public class JsonLocation
    {
        public string Name;

        [JsonConverter(typeof(StringEnumConverter))]
        public LocationType LocationType;

        public string Logic;
        public string HardLogic;
        public Item Item;
    }

    public class Location
    {
        public string Name { get; private set; }
        public LocationType LocationType { get; private set; }
        public Item Item { get; private set; }
        public LocationID Id { get; private set; }

        public BinaryNode LogicTree;
        public string ParentAreaName;
        public bool IsLocked = false;

        private string logicString;
        private readonly string hardLogicString;

        public Location(JsonLocation jsonLocation)
        {
            Name = jsonLocation.Name;
            LocationType = jsonLocation.LocationType;
            logicString = jsonLocation.Logic;
            hardLogicString = jsonLocation.HardLogic;
            Item = jsonLocation.Item;
            Enum.TryParse(Name.RemoveWhitespace(), out LocationID temp);
            Id = temp;
        }
        
        public bool CanReach(PlayerState state)
        {
            return LogicTree.Evaluate(state) && state.CanReach(ParentAreaName);
        }

        public void UseHardRules()
        {
            logicString = hardLogicString;
        }

        public void PlaceItem(Item item)
        {
            Item = item;
        }

        public void AppendLogicString(string append)
        {
            logicString = string.Format($"({logicString}){append}");
        }

        public void BuildLogicTree()
        {
            LogicTree = LogicParsing.LogicTree.ParseAndBuildLogic(logicString);
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
        FinalBoss,
        Puzzle,
        Dissonance,
        Fairy
    }
}
