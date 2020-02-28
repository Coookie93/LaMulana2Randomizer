using MonoMod;
using UnityEngine;
using L2Word;
using L2Base;
using System;

#pragma warning disable 0649, 0414, 0108
namespace LM2RandomiserMod.Patches
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
            if (item_name.Contains("Mantra") && !item_name.Equals("Mantra"))
            {
                this.setFlagData(2, item_name, (short)(this.getItemNum(item_name) + num));
            }
            else
            {
                orig_setItem(item_name, num, direct, loadcall, sub_add);
            }
        }

        [MonoModReplace]
        public int isHaveItem(string name)
        {
            short num = 0;
            int seet = this.SeetNametoNo("02Items");
            if (name == "A_Jewel" || name == "Ankh Jewel")
            {
                this.getFlag(this.SeetNametoNo("00system"), "A_Jewel", ref num);
            }
            else if (name == "Whip2")
            {
                this.getFlag(seet, "Whip", ref num);
                if (num >= 2)
                {
                    num = 1;
                }
                else
                {
                    num = 0;
                }
            }
            else if (name == "Whip3")
            {
                this.getFlag(seet, "Whip", ref num);
                if (num == 3)
                {
                    num = 1;
                }
                else
                {
                    num = 0;
                }
            }
            else if (name == "Shield2")
            {
                this.getFlag(seet, "Shield", ref num);
                if (num == 2)
                {
                    num = 1;
                }
                else
                {
                    num = 0;
                }
            }
            else if (name == "Shield3")
            {
                this.getFlag(seet, "Shield", ref num);
                if (num == 3)
                {
                    num = 1;
                }
                else
                {
                    num = 0;
                }
            }
            else
            {
                this.getFlag(seet, name, ref num);
            }
            return (int)num;
        }

#if DEV
        [MonoModIgnore]
        private TextMesh boss_hp_text;

        [MonoModIgnore]
        private TextMesh boss_dmg_text;

        [MonoModReplace]
        public void debugDrawBossHPText(int hp)
        {
            this.boss_hp_text.color = Color.white;
            this.boss_hp_text.text = hp.ToString();
        }

        [MonoModReplace]
        public void debugDrawBossDmgText(int dmg)
        {
            this.boss_dmg_text.color = Color.white;
            this.boss_dmg_text.text = dmg.ToString();
        }
#endif
    }
}
