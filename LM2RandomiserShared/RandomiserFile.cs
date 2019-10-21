using System;
using System.Collections.Generic;

namespace LM2RandomizerShared
{
    [Serializable]
    public class RandomiserFile
    {
        public Dictionary<int, ItemID> ItemLocationMap { get; }

        public RandomiserFile()
        {
            ItemLocationMap = new Dictionary<int, ItemID>();
        }
    }
}
