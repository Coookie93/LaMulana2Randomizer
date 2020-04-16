using LaMulana2Randomizer.LogicParsing;

namespace LaMulana2Randomizer
{
    public class JsonConnection
    {
        public string Name;
        public string ConnectingAreaName;
        public string Logic;
        public ConnectionType ConnectionType;
        public bool IsBackSide;
    }

    public class Connection
    {
        public string Name { get; private set; }
        public string ParentAreaName { get; private set; }
        public ConnectionType ConnectionType { get; private set; }
        public bool IsBackside { get; private set; }
        public BinaryNode LogicTree { get; private set; }

        public string ConnectingAreaName;
        public bool Checking = false;

        private string logicString;

        public Connection(JsonConnection jsonConnection, string parentAreaName) 
        {
            Name = jsonConnection.Name;
            ConnectingAreaName = jsonConnection.ConnectingAreaName;
            logicString = jsonConnection.Logic;
            ConnectionType = jsonConnection.ConnectionType;
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

    public enum ConnectionType
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
