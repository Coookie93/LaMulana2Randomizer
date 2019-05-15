using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MonoMod;
using L2Base;
using L2Menu;
using L2Word;

#pragma warning disable 0626, 0649, 0414, 0108
namespace LM2RandomiserMod.Patches
{
    [MonoModPatch("global::ItemDialog")]
    public class patched_ItemDialog : global::ItemDialog
    {
        [MonoModIgnore]
        private Camera cam;

        [MonoModIgnore]
        private string[] MessString;

        [MonoModIgnore]
        private int[] MessInt;

        [MonoModIgnore]
        private float PositionD;

        [MonoModIgnore]
        private float PositionF;

        [MonoModIgnore]
        private CanvasGroup cgroup;

        [MonoModIgnore]
        private ItemDialogController con;

        [MonoModIgnore]
        private float move_count;

        [MonoModReplace]
        public override void StartSwitch()
        {
            if (!this.first)
            {
                this.cam = GameObject.Find("BGScrollSystemBase").GetComponent<BGScrollSystem>().bgCamera.BaseCamera;
                if (this.MessString[1] == "kataribe")
                {
                    this.PositionF = 100f;
                    this.PositionD = 120f;
                }
                else
                {
                    Vector3 position = this.sys.getPlayer().gameObject.transform.position;
                    if (this.cam.WorldToScreenPoint(position).y > 224f)
                    {
                        this.PositionF = -100f;
                        this.PositionD = -120f;
                    }
                    else
                    {
                        this.PositionF = 100f;
                        this.PositionD = 120f;
                    }
                    this.sys.setSysFlag(SYSTEMFLAG.MENUOPEN);
                }
                this.cgroup = this.con.DialogBase.GetComponent<CanvasGroup>();
                this.cgroup.alpha = 0f;
                if (this.MessString[0] == "Message")
                {
                    this.con.Icon.gameObject.SetActive(false);
                }
                else
                {
                    this.con.Icon.gameObject.SetActive(true);
                    this.con.Icon.sprite = L2Math.Load("Textures/icons_itemmenu", this.MessString[0]);
                }
                string str;
                string str2;
                string str3;
                if (this.MessString[0] == "N Chip")
                {
                    str = this.sys.getMojiText(true, "system", "itemDialog1", mojiScriptType.system);
                    str2 = this.GetMonsterName(this.MessInt[0]);
                    str3 = this.sys.getMojiText(true, "system", "itemDialog2", mojiScriptType.system);
                }
                else if (this.MessString[0] == "R Chip")
                {
                    str = this.sys.getMojiText(true, "system", "itemDialog1", mojiScriptType.system);
                    str2 = this.GetMonsterName(this.MessInt[0]);
                    str3 = this.sys.getMojiText(true, "system", "itemDialog2", mojiScriptType.system);
                }
                else if (this.MessString[0] == "SR Chip")
                {
                    str = this.sys.getMojiText(true, "system", "itemDialog1", mojiScriptType.system);
                    str2 = this.GetMonsterName(this.MessInt[0]);
                    str3 = this.sys.getMojiText(true, "system", "itemDialog2", mojiScriptType.system);
                }
                else if (this.MessString[0] == "UR Chip")
                {
                    str = this.sys.getMojiText(true, "system", "itemDialog1", mojiScriptType.system);
                    str2 = this.GetMonsterName(this.MessInt[0]);
                    str3 = this.sys.getMojiText(true, "system", "itemDialog2", mojiScriptType.system);
                }
                else if (this.MessString[0] == "Message")
                {
                    str = string.Empty;
                    str2 = this.sys.getMojiText(true, this.MessString[1], this.MessString[2], mojiScriptType.system);
                    str3 = string.Empty;
                }
                else if(this.MessString[0].Contains("Mantra") && !this.MessString[0].Equals("Mantra"))
                {
                    this.con.Icon.gameObject.SetActive(false);
                    str = this.sys.getMojiText(true, "system", "itemDialog1", mojiScriptType.system);
                    string mojiName = MessString[0].Equals("Mantra10") ? "mantra1stM10" : "mantra1stM" + MessString[0].Substring(6, 1);
                    str2 = this.sys.getMojiText(false, "menu", mojiName, mojiScriptType.system);
                    str3 = this.sys.getMojiText(true, "system", "itemDialog2", mojiScriptType.system);
                }
                else
                {
                    str = this.sys.getMojiText(true, "system", "itemDialog1", mojiScriptType.system);
                    string itemSheetName = this.sys.getItemSheetName(this.MessString[0]);
                    str2 = this.sys.getMojiText(false, itemSheetName, this.MessString[0], mojiScriptType.item);
                    str3 = this.sys.getMojiText(true, "system", "itemDialog2", mojiScriptType.system);
                }
                this.con.DialogText.text = str + str2 + str3;
                this.sys.setSysFlag(SYSTEMFLAG.ITDLRBLOCK);
                this.sta = 1;
                this.con.DialogBase.transform.localPosition = new Vector3(0f, this.PositionD, 600f);
                this.move_count = 0f;
                this.m_obj.SetActive(true);
            }
        }
    }
}
