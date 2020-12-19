using System;
using MonoMod;
using L2Base;
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
        
        //Change this so shops can handle having all the different item types in them
        [MonoModReplace]
        public override bool itemCallBack(string tab, string name, int vale, int num)
        {
            string[] weapons = { "Knife", "Rapier", "Axe", "Katana", "Shuriken", "R-Shuriken", "E-Spear", "Flare Gun", "Bomb",
                                    "Chakram", "Caltrops", "Clay Doll", "Origin Seal", "Birth Seal", "Life Seal", "Death Seal"};
            
            if (item_copunter > 2)
            {
                return false;
            }
            true_name[item_copunter] = name;
            if (sys.isAnkJewel(name))
            {
                name = "Ankh Jewel";
            }
            else if (sys.isMap(name))
            {
                name = "Map";
            }
            item_id[item_copunter] = name;
            
            if (name.Contains("Mantra") && !name.Equals("Mantra"))
            {
                icon[item_copunter] = L2SystemCore.getShopIconSprite(L2SystemCore.getItemData("Mantra"));
                shop_item[item_copunter].sprite = icon[item_copunter];

                string mojiName = name.Equals("Mantra10") ? "mantra1stM10" : "mantra1stM" + name.Substring(6, 1);
                item_name[item_copunter].text = sys.getMojiText(true, sys.mojiSheetNameToNo(tab, sys.getMojiScript(mojiScriptType.system)),
                    sys.mojiIdToNo(tab, mojiName, sys.getMojiScript(mojiScriptType.system)), sys.getNowLangage(), sys.getMojiScript(mojiScriptType.system));
            }
            else
            {
                if (name.Equals("Map"))
                {
                    icon[item_copunter] = L2SystemCore.getMapIconSprite(L2SystemCore.getItemData("Map"));
                }
                else if (name.Contains("Crystal S"))
                {
                    name = "Crystal S";
                    icon[item_copunter] = L2SystemCore.getShopIconSprite(L2SystemCore.getItemData(name));
                }
                else if (name.Contains("Sacred Orb"))
                {
                    name = "Sacred Orb";
                    icon[item_copunter] = L2SystemCore.getMenuIconSprite(L2SystemCore.getItemData(name));
                }
                else if (name.Contains("Whip"))
                {
                    short data = 0;
                    sys.getFlag(2, "Whip", ref data);
                    if(data == 0)
                    {
                        name = "Whip";
                    }
                    else if (data == 1) 
                    { 
                        name = "Whip2";
                        vale *= 2;
                    }
                    else if (data >= 2) 
                    { 
                        name = "Whip3";
                        vale *= 4;
                    }
                    icon[item_copunter] = L2SystemCore.getMenuIconSprite(L2SystemCore.getItemData(name));
                }
                else if (name.Contains("Shield"))
                {
                    short data = 0;
                    sys.getFlag(2, 184, ref data);
                    if(data == 0)
                    {
                        name = "Shield";
                    }
                    else if (data == 1) 
                    { 
                        name = "Shield2";
                        vale *= 2;
                    }
                    else if (data >= 2) 
                    { 
                        name = "Shield3";
                        vale *= 4;
                    }
                    icon[item_copunter] = Load("Textures/icons_shops", name);
                }
                else if (name.Equals("MSX"))
                {
                    icon[item_copunter] = L2SystemCore.getShopIconSprite(L2SystemCore.getItemData("MSX3p"));
                }
                else if (Array.IndexOf(weapons, name) > -1)
                {
                    icon[item_copunter] = L2SystemCore.getMenuIconSprite(L2SystemCore.getItemData(name));
                }
                else {
                    icon[item_copunter] = Load("Textures/icons_shops", name);
                }

                shop_item[item_copunter].sprite = icon[item_copunter];
                item_name[item_copunter].text = sys.getMojiText(true, sys.mojiSheetNameToNo(tab, sys.getMojiScript(mojiScriptType.item)),
                        sys.mojiIdToNo(tab, name, sys.getMojiScript(mojiScriptType.item)), sys.getNowLangage(), sys.getMojiScript(mojiScriptType.item));
            }
            
            item_value[item_copunter] = vale;
            if (vale > 999)
            {
                item_valu[item_copunter].text = L2Math.numToText(vale, 4);
            }
            else
            {
                item_valu[item_copunter].text = L2Math.numToText(vale, 3);
            }
            item_num[item_copunter] = num;
            item_copunter++;
            return true;
        }

        private extern string orig_exchangeItemName(string name);
        public string exchangeItemName(string name)
        {
            return orig_exchangeItemName(name);
        }

        [MonoModIgnore]
        private bool[] isSouldOut;

        [MonoModReplace]
        public void setSouldOut()
        {
            short num = 0;
            for (int i = 0; i < 3; i++)
            {
                string text = exchangeItemName(item_id[i]);
                if (text != item_id[i])
                {
                    sys.getFlag(sys.SeetNametoNo("02Items"), text, ref num);
                    if (num != 0)
                    {
                        if (item_id[i] == "Pistol-b")
                        {
                            if (sys.getItemNum("pistolBox") >= sys.getItemMax("pistolBox"))
                            {
                                isSouldOut[i] = true;
                            }
                            else
                            {
                                isSouldOut[i] = false;
                            }
                        }
                        else if (sys.getItemNum(true_name[i]) >= sys.getItemMax(item_id[i]))
                        {
                            isSouldOut[i] = true;
                        }
                        else
                        {
                            isSouldOut[i] = false;
                        }
                    }
                    else
                    {
                        isSouldOut[i] = true;
                    }
                }
                else if (item_id[i] == "Pepper")
                {
                    short num2 = 0;
                    sys.getFlag(0, "Pepper-b", ref num2);
                    if (num2 == 0)
                    {
                        isSouldOut[i] = false;
                    }
                    else
                    {
                        isSouldOut[i] = true;
                    }
                }
                else if (sys.getItemNum(true_name[i]) >= sys.getItemMax(item_id[i]))
                {
                    isSouldOut[i] = true;
                }
                else
                {
                    isSouldOut[i] = false;
                }
            }
            isSouldOut[3] = false;
            drawItems();
        }
    }
}
