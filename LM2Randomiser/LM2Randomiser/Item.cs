using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LM2Randomiser
{
    public class Item
    {
        public string name;
        public int id;
        public bool isRequired;

        public Item(string name, int id, bool isRequired = false)
        {
            this.name = name;
            this.id = id;
            this.isRequired = isRequired;
        }
    }
}
