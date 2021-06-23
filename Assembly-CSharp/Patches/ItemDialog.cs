using UnityEngine;
using MonoMod;
using L2Base;
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
            if (!first)
            {
                cam = GameObject.Find("BGScrollSystemBase").GetComponent<BGScrollSystem>().bgCamera.BaseCamera;
                if (MessString[1] == "kataribe")
                {
                    PositionF = 100f;
                    PositionD = 120f;
                }
                else
                {
                    Vector3 position = sys.getPlayer().gameObject.transform.position;
                    if (cam.WorldToScreenPoint(position).y > 224f)
                    {
                        PositionF = -100f;
                        PositionD = -120f;
                    }
                    else
                    {
                        PositionF = 100f;
                        PositionD = 120f;
                    }
                    sys.setSysFlag(SYSTEMFLAG.MENUOPEN);
                }
                cgroup = con.DialogBase.GetComponent<CanvasGroup>();
                cgroup.alpha = 0f;
                if (MessString[0] == "Message")
                {
                    con.Icon.gameObject.SetActive(false);
                }
                else
                {
                    con.Icon.gameObject.SetActive(true);
                    if (MessString[0].Contains("Whip"))
                    {
                        short data = 0;
                        sys.getFlag(2, "Whip", ref data);
                        if (MessString[1] == "kataribe")
                        {
                            if (data == 0) MessString[0] = "Whip";
                            else if (data == 1) MessString[0] = "Whip2";
                            else if (data >= 2) MessString[0] = "Whip3";
                        }
                        else
                        {
                            if (data == 1) MessString[0] = "Whip";
                            else if (data == 2) MessString[0] = "Whip2";
                            else if (data >= 3) MessString[0] = "Whip3";
                        }
                    }
                    else if (MessString[0].Contains("Shield"))
                    {
                        short data = 0;
                        sys.getFlag(2, 196, ref data);
                        if (MessString[1] == "kataribe")
                        {
                            if (data == 0) MessString[0] = "Shield";
                            else if (data == 1) MessString[0] = "Shield2";
                            else if (data >= 2) MessString[0] = "Shield3";
                        }
                        else
                        {
                            if (data == 1) MessString[0] = "Shield";
                            else if (data == 2) MessString[0] = "Shield2";
                            else if (data >= 3) MessString[0] = "Shield3";
                        }
                    }
                    else if (MessString[0].Contains("Research"))
                    {
                        MessString[0] = "Research";
                    }
                    else if (MessString[0].Contains("Beherit"))
                    {
                        MessString[0] = "Beherit";
                    }
                    con.Icon.sprite = L2Math.Load("Textures/icons_itemmenu", MessString[0]);
                }
                string str;
                string str2;
                string str3;
                if (MessString[0] == "N Chip")
                {
                    str = sys.getMojiText(true, "system", "itemDialog1", mojiScriptType.system);
                    str2 = GetMonsterName(MessInt[0]);
                    str3 = sys.getMojiText(true, "system", "itemDialog2", mojiScriptType.system);
                }
                else if (MessString[0] == "R Chip")
                {
                    str = sys.getMojiText(true, "system", "itemDialog1", mojiScriptType.system);
                    str2 = GetMonsterName(MessInt[0]);
                    str3 = sys.getMojiText(true, "system", "itemDialog2", mojiScriptType.system);
                }
                else if (MessString[0] == "SR Chip")
                {
                    str = sys.getMojiText(true, "system", "itemDialog1", mojiScriptType.system);
                    str2 = GetMonsterName(MessInt[0]);
                    str3 = sys.getMojiText(true, "system", "itemDialog2", mojiScriptType.system);
                }
                else if (MessString[0] == "UR Chip")
                {
                    str = sys.getMojiText(true, "system", "itemDialog1", mojiScriptType.system);
                    str2 = GetMonsterName(MessInt[0]);
                    str3 = sys.getMojiText(true, "system", "itemDialog2", mojiScriptType.system);
                }
                else if (MessString[0] == "Message")
                {
                    str = string.Empty;
                    str2 = sys.getMojiText(true, MessString[1], MessString[2], mojiScriptType.system);
                    str3 = string.Empty;
                }
                else if(MessString[0].Contains("Mantra") && !MessString[0].Equals("Mantra"))
                {
                    con.Icon.gameObject.SetActive(false);
                    str = sys.getMojiText(true, "system", "itemDialog1", mojiScriptType.system);
                    string mojiName = "mantra1stM" + MessString[0].Substring(6);
                    str2 = sys.getMojiText(false, "menu", mojiName, mojiScriptType.system);
                    str3 = sys.getMojiText(true, "system", "itemDialog2", mojiScriptType.system);
                }
                else if (MessString[0].Equals("Nothing"))
                {
                    con.Icon.gameObject.SetActive(false);
                    str = sys.getMojiText(true, "system", "itemDialog1", mojiScriptType.system);
                    str2 = "Nothing";
                    str3 = sys.getMojiText(true, "system", "itemDialog2", mojiScriptType.system);
                }
                else if (MessString[0].Equals("Beherit"))
                {
                    con.Icon.gameObject.SetActive(false);
                    str = sys.getMojiText(true, "system", "itemDialog1", mojiScriptType.system);
                    short data = 0;
                    sys.getFlag(2, 3, ref data);
                    if(data > 1)
                    {
                        str2 = "Dissonance " + (data - 1);
                    }
                    else
                    {
                        string itemSheetName = sys.getItemSheetName(MessString[0]);
                        str2 = sys.getMojiText(false, itemSheetName, MessString[0], mojiScriptType.item);
                    }
                    str3 = sys.getMojiText(true, "system", "itemDialog2", mojiScriptType.system);
                }
                else
                {
                    str = sys.getMojiText(true, "system", "itemDialog1", mojiScriptType.system);
                    string itemSheetName = sys.getItemSheetName(MessString[0]);
                    str2 = sys.getMojiText(false, itemSheetName, MessString[0], mojiScriptType.item);
                    str3 = sys.getMojiText(true, "system", "itemDialog2", mojiScriptType.system);
                }
                con.DialogText.text = str + str2 + str3;
                sys.setSysFlag(SYSTEMFLAG.ITDLRBLOCK);
                sta = 1;
                con.DialogBase.transform.localPosition = new Vector3(0f, PositionD, 600f);
                move_count = 0f;
                m_obj.SetActive(true);
            }
        }
    }
}
