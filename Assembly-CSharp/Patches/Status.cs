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
			this.clearItemsNum();
			string weaponName = string.Empty;
			if (now_wea != MAINWEAPON.NON)
			{
				weaponName = sys.exchengeMainWeaponEnumToName(now_wea);
				haveMainWeapon(now_wea, true);
			}
			else if (now_sub != SUBWEAPON.NON)
			{
				weaponName = sys.exchengeSubWeaponEnumToName(now_sub);
				sys.haveSubWeapon(now_sub, true, true);
			}
			sys.setItem(weaponName, 1, false, false, true);
			sys.equipItem(weaponName, true);

			this.player_level = lv;
			if (this.player_level < 1)
			{
				this.player_level = 1;
			}
			if (hp < 1)
			{
				this.l2_hp = this.HPTANK * lv;
			}
			else
			{
				this.l2_hp = hp;
			}
			this.max_hp = this.HPTANK * lv;
			this.max_coin = mcoin;
			this.l2_coin = coin;
			this.setMainWeapon(now_wea);
			this.setSubWeapon(now_sub);
			this.setUseItem(now_use);
			this.statusbar.setInitHP(this.l2_hp / 100, (float)(this.max_hp / 100));
			this.l2Exp = exp;
			this.statusbar.setExp(exp);
			if (this.max_coin < 1000)
			{
				this.statusbar.setCoin(this.l2_coin, 3);
				this.statusbar.changeCoinMax(999);
			}
			else
			{
				this.statusbar.setCoin(this.l2_coin, 4);
				this.statusbar.changeCoinMax(2000);
			}
			this.l2_wait = wait;
			this.statusbar.setWait(wait);
			this.statusbar.setMain(this.l2_eq_main, this.l2_main[(int)now_wea]);
			this.statusbar.setSub(this.l2_eq_sub, this.l2_sub[(int)now_sub]);
			this.statusbar.setUse(this.l2_eq_use, this.l2_use[(int)now_use]);
		}

		[MonoModReplace]
		public MAINWEAPON changeMainWeapon(int slide_vector)
		{
			int num = 0;
			MAINWEAPON mainweapon = this.getMainWeapon();
			for (;;)
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
							if (this.isMainWeapon(MAINWEAPON.HWHIP))
							{
								mainweapon = MAINWEAPON.HWHIP;
							}
							else if (this.isMainWeapon(MAINWEAPON.MWIHP))
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
							mainweapon = MAINWEAPON.SWORD;
							break;
						case MAINWEAPON.MWIHP:
							mainweapon = MAINWEAPON.SWORD;
							break;
						case MAINWEAPON.HWHIP:
							mainweapon = MAINWEAPON.SWORD;
							break;
						case MAINWEAPON.KNIFE:
							if (this.isMainWeapon(MAINWEAPON.HWHIP))
							{
								mainweapon = MAINWEAPON.HWHIP;
							}
							else if (this.isMainWeapon(MAINWEAPON.MWIHP))
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
					break;

				num++;
				if (num > 4)
				{
					mainweapon = MAINWEAPON.NON;
					return mainweapon;
				}
			}

			return mainweapon;
		}
	}
}
