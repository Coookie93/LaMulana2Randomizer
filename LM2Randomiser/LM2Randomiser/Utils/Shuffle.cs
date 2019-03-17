using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LM2Randomiser.Utils
{
    public abstract class Shuffle
    {
        public static List<T> FisherYates<T>(List<T> list, Randomiser world)
        {
            int max = list.Count;
            for (int i = 0; i < max; i++)
            {
                int r = i + world.Random.Next(max - i);
                T temp = list[r];
                list[r] = list[i];
                list[i] = temp;
            }

            return list;
        }
    }
}
