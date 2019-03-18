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
//namespace LM2TestMod
//{
//    [MonoModPatch("global::ShopScript")]
//    public class ShopScript : global::ShopScript
//    {
//        [MonoModIgnore]
//        private Sprite[] icon;

//        [MonoModIgnore]
//        private int item_copunter;

//        [MonoModIgnore]
//        private Image[] shop_item;

//        public extern bool orig_itemCallBack(string tab, string name, int vale, int num);
//        public override bool itemCallBack(string tab, string name, int vale, int num)
//        {
//            bool result = orig_itemCallBack(tab, name, vale, num);
//            if (result && this.sys.isMap(name))
//            {
//                int counter = this.item_copunter - 1;
//                this.icon[counter] = L2SystemCore.getMapIconSprite(L2SystemCore.getItemData("Map"));
//                this.shop_item[counter].sprite = this.icon[counter];
//            }
//            return result;
//        }
//    }
//}
