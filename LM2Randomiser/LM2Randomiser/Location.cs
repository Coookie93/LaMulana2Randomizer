using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LM2Randomiser.RuleParsing;

namespace LM2Randomiser
{
    public class Location
    {
        public string name;
        public Area parentArea;
        public BinaryNode ruleTree;
        public Item item;
        public LocationType locationType;
        public bool isLocked;

        public Location(string name, Area parent, LocationType locationType)
        {
            this.name = name;
            this.parentArea = parent;
            this.locationType = locationType;
            isLocked = false;
        }

        public bool CanReach(PlayerState state)
        {
            return ruleTree.Evaluate(state) && state.CanReach(parentArea);
        }
    }

    public enum LocationType
    {
        Shop,
        Default
    }
}
