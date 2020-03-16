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
                    if (!this.sys.getL2Keys(L2KEYS.up, KEYSTATE.REPEAT))
                    {
                        if (!this.sys.getL2Keys(L2KEYS.down, KEYSTATE.REPEAT))
                        {
                            if (!this.sys.getL2Keys(L2KEYS.left, KEYSTATE.DOWN))
                            {
                                if (this.sys.getL2Keys(L2KEYS.right, KEYSTATE.DOWN))
                                {
                                }
                            }
                        }
                    }
                    if (this.sys.getL2Keys(L2KEYS.ok, KEYSTATE.DOWN))
                    {
                        this.sys.setKeyBlock(true);
                        this.con.anime.Play("snap scan");
                        this.L2Core.seManager.playSE(null, 155);
                        this.sta = 2;
                    }
                    if (this.sys.getL2Keys(L2KEYS.cancel, KEYSTATE.DOWN))
                    {
                    }
                    break;
                case 2:
                    if (this.con.anime.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                    {
                        this.sys.setKeyBlock(false);
                        this.con.anime.Play("snap scanEnd");
                        this.SnapShotTargetSc = this.L2Core.ScrollSystem.getSnapShotTarget();
                        this.L2Core.seManager.playSE(null, 37);
                        if (this.SnapShotTargetSc == null)
                        {
                            this.sta = 3;
                        }
                        else if (this.SnapShotTargetSc.mode == SnapShotTargetScript.SnapShotMode.MESSAGE)
                        {
                            this.con.Contents.SetActive(true);
                            this.con.ContentsText.text = this.sys.getMojiText(false, this.SnapShotTargetSc.sheetName, this.SnapShotTargetSc.cellName, mojiScriptType.sekihi);
                            this.sta = 4;
                        }
                        else if (this.SnapShotTargetSc.mode == SnapShotTargetScript.SnapShotMode.SOFTWARE)
                        {

                            this.sys.setKeyBlock(true);
                            L2Rando rando = GameObject.FindObjectOfType<L2Rando>();
                            if(rando != null && rando.Randomising)
                            {
                                LocationID locationID = rando.GetLocationIDForMural(SnapShotTargetSc);
                                ItemID itemID = rando.GetItemIDForLocation(locationID);
                                ItemInfo itemInfo = ItemDB.GetItemInfo(itemID);

                                this.GetItemID = itemInfo.boxName;

                                int flagValue = 0;
                                if (itemID == ItemID.MobileSuperx3P)
                                    flagValue = 1;

                                if (this.sys.isHaveItem(itemInfo.shopName) > flagValue)
                                {
                                    this.HaveItems = true;
                                }
                                else
                                {
                                    this.HaveItems = false;
                                    this.sys.setItem(this.GetItemID, 1, false, false, true);
                                    this.sys.setEffectFlag(rando.CreateGetFlags(itemID, itemInfo));
                                }
                            }
                            else
                            {
                                this.item = L2SystemCore.getItemData(this.SnapShotTargetSc.itemName);
                                this.GetItemID = this.item.getItemId();
                                if (this.sys.isHaveItem(this.GetItemID) > 0)
                                {
                                    this.HaveItems = true;
                                }
                                else
                                {
                                    this.HaveItems = false;
                                    this.sys.setItem(this.GetItemID, 1, true, false, true);
                                }
                            }

                            this.DrawBinalyCount = 0;
                            this.sta = 5;
                        }
                        else
                        {
                            Debug.Log("SnapShotTargetScriptはあったけど、何かおかしい" + this.SnapShotTargetSc.mode);
                            this.sta = 3;
                        }
                    }
                    break;
                case 3:
                    this.con.NoContents.SetActive(true);
                    this.sta = 10;
                    break;
                case 4:
                    this.CreateBunsyouData_Message();
                    if (this.sys.getSoftLive(this.sys.isNowSoftSet())[3])
                    {
                        this.SetHelpBar(1);
                    }
                    this.sta = 9;
                    break;
                case 5:
                    this.con.Contents.SetActive(true);
                    for (int i = 0; i < 10; i++)
                    {
                        this.BinalyData[i] = string.Empty;
                        for (int j = 0; j < 100; j++)
                        {
                            int num = this.rand() / 10 % 2;
                            this.BinalyData[i] = this.BinalyData[i] + num.ToString();
                        }
                    }
                    this.con.ContentsText.text = this.BinalyData[0];
                    this.BinalyStart = 0;
                    this.sta = 6;
                    break;
                case 6:
                    {
                        string text = string.Empty;
                        if (this.DrawBinalyCount > 7)
                        {
                            int num2 = this.BinalyStart;
                            for (int k = 0; k < 8; k++)
                            {
                                text += this.BinalyData[num2];
                                num2++;
                                if (num2 > 9)
                                {
                                    num2 = 0;
                                }
                            }
                            this.BinalyStart++;
                            if (this.BinalyStart > 9)
                            {
                                this.BinalyStart = 0;
                            }
                        }
                        else
                        {
                            text = string.Empty;
                            for (int l = 0; l < this.DrawBinalyCount; l++)
                            {
                                text += this.BinalyData[l];
                            }
                        }
                        this.con.ContentsText.text = text;
                        this.DrawBinalyCount++;
                        if (this.DrawBinalyCount == 120)
                        {
                            if (this.HaveItems)
                            {
                                this.con.Contents.gameObject.SetActive(false);
                                this.con.SameContents.gameObject.SetActive(true);
                                this.sys.setKeyBlock(false);
                                this.sta = 10;
                            }
                            else
                            {
                                this.sys.getMenuObjectNF(1).setMess(this.GetItemID);
                                this.sys.getMenuObjectNF(1).setMess("snap");
                                this.sys.getMenuObjectNF(1).StartSwitch();
                                this.sys.getL2SystemCore().seManager.playSE(null, 39);
                                this.Dialog_on = 1;
                                this.sys.setKeyBlock(false);
                                this.sta = 10;
                            }
                        }
                        break;
                    }
                default:
                    switch (sta)
                    {
                        case 80:
                            this.scroll_count--;
                            if (this.scroll_count != 0)
                            {
                                this.scroll_pos += 80f;
                                this.SnapBase.transform.localPosition = new Vector3(this.scroll_pos, 0f, 600f);
                            }
                            else
                            {
                                this.SnapBase.transform.localPosition = new Vector3(0f, 0f, 600f);
                                this.sta = 1;
                            }
                            break;
                        case 81:
                            this.scroll_count--;
                            if (this.scroll_count != 0)
                            {
                                this.scroll_pos -= 80f;
                                this.SnapBase.transform.localPosition = new Vector3(this.scroll_pos, 0f, 600f);
                            }
                            else
                            {
                                this.SnapBase.transform.localPosition = new Vector3(0f, 0f, 600f);
                                this.sta = 1;
                            }
                            break;
                        case 82:
                            this.scroll_count--;
                            if (this.scroll_count != 0)
                            {
                                this.scroll_pos -= 80f;
                                this.SnapBase.transform.localPosition = new Vector3(this.scroll_pos, 0f, 600f);
                            }
                            else
                            {
                                this.con.Contents.SetActive(false);
                                this.con.NoContents.SetActive(false);
                                this.con.SameContents.gameObject.SetActive(false);
                                this.m_obj.SetActive(false);
                                this.sta = 0;
                                this.sys.callSlideEnd();
                            }
                            break;
                        case 83:
                            this.scroll_count--;
                            if (this.scroll_count != 0)
                            {
                                this.scroll_pos += 80f;
                                this.SnapBase.transform.localPosition = new Vector3(this.scroll_pos, 0f, 600f);
                            }
                            else
                            {
                                this.con.Contents.SetActive(false);
                                this.con.NoContents.SetActive(false);
                                this.con.SameContents.gameObject.SetActive(false);
                                this.m_obj.SetActive(false);
                                this.sta = 0;
                                this.sys.callSlideEnd();
                            }
                            break;
                        default:
                            if (sta != 100)
                            {
                                MonoBehaviour.print("エラーだってよ");
                            }
                            else
                            {
                                if (this.Bunsyouemon)
                                {
                                    this.sys.getMenuObjectNF(4).EndSwitch();
                                }
                                this.sys.setInfoBar(false, false, 0f);
                                this.m_obj.SetActive(false);
                                this.sta = 0;
                            }
                            break;
                    }
                    break;
                case 9:
                    if (this.sys.getSoftLive(this.sys.isNowSoftSet())[3] && this.sys.getL2Keys(L2KEYS.use, KEYSTATE.DOWN))
                    {
                        this.SetHelpBar(0);
                        this.sys.setBunsyouData(ref this.bun);
                        this.sys.getMenuObjectNF(4).setMess("w");
                        this.sys.getMenuObjectNF(4).StartSwitch();
                        this.Bunsyouemon = true;
                        this.sta = 10;
                    }
                    break;
                case 10:
                    this.sys.itemCompleteCheck();
                    this.sys.softCompleteCheck();
                    this.sta = 11;
                    break;
                case 11:
                    break;
            }
            return true;
        }
    }
}
