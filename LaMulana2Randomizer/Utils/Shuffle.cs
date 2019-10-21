using System;
using System.Collections.Generic;

namespace LM2Randomizer.Utils
{
    public abstract class Shuffle
    {
        public static List<T> FisherYates<T>(List<T> list, Random random)
        {
            int max = list.Count;
            for (int i = 0; i < max; i++)
            {
                int r = i + random.Next(max - i);
                T temp = list[r];
                list[r] = list[i];
                list[i] = temp;
            }
            return list;
        }
    }
}
