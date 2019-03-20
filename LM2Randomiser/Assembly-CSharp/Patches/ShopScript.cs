using System;
using MonoMod;
using L2Base;
using L2Hit;
using L2Menu;
using L2Word;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0626, 0649, 0414
namespace LM2RandomiserMod.Patches
{
    [MonoModPatch("global::ShopScript")]
    public class ShopScript : global::ShopScript
    {
        [MonoModIgnore]
        private Sprite[] icon;

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
                                    "Chakram", "Caltrops, Clay Doll", "Origin Seal", "Birth Seal", "Life Seal", "Death Seal"};

            bool result = orig_itemCallBack(tab, name, vale, num);
            if (result && this.sys.isMap(name))
            {
                int counter = this.item_copunter - 1;
                this.icon[counter] = L2SystemCore.getMenuIconSprite(L2SystemCore.getItemData("Map"));
                this.shop_item[counter].sprite = this.icon[counter];
            }
            else if (result && name.Contains("Sacred Orb"))
            {
                int counter = this.item_copunter - 1;
                this.icon[counter] = L2SystemCore.getMenuIconSprite(L2SystemCore.getItemData("Sacred Orb"));
                this.shop_item[counter].sprite = this.icon[counter];
                this.item_name[counter].text = this.sys.getMojiText(true, this.sys.mojiSheetNameToNo(tab, this.sys.getMojiScript(mojiScriptType.item)),
                    this.sys.mojiIdToNo(tab, "Sacred Orb", this.sys.getMojiScript(mojiScriptType.item)), this.sys.getNowLangage(), this.sys.getMojiScript(mojiScriptType.item));
            }
            else if (result && name.Contains("Crystal S"))
            {
                int counter = this.item_copunter - 1;
                this.icon[counter] = L2SystemCore.getMenuIconSprite(L2SystemCore.getItemData("Crystal S"));
                this.shop_item[counter].sprite = this.icon[counter];
                this.item_name[counter].text = this.sys.getMojiText(true, this.sys.mojiSheetNameToNo(tab, this.sys.getMojiScript(mojiScriptType.item)),
                    this.sys.mojiIdToNo(tab, "Crystal S", this.sys.getMojiScript(mojiScriptType.item)), this.sys.getNowLangage(), this.sys.getMojiScript(mojiScriptType.item));
            }
            else
            {
                if(Array.IndexOf(weapons,name) > -1) {
                    int counter = this.item_copunter - 1;
                    this.icon[counter] = L2SystemCore.getMenuIconSprite(L2SystemCore.getItemData(name));
                    this.shop_item[counter].sprite = this.icon[counter];
                }
            }
            return result;
        }
    }
}
