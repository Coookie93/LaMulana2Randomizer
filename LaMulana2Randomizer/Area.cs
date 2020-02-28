using System.Collections.Generic;

namespace LaMulana2Randomizer
{
    public class JsonArea
    {
        public string Name;
        public List<JsonLocation> Locations;
        public List<JsonConnection> Exits;
    }

    public class Area
    {
        public string Name { get; private set; }
        public List<Connection> Exits { get; private set; }
        public List<Connection> Entrances { get; private set; }

        public bool Checking = false;

        public Area(string name)
        {
            this.Name = name;
            Entrances = new List<Connection>();
            Exits = new List<Connection>();
        }

        public bool CanReach(PlayerState state)
        {
            foreach(Connection entrance in Entrances)
            {
                if(state.CanReach(entrance)) {
                    return true;
                }
            }
            return false;
        }
    }
}
