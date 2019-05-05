using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using LM2Randomiser.RuleParsing;

namespace LM2Randomiser
{
    public class Connection
    {
        public string name;
        public string connectingAreaName;
        public string ruleString;
        
        [JsonIgnore]
        public Area parentArea;

        [JsonIgnore]
        public Area connectingArea;
        
        [JsonIgnore]
        public BinaryNode ruleTree;

        [JsonIgnore]
        public bool checking = false;

        [JsonConstructor]
        public Connection() { }

        public bool CanReach(PlayerState state)
        {
            return ruleTree.Evaluate(state) && state.CanReach(parentArea);
        }

        public void AppendRuleString(string appendage)
        {
            ruleString += appendage;
        }
    }

}
