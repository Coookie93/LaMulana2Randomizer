using MonoMod;
using UnityEngine;
using L2Word;

#pragma warning disable 0649, 0414, 0108
namespace LM2RandomiserMod
{
    [MonoModPatch("L2Base.L2System")]
    public class patched_L2System : L2Base.L2System
    {
        [MonoModIgnore]
        private L2ShopDataBase l2sdb;

        [MonoModIgnore]
        private L2TalkDataBase l2tdb;

        //Create and Initialise the randomiser
        private void orig_Start() { }
        private void Start()
        {
            orig_Start();
            GameObject obj = new GameObject();
            L2Rando component = obj.AddComponent<L2Rando>() as L2Rando;
            component.Initialise(this.l2sdb, this.l2tdb, this);
            DontDestroyOnLoad(obj);
        }

        //Change this method to handel mantras
        public void orig_setItem(string item_name, int num, bool direct = false, bool loadcall = false, bool sub_add = true) { }
        public void setItem(string item_name, int num, bool direct = false, bool loadcall = false, bool sub_add = true)
        {
            if (item_name.Contains("Mantra") && !name.Equals("Mantra"))
            {
                this.setFlagData(2, item_name, (short)(this.getItemNum(item_name) + num));
            }
            else
            {
                orig_setItem(item_name, num, direct, loadcall, sub_add);
            }
        }
    }
}
