using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace LM2Randomiser
{
    public class Area
    {
        public string name;
        public List<Location> locations;
        public List<Connection> exits;

        [JsonIgnore]
        public List<Connection> entrances;
        
        [JsonIgnore]
        public bool checking = false;

        [JsonConstructor]
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
