using System.Collections.Generic;

namespace LaMulana2Randomizer
{
    public class JsonArea
    {
        public string Name;
        public List<JsonLocation> Locations;
        public List<JsonExit> Exits;
    }

    public class Area
    {
        public string Name { get; private set; }
        public List<Exit> Exits { get; private set; }
        public List<Exit> Entrances { get; private set; }

        public bool Checking = false;

        public Area(string name)
        {
            Name = name;
            Entrances = new List<Exit>();
            Exits = new List<Exit>();
        }

        public bool CanReach(PlayerState state)
        {
            foreach(Exit entrance in Entrances)
            {
                if(state.CanReach(entrance)) {
                    return true;
                }
            }
            return false;
        }
    }
}
