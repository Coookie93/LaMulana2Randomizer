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
        public LocationID ID { get; private set; }

        public AreaID ParentAreaID { get; private set; }
        //public string ParentAreaName { get; private set; }
        public BinaryNode LogicTree { get; private set; }

        public bool IsLocked = false;
        public bool RandomPlacement = false;

        private string logicString;
        private readonly string hardLogicString;

        public Location(string name, LocationID id, LocationType locationType, string logic, AreaID parentAreaID)
        {
            Name = name;
            ID = id;
            LocationType = locationType;
            logicString = logic;
            ParentAreaID = parentAreaID;
        }

        public Location(JsonLocation jsonLocation, AreaID parentAreaID)
        {
            Name = jsonLocation.Name;
            LocationType = jsonLocation.LocationType;
            logicString = jsonLocation.Logic;
            hardLogicString = jsonLocation.HardLogic;
            Item = jsonLocation.Item;
            Enum.TryParse(Name.RemoveWhitespace(), out LocationID temp);
            ID = temp;
            ParentAreaID = parentAreaID;
        }

        public bool CanReach(PlayerState state)
        {
            return LogicTree.Evaluate(state) && state.CanReach(ParentAreaID);
        }

        public void UseHardLogic()
        {
            logicString = hardLogicString;
        }

        public void PlaceItem(Item item, bool randomPlacement)
        {
            Item = item;
            RandomPlacement = randomPlacement;
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
