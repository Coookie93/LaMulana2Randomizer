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
        public static bool CreatePool(string filePath,  out List<Item> itemPool)
        {
            string currentDir = Directory.GetCurrentDirectory();

            itemPool = new List<Item>();

            List<string[]> itemData;
            if (FileUtils.GetData(filePath, 2, out itemData))
            {
                foreach(var data in itemData)
                {
                    itemPool.Add(new Item(data[0], Int32.Parse(data[1]) + 1));
                }
            }
            else
            {
                //early out if item data can't be read properly
                return false;
            }

            return true;
        }

        public static Item GetAndRemove(ItemID id, List<Item> itemPool)
        {
            Item result = null;
            foreach(var item in itemPool)
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
