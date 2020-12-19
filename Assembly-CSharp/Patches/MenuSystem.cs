using L2Base;
using L2Menu;
using MonoMod;
using MonoMod.ModInterop;

#pragma warning disable 0626, 0649, 0414, 0108
namespace LM2RandomiserMod.Patches
{
    [MonoModPatch("L2Menu.MenuSystem")]
    public class patched_MenuSystem : L2Menu.MenuSystem
	{
        public patched_MenuSystem(L2System l2sys) : base(l2sys)
        {
            typeof(patched_MenuSystem).ModInterop();
        }

		[MonoModIgnore]
		private L2System sys;

		[MonoModIgnore]
		private int flagq_count;

		[MonoModIgnore]
		private MojiScFlagQ[] flagq;

		[MonoModReplace]
		public void setMojiFlagQue(int sheet, int id, short vale)
		{
			if (flagq_count >= flagq.Length)
			{
				sys.L2Print("もじもじ君スクリプト用のフラグキューが一杯で入らないですよ");
				return;
			}
			for (int i = 0; i < flagq_count; i++)
			{
				if (flagq[i].flag_sheet == sheet && flagq[i].flag_name == id)
				{
					if(sheet == 3 && id == 30)
					{
						flagq[i].flag_vale += 4;
					}
					else if(sheet == 0 && id == 2)
					{
						flagq[i].flag_vale++;
					}
					else if(sheet == 0 && id == 32)
					{
						flagq[i].flag_vale++;
					}
					else if (sheet == 5 && id == 47)
					{
						flagq[i].flag_vale++;
					}
					return;
				}
			}
			flagq[flagq_count].flag_sheet = sheet;
			flagq[flagq_count].flag_name = id;
			flagq[flagq_count].flag_vale = vale;
			flagq_count++;
		}
	}
}
