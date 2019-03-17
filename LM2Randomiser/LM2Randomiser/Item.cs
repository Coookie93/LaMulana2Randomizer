using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LM2Randomiser
{
    public class Item
    {
        public string name;
        public string internalName;
        public bool isRequired;

        public Item(string name, string internalName, bool isRequired = false)
        {
            this.name = name;
            this.internalName = internalName;
            this.isRequired = isRequired;
        }
    }
}
