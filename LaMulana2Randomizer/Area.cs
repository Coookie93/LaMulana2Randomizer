using System.Collections.Generic;

namespace LaMulana2Randomizer
{
    public class JsonArea
    {
        public string Name;
        public bool IsBackside;
        public List<JsonLocation> Locations;
        public List<JsonExit> Exits;
    }

    public class Area
    {
        public string Name { get; private set; }
        public bool IsBackside { get; private set; }
        public List<Exit> Exits { get; private set; }
        public List<Exit> Entrances { get; private set; }

        public bool Checking = false;

        public Area(JsonArea area)
        {
            Name = area.Name;
            IsBackside = area.IsBackside;
            Entrances = new List<Exit>();
            Exits = new List<Exit>();
        }

        public bool CanReach(PlayerState state)
        {
            foreach(Exit entrance in Entrances)
            {
                if (state.EscapeCheck && (entrance.ExitType == ExitType.PrisonExit || entrance.ExitType == ExitType.PrisonGate 
                    || entrance.ExitType == ExitType.Pyramid || entrance.ExitType == ExitType.Corridor))
                    continue;

                if(state.CanReach(entrance))
                    return true;
            }
            return false;
        }
    }
}
