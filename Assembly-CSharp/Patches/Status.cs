using MonoMod;
using UnityEngine;
using L2Word;
using L2Menu;
using L2Hit;

#pragma warning disable 0649, 0414, 0108, 0626
namespace LM2RandomiserMod.Patches
{
	[MonoModPatch("L2STATUS.Status")]
	public class patched_Status : L2STATUS.Status
	{
		[MonoModIgnore]
		private StatusBarIF statusbar;

		[MonoModIgnore]
		private int player_level;

		[MonoModIgnore]
		private int HPTANK;

		[MonoModReplace]
		public void resetPlayerStatus(int lv, int hp, int mcoin, int coin, int wait, int exp, MAINWEAPON now_wea, int now_wea_num, SUBWEAPON now_sub, int now_sub_num, USEITEM now_use, int now_use_num)
		{
			sys.setFlagData(2, 62, 0);
			clearItemsNum();
			string weaponName = string.Empty;
			if (now_wea != MAINWEAPON.NON)
			{
				weaponName = sys.exchengeMainWeaponEnumToName(now_wea);
			}
			else if (now_sub != SUBWEAPON.NON)
			{
				weaponName = sys.exchengeSubWeaponEnumToName(now_sub);
			}

			if (!string.IsNullOrEmpty(weaponName))
			{
				sys.setItem(weaponName, 1, false, false, true);
				sys.equipItem(weaponName, true);
			}

			player_level = lv;
			if (player_level < 1)
			{
				player_level = 1;
			}
			if (hp < 1)
			{
				l2_hp = HPTANK * lv;
			}
			else
			{
				l2_hp = hp;
			}
			max_hp = HPTANK * lv;
			max_coin = mcoin;
			l2_coin = coin;
			setMainWeapon(now_wea);
			setSubWeapon(now_sub);
			setUseItem(now_use);
			statusbar.setInitHP(l2_hp / 100, (float)(max_hp / 100));
			l2Exp = exp;
			statusbar.setExp(exp);
			if (max_coin < 1000)
			{
				statusbar.setCoin(l2_coin, 3);
				statusbar.changeCoinMax(999);
			}
			else
			{
				statusbar.setCoin(l2_coin, 4);
				statusbar.changeCoinMax(2000);
			}
			l2_wait = wait;
			statusbar.setWait(wait);
			statusbar.setMain(l2_eq_main, l2_main[(int)now_wea]);
			statusbar.setSub(l2_eq_sub, l2_sub[(int)now_sub]);
			statusbar.setUse(l2_eq_use, l2_use[(int)now_use]);
		}

		[MonoModReplace]
		public MAINWEAPON changeMainWeapon(int slide_vector)
		{
			MAINWEAPON mainweapon = getMainWeapon();
			for(int i = 0; i < 5; i++)
			{
				if (slide_vector == 1)
				{
					switch (mainweapon)
					{
						case MAINWEAPON.LWHIP:
							mainweapon = MAINWEAPON.KNIFE;
							break;
						case MAINWEAPON.MWIHP:
							mainweapon = MAINWEAPON.KNIFE;
							break;
						case MAINWEAPON.HWHIP:
							mainweapon = MAINWEAPON.KNIFE;
							break;
						case MAINWEAPON.KNIFE:
							mainweapon = MAINWEAPON.RAPIER;
							break;
						case MAINWEAPON.RAPIER:
							mainweapon = MAINWEAPON.AXE;
							break;
						case MAINWEAPON.AXE:
							mainweapon = MAINWEAPON.SWORD;
							break;
						case MAINWEAPON.SWORD:
						case MAINWEAPON.NON:
							if (isMainWeapon(MAINWEAPON.HWHIP))
							{
								mainweapon = MAINWEAPON.HWHIP;
							}
							else if (isMainWeapon(MAINWEAPON.MWIHP))
							{
								mainweapon = MAINWEAPON.MWIHP;
							}
							else
							{
								mainweapon = MAINWEAPON.LWHIP;
							}
							break;
					}
				}
				else
				{
					switch (mainweapon)
					{
						case MAINWEAPON.LWHIP:
						case MAINWEAPON.MWIHP:
						case MAINWEAPON.HWHIP:
						case MAINWEAPON.NON:
							mainweapon = MAINWEAPON.SWORD;
							break;
						case MAINWEAPON.KNIFE:
							if (isMainWeapon(MAINWEAPON.HWHIP))
							{
								mainweapon = MAINWEAPON.HWHIP;
							}
							else if (isMainWeapon(MAINWEAPON.MWIHP))
							{
								mainweapon = MAINWEAPON.MWIHP;
							}
							else
							{
								mainweapon = MAINWEAPON.LWHIP;
							}
							break;
						case MAINWEAPON.RAPIER:
							mainweapon = MAINWEAPON.KNIFE;
							break;
						case MAINWEAPON.AXE:
							mainweapon = MAINWEAPON.RAPIER;
							break;
						case MAINWEAPON.SWORD:
							mainweapon = MAINWEAPON.AXE;
							break;
					}
				}

				if (isMainWeapon(mainweapon))
					return mainweapon;
			}

			return MAINWEAPON.NON;
		}
	}
}
