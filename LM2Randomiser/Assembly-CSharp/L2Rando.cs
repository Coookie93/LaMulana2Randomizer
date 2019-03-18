using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using L2Word;
using L2Flag;

namespace LM2RandomiserMod
{
    public class L2Rando : MonoBehaviour
    {
        private bool showText = true;
        private bool shuffleChests = true;
        private string sceneLoaded = string.Empty;

        private L2ShopDataBase shopDataBase;

        private L2System sys;

        private TreasureBoxScript[] cachedBoxes;
        private EventItemScript[] cachedItems;
        

        private void OnGUI()
        {
            if (this.showText)
            {
                GUI.Label(new Rect(100f, 0f, 100f, 22f), "Treasure Chests");
                if (cachedBoxes != null)
                {
                    for (int i = 0; i < cachedBoxes.Length; i++)
                    {
                        EventItemScript es = cachedBoxes[i].itemObj.GetComponent<EventItemScript>();
                        GUI.Label(new Rect(100f, 22f + (float)i * 22f, 100f, 22f), es.itemLabel);
                    }
                }
                GUI.Label(new Rect(0f, 0f, 100f, 22f), "Free Items");
                if (cachedItems != null)
                {
                    for (int i = 0; i < cachedItems.Length; i++)
                    {
                        GUI.Label(new Rect(0f, 25f + (float)i * 22f, 200f, 22f), cachedItems[i].itemLabel);
                    }
                }
                GUI.Label(new Rect(Screen.width - 150f, 0f, 150f, 25f), "Scene Loaded: " + sceneLoaded);
                short data = 0;
                sys.getFlagSys().getFlag(3, 30, ref data);
                GUI.Label(new Rect(Screen.width - 150f, 25f, 150f, 25f), "StoryChecker Count: " + data);
            }
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            cachedBoxes = UnityEngine.Object.FindObjectsOfType<TreasureBoxScript>();
            cachedItems = UnityEngine.Object.FindObjectsOfType<EventItemScript>();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F11))
            {
                this.showText = !this.showText;
            }

            if (Input.GetKeyDown(KeyCode.F9))
            {
                L2FlagBoxEnd[] l2FlagBoxEnds = new L2FlagBoxEnd[1];
                l2FlagBoxEnds[0] = new L2FlagBoxEnd();
                l2FlagBoxEnds[0].calcu = CALCU.ADD;
                l2FlagBoxEnds[0].seet_no1 = 3;
                l2FlagBoxEnds[0].flag_no1 = 30;
                l2FlagBoxEnds[0].data = 4;
                this.sys.setEffectFlag(l2FlagBoxEnds);
            }

            if (Input.GetKeyDown(KeyCode.F10))
            {
                sys.setItem("Shuriken", 1, false, false, true);
                sys.setItem("Chakram", 1, false, false, true);
                sys.setItem("E-Spear", 1, false, false, true);
                sys.setItem("Caltrops", 1, false, false, true);
                sys.setItem("Flare Gun", 1, false, false, true);
                sys.setItem("Bomb", 1, false, false, true);
                sys.setItem("R-Shuriken", 1, false, false, true);
                sys.setItem("Feather", 1, false, false, true);
                sys.setItem("G Claw", 1, false, false, true);
                sys.setItem("Sacred Orb", 1, false, false, true);
                sys.setItem("Sacred Orb", 1, false, false, true);
                sys.setItem("Sacred Orb", 1, false, false, true);
                sys.setItem("Sacred Orb", 1, false, false, true);
                sys.setItem("Origin Seal", 1, false, false, true);
                sys.setItem("Birth Seal", 1, false, false, true);
                sys.setItem("F Ship", 1, false, false, true);
                sys.setItem("Whip3", 1, false, false, true);
                sys.setItem("Ankh Jewel", 1, false, false, true);
                sys.setItem("Pepper", 1, false, false, true);
                sys.setItem("Snapshots", 1, false, false, true);
                sys.setItem("Djed Pillar", 1, false, false, true);
                sys.setItem("Beherit", 1, false, false, true);
                sys.setItem("Ring", 1, false, false, true);
                sys.setItem("Lamp", 1, false, false, true);
            }

            //if (Input.GetKeyDown(KeyCode.F10))
            //{
            //    int sceaneNo = sys.getL2SystemCore().SceaneNo;
            //    string fieldName = String.Empty;
            //    try
            //    {
            //        fieldName = this.sys.getMojiText(false, "field", this.sys.SceenNoToFieldID(sceaneNo), mojiScriptType.mapname);
            //    }
            //    catch (Exception ex)
            //    {
            //        //Fuck
            //    }

            //    string filePath = "C:\\Users\\Ben\\Documents\\LM2\\XMLFlags\\";

            //    List<ItemInfo> flags = new List<ItemInfo>();
            //    if (cachedBoxes != null)
            //    {
            //        for (int i = 0; i < cachedBoxes.Length; i++)
            //        {
            //            TreasureBoxScript ts = cachedBoxes[i];
            //            ItemInfo flag = new ItemInfo();
            //            flag.openFlags = ts.openFlags;

            //            EventItemScript es = ts.itemObj.GetComponent<EventItemScript>();
            //            flag.activeFlags = es.itemActiveFlag;
            //            flag.getFlags = es.itemGetFlags;
            //            flag.name = es.itemLabel;
            //            if (es.itemGetFlags != null)
            //            {
            //                L2FlagBoxEnd end = es.itemGetFlags[0];
            //                L2FlagBase flagBase;
            //                if (sys.getFlagSys().getFlagBaseObject(end.seet_no1, end.flag_no1, out flagBase))
            //                {
            //                    flag.flagName = flagBase.flagName;
            //                }
            //                else
            //                {
            //                    flag.flagName = "";
            //                }
            //            }
            //            flags.Add(flag);
            //        }
            //    }
            //    if (cachedItems != null)
            //    {
            //        for (int i = 0; i < cachedItems.Length; i++)
            //        {
            //            EventItemScript es = cachedItems[i];
            //            ItemInfo flag = new ItemInfo();
            //            flag.name = es.itemLabel;
            //            if (es.itemActiveFlag != null)
            //            {
            //                L2FlagBoxParent[] parent = new L2FlagBoxParent[1];
            //                parent[0] = new L2FlagBoxParent();
            //                parent[0].logoc = es.itemActiveFlag[0].logoc;
            //                parent[0].BOX = new L2FlagBox[1];
            //                parent[0].BOX[0] = new L2FlagBox();
            //                parent[0].BOX[0].comp = COMPARISON.GreaterEq;
            //                parent[0].BOX[0].logic = LOGIC.NON;
            //                parent[0].BOX[0].seet_no1 = es.itemActiveFlag[0].BOX[0].seet_no1;
            //                parent[0].BOX[0].flag_no1 = es.itemActiveFlag[0].BOX[0].flag_no1;
            //                parent[0].BOX[0].seet_no2 = es.itemActiveFlag[0].BOX[0].seet_no2;
            //                parent[0].BOX[0].flag_no2 = 1;
            //                flag.openFlags = parent;
            //            }
            //            flag.activeFlags = es.itemActiveFlag;
            //            flag.getFlags = es.itemGetFlags;
            //            if (es.itemGetFlags != null)
            //            {
            //                L2FlagBoxEnd end = es.itemGetFlags[0];
            //                L2FlagBase flagBase;
            //                if (sys.getFlagSys().getFlagBaseObject(end.seet_no1, end.flag_no1, out flagBase))
            //                {
            //                    flag.flagName = flagBase.flagName;
            //                }
            //                else
            //                {
            //                    flag.flagName = "";
            //                }
            //            }
            //            flags.Add(flag);
            //        }
            //    }
            //    XmlSerializer serializer = new XmlSerializer(typeof(List<ItemInfo>));
            //    TextWriter writer = new StreamWriter(filePath + fieldName + ".xml");
            //    serializer.Serialize(writer, flags);
            //    writer.Close();
            //}
        }

        public void Initialise(L2ShopDataBase shopDataBase, L2System system)
        {
            this.shopDataBase = shopDataBase;
            this.sys = system;
        }

        //private void WriteToFile(string filePath, string fieldName)
        //{
        //    using (StreamWriter sr = File.CreateText(filePath + "TreasureChestFlags\\" + fieldName + ".txt"))
        //    {
        //        sr.WriteLine(fieldName.ToUpper());
        //        sr.WriteLine("Total Treasure Chest Count: {0}", cachedBoxes.Length);
        //        for (int i = 0; i < cachedBoxes.Length; i++)
        //        {
        //            EventItemScript es = cachedBoxes[i].itemObj.GetComponent<EventItemScript>();
        //            sr.WriteLine();
        //            sr.WriteLine(new String('/', 30));
        //            sr.WriteLine(es.name + " - " + es.itemLabel);
        //            sr.WriteLine(new String('/', 30));
        //            sr.WriteLine();
        //            WriteFlagBoxParent(sr, cachedBoxes[i].openFlags, "OPEN FLAGS");
        //            WriteFlagBoxParent(sr, cachedBoxes[i].unlockFlags, "UNLOCK FLAGS");
        //            WriteFlagBoxParent(sr, cachedBoxes[i].itemFlags, "ITEM FLAGS");
        //            WriteFlagBoxParent(sr, cachedBoxes[i].forceOpenFlags, "FORCE OPEN FLAGS");
        //            WriteFlagBoxEnd(sr, cachedBoxes[i].openActionFlags, "OPEN ACTION FLAGS");
        //            WriteFlagBoxParent(sr, es.itemActiveFlag, "ITEM ACTIVE FLAGS");
        //            WriteFlagBoxEnd(sr, es.itemGetFlags, "ITEM GET FLAGS");
        //        }
        //    }
        //}

        //private void WriteToFile2(string filePath, string fieldName)
        //{
        //    using (StreamWriter sr = File.CreateText(filePath + "EventItemFlags\\" + fieldName + ".txt"))
        //    {
        //        sr.WriteLine(fieldName.ToUpper());
        //        sr.WriteLine("Total Event Item Count: {0}", cachedItems.Length);
        //        for (int i = 0; i < cachedItems.Length; i++)
        //        {
        //            EventItemScript es = cachedItems[i];
        //            sr.WriteLine();
        //            sr.WriteLine(new String('/', 30));
        //            sr.WriteLine(es.name + " - " + es.itemLabel);
        //            sr.WriteLine(new String('/', 30));
        //            sr.WriteLine();
        //            sr.WriteLine("Item Value: " + es.itemValue);
        //            sr.WriteLine();
        //            WriteFlagBoxParent(sr, es.itemActiveFlag, "ITEM ACTIVE FLAGS");
        //            WriteFlagBoxEnd(sr, es.itemGetFlags, "ITEM GET FLAGS");
        //        }
        //    }
        //}

        //private void WriteFlagBoxParent(StreamWriter sr, L2FlagBoxParent[] parentBoxes, string name)
        //{
        //    sr.WriteLine(name);
        //    sr.WriteLine(new String('-', 15));
        //    for (int j = 0; j < parentBoxes.Length; j++)
        //    {
        //        L2FlagBoxParent parent = parentBoxes[j];
        //        sr.WriteLine(parent.logoc);
        //        for (int k = 0; k < parent.BOX.Length; k++)
        //        {
        //            L2FlagBox box = parent.BOX[k];
        //            sr.WriteLine("Flag {0}", k);
        //            sr.WriteLine("Comparison: {0}", box.comp);
        //            sr.WriteLine("Logic: {0}", box.logic);
        //            sr.WriteLine("FlagL");
        //            sr.WriteLine("SeetNo: {0}", box.seet_no1);
        //            sr.WriteLine("FlagNo: {0}", box.flag_no1);
        //            sr.WriteLine("FlagR");
        //            sr.WriteLine("SeetNo: {0}", box.seet_no2);
        //            sr.WriteLine("FlagNo: {0}", box.flag_no2);
        //            sr.WriteLine();
        //            WriteFlagBase(sr, box.seet_no1, box.flag_no1, "FlagBase L");
        //            WriteFlagBase(sr, box.seet_no2, box.flag_no2, "FlagBase R");
        //        }
        //    }
        //    sr.WriteLine();
        //}

        //private void WriteFlagBoxEnd(StreamWriter sr, L2FlagBoxEnd[] endBoxes, string name)
        //{
        //    sr.WriteLine(name);
        //    sr.WriteLine(new String('-', 15));
        //    for (int j = 0; j < endBoxes.Length; j++)
        //    {
        //        L2FlagBoxEnd box = endBoxes[j];
        //        sr.WriteLine("Flag {0}", j);
        //        sr.WriteLine("Calculation: {0}", box.calcu);
        //        sr.WriteLine("SeetNo: {0}", box.seet_no1);
        //        sr.WriteLine("FlagNo: {0}", box.flag_no1);
        //        sr.WriteLine("Data: {0}", box.data);
        //        sr.WriteLine();
        //        WriteFlagBase(sr, box.seet_no1, box.flag_no1, "FlagBase");
        //    }
        //}

        //private void WriteFlagBase(StreamWriter sr, int seet, int flag, string name)
        //{
        //    L2FlagBase flagBase;
        //    sr.WriteLine(name);
        //    if (sys.getFlagSys().getFlagBaseObject(seet, flag, out flagBase))
        //    {
        //        sr.WriteLine(flagBase.flagName);
        //        sr.WriteLine(flagBase.getMyHash());
        //        sr.WriteLine(flagBase.memo);
        //    }
        //    else
        //    {
        //        sr.WriteLine("null");
        //    }
        //    sr.WriteLine();
        //}


    }

    //public class ItemInfo
    //{
    //    public string flagName;
    //    public string name;
    //    public L2FlagBoxParent[] openFlags;
    //    public L2FlagBoxParent[] activeFlags;
    //    public L2FlagBoxEnd[] getFlags;
    //}
}