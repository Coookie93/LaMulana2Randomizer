using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoMod;

#pragma warning disable 0649, 0414, 0108
namespace LM2RandomiserMod.Patches
{
    [MonoModPatch("L2Menu.SeihaiMenu")]
    public class patched_SeihaiMenu : L2Menu.SeihaiMenu
    {
        [MonoModReplace]
		private void getOnHolyNum(out int omote, out int ura)
		{
			uint fieldWarpFlag = this.sys.getFieldWarpFlag();
			omote = 0;
			ura = 0;
			for (int i = 0; i < 10; i++)
			{
				if ((fieldWarpFlag >> i & 1u) == 1u)
				{
					omote++;
				}
			}
			for (int j = 10; j < 20; j++)
			{
				if ((fieldWarpFlag >> j & 1u) == 1u)
				{
					ura++;
				}
			}

			if (!sys.getPlayer()._uraWarp)
				ura = 0;
		}

		[MonoModIgnore]
		private int[] warppointbuffer_omote;

		[MonoModIgnore]
		private int[] warppointbuffer_ura;

		[MonoModReplace]
		private int getNowFieldPoint(int omotenum, int uranum, out int uraomote)
		{
			ViewProperty currentView = sys.getL2SystemCore().ScrollSystem.getCurrentView();
			int sceaneNo = sys.getL2SystemCore().SceaneNo;
			bool flag = sys.getPlayer()._uraWarp && uranum > 0;
			int num;
			switch (sceaneNo)
			{
				case 0:
					uraomote = 0;
					num = 1;
					break;
				case 1:
					uraomote = 0;
					num = 0;
					break;
				case 2:
					uraomote = 0;
					num = 2;
					break;
				case 3:
					if (currentView.ViewY >= 5 && flag)
					{
						uraomote = 1;
						num = 12;
					}
					else
					{
						uraomote = 0;
						num = 3;
					}
					break;
				case 4:
					if (currentView.ViewX >= 4 && flag)
					{
						uraomote = 1;
						num = 13;
					}
					else
					{
						uraomote = 0;
						num = 4;
					}
					break;
				case 5:
					uraomote = 0;
					num = 5;
					break;
				case 6:
					uraomote = 0;
					num = 6;
					break;
				case 7:
					uraomote = 0;
					num = 7;
					break;
				case 8:
					uraomote = 0;
					num = 8;
					break;
				case 9:
					uraomote = 0;
					num = 9;
					break;
				case 10:
					if (flag)
					{
						uraomote = 1;
						num = 14;
					}
					else
					{
						uraomote = 0;
						num = 0;
					}
					break;
				case 11:
					if (flag)
					{
						uraomote = 1;
						num = 16;
					}
					else
					{
						uraomote = 0;
						num = 0;
					}
					break;
				case 12:
					if (flag)
					{
						uraomote = 1;
						num = 17;
					}
					else
					{
						uraomote = 0;
						num = 0;
					}
					break;
				case 13:
					if (flag)
					{
						uraomote = 1;
						num = 18;
					}
					else
					{
						uraomote = 0;
						num = 0;
					}
					break;
				case 14:
					if (flag)
					{
						uraomote = 1;
						num = 10;
					}
					else
					{
						uraomote = 0;
						num = 0;
					}
					break;
				case 15:
					if (flag)
					{
						uraomote = 1;
						num = 11;
					}
					else
					{
						uraomote = 0;
						num = 0;
					}
					break;
				case 28:
					if (flag)
					{
						uraomote = 1;
						num = 15;
					}
					else
					{
						uraomote = 0;
						num = 0;
					}
					break;
				case 32:
					uraomote = 0;
					num = 0;
					break;
				default:
					uraomote = 0;
					num = -1;
					break;
			}

			if (omotenum == 0 && uraomote == 0)
			{
				num = -1;
				uraomote = 1;
			}

			if (num != -1)
			{
				if (uraomote == 0)
				{
					for (int i = 0; i < omotenum; i++)
					{
						if (this.warppointbuffer_omote[i] == num)
						{
							return i;
						}
					}
				}
				else if (uraomote == 1)
				{
					for (int i = 0; i < uranum; i++)
					{
						if (this.warppointbuffer_ura[i] == num)
						{
							return i;
						}
					}
				}
			}
			return 0;
		}
	}
}
