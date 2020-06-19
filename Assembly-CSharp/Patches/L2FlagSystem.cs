using System;
using System.Collections.Generic;
using L2Base;
using L2Flag;
using UnityEngine;
using MonoMod;
using MonoMod.ModInterop;

#pragma warning disable 0626, 0649, 0414, 0108
namespace LM2RandomiserMod.Patches
{
    [MonoModPatch("L2Flag.L2FlagSystem")]
    public class patched_L2FlagSystem : L2Flag.L2FlagSystem
    {
        [NonSerialized] public Queue<string> flagWatch = new Queue<string>();
        [NonSerialized] public Queue<L2FlagBoxEnd> flags = new Queue<L2FlagBoxEnd>();
        [NonSerialized] private ItemTracker ItemTracker;

        public patched_L2FlagSystem(L2System l2sys) : base(l2sys)
        {
            typeof(patched_L2FlagSystem).ModInterop();
        }

        public extern bool orig_setFlagData(int sheet_no, string name, short data);
        public extern bool orig_setFlagData(int sheet_no, int flag_no, short data);
        public extern void orig_addFlag(int seet_no1, int flag_no1, short value, CALCU cul);

        public bool setFlagData(int sheet_no, string name, short data)
        {
#if DEV
            AddFlagToWatch(sheet_no, name, data);
#endif
            bool result = orig_setFlagData(sheet_no, name, data);

            if (ItemTracker == null)
                ItemTracker = GameObject.FindObjectOfType<ItemTracker>();

            if(ItemTracker != null)
                ItemTracker.Add(sheet_no, getFlagNo(sheet_no, name));

            return result;
        }

        public bool setFlagData(int sheet_no, int flag_no, short data)
        {
            string name = flag.cellData[sheet_no][flag_no + 1][0][0];
#if DEV
            AddFlagToWatch(sheet_no, name, data);
#endif
            bool result = orig_setFlagData(sheet_no, flag_no, data);

            if (ItemTracker == null)
                ItemTracker = GameObject.FindObjectOfType<ItemTracker>();

            if (ItemTracker != null)
                ItemTracker.Add(sheet_no, flag_no);

            return result;
        }

        public void addFlag(int seet_no1, int flag_no1, short value, CALCU cul)
        {
            string name = flag.cellData[seet_no1][flag_no1 + 1][0][0];
#if DEV
            AddFlagToWatch(seet_no1, name, value, cul);
#endif
            orig_addFlag(seet_no1, flag_no1, value, cul);

            if (ItemTracker == null)
                ItemTracker = GameObject.FindObjectOfType<ItemTracker>();

            if (ItemTracker != null)
                ItemTracker.Add(seet_no1, flag_no1);
        }

        public void AddFlagToWatch(int sheet_no, string name, short data, CALCU cul)
        {
            if (name.StartsWith("playtime")) return;
            if (name.Contains("pDoor")) return;
            if (name == "Gold" || name == "weight" || name == "Playtime") return;

            if (flagWatch == null)
                flagWatch = new Queue<string>();

            short oldData = 0;
            if (!getFlag(sheet_no, name, ref oldData)) return;

            short difference;
            switch (cul)
            {
                case CALCU.EQR:
                    difference = (short)(data - oldData);
                    break;
                case CALCU.ADD:
                    difference = data;
                    break;
                case CALCU.SUB:
                    difference = (short)-data;
                    break;
                default:
                    return;
            }

            flagWatch.Enqueue($"[{sheet_no},{getFlagNo(sheet_no, name)}]{flag.seetName[sheet_no]}.{name} = {oldData + difference} (diff:{difference})");

            if (flagWatch.Count > 12)
                flagWatch.Dequeue();
        }

        public void AddFlagToWatch(int sheet_no, string name, short data)
        {
            if (name.StartsWith("playtime")) return;
            if (name.Contains("pDoor")) return;
            if (name == "Gold" || name == "weight" || name == "Playtime") return;

            if (flagWatch == null)
                flagWatch = new Queue<string>();

            short oldData = 0;
            if (!getFlag(sheet_no, name, ref oldData)) return;

            short difference = (short)(data - oldData);

            flagWatch.Enqueue($"[{sheet_no},{getFlagNo(sheet_no, name)}]{flag.seetName[sheet_no]}.{name} = {data} (diff:{difference})");

            if (flagWatch.Count > 12)
                flagWatch.Dequeue();
        }
        public Queue<string> GetFlagWatches()
        {
            return flagWatch;
        }

        public Queue<L2FlagBoxEnd> GetFlags()
        {
            return flags;
        }
    }
}
