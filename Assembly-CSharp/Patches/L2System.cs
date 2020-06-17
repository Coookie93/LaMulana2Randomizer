using MonoMod;
using UnityEngine;
using L2Word;
using L2STATUS;
using L2Menu;
using L2Hit;

#pragma warning disable 0649, 0414, 0108, 0626
namespace LM2RandomiserMod.Patches
{
    [MonoModPatch("L2Base.L2System")]
    public class patched_L2System : L2Base.L2System
    {
        [MonoModIgnore]
        private L2ShopDataBase l2sdb;

        [MonoModIgnore]
        private L2TalkDataBase l2tdb;

        //Create and Initialise the randomiser
        private void orig_Start() { }
        private void Start()
        {
            orig_Start();
            GameObject obj = new GameObject();
            L2Rando component = obj.AddComponent<L2Rando>() as L2Rando;
            component.Initialise(this.l2sdb, this.l2tdb, this);
            DontDestroyOnLoad(obj);
        }

        [MonoModReplace]
		public void setItem(string item_name, int num, bool direct = false, bool loadcall = false, bool sub_add = true)
		{
			int num2 = this.SeetNametoNo("02Items");
			int num3 = this.SeetNametoNo("00system");
			if (num2 < 0)
			{
				return;
			}

			if (item_name.Contains("Mantra") && !item_name.Equals("Mantra"))
			{
				this.setFlagData(num2, item_name, (short)num);
				return;
			}

			if (item_name == "Whip")
			{
				if (num == 1)
				{
					this.playerst.setMainWeaponNum(MAINWEAPON.MWIHP, 0);
					this.haveMainWeapon(MAINWEAPON.MWIHP, false);
					this.playerst.setMainWeaponNum(MAINWEAPON.HWHIP, 0);
					this.haveMainWeapon(MAINWEAPON.HWHIP, false);
				}
				else if (num == 2)
				{
					item_name = "Whip2";
					num = 1;
					this.playerst.setMainWeaponNum(MAINWEAPON.HWHIP, 0);
					this.haveMainWeapon(MAINWEAPON.HWHIP, false);
				}
				else if (num == 3)
				{
					item_name = "Whip3";
					num = 1;
				}
			}
			if (num == 0)
			{
				return;
			}
			if (item_name == "Weight")
			{
				if (direct)
				{
					this.playerst.setWait(num);
				}
				else
				{
					this.playerst.addWait(num);
				}
				return;
			}
			if (item_name == "Gold Bangle")
			{
				this.playerst.setMaxCoin(2000);
				this.playerst.setCoin(this.playerst.getCoin());
			}
			if (item_name == "Gold")
			{
				if (direct)
				{
					this.playerst.setCoin(num);
				}
				else
				{
					this.playerst.addCoin(num);
				}
				return;
			}
			if (item_name == "Soul")
			{
				if (direct)
				{
					this.playerst.setExp(num);
				}
				else
				{
					this.playerst.addExp(num);
				}
			}
			else if (item_name == "Sacred Orb")
			{
				if (direct)
				{
					this.playerst.setPLayerLevel(num);
				}
				else
				{
					this.playerst.setPLayerLevel(this.playerst.getPlayerLevel() + 1);
				}
			}
			else
			{
				if (this.isAnkJewel(item_name))
				{
					this.setFlagData(num2, item_name, (short)num);
					short num4 = 0;
					if (!direct)
					{
						this.getFlag(num3, "A_Jewel", ref num4);
						num += (int)num4;
					}
					this.setFlagData(num3, "A_Jewel", (short)num);
					item_name = "A_Jewel";
					goto IL_3BF;
				}
				if (item_name == "Ankh Jewel")
				{
					short num5 = 0;
					if (!direct)
					{
						this.getFlag(num3, "A_Jewel", ref num5);
						num += (int)num5;
					}
					this.setFlagData(num3, "A_Jewel", (short)num);
					goto IL_3BF;
				}
				if (item_name == "A_Jewel")
				{
					short num6 = 0;
					if (!direct)
					{
						this.getFlag(num3, "A_Jewel", ref num6);
						num += (int)num6;
					}
					this.setFlagData(num3, "A_Jewel", (short)num);
					goto IL_3BF;
				}
				if (item_name == "pistolBox")
				{
					goto IL_3BF;
				}
				if (item_name == "Pepper")
				{
					if (!direct)
					{
						this.setFlagData(num3, "Pepper-b", 69);
					}
				}
				else if (L2SystemCore.getItemData(item_name).isEquipableItem())
				{
					if (!L2SystemCore.getItemData(item_name).isSoftWare())
					{
						if (L2SystemCore.getItemData(item_name).isForceEquipItem())
						{
							if (!direct)
							{
								this.equipItem(item_name, true);
							}
							if (this.getPlayer() != null)
							{
								this.getPlayer().checkEquipItem();
							}
						}
					}
					else if (item_name == "Xelputter" && !loadcall)
					{
						for (int i = 0; i < 1; i++)
						{
							int[] softLiveData = this.getSoftLiveData(i);
							for (int j = 0; j < softLiveData.Length; j++)
							{
								if (softLiveData[j] == -1)
								{
									softLiveData[j] = 0;
									this.setSoftLive(i, ItemDatabaseSystem.ItemNames.Xelputter, true);
									break;
								}
							}
						}
					}
				}
			}
			if (direct)
			{
				if (item_name == "Shield1")
				{
					this.setFlagData(num2, "Shield", 1);
				}
				else if (item_name == "Shield2")
				{
					this.setFlagData(num2, "Shield", 2);
				}
				else if (item_name == "Shield3")
				{
					this.setFlagData(num2, "Shield", 3);
				}
				else
				{
					this.setFlagData(num2, item_name, (short)num);
				}
			}
			else if (item_name == "Shield")
			{
				this.setFlagData(num2, "Shield", 1);
			}
			else if (item_name == "Shield2")
			{
				this.setFlagData(num2, "Shield", 2);
			}
			else if (item_name == "Shield3")
			{
				this.setFlagData(num2, "Shield", 3);
			}
			else
			{

				this.setFlagData(num2, item_name, (short)num);//(short)(this.getItemNum(item_name) + 
			}
			IL_3BF:
			MAINWEAPON mainweapon = this.exchengeMainWeaponNameToEnum(item_name);
			if (mainweapon != MAINWEAPON.NON)
			{
				this.haveMainWeapon(mainweapon, true);
				if (direct)
				{
					if (mainweapon == MAINWEAPON.LWHIP)
					{
						this.setFlagData(num2, "Whip", (short)num);
						this.playerst.setMainWeaponNum(mainweapon, num);
					}
					else if (mainweapon == MAINWEAPON.MWIHP)
					{
						this.setFlagData(num2, "Whip", 2);
						this.playerst.setMainWeaponNum(mainweapon, num);
					}
					else if (mainweapon == MAINWEAPON.HWHIP)
					{
						this.setFlagData(num2, "Whip", 3);
						this.playerst.setMainWeaponNum(mainweapon, num);
					}
				}
				else
				{
					if (mainweapon == MAINWEAPON.LWHIP)
					{
						this.setFlagData(num2, "Whip", (short)num);
					}
					else if (mainweapon == MAINWEAPON.MWIHP)
					{
						this.setFlagData(num2, "Whip", 2);
					}
					else if (mainweapon == MAINWEAPON.HWHIP)
					{
						this.setFlagData(num2, "Whip", 3);
					}
					this.addMainWeaponNum(mainweapon, 1);
				}
				return;
			}
			SUBWEAPON subweapon = this.exchengeSubWeaponNameToEnum(item_name);
			if (subweapon != SUBWEAPON.NON)
			{
				if (subweapon > SUBWEAPON.SUB_ANKJEWEL && subweapon != SUBWEAPON.SUB_REGUN)
				{
					if (direct)
					{
						this.playerst.setSubWeaponNum(subweapon, num, false);
					}
					else
					{
						this.addSubWeaponNum(subweapon, num);
					}
				}
				else if (loadcall)
				{
					if (subweapon == SUBWEAPON.SUB_SHIELD1)
					{
						if (num == 2)
						{
							subweapon = SUBWEAPON.SUB_SHIELD2;
						}
						else if (num == 3)
						{
							subweapon = SUBWEAPON.SUB_SHIELD3;
						}
					}
					this.haveSubWeapon(subweapon, true, false);
				}
				else
				{
					this.haveSubWeapon(subweapon, true, sub_add);
					if (subweapon == SUBWEAPON.SUB_REGUN)
					{
						this.playerst.addSubWeaponNum(subweapon, num);
					}
				}
				return;
			}
			USEITEM useitem = this.exchengeUseItemNameToEnum(item_name);
			if (useitem != USEITEM.NON)
			{
				this.haveUsesItem(useitem, true);
				if (direct)
				{
					this.playerst.setUseItemNum(useitem, num);
					if (useitem == USEITEM.USE_PEPPER_B)
					{
						this.setFlagData(num3, "Pepper-b", (short)num);
					}
					else if (useitem == USEITEM.USE_CRYSTAL_S_B)
					{
						this.setFlagData(num3, "Crystal S-b", (short)num);
					}
				}
				else
				{
					this.addUseItemNum(useitem, 1);
					if (useitem == USEITEM.USE_PEPPER_B)
					{
						this.setFlagData(num3, "Pepper-b", (short)this.playerst.getUseItemNum(useitem));
					}
					else if (useitem == USEITEM.USE_CRYSTAL_S_B)
					{
						this.setFlagData(num3, "Crystal S-b", (short)this.playerst.getUseItemNum(useitem));
					}
				}
				return;
			}
		}

		[MonoModReplace]
        public int isHaveItem(string name)
        {
            short num = 0;
            int seet = this.SeetNametoNo("02Items");
            if (name == "A_Jewel" || name == "Ankh Jewel")
            {
                this.getFlag(this.SeetNametoNo("00system"), "A_Jewel", ref num);
            }
            else if (name == "Whip2")
            {
                this.getFlag(seet, "Whip", ref num);
                if (num >= 2)
                {
                    num = 1;
                }
                else
                {
                    num = 0;
                }
            }
            else if (name == "Whip3")
            {
                this.getFlag(seet, "Whip", ref num);
                if (num == 3)
                {
                    num = 1;
                }
                else
                {
                    num = 0;
                }
            }
            else if (name == "Shield2")
            {
                this.getFlag(seet, "Shield", ref num);
                if (num >= 2)
                {
                    num = 1;
                }
                else
                {
                    num = 0;
                }
            }
            else if (name == "Shield3")
            {
                this.getFlag(seet, "Shield", ref num);
                if (num == 3)
                {
                    num = 1;
                }
                else
                {
                    num = 0;
                }
            }
            else
            {
                this.getFlag(seet, name, ref num);
            }
            return (int)num;
        }

        [MonoModIgnore]
        private Status playerst;

        [MonoModIgnore]
        private MenuSystem menusys;

        [MonoModReplace]
        public void gameFlagResets()
        {
			ItemTracker itemTracker = FindObjectOfType<ItemTracker>();
			if (itemTracker != null)
				itemTracker.Add(102, 0);

			this.fsys.allReset();
			this.playerst.clearItemsNum();
			this.menusys.menuSysReStart();
			this.setSystemDataToClothFlag();

			L2Rando l2Rando = FindObjectOfType<L2Rando>();
			if (l2Rando != null && l2Rando.IsRandomising)
			{
				if (l2Rando.MoneyStart)
					Init_Coin_num = 100;

				if (l2Rando.WeightStart)
					Init_Weight_num = 20;

				if(l2Rando.RemoveITStatue)
					this.setFlagData(8, 10, 1);
			}

			this.playerst.addCoin(Init_Coin_num);
			this.playerst.addWait(Init_Weight_num);
            this.playerst.resetPlayerStatus(this.Init_PLayer_lv, 0, 999, this.Init_Coin_num, this.Init_Weight_num, 0, MAINWEAPON.KNIFE, 0, SUBWEAPON.NON, 0, USEITEM.NON, 0);
            this.playerst.resetExp();
            this.setFlagData(0, 42, 1);
			this.setFlagData(4, 60, 4);
			this.setFlagData(4, 62, 2);
		}

		public extern void orig_loadInitFlagToItem();

		public void loadInitFlagToItem()
		{
			ItemTracker itemTracker = FindObjectOfType<ItemTracker>();
			if (itemTracker != null)
				itemTracker.Add(100, 0);

			orig_loadInitFlagToItem();

			if(itemTracker != null)
			{
				itemTracker.Add(2, 152);
				itemTracker.Add(2, 153);
				itemTracker.Add(2, 154);
				itemTracker.Add(2, 155);
				itemTracker.Add(2, 156);
				itemTracker.Add(2, 157);
				itemTracker.Add(2, 158);
				itemTracker.Add(2, 159);
				itemTracker.Add(2, 160);
				itemTracker.Add(2, 161);
				itemTracker.Add(3, 10);
				itemTracker.Add(3, 11);
				itemTracker.Add(3, 12);
				itemTracker.Add(3, 13);
				itemTracker.Add(3, 14);
				itemTracker.Add(3, 15);
				itemTracker.Add(3, 16);
				itemTracker.Add(3, 17);
				itemTracker.Add(3, 18);
				itemTracker.Add(101, 0);
			}
		}

#if DEV
		[MonoModIgnore]
        private TextMesh boss_hp_text;

        [MonoModIgnore]
        private TextMesh boss_dmg_text;

        [MonoModReplace]
        public void debugDrawBossHPText(int hp)
        {
            this.boss_hp_text.color = Color.magenta;
            this.boss_hp_text.text = hp.ToString();
        }

        [MonoModReplace]
        public void debugDrawBossDmgText(int dmg)
        {
            this.boss_dmg_text.color = Color.magenta;
            this.boss_dmg_text.text = dmg.ToString();
        }
#endif
    }
}
