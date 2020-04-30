using LaMulana2Randomizer.LogicParsing;
using LaMulana2RandomizerShared;

namespace LaMulana2Randomizer
{
    public class JsonExit
    {
        public string Name;
        public ExitID ID;
        public string ConnectingAreaName;
        public string Logic;
        public ExitType ConnectionType;
        public bool IsBackSide;
    }

    public class Exit
    {
        public string Name { get; private set; }
        public string ParentAreaName { get; private set; }
        public ExitID ID { get; private set; }
        public ExitType ExitType { get; private set; }
        public bool IsBackside { get; private set; }
        public BinaryNode LogicTree { get; private set; }

        public string ConnectingAreaName;
        public bool Checking = false;

        private string logicString;

        public Exit(JsonExit jsonConnection, string parentAreaName) 
        {
            Name = jsonConnection.Name;
            ID = jsonConnection.ID;
            ConnectingAreaName = jsonConnection.ConnectingAreaName;
            logicString = jsonConnection.Logic;
            ExitType = jsonConnection.ConnectionType;
            IsBackside = jsonConnection.IsBackSide;
            ParentAreaName = parentAreaName;
            if (string.IsNullOrEmpty(Name))
                Name = $"{ParentAreaName} to {ConnectingAreaName}";
        }

        public bool CanReach(PlayerState state)
        {
            return LogicTree.Evaluate(state) && state.CanReach(ParentAreaName);
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

    public enum ExitType
    {
        LeftDoor,
        RightDoor,
        DownLadder,
        UpLadder,
        Gate,
        SoulGate,
        Corridor,
        Internal,
        Unique
    }
}
