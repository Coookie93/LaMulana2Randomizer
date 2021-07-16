using System.Collections.Generic;
using Newtonsoft.Json;

namespace LaMulana2Randomizer
{
    public class SpoilerLog
    {
        public int Seed;

        [JsonProperty("Settings String")]
        public string SettingsString;

        public Settings Settings;

        [JsonProperty("Starting Area")]
        public string StartingArea;

        [JsonProperty("Starting Weapon")]
        public string StartingWeapon;

        [JsonProperty("Starting Items")]
        public List<string> StartingItems;

        [JsonProperty("Cursed Locations")]
        public List<string> CursedLocations;

        public SortedList<string, string> Entrances;

        [JsonProperty("Soul Gates")]
        public SortedList<int, SortedList<string, string>> SoulGates;

        public Dictionary<string, string> Locations;
        public Dictionary<int, Dictionary<string, string>> Playthrough;
    }
}
