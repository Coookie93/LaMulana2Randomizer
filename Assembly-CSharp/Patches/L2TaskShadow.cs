//using MonoMod;
//using L2Flag;
//using LM2RandomiserShared;

//#pragma warning disable 0626, 0649, 0414, 0108
//namespace LM2RandomiserMod.Patches
//{
//    [MonoModPatch("L2Task.L2TaskShadow")]
//    public class patched_L2TaskShadow : L2Task.L2TaskShadow
//    {
//        public extern void orig_Init();
//        public override void Init()
//        {
//            orig_Init();

//            foreach (L2FlagBoxParent flagBoxParent in startflag)
//            {
//                foreach (L2FlagBox flagBox in flagBoxParent.BOX)
//                {
//                    if (flagBox.seet_no1 == 3 && flagBox.flag_no1 == 30 && flagBox.flag_no2 == 80)
//                    {
//                        L2Rando rando = FindObjectOfType<L2Rando>();
//                        if(rando == null)
//                        {
//                            return;
//                        }

//                        ItemID itemID = rando.GetItemIDForLocation(LocationID.FreyasItem);
//                        ItemInfo itemInfo = ItemDB.GetItemInfo(itemID);

//                        flagBoxParent.BOX = new L2FlagBox[1];

//                        flagBoxParent.BOX[0] = new L2FlagBox
//                        {
//                            logic = LOGIC.OR,
//                            comp = COMPARISON.Less,

//                            seet_no1 = 2,
//                            flag_no1 = itemInfo.itemFlag,

//                            seet_no2 = -1,
//                            flag_no2 = 1
//                        };

//                        if (itemID == ItemID.ChainWhip || itemID == ItemID.SilverShield || itemID == ItemID.MobileSuperx3P)
//                        {
//                            flagBoxParent.BOX[0].flag_no2 = 2;
//                        }
//                        else if (itemID == ItemID.FlailWhip || itemID == ItemID.AngelShield)
//                        {
//                            flagBoxParent.BOX[0].flag_no2 = 3;
//                        }
//                    }
//                }
//            }
//        }
//    }
//}
