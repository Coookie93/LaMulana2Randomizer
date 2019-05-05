using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LM2Randomiser.Utils;

namespace LM2Randomiser
{
    public class ItemPool
    {
        public static Item GetAndRemove(ItemID id, List<Item> itemPool)
        {
            Item result = null;
            foreach(Item item in itemPool)
            {
                if(id == item.id)
                {
                    result = item;
                }
            }
            itemPool.Remove(result);
            return result;
        }
    }
}
