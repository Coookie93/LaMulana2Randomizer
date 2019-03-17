using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LM2Randomiser
{
    public class Area
    {
        public string name;
        public List<Location> locations;
        public List<Connection> entrances;
        public List<Connection> exits;

        public bool checking = false;

        public Area(string name)
        {
            this.name = name;
            locations = new List<Location>();
            entrances = new List<Connection>();
            exits = new List<Connection>();
        }


        public bool CanReach(PlayerState state)
        {
            foreach(Connection entrance in entrances)
            {
                if(state.CanReach(entrance)) {
                    return true;
                }
            }

            return false;
        }
    }
}
