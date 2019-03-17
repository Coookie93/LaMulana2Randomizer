using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LM2Randomiser.RuleParsing;

namespace LM2Randomiser
{
    public class Connection
    {
        public string name;
        public Area parentArea;
        public Area connectingArea;
        public string connectingAreaName;
        public BinaryNode ruleTree;

        public bool checking = false;

        public Connection(string name, Area parent)
        {
            this.name = parent.name + " to " + name;
            this.parentArea = parent;
            this.connectingAreaName = name;
        }

        public bool CanReach(PlayerState state)
        {
            return ruleTree.Evaluate(state) && state.CanReach(parentArea);
        }
    }

}
