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
        private Image[] shop_item;

        [MonoModIgnore]
        private TextMeshProUGUI[] item_name;

        public extern bool orig_itemCallBack(string tab, string name, int vale, int num);
        public override bool itemCallBack(string tab, string name, int vale, int num)
        {
            string[] weapons = { "Whip2", "Whip3", "Knife", "Rapier", "Axe", "Katana", "Shuriken", "R-Shuriken", "E-Spear", "Flare Gun", "Bomb",
                                    "Chakram", "Caltrops", "Clay Doll", "Origin Seal", "Birth Seal", "Life Seal", "Death Seal"};

            bool result = orig_itemCallBack(tab, name, vale, num);
            if (result)
            {
                int counter = this.item_copunter - 1;
                if (this.sys.isMap(name))
                {
                    this.icon[counter] = L2SystemCore.getMenuIconSprite(L2SystemCore.getItemData("Map"));
                    this.shop_item[counter].sprite = this.icon[counter];
                }
                else if (name.Equals("MSX"))
                {
                    this.icon[counter] = L2SystemCore.getShopIconSprite(L2SystemCore.getItemData("MSX3p"));
                    this.shop_item[counter].sprite = this.icon[counter];
                }
                else if (name.Contains("Sacred Orb"))
                {
                    this.icon[counter] = L2SystemCore.getMenuIconSprite(L2SystemCore.getItemData("Sacred Orb"));
                    this.shop_item[counter].sprite = this.icon[counter];
                    this.item_name[counter].text = this.sys.getMojiText(true, this.sys.mojiSheetNameToNo(tab, this.sys.getMojiScript(mojiScriptType.item)),
                        this.sys.mojiIdToNo(tab, "Sacred Orb", this.sys.getMojiScript(mojiScriptType.item)), this.sys.getNowLangage(), this.sys.getMojiScript(mojiScriptType.item));
                }
                else if (name.Contains("Crystal S"))
                {
                    this.icon[counter] = L2SystemCore.getMenuIconSprite(L2SystemCore.getItemData("Crystal S"));
                    this.shop_item[counter].sprite = this.icon[counter];
                    this.item_name[counter].text = this.sys.getMojiText(true, this.sys.mojiSheetNameToNo(tab, this.sys.getMojiScript(mojiScriptType.item)),
                        this.sys.mojiIdToNo(tab, "Crystal S", this.sys.getMojiScript(mojiScriptType.item)), this.sys.getNowLangage(), this.sys.getMojiScript(mojiScriptType.item));
                }
                else
                {
                    if (Array.IndexOf(weapons, name) > -1)
                    {
                        this.icon[counter] = L2SystemCore.getMenuIconSprite(L2SystemCore.getItemData(name));
                        this.shop_item[counter].sprite = this.icon[counter];
                    }
                }
            }
            return result;
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
