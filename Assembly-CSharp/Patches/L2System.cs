using MonoMod;
using UnityEngine;
using L2Word;
using L2STATUS;
using L2Menu;
using L2Hit;
using L2Base;
using LaMulana2RandomizerShared;

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
            component.Initialise(l2sdb, l2tdb, this);
            DontDestroyOnLoad(obj);
        }

        [MonoModReplace]
		public void setItem(string item_name, int num, bool direct = false, bool loadcall = false, bool sub_add = true)
		{
			int value = num;
			int num2 = SeetNametoNo("02Items");
			int num3 = SeetNametoNo("00system");
			if (num2 < 0)
				return;

			if (!direct)
			{
				if (item_name.Contains("Whip"))
				{
					if (item_name == "Whip1") setFlagData(num2, 190, 1);
					else if (item_name == "Whip2") setFlagData(num2, 191, 1);
					else if (item_name == "Whip3") setFlagData(num2, 192, 1);

					short data = 0;
					getFlag(2, "Whip", ref data);
					value = data + 1;
				}
				else if (item_name.Contains("Shield"))
				{
					if (item_name == "Shield1") setFlagData(num2, 193, 1);
					else if (item_name == "Shield2") setFlagData(num2, 194, 1);
					else if (item_name == "Shield3") setFlagData(num2, 195, 1);

					short data = 0;
					getFlag(2, 196, ref data);
					value = data + 1;
					setFlagData(num2, 196, (short)(data + 1));
				}
				else if (item_name.Contains("Research"))
				{
					switch (item_name)
					{
						case "Research1": setFlagData(num2, 180, 1); break;
						case "Research2": setFlagData(num2, 181, 1); break;
						case "Research3": setFlagData(num2, 182, 1); break;
						case "Research4": setFlagData(num2, 183, 1); break;
						case "Research5": setFlagData(num2, 184, 1); break;
						case "Research6": setFlagData(num2, 185, 1); break;
						case "Research7": setFlagData(num2, 186, 1); break;
						case "Research8": setFlagData(num2, 187, 1); break;
						case "Research9": setFlagData(num2, 188, 1); break;
						case "Research10": setFlagData(num2, 189, 1); break;
						default: break;
					}

					item_name = "Research";
				}
				else if (item_name.Contains("Beherit"))
				{
					switch (item_name)
					{
						case "Beherit1": setFlagData(num2, 170, 1); break;
						case "Beherit2": setFlagData(num2, 171, 1); break;
						case "Beherit3": setFlagData(num2, 172, 1); break;
						case "Beherit4": setFlagData(num2, 173, 1); break;
						case "Beherit5": setFlagData(num2, 174, 1); break;
						case "Beherit6": setFlagData(num2, 175, 1); break;
						case "Beherit7": setFlagData(num2, 176, 1); break;
						default: break;
					}

					USEITEM item = exchengeUseItemNameToEnum("Beherit");
					haveUsesItem(item, true);
					addUseItemNum(item, 1);
					return;
				}
				else if (item_name.Contains("Mantra") && !item_name.Equals("Mantra"))
				{
					setFlagData(num2, item_name, (short)value);
					return;
				}

				if (item_name.Equals("Lamp"))
				{
					value = 2;
				}
			}

			if (item_name.Contains("Whip"))
			{
				if (value == 1)
				{
					item_name = "Whip";
					playerst.setMainWeaponNum(MAINWEAPON.MWIHP, 0);
					haveMainWeapon(MAINWEAPON.MWIHP, false);
					playerst.setMainWeaponNum(MAINWEAPON.HWHIP, 0);
					haveMainWeapon(MAINWEAPON.HWHIP, false);
				}
				else if (value == 2)
				{
					item_name = "Whip2";
					value = 1;
					playerst.setMainWeaponNum(MAINWEAPON.HWHIP, 0);
					haveMainWeapon(MAINWEAPON.HWHIP, false);
				}
				else if (value == 3)
				{
					item_name = "Whip3";
					value = 1;
				}
			}
			else if (item_name.Contains("Shield"))
			{
				if (value == 1) item_name = "Shield";
				else if (value == 2) item_name = "Shield2";
				else if (value == 3) item_name = "Shield3";
				setFlagData(num2, "Shield", (short)value);
			}

			if (value == 0)
				return;

			if (item_name == "Weight")
			{
				if (direct)
					playerst.setWait(value);
				else
					playerst.addWait(value);
				return;
			}
			else if (item_name == "Gold Bangle")
			{
				playerst.setMaxCoin(2000);
				playerst.setCoin(playerst.getCoin());
			}
			else if (item_name == "Gold")
			{
				if (direct)
					playerst.setCoin(value);
				else
					playerst.addCoin(value);
				return;
			}
			else if (item_name == "Soul")
			{
				if (direct)
					playerst.setExp(value);
				else
					playerst.addExp(value);
			}
			else if (item_name == "Sacred Orb")
			{
				if (direct)
					playerst.setPLayerLevel(value);
				else
					playerst.setPLayerLevel(playerst.getPlayerLevel() + 1);
			}
			else
			{
				if (isAnkJewel(item_name))
				{
					setFlagData(num2, item_name, (short)value);
					short num4 = 0;
					if (!direct)
					{
						getFlag(num3, "A_Jewel", ref num4);
						value += (int)num4;
					}
					setFlagData(num3, "A_Jewel", (short)value);
					item_name = "A_Jewel";
					goto IL_3BF;
				}

				if (item_name == "Ankh Jewel")
				{
					short num5 = 0;
					if (!direct)
					{
						getFlag(num3, "A_Jewel", ref num5);
						value += (int)num5;
					}
					setFlagData(num3, "A_Jewel", (short)value);
					goto IL_3BF;
				}

				if (item_name == "A_Jewel")
				{
					short num6 = 0;
					if (!direct)
					{
						getFlag(num3, "A_Jewel", ref num6);
						value += (int)num6;
					}
					setFlagData(num3, "A_Jewel", (short)value);
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
						setFlagData(num3, "Pepper-b", 69);
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
								equipItem(item_name, true);
							}
							if (getPlayer() != null)
							{
								getPlayer().checkEquipItem();
							}
						}
					}
					else if (item_name == "Xelputter" && !loadcall)
					{
						for (int i = 0; i < 1; i++)
						{
							int[] softLiveData = getSoftLiveData(i);
							for (int j = 0; j < softLiveData.Length; j++)
							{
								if (softLiveData[j] == -1)
								{
									softLiveData[j] = 0;
									setSoftLive(i, ItemDatabaseSystem.ItemNames.Xelputter, true);
									break;
								}
							}
						}
					}
				}
			}
			if (direct)
			{
				if (item_name == "Shield")
				{
					if (value == 1) item_name = "Shield";
					else if (value == 2) item_name = "Shield2";
					else if (value == 3) item_name = "Shield3";
					setFlagData(num2, "Shield", (short)value);
				}
				else
				{
					setFlagData(num2, item_name, (short)value);
				}
			}
			else
			{
				setFlagData(num2, item_name, (short)value);
			}
			IL_3BF:
			MAINWEAPON mainweapon = exchengeMainWeaponNameToEnum(item_name);
			if (mainweapon != MAINWEAPON.NON)
			{
				haveMainWeapon(mainweapon, true);
				if (direct)
				{
					if (mainweapon == MAINWEAPON.LWHIP)
					{
						setFlagData(num2, "Whip", 1);
						playerst.setMainWeaponNum(mainweapon, value);
					}
					else if (mainweapon == MAINWEAPON.MWIHP)
					{
						setFlagData(num2, "Whip", 2);
						playerst.setMainWeaponNum(mainweapon, value);
					}
					else if (mainweapon == MAINWEAPON.HWHIP)
					{
						setFlagData(num2, "Whip", 3);
						playerst.setMainWeaponNum(mainweapon, value);
					}
				}
				else
				{
					if (mainweapon == MAINWEAPON.LWHIP)
					{
						setFlagData(num2, "Whip", 1);
					}
					else if (mainweapon == MAINWEAPON.MWIHP)
					{
						setFlagData(num2, "Whip", 2);
					}
					else if (mainweapon == MAINWEAPON.HWHIP)
					{
						setFlagData(num2, "Whip", 3);
					}
					addMainWeaponNum(mainweapon, 1);
				}
				return;
			}
			SUBWEAPON subweapon = exchengeSubWeaponNameToEnum(item_name);
			if (subweapon != SUBWEAPON.NON)
			{
				if (subweapon > SUBWEAPON.SUB_ANKJEWEL && subweapon != SUBWEAPON.SUB_REGUN)
				{
					if (direct)
					{
						playerst.setSubWeaponNum(subweapon, value, false);
					}
					else
					{
						addSubWeaponNum(subweapon, value);
					}
				}
				else if (loadcall)
				{
					if (subweapon == SUBWEAPON.SUB_SHIELD1)
					{
						if (value == 2)
						{
							subweapon = SUBWEAPON.SUB_SHIELD2;
						}
						else if (value == 3)
						{
							subweapon = SUBWEAPON.SUB_SHIELD3;
						}
					}
					haveSubWeapon(subweapon, true, false);
				}
				else
				{
					haveSubWeapon(subweapon, true, sub_add);
					if (subweapon == SUBWEAPON.SUB_REGUN)
					{
						playerst.addSubWeaponNum(subweapon, value);
					}
				}
				return;
			}
			USEITEM useitem = exchengeUseItemNameToEnum(item_name);
			if (useitem != USEITEM.NON)
			{
				haveUsesItem(useitem, true);
				if (direct)
				{
					playerst.setUseItemNum(useitem, value);
					if (useitem == USEITEM.USE_PEPPER_B)
					{
						setFlagData(num3, "Pepper-b", (short)value);
					}
					else if (useitem == USEITEM.USE_CRYSTAL_S_B)
					{
						setFlagData(num3, "Crystal S-b", (short)value);
					}
				}
				else
				{
					addUseItemNum(useitem, 1);
					if (useitem == USEITEM.USE_PEPPER_B)
					{
						setFlagData(num3, "Pepper-b", (short)playerst.getUseItemNum(useitem));
					}
					else if (useitem == USEITEM.USE_CRYSTAL_S_B)
					{
						setFlagData(num3, "Crystal S-b", (short)playerst.getUseItemNum(useitem));
					}
				}
				return;
			}
		}

		[MonoModReplace]
        public int isHaveItem(string name)
        {
            short num = 0;
            int seet = SeetNametoNo("02Items");

			switch (name)
			{
				case "Beherit1": getFlag(seet, 170, ref num); break;
				case "Beherit2": getFlag(seet, 171, ref num); break;
				case "Beherit3": getFlag(seet, 172, ref num); break;
				case "Beherit4": getFlag(seet, 173, ref num); break;
				case "Beherit5": getFlag(seet, 174, ref num); break;
				case "Beherit6": getFlag(seet, 175, ref num); break;
				case "Beherit7": getFlag(seet, 176, ref num); break;
				case "Research1": getFlag(seet, 180, ref num); break;
				case "Research2": getFlag(seet, 181, ref num); break;
				case "Research3": getFlag(seet, 182, ref num); break;
				case "Research4": getFlag(seet, 183, ref num); break;
				case "Research5": getFlag(seet, 184, ref num); break;
				case "Research6": getFlag(seet, 185, ref num); break;
				case "Research7": getFlag(seet, 186, ref num); break;
				case "Research8": getFlag(seet, 187, ref num); break;
				case "Research9": getFlag(seet, 188, ref num); break;
				case "Research10": getFlag(seet, 189, ref num); break;
				case "Whip1": getFlag(seet, 190, ref num); break;
				case "Whip2": getFlag(seet, 191, ref num); break;
				case "Whip3": getFlag(seet, 192, ref num); break;
				case "Shield1": getFlag(seet, 193, ref num); break;
				case "Shield2": getFlag(seet, 194, ref num); break;
				case "Shield3": getFlag(seet, 195, ref num); break;
				default:
				{
					if (name == "A_Jewel" || name == "Ankh Jewel")
					{
						getFlag(SeetNametoNo("00system"), "A_Jewel", ref num);
					}
					else
					{
						getFlag(seet, name, ref num);
					}
					break;
				}
			}
            return num;
        }

        [MonoModIgnore]
        private Status playerst;

		public int getItemNum(string item_name)
		{
			short num = 0;
			int num2 = SeetNametoNo("02Items");
			if (num2 < 0)
				return -1;

			switch (item_name)
			{
				case "Weight": return playerst.getWait();
				case "Money": return playerst.getCoin();
				case "Beherit1": getFlag(num2, 170, ref num); return num;
				case "Beherit2": getFlag(num2, 171, ref num); return num;
				case "Beherit3": getFlag(num2, 172, ref num); return num;
				case "Beherit4": getFlag(num2, 173, ref num); return num;
				case "Beherit5": getFlag(num2, 174, ref num); return num;
				case "Beherit6": getFlag(num2, 175, ref num); return num;
				case "Beherit7": getFlag(num2, 176, ref num); return num;
				case "Research1": getFlag(num2, 180, ref num); return num;
				case "Research2": getFlag(num2, 181, ref num); return num;
				case "Research3": getFlag(num2, 182, ref num); return num;
				case "Research4": getFlag(num2, 183, ref num); return num;
				case "Research5": getFlag(num2, 184, ref num); return num;
				case "Research6": getFlag(num2, 185, ref num); return num;
				case "Research7": getFlag(num2, 186, ref num); return num;
				case "Research8": getFlag(num2, 187, ref num); return num;
				case "Research9": getFlag(num2, 188, ref num); return num;
				case "Research10": getFlag(num2, 189, ref num); return num;
				case "Whip1": getFlag(num2, 190, ref num); return num;
				case "Whip2": getFlag(num2, 191, ref num); return num;
				case "Whip3": getFlag(num2, 192, ref num); return num;
				case "Shield1": getFlag(num2, 193, ref num); return num;
				case "Shield2": getFlag(num2, 194, ref num); return num;
				case "Shield3": getFlag(num2, 195, ref num); return num;
				case "MSX": getFlag(num2, "MSX", ref num); return num == 2 ? 1 : 0;
				default:
				{
					if (item_name == "Weight")
					{
						return playerst.getWait();
					}
					if (item_name == "Gold")
					{
						return playerst.getCoin();
					}
					if (item_name == "A_Jewel" || item_name == "Ankh Jewel")
					{
						getFlag(SeetNametoNo("00system"), "A_Jewel", ref num);
						return num;
					}
					if (!getFlag(num2, item_name, ref num))
					{
						num = -1;
					}
					MAINWEAPON mainweapon = exchengeMainWeaponNameToEnum(item_name);
					if (mainweapon != MAINWEAPON.NON)
					{
						getFlag(num2, item_name, ref num);
						return num > 0 ? 1 : 0;
					}
					SUBWEAPON subweapon = exchengeSubWeaponNameToEnum(item_name);
					if (subweapon != SUBWEAPON.NON && subweapon > SUBWEAPON.SUB_ANKJEWEL)
					{
						num = (short)getSubWeaponNum(subweapon);
					}
					USEITEM useitem = exchengeUseItemNameToEnum(item_name);
					if (useitem != USEITEM.NON)
					{
						num = (short)getUseItemNum(useitem);
					}
					return num;
				}
			}
		}

        [MonoModIgnore]
        private MenuSystem menusys;

        [MonoModReplace]
        public void gameFlagResets()
        {
			ItemTracker itemTracker = FindObjectOfType<ItemTracker>();
			if (itemTracker != null)
				itemTracker.Add(102, 0);

			fsys.allReset();
			playerst.clearItemsNum();
			menusys.menuSysReStart();
			setSystemDataToClothFlag();

			MAINWEAPON mainWeapon = MAINWEAPON.NON;
			SUBWEAPON subWeapon = SUBWEAPON.NON;

			L2Rando l2Rando = FindObjectOfType<L2Rando>();
			if (l2Rando != null && l2Rando.IsRandomising)
			{
				Init_Coin_num = l2Rando.StartingMoney;
				Init_Weight_num = l2Rando.StartingWeights;

				ItemInfo itemInfo = ItemDB.GetItemInfo(l2Rando.StartingWeapon);
				if (itemInfo != null)
				{
					if (l2Rando.StartingWeapon == ItemID.Whip1 || l2Rando.StartingWeapon == ItemID.Whip2 || l2Rando.StartingWeapon == ItemID.Whip3)
					{
						setFlagData(itemInfo.ItemSheet, itemInfo.ItemFlag, 1);
						mainWeapon = exchengeMainWeaponNameToEnum("Whip");
					}
					else if (l2Rando.StartingWeapon == ItemID.ClaydollSuit)
					{
						setItem(itemInfo.BoxName, 1, false, false, true);
						setEffectFlag(l2Rando.CreateGetFlags(l2Rando.StartingWeapon, itemInfo));
					}
					else
					{
						mainWeapon = exchengeMainWeaponNameToEnum(itemInfo.BoxName);
						subWeapon = exchengeSubWeaponNameToEnum(itemInfo.BoxName);
					}
				}
				else
				{
					mainWeapon = MAINWEAPON.LWHIP;
				}

				foreach (ItemID itemID in l2Rando.StartingItems)
				{
					itemInfo = ItemDB.GetItemInfo(itemID);
					setItem(itemInfo.BoxName, 1, false, false, true);
					setEffectFlag(l2Rando.CreateGetFlags(itemID, itemInfo));
				}

				if (l2Rando.RemoveITStatue)
					setFlagData(8, 10, 1);

				setFlagData(0, 12, 0);
				setFlagData(5, 47, (short)(12 - l2Rando.RequiredSkulls));

				switch (l2Rando.StartingArea)
				{
					case AreaID.VoD: 
					{ 
						setFlagData(0, 12, 1); 
						break; 
					}
					case AreaID.RoY: 
					{ 
						setFlagData(0, 13, 1); 
						break; 
					}
					case AreaID.AnnwfnMain: 
					{ 
						setFlagData(0, 14, 1); 
						break; 
					}
					case AreaID.IBMain: 
					{ 
						setFlagData(0, 15, 1); 
						break; 
					}
					case AreaID.ITLeft: 
					{ 
						setFlagData(0, 16, 1); 
						break; 
					}
					case AreaID.DFMain: 
					{ 
						setFlagData(0, 17, 1); 
						break; 
					}
					case AreaID.SotFGGrail:
					{
						setFlagData(0, 18, 1);
						setFlagData(10, 27, 1);
						setFlagData(10, 87, 1);
						break;
					}
					case AreaID.TSLeft:
					{
						setFlagData(0, 20, 1);
						setFlagData(12, 38, 1);
						setFlagData(12, 45, 1);
						setFlagData(12, 50, 1);
						break;
					}
					case AreaID.ValhallaMain: 
					{
						setFlagData(0, 26, 1); 
						break; 
					}
					case AreaID.DSLMMain: 
					{
						setFlagData(0, 28, 1); 
						break; 
					} 
					case AreaID.ACTablet: 
					{
						setFlagData(0, 29, 1); 
						break;
					}
					case AreaID.HoMTop:
					{
						setFlagData(0, 30, 1);
						setFlagData(17, 2, 1);
						setFlagData(17, 62, 1);
						break;
					}
				}

				l2Rando.StartingGame = true;
			}

			playerst.addCoin(Init_Coin_num);
			playerst.addWait(Init_Weight_num);
            playerst.resetPlayerStatus(Init_PLayer_lv, 0, 999, Init_Coin_num, Init_Weight_num, 0, mainWeapon, 0, subWeapon, 0, USEITEM.NON, 0);
            playerst.resetExp();
            setFlagData(0, 42, 1);
			setFlagData(4, 60, 4);
			setFlagData(4, 62, 2);
		}

		[MonoModIgnore]
		private int[] Systemflagbuffer;

		[MonoModIgnore]
		private float gamespeed;

		[MonoModIgnore]
		private int RINGBUFF_MAX;

		[MonoModIgnore]
		private NewPlayer playerobject;

		[MonoModIgnore]
		private L2SystemCore l2core;

		[MonoModReplace]
		public void reInitSystem(bool callTitle = true)
		{
			fsys.allReset();
			for (int i = 0; i < 16; i++)
			{
				Systemflagbuffer[i] = 0;
			}
			setSysFlag(SYSTEMFLAG.LOADSTATUS);
			setSysFlag(SYSTEMFLAG.INISTATUS);
			setSysFlag(SYSTEMFLAG.SYSTEMINI);
			setSysFlag(SYSTEMFLAG.LIVEPLAYER);
			gamespeed = 1f;
			axisRingIndex = 0;
			for (int i = 0; i < RINGBUFF_MAX; i++)
			{
				axisRingH[i] = 0f;
				axisRingV[i] = 0f;
			}
			setSysFlag(SYSTEMFLAG.SYSTEMSTART);
			playerst.resetPlayerStatus(Init_PLayer_lv, 0, 999, Init_Coin_num, Init_Weight_num, 0, MAINWEAPON.NON, 0, SUBWEAPON.NON, 0, USEITEM.NON, 0);
			BGCharacterTraceCamera component = playerobject.GetComponent<BGCharacterTraceCamera>();
			delTask(component);
			delTask(playerobject);
			deletePlayer();
			clearSceneTasks();
			reSetAlertMessNo();
			if (callTitle)
			{
				l2core.loadDemoSceane("Title");
			}
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
            boss_hp_text.color = Color.magenta;
            boss_hp_text.text = hp.ToString();
        }

        [MonoModReplace]
        public void debugDrawBossDmgText(int dmg)
        {
            boss_dmg_text.color = Color.magenta;
            boss_dmg_text.text = dmg.ToString();
        }

#endif
	}
}
