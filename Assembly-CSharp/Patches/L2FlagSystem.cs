using System;
using System.Collections.Generic;
using L2Base;
using L2Flag;
using MonoMod;
using MonoMod.ModInterop;

#pragma warning disable 0626, 0649, 0414, 0108
namespace LM2RandomiserMod.Patches
{
    [MonoModPatch("L2Flag.L2FlagSystem")]
    public class patched_L2FlagSystem : L2Flag.L2FlagSystem
    {
        [NonSerialized] public List<string> flagWatch = new List<string>();

        public patched_L2FlagSystem(L2System l2sys) : base(l2sys)
        {
            typeof(patched_L2FlagSystem).ModInterop();
        }

        public extern bool orig_setFlagData(int sheet_no, string name, short data);
        public extern bool orig_setFlagData(int sheet_no, int flag_no, short data);
        public extern void orig_addFlag(int seet_no1, int flag_no1, short value, CALCU cul);

        public bool setFlagData(int sheet_no, string name, short data)
        {
            AddFlagToWatch(sheet_no, name, data);

            return orig_setFlagData(sheet_no, name, data);
        }

        public bool setFlagData(int sheet_no, int flag_no, short data)
        {
            AddFlagToWatch(sheet_no, flag.cellData[sheet_no][flag_no + 1][0][0], data);

            return orig_setFlagData(sheet_no, flag_no, data);
        }

        public void addFlag(int seet_no1, int flag_no1, short value, CALCU cul)
        {
            AddFlagToWatch(seet_no1, flag.cellData[seet_no1][flag_no1 + 1][0][0], value, cul);

            orig_addFlag(seet_no1, flag_no1, value, cul);
        }

        public void AddFlagToWatch(int sheet_no, string name, short data, CALCU cul)
        {
            if (name.StartsWith("playtime")) return;
            if (name.Contains("pDoor")) return;
            if (name == "Gold" || name == "weight" || name == "Playtime") return;

            if (flagWatch == null)
            {
                flagWatch = new List<String>();
            }

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

            flagWatch.Add($"{flag.seetName[sheet_no]}.{name} = {oldData + difference} (diff:{difference})");
        }

        public void AddFlagToWatch(int sheet_no, string name, short data)
        {
            if (name.StartsWith("playtime")) return;
            if (name.Contains("pDoor")) return;
            if (name == "Gold" || name == "weight" || name == "Playtime") return;

            if (flagWatch == null)
            {
                flagWatch = new List<String>();
            }

            short oldData = 0;
            if (!getFlag(sheet_no, name, ref oldData)) return;

            short difference = (short)(data - oldData);

            flagWatch.Add($"{flag.seetName[sheet_no]}.{name} = {data} (diff:{difference})");
        }
        public List<string> GetFlagWatches()
        {
            return flagWatch;
        }
    }
}
