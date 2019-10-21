using LM2Randomizer.RuleParsing;

namespace LM2Randomizer
{
    public class JsonConnection
    {
        public string RuleString;
        public string ConnectingAreaName;
        public bool IsBackSide;
    }

    public class Connection
    {
        public string Name { get; private set; }
        public bool IsBackside { get; private set; }
        public BinaryNode Rules { get; private set; }

        public string ParentAreaName;
        public string ConnectingAreaName;
        public bool Checking = false;

        private string ruleString;

        public Connection(JsonConnection jsonConnection, string parentAreaName) 
        {
            ruleString = jsonConnection.RuleString;
            ConnectingAreaName = jsonConnection.ConnectingAreaName;
            IsBackside = jsonConnection.IsBackSide;
            ParentAreaName = parentAreaName;
            Name = $"{ParentAreaName} to {ConnectingAreaName}";
        }

        public bool CanReach(PlayerState state)
        {
            return Rules.Evaluate(state) && state.CanReach(ParentAreaName);
        }

        public void AppendRuleString(string append)
        {
            ruleString += append;
        }

        public void BuildRuleTree()
        {
            Rules = RuleTree.ParseAndBuildRules(ruleString);
        }
    }

}
