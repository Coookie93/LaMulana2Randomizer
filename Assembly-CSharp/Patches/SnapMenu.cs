using MonoMod;
using UnityEngine;
using L2Base;
using L2Menu;
using L2Word;
using LaMulana2RandomizerShared;

#pragma warning disable 0626, 0649, 0414, 0108
namespace LM2RandomiserMod.Patches
{
    [MonoModPatch("global::SnapMenu")]
    public class patched_SnapMenu : global::SnapMenu
    {
        [MonoModIgnore]
        private float scroll_pos;

        [MonoModIgnore]
        private int scroll_count;

        [MonoModIgnore]
        private RectTransform SnapBase;

        [MonoModIgnore]
        private SnapMenuController con;

        [MonoModIgnore]
        private L2SystemCore L2Core;

        [MonoModIgnore]
        private ItemData item;

        [MonoModIgnore]
        private SnapShotTargetScript SnapShotTargetSc;

        [MonoModIgnore]
        private int DrawBinalyCount;

        [MonoModIgnore]
        private string[] BinalyData;

        [MonoModIgnore]
        private int BinalyStart;

        [MonoModIgnore]
        private string GetItemID;
        
        [MonoModIgnore]
        private int Dialog_on;

        [MonoModIgnore]
        private bool HaveItems;

        [MonoModIgnore]
        private BUNSYOEMON bun;

        [MonoModIgnore]
        private bool Bunsyouemon;

        [MonoModIgnore]
        private void CreateBunsyouData_Message() { }

        [MonoModIgnore]
        private void SetHelpBar(int no) { }
        
        //Change this method to allow Murals to give randomised items
        [MonoModReplace]
        public override bool Farst()
        {
            int sta = this.sta;
            switch (sta)
            {
                case 0:
                    break;
                case 1:
                    if (!sys.getL2Keys(L2KEYS.up, KEYSTATE.REPEAT))
                    {
                        if (!sys.getL2Keys(L2KEYS.down, KEYSTATE.REPEAT))
                        {
                            if (!sys.getL2Keys(L2KEYS.left, KEYSTATE.DOWN))
                            {
                                if (sys.getL2Keys(L2KEYS.right, KEYSTATE.DOWN))
                                {
                                }
                            }
                        }
                    }
                    if (sys.getL2Keys(L2KEYS.ok, KEYSTATE.DOWN))
                    {
                        sys.setKeyBlock(true);
                        con.anime.Play("snap scan");
                        L2Core.seManager.playSE(null, 155);
                        this.sta = 2;
                    }
                    if (sys.getL2Keys(L2KEYS.cancel, KEYSTATE.DOWN))
                    {
                    }
                    break;
                case 2:
                    if (con.anime.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                    {
                        sys.setKeyBlock(false);
                        con.anime.Play("snap scanEnd");
                        SnapShotTargetSc = L2Core.ScrollSystem.getSnapShotTarget();
                        L2Core.seManager.playSE(null, 37);
                        if (SnapShotTargetSc == null)
                        {
                            this.sta = 3;
                        }
                        else if (SnapShotTargetSc.mode == SnapShotTargetScript.SnapShotMode.MESSAGE)
                        {
                            con.Contents.SetActive(true);
                            con.ContentsText.text = sys.getMojiText(false, SnapShotTargetSc.sheetName, SnapShotTargetSc.cellName, mojiScriptType.sekihi);
                            this.sta = 4;
                        }
                        else if (SnapShotTargetSc.mode == SnapShotTargetScript.SnapShotMode.SOFTWARE)
                        {

                            sys.setKeyBlock(true);
                            L2Rando rando = GameObject.FindObjectOfType<L2Rando>();
                            if(rando != null && rando.IsRandomising)
                            {
                                LocationID locationID = rando.GetLocationIDForMural(SnapShotTargetSc);

                                ItemID itemID = rando.GetItemIDForLocation(locationID);
                                ItemInfo itemInfo = ItemDB.GetItemInfo(itemID);

                                GetItemID = itemInfo.BoxName;

                                int flagValue = 0;
                                if (itemID == ItemID.MobileSuperx3P)
                                    flagValue = 1;

                                HaveItems = false;
                                if (itemInfo.BoxName.Contains("Research") || itemInfo.BoxName.Equals("Nothing") || itemInfo.BoxName.Contains("Beherit")) 
                                {
                                    short data = 0;
                                    sys.getFlag(itemInfo.ItemSheet, itemInfo.ItemFlag, ref data);
                                    if (data > 0)
                                        HaveItems = true;

                                }
                                else if (sys.isHaveItem(itemInfo.ShopName) > flagValue)
                                {
                                    HaveItems = true;
                                }

                                if(!HaveItems)
                                {
                                    if(!itemInfo.BoxName.Equals("Nothing"))
                                        sys.setItem(GetItemID, 1, false, false, true);

                                    sys.setEffectFlag(rando.CreateGetFlags(itemID, itemInfo));
                                }
                            }
                            else
                            {
                                item = L2SystemCore.getItemData(SnapShotTargetSc.itemName);
                                GetItemID = item.getItemId();
                                if (sys.isHaveItem(GetItemID) > 0)
                                {
                                    HaveItems = true;
                                }
                                else
                                {
                                    HaveItems = false;
                                    sys.setItem(GetItemID, 1, true, false, true);
                                }
                            }

                            DrawBinalyCount = 0;
                            this.sta = 5;
                        }
                        else
                        {
                            Debug.Log("SnapShotTargetScriptはあったけど、何かおかしい" + SnapShotTargetSc.mode);
                            this.sta = 3;
                        }
                    }
                    break;
                case 3:
                    con.NoContents.SetActive(true);
                    this.sta = 10;
                    break;
                case 4:
                    CreateBunsyouData_Message();
                    if (sys.getSoftLive(sys.isNowSoftSet())[3])
                    {
                        SetHelpBar(1);
                    }
                    this.sta = 9;
                    break;
                case 5:
                    con.Contents.SetActive(true);
                    for (int i = 0; i < 10; i++)
                    {
                        BinalyData[i] = string.Empty;
                        for (int j = 0; j < 100; j++)
                        {
                            int num = rand() / 10 % 2;
                            BinalyData[i] = BinalyData[i] + num.ToString();
                        }
                    }
                    con.ContentsText.text = BinalyData[0];
                    BinalyStart = 0;
                    this.sta = 6;
                    break;
                case 6:
                    {
                    string text = string.Empty;
                    if (DrawBinalyCount > 7)
                    {
                        int num2 = BinalyStart;
                        for (int k = 0; k < 8; k++)
                        {
                            text += BinalyData[num2];
                            num2++;
                            if (num2 > 9)
                            {
                                num2 = 0;
                            }
                        }
                        BinalyStart++;
                        if (BinalyStart > 9)
                        {
                            BinalyStart = 0;
                        }
                    }
                    else
                    {
                        text = string.Empty;
                        for (int l = 0; l < DrawBinalyCount; l++)
                        {
                            text += BinalyData[l];
                        }
                    }
                    con.ContentsText.text = text;
                        DrawBinalyCount++;
                        if (DrawBinalyCount == 120)
                        {
                            if (HaveItems)
                            {
                                con.Contents.gameObject.SetActive(false);
                                con.SameContents.gameObject.SetActive(true);
                                sys.setKeyBlock(false);
                                this.sta = 10;
                            }
                            else
                            {
                                sys.getMenuObjectNF(1).setMess(GetItemID);
                                sys.getMenuObjectNF(1).setMess("snap");
                                sys.getMenuObjectNF(1).StartSwitch();
                                sys.getL2SystemCore().seManager.playSE(null, 39);
                                Dialog_on = 1;
                                sys.setKeyBlock(false);
                                this.sta = 10;
                            }
                        }
                        break;
                    }
                default:
                    switch (sta)
                    {
                        case 80:
                            scroll_count--;
                            if (scroll_count != 0)
                            {
                                scroll_pos += 80f;
                                SnapBase.transform.localPosition = new Vector3(scroll_pos, 0f, 600f);
                            }
                            else
                            {
                                SnapBase.transform.localPosition = new Vector3(0f, 0f, 600f);
                                this.sta = 1;
                            }
                            break;
                        case 81:
                            scroll_count--;
                            if (scroll_count != 0)
                            {
                                scroll_pos -= 80f;
                                SnapBase.transform.localPosition = new Vector3(scroll_pos, 0f, 600f);
                            }
                            else
                            {
                                SnapBase.transform.localPosition = new Vector3(0f, 0f, 600f);
                                this.sta = 1;
                            }
                            break;
                        case 82:
                            scroll_count--;
                            if (scroll_count != 0)
                            {
                                scroll_pos -= 80f;
                                SnapBase.transform.localPosition = new Vector3(scroll_pos, 0f, 600f);
                            }
                            else
                            {
                                con.Contents.SetActive(false);
                                con.NoContents.SetActive(false);
                                con.SameContents.gameObject.SetActive(false);
                                m_obj.SetActive(false);
                                this.sta = 0;
                                sys.callSlideEnd();
                            }
                            break;
                        case 83:
                            scroll_count--;
                            if (scroll_count != 0)
                            {
                                scroll_pos += 80f;
                                SnapBase.transform.localPosition = new Vector3(scroll_pos, 0f, 600f);
                            }
                            else
                            {
                                con.Contents.SetActive(false);
                                con.NoContents.SetActive(false);
                                con.SameContents.gameObject.SetActive(false);
                                m_obj.SetActive(false);
                                this.sta = 0;
                                sys.callSlideEnd();
                            }
                            break;
                        default:
                            if (sta != 100)
                            {
                                MonoBehaviour.print("エラーだってよ");
                            }
                            else
                            {
                                if (Bunsyouemon)
                                {
                                    sys.getMenuObjectNF(4).EndSwitch();
                                }
                                sys.setInfoBar(false, false, 0f);
                                m_obj.SetActive(false);
                                this.sta = 0;
                            }
                            break;
                    }
                    break;
                case 9:
                    if (sys.getSoftLive(sys.isNowSoftSet())[3] && sys.getL2Keys(L2KEYS.use, KEYSTATE.DOWN))
                    {
                        SetHelpBar(0);
                        sys.setBunsyouData(ref bun);
                        sys.getMenuObjectNF(4).setMess("w");
                        sys.getMenuObjectNF(4).StartSwitch();
                        Bunsyouemon = true;
                        this.sta = 10;
                    }
                    break;
                case 10:
                    sys.itemCompleteCheck();
                    sys.softCompleteCheck();
                    this.sta = 11;
                    break;
                case 11:
                    break;
            }
            return true;
        }
    }
}
