using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MonoMod;
using L2Flag;

#pragma warning disable 0626, 0649, 0414, 0108
namespace LM2RandomiserMod.Patches
{
    [MonoModPatch("L2Task.L2TaskShadow")]
    public class patched_L2TaskShadow : L2Task.L2TaskShadow
    {
        public extern void orig_Init();
        public override void Init()
        {
            orig_Init();

            foreach (L2FlagBoxParent flagBoxParent in startflag)
            {
                foreach (L2FlagBox flagBox in flagBoxParent.BOX)
                {
                    if (flagBox.seet_no1 == 3 && flagBox.flag_no1 == 30 && flagBox.flag_no2 == 80)
                    {
                        L2Rando rando = GameObject.FindObjectOfType<L2Rando>();
                        ItemID itemID = rando.GetItemIDForLocation(LocationID.FreyasItem);
                        ItemInfo itemInfo = ItemFlags.GetItemInfo(itemID);

                        flagBoxParent.BOX = new L2FlagBox[2];

                        flagBoxParent.BOX[0] = flagBox;
                        flagBoxParent.BOX[0].logic = LOGIC.OR;
                        
                        flagBoxParent.BOX[1] = new L2FlagBox();
                        flagBoxParent.BOX[1].logic = LOGIC.OR;
                        flagBoxParent.BOX[1].comp = COMPARISON.Less;
                        
                        flagBoxParent.BOX[1].seet_no1 = 2;
                        flagBoxParent.BOX[1].seet_no2 = -1;

                        flagBoxParent.BOX[1].flag_no1 = itemInfo.itemFlag;
                        flagBoxParent.BOX[1].flag_no2 = 1;
                        
                        if (itemID == ItemID.ChainWhip || itemID == ItemID.SilverShield || itemID == ItemID.MobileSuperx3P)
                        {
                            flagBox.flag_no2 = 2;
                        }
                        else if (itemID == ItemID.FlailWhip || itemID == ItemID.AngelShield)
                        {
                            flagBox.flag_no2 = 3;
                        }
                    }
                }
            }
        }
    }
}
