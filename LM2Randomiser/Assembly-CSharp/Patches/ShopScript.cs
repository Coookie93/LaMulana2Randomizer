using System;
using MonoMod;
using L2Base;
using L2Hit;
using L2Menu;
using L2Word;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0626, 0649, 0414, 0108
namespace LM2RandomiserMod.Patches
{
    [MonoModPatch("global::ShopScript")]
    public class patched_ShopScript : global::ShopScript
    {
        [MonoModIgnore]
        private Sprite[] icon;

        [MonoModIgnore]
        private string[] item_id;

        [MonoModIgnore]
        private bool[] isSouldOut;

        [MonoModIgnore]
        private int item_copunter;

        [MonoModIgnore]
        private int[] item_num;

        [MonoModIgnore]
        private int[] item_value;

        [MonoModIgnore]
        private Image[] shop_item;

        [MonoModIgnore]
        private Text[] item_valu;

        [MonoModIgnore]
        private TextMeshProUGUI[] item_name;
        
        [MonoModReplace]
        public override bool itemCallBack(string tab, string name, int vale, int num)
        {
            string[] weapons = { "Whip2", "Whip3", "Knife", "Rapier", "Axe", "Katana", "Shuriken", "R-Shuriken", "E-Spear", "Flare Gun", "Bomb",
                                    "Chakram", "Caltrops", "Clay Doll", "Origin Seal", "Birth Seal", "Life Seal", "Death Seal"};
            
            if (this.item_copunter > 2)
            {
                return false;
            }
            this.true_name[this.item_copunter] = name;
            if (this.sys.isAnkJewel(name))
            {
                name = "Ankh Jewel";
            }
            else if (this.sys.isMap(name))
            {
                name = "Map";
            }
            this.item_id[this.item_copunter] = name;
            
            if (name.Contains("Mantra") && !name.Equals("Mantra"))
            {
                this.icon[this.item_copunter] = L2SystemCore.getShopIconSprite(L2SystemCore.getItemData("Mantra"));
                this.shop_item[this.item_copunter].sprite = this.icon[this.item_copunter];

                string mojiName = name.Equals("Mantra10") ? "mantra1stM10" : "mantra1stM" + name.Substring(6, 1);
                this.item_name[this.item_copunter].text = this.sys.getMojiText(true, this.sys.mojiSheetNameToNo(tab, this.sys.getMojiScript(mojiScriptType.system)),
                    this.sys.mojiIdToNo(tab, mojiName, this.sys.getMojiScript(mojiScriptType.system)), this.sys.getNowLangage(), this.sys.getMojiScript(mojiScriptType.system));
            }
            else
            {
                if (this.sys.isMap(name))
                {
                    this.icon[this.item_copunter] = L2SystemCore.getMapIconSprite(L2SystemCore.getItemData("Map"));
                }
                else if (name.Equals("MSX"))
                {
                    this.icon[this.item_copunter] = L2SystemCore.getShopIconSprite(L2SystemCore.getItemData("MSX3p"));
                }
                else if (name.Contains("Sacred Orb"))
                {
                    this.icon[this.item_copunter] = L2SystemCore.getMenuIconSprite(L2SystemCore.getItemData("Sacred Orb"));
                    name = "Sacred Orb";
                }
                else if (name.Contains("Crystal S"))
                {
                    this.icon[this.item_copunter] = L2SystemCore.getShopIconSprite(L2SystemCore.getItemData("Crystal S"));
                    name = "Crystal S";
                }
                else if (Array.IndexOf(weapons, name) > -1)
                {
                    this.icon[this.item_copunter] = L2SystemCore.getMenuIconSprite(L2SystemCore.getItemData(name));
                }
                else {
                    this.icon[this.item_copunter] = ShopScript.Load("Textures/icons_shops", name);
                }

                this.shop_item[this.item_copunter].sprite = this.icon[this.item_copunter];
                this.item_name[this.item_copunter].text = this.sys.getMojiText(true, this.sys.mojiSheetNameToNo(tab, this.sys.getMojiScript(mojiScriptType.item)),
                        this.sys.mojiIdToNo(tab, name, this.sys.getMojiScript(mojiScriptType.item)), this.sys.getNowLangage(), this.sys.getMojiScript(mojiScriptType.item));
            }
            
            this.item_value[this.item_copunter] = vale;
            if (vale > 999)
            {
                this.item_valu[this.item_copunter].text = L2Math.numToText(vale, 4);
            }
            else
            {
                this.item_valu[this.item_copunter].text = L2Math.numToText(vale, 3);
            }
            this.item_num[this.item_copunter] = num;
            this.item_copunter++;
            return true;
        }

        public void orig_setSouldOut() { }
        public void setSouldOut()
        {
            orig_setSouldOut();

            for (int i = 0; i < 3; i++)
            {
                if (this.item_id[i] == "MSX")
                {
                    short num = 0;
                    this.sys.getFlag(this.sys.SeetNametoNo("02Items"), "MSX", ref num);
                    if (num >= 2)
                    {
                        this.isSouldOut[i] = true;
                    }
                    else
                    {
                        this.isSouldOut[i] = false;
                    }
                }
            }
            this.drawItems();
        }
    }
}
