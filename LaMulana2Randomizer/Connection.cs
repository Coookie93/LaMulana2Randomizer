using LaMulana2Randomizer.LogicParsing;

namespace LaMulana2Randomizer
{
    public class JsonConnection
    {
        public string Logic;
        public string ConnectingAreaName;
        public bool IsBackSide;
    }

    public class Connection
    {
        public string Name { get; private set; }
        public bool IsBackside { get; private set; }
        public BinaryNode LogicTree { get; private set; }

        public string ParentAreaName;
        public string ConnectingAreaName;
        public bool Checking = false;

        private string logicString;

        public Connection(JsonConnection jsonConnection, string parentAreaName) 
        {
            logicString = jsonConnection.Logic;
            ConnectingAreaName = jsonConnection.ConnectingAreaName;
            IsBackside = jsonConnection.IsBackSide;
            ParentAreaName = parentAreaName;
            Name = $"{ParentAreaName} to {ConnectingAreaName}";
        }

        public bool CanReach(PlayerState state)
        {
            return LogicTree.Evaluate(state) && state.CanReach(ParentAreaName);
        }

        public void AppendRuleString(string append)
        {
            logicString = string.Format($"({logicString}){append}");
        }

        public void BuildLogicTree()
        {
            LogicTree = LogicParsing.LogicTree.ParseAndBuildLogic(logicString);
        }
    }

}
