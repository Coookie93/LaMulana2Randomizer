using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using MonoMod;
using UnityEngine;
using UnityEngine.SceneManagement;
using LM2RandomiserMod.Patches;
using L2Word;
using L2Flag;
using L2Base;
using L2Task;

namespace LM2RandomiserMod
{
    public class L2Rando : MonoBehaviour
    {
        //number of items being randomised
        private const int ITEM_COUNT = 186;

        private bool randomising = false;
        private Dictionary<int, int> locationToItemMap;

        private bool showText = false;
        private string error;

        private TreasureBoxScript[] cachedBoxes;
        private EventItemScript[] cachedItems;

        private L2ShopDataBase shopDataBase;
        private L2TalkDataBase talkDataBase;
        private L2System sys;
        
        public ItemID GetItemIDForLocation(LocationID locationID)
        {
            locationToItemMap.TryGetValue((int)locationID, out int id);
            return (ItemID)id;
        }

        public LocationID GetLocationIDForMural(SnapShotTargetScript snapTarget)
        {
            if (snapTarget.itemName == ItemDatabaseSystem.ItemNames.BeoEgLana)
            {
                return (LocationID)snapTarget.itemName;
            }
            else if (snapTarget.itemName == ItemDatabaseSystem.ItemNames.Mantra)
            {
                if (snapTarget.cellName.Equals(""))
                {
                    return (LocationID)snapTarget.itemName;
                }
                else
                {
                    try
                    {
                        return (LocationID)Enum.Parse(typeof(LocationID), snapTarget.cellName, true);
                    }
                    catch
                    {
                        return LocationID.None;
                    }
                }
            }

            return LocationID.None;
        }

        public L2FlagBoxEnd[] CreateGetFlags(ItemID itemID, ItemInfo itemInfo)
        {
            ItemID[] storyItems = {ItemID.DjedPillar,  ItemID.Mjolnir, ItemID.AncientBattery, ItemID.LampofTime, ItemID.PochetteKey,
                ItemID.PyramidCrystal, ItemID.Vessel, ItemID.EggofCreation, ItemID.GiantsFlute, ItemID.CogofAntiquity, ItemID.MulanaTalisman,
                ItemID.HolyGrail, ItemID.Gloves, ItemID.DinosaurFigure, ItemID.GaleFibula, ItemID.FlameTorc, ItemID.PowerBand, ItemID.GrappleClaw,
                ItemID.GaneshaTalisman, ItemID.MaatsFeather, ItemID.Feather, ItemID.FreysShip, ItemID.Harp, ItemID.DestinyTablet, ItemID.SecretTreasureofLife,
                ItemID.OriginSigil, ItemID.BirthSigil, ItemID.LifeSigil, ItemID.DeathSigil, ItemID.ClaydollSuit};

            L2FlagBoxEnd[] getFlags = null;
            if (itemID >= ItemID.SacredOrb0 && itemID <= ItemID.SacredOrb9)
            {
                getFlags = new L2FlagBoxEnd[2];
                getFlags[0] = new L2FlagBoxEnd { calcu = CALCU.EQR, seet_no1 = 2, flag_no1 = itemInfo.itemFlag, data = 1 };
                getFlags[1] = new L2FlagBoxEnd { calcu = CALCU.ADD, seet_no1 = 0, flag_no1 = 2, data = 1 };
            }
            else if(itemID >= ItemID.CrystalSkull1 && itemID <= ItemID.CrystalSkull12)
            {
                getFlags = new L2FlagBoxEnd[3];
                getFlags[0] = new L2FlagBoxEnd { calcu = CALCU.EQR, seet_no1 = 2, flag_no1 = itemInfo.itemFlag, data = 1 };
                getFlags[1] = new L2FlagBoxEnd { calcu = CALCU.ADD, seet_no1 = 0, flag_no1 = 32, data = 1 };
                getFlags[2] = new L2FlagBoxEnd { calcu = CALCU.ADD, seet_no1 = 3, flag_no1 = 30, data = 4 };
            }
            else if ((itemID >= ItemID.AnkhJewel1 && itemID <= ItemID.AnkhJewel9) || Array.IndexOf(storyItems, itemID) > -1)
            {
                getFlags = new L2FlagBoxEnd[2];
                getFlags[0] = new L2FlagBoxEnd { calcu = CALCU.EQR, seet_no1 = 2, flag_no1 = itemInfo.itemFlag, data = 1 };
                getFlags[1] = new L2FlagBoxEnd { calcu = CALCU.ADD, seet_no1 = 3, flag_no1 = 30, data = 4 };
            }
            else
            {
                getFlags = new L2FlagBoxEnd[1];
                short flagValue = 1;
                if(itemID == ItemID.ChainWhip || itemID == ItemID.SilverShield || itemID == ItemID.MobileSuperx3P)
                {
                    flagValue = 2;
                }
                else if (itemID == ItemID.FlailWhip || itemID == ItemID.AngelShield)
                {
                    flagValue = 3;
                }

                getFlags[0] = new L2FlagBoxEnd { calcu = CALCU.EQR, seet_no1 = 2, flag_no1 = itemInfo.itemFlag, data = flagValue };
            }

            return getFlags;
        }

        void OnGUI()
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
                GUI.Label(new Rect(0, Screen.height - 75f, 500f, 50f), error);
                GUI.Label(new Rect(0, Screen.height - 25f, 50f, 25f), randomising.ToString());
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
            if (randomising)
            {
                cachedBoxes = GameObject.FindObjectsOfType<TreasureBoxScript>();
                cachedItems = GameObject.FindObjectsOfType<EventItemScript>();
               
                if (cachedBoxes != null)
                {
                    //loop over all the boxes in the current scene and change their item and flags
                    foreach (TreasureBoxScript box in cachedBoxes)
                    {
                        ChangeBox(box);
                    }
                }

                if (cachedItems != null)
                {
                    //loop over all the event items in the current scene and change their flags
                    foreach (EventItemScript eventItem in cachedItems)
                    {
                        ChangeEventItem(eventItem);
                    }
                }

                //these cause the message to popup when you record a mantra for the first time, so just deactivate
                //the gameobject so they don't appear
                foreach (FlagDialogueScript flagDialogue in GameObject.FindObjectsOfType<FlagDialogueScript>())
                {
                    if (flagDialogue.cellName.Contains("mantraDialog"))
                    {
                        flagDialogue.gameObject.SetActive(false);
                    }
                }

                //Change the snapshot type
                foreach (SnapShotTargetScript snapTarget in GameObject.FindObjectsOfType<SnapShotTargetScript>())
                {
                    LocationID locationID = GetLocationIDForMural(snapTarget);
                    if (locationID != LocationID.None)
                    {
                        snapTarget.mode = SnapShotTargetScript.SnapShotMode.SOFTWARE;
                    }
                }
            }
        }
        
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F11))
            {
                this.showText = !this.showText;
            }
        }
        
        public void Initialise(L2ShopDataBase shopDataBase, L2TalkDataBase talkDataBase, patched_L2System system)
        {
            this.shopDataBase = shopDataBase;
            this.talkDataBase = talkDataBase;
            this.sys = system;

            StartCoroutine(Setup());
        }

        private Dictionary<int, int> LoadSeedFile()
        {
            Dictionary<int, int> itemLocations = null;
            FileStream fs = null;
            BinaryFormatter formatter;
            try
            {
                fs = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "LM2Randomiser\\Seed\\seed.lm2"), FileMode.Open);
                formatter = new BinaryFormatter();
                itemLocations = (Dictionary<int, int>)formatter.Deserialize(fs);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }

            return itemLocations;
        }

        private IEnumerator Setup()
        {
            //load the locationToItemMap from seed.lm2
            locationToItemMap = LoadSeedFile();

            //need to wait to ensure that the game has setup all its systems
            yield return new WaitForSeconds(1f);

            //if we successfully loaded and the seed has the right amount of locations
            if (locationToItemMap != null && locationToItemMap.Count == ITEM_COUNT)
            {
                randomising = true;
                ChangeShopItems();
                ChangeShopThanks();
                ChangeDialogueItems();
                MojiScriptFixes();
            }
            else
            {
                if (locationToItemMap != null)
                {
                    error += String.Format(", total items randomised: {0}, should be {1}.", locationToItemMap.Count, ITEM_COUNT);
                }
            }
        }
        
        private void ChangeBox(TreasureBoxScript box)
        {
            ItemData oldItemData = GetItemDataFromName(box.itemObj.name);

            if (oldItemData != null && locationToItemMap.TryGetValue((int)oldItemData.getItemName(), out int id))
            {
                ItemID newItemID = (ItemID)id;

                ItemInfo newItemInfo = ItemFlags.GetItemInfo(newItemID);

                AbstractItemBase item = box.itemObj.GetComponent<AbstractItemBase>();

                //the flags the box uses to check whether you have that item already if true the box will be open
                //im pretty sure that it wont spawn the item if it this check is true on intialisation therefore if
                //you collect the item that was originally in the box you now cant get the item in the box since it
                //only spawns the item on box open
                foreach (L2FlagBoxParent flagBoxParent in box.openFlags)
                {
                    foreach(L2FlagBox flagBox in flagBoxParent.BOX)
                    {
                        if (flagBox.seet_no1 == 2)
                        {
                            flagBox.flag_no1 = newItemInfo.itemFlag;
                            flagBox.flag_no2 = 1;

                            //the whips and shields use the same flag just increment higher with each upgrade cant just use the same as other items
                            if (newItemID == ItemID.ChainWhip || newItemID == ItemID.SilverShield || newItemID == ItemID.MobileSuperx3P)
                            {
                                flagBox.flag_no2 = 2;
                            }
                            else if (newItemID == ItemID.FlailWhip || newItemID == ItemID.AngelShield)
                            {
                                flagBox.flag_no2 = 3;
                            }
                        }
                    }
                }
                
                //flags the item uses to check to see if it should be active and visible to the user, important that these are
                //changed because if you only change the label the it will use the original items flags to check. This means that 
                //if you change another item to what was this items original is and collect it when it comes to collecting the item 
                //this has been changed too if won't be active as it thinks you already have it
                foreach (L2FlagBoxParent flagBoxParent in item.itemActiveFlag)
                {
                    foreach (L2FlagBox flagBox in flagBoxParent.BOX)
                    {
                        if (flagBox.seet_no1 == 2)
                        {
                            flagBox.flag_no1 = newItemInfo.itemFlag;
                            flagBox.comp = COMPARISON.Equal;
                            flagBox.flag_no2 = 0;

                            //the whips and shields use the same flag just increment higher with each upgrade cant just use the same as other items
                            if (newItemID == ItemID.ChainWhip || newItemID == ItemID.SilverShield || newItemID == ItemID.MobileSuperx3P)
                            {
                                flagBox.flag_no2 = 1;
                                flagBox.comp = COMPARISON.LessEq;
                            }
                            else if (newItemID == ItemID.FlailWhip || newItemID == ItemID.AngelShield)
                            {
                                flagBox.flag_no2 = 2;
                                flagBox.comp = COMPARISON.LessEq;
                            }
                            else if (newItemID == ItemID.Buckler)
                            {
                                flagBox.comp = COMPARISON.LessEq;
                            }
                        }
                    }
                }
                //flags that the item sets when you collect it, important to change otherwise the original item will also be collected
                //when you pick up the item because by default it sets the original items flags again, also other flags can be set here
                //usually items that add to flags that are used as a type of counter eg.Sacred Orbs orb count
                item.itemGetFlags = CreateGetFlags(newItemID, newItemInfo);

                //name used when calling setitem
                item.itemLabel = newItemInfo.boxName;

                //change the sprite to match the new item
                Sprite sprite;
                if (newItemID >= ItemID.Heaven && newItemID <= ItemID.Night)
                {
                    sprite = L2SystemCore.getMapIconSprite(L2SystemCore.getItemData("Mantra"));
                }
                else
                {
                    sprite = L2SystemCore.getMapIconSprite(L2SystemCore.getItemData(newItemInfo.boxName));
                }
                item.gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
            }
        }

        private void ChangeEventItem(EventItemScript eventItem)
        {
            ItemData oldItemData = GetItemDataFromName(eventItem.name);
            
            if (oldItemData != null && locationToItemMap.TryGetValue((int)oldItemData.getItemName(), out int id))
            {
                ItemID newItemID = (ItemID)id;

                //get the item data for the new item, only really need the names here
                ItemInfo newItemInfo = ItemFlags.GetItemInfo(newItemID);
                
                //flags the item uses to check to see if it should be active and visible to the user, important that these are
                //changed because if you only change the label the it will use the original items flags to check. This means that 
                //if you change another item to what was this items original is and collect it when it comes to collecting the item 
                //this has been changed too if won't be active as it thinks you already have it
                foreach (L2FlagBoxParent flagBoxParent in eventItem.itemActiveFlag)
                {
                    foreach (L2FlagBox flagBox in flagBoxParent.BOX)
                    {
                        if (flagBox.seet_no1 == 2)
                        {
                            flagBox.flag_no1 = newItemInfo.itemFlag;
                            flagBox.comp = COMPARISON.Equal;
                            flagBox.flag_no2 = 0;

                            //the whips and shields use the same flag just increment higher with each upgrade cant just use the same as other items
                            if (newItemID == ItemID.ChainWhip || newItemID == ItemID.SilverShield || newItemID == ItemID.MobileSuperx3P)
                            {
                                flagBox.flag_no2 = 1;
                                flagBox.comp = COMPARISON.LessEq;
                            }
                            else if (newItemID == ItemID.FlailWhip || newItemID == ItemID.AngelShield)
                            {
                                flagBox.flag_no2 = 2;
                                flagBox.comp = COMPARISON.LessEq;
                            }
                            else if (newItemID == ItemID.Buckler)
                            {
                                flagBox.comp = COMPARISON.LessEq;
                            }
                        }
                    }
                }

                //flags that the item sets when you collect it, important to change otherwise the original item will also be collected
                //when you pick up the item because by default it sets the original items flags again, also other flags can be set here
                //usually items that add to flags that are used as a type of counter eg.Sacred Orbs orb count
                eventItem.itemGetFlags = CreateGetFlags(newItemID, newItemInfo);

                //name used when calling setitem
                eventItem.itemLabel = newItemInfo.boxName;

                //change the sprite to match the new itemSprite sprite;
                Sprite sprite;
                if (newItemID >= ItemID.Heaven && newItemID <= ItemID.Night)
                {
                    sprite = L2SystemCore.getMapIconSprite(L2SystemCore.getItemData("Mantra"));
                }
                else
                {
                    sprite = L2SystemCore.getMapIconSprite(L2SystemCore.getItemData(newItemInfo.boxName));
                }
                eventItem.gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
            }
        }

        private ItemData GetItemDataFromName(string objName)
        {
            if (objName.Contains("ItemSym "))
            {
                string name = objName.Substring(8);

                if(name.Contains("SacredOrb"))
                {
                    name = name.Insert(6, " ");
                }
                else if(name.Equals("MSX3p"))
                {
                    name = "MSX";
                }
                return L2SystemCore.getItemData(name);
            }
            return null;
        }
        
        private void ChangeShopItems()
        {
            shopDataBase.cellData[0][25][1][0] = CreateShopItemsString(LocationID.SidroShop1, LocationID.SidroShop2, LocationID.SidroShop3);
            shopDataBase.cellData[1][26][1][0] = CreateShopItemsString(LocationID.ModroShop1, LocationID.ModroShop2, LocationID.ModroShop3);
            shopDataBase.cellData[2][24][1][0] = CreateShopItemsString(LocationID.NeburShop1, LocationID.NeburShop2, LocationID.NeburShop3);
            shopDataBase.cellData[3][25][1][0] = CreateShopItemsString(LocationID.HinerShop1, LocationID.HinerShop2, LocationID.HinerShop3);
            shopDataBase.cellData[4][24][1][0] = CreateShopItemsString(LocationID.HinerShop1, LocationID.HinerShop2, LocationID.HinerShop4);
            shopDataBase.cellData[5][24][1][0] = CreateShopItemsString(LocationID.KorobokShop1, LocationID.KorobokShop2, LocationID.KorobokShop3);
            shopDataBase.cellData[6][24][1][0] = CreateShopItemsString(LocationID.PymShop1, LocationID.PymShop2, LocationID.PymShop3);
            shopDataBase.cellData[7][24][1][0] = CreateShopItemsString(LocationID.PiebalusaShop1, LocationID.PiebalusaShop2, LocationID.PiebalusaShop3);
            shopDataBase.cellData[8][24][1][0] = CreateShopItemsString(LocationID.HiroRoderickShop1, LocationID.HiroRoderickShop2, LocationID.HiroRoderickShop3);
            shopDataBase.cellData[9][24][1][0] = CreateShopItemsString(LocationID.BtkShop1, LocationID.BtkShop2, LocationID.BtkShop3);
            shopDataBase.cellData[10][24][1][0] = CreateShopItemsString(LocationID.BtkShop1, LocationID.BtkShop2, LocationID.BtkShop3);
            shopDataBase.cellData[11][24][1][0] = CreateShopItemsString(LocationID.MinoShop1, LocationID.MinoShop2, LocationID.MinoShop3);
            shopDataBase.cellData[12][24][1][0] = CreateShopItemsString(LocationID.ShuhokaShop1, LocationID.ShuhokaShop2, LocationID.ShuhokaShop3);
            shopDataBase.cellData[13][24][1][0] = CreateShopItemsString(LocationID.HydlitShop1, LocationID.HydlitShop2, LocationID.HydlitShop3);
            shopDataBase.cellData[14][24][1][0] = CreateShopItemsString(LocationID.AytumShop1, LocationID.AytumShop2, LocationID.AytumShop3);
            shopDataBase.cellData[15][24][1][0] = CreateShopItemsString(LocationID.AshGeenShop1, LocationID.AshGeenShop2, LocationID.AshGeenShop3);
            shopDataBase.cellData[16][24][1][0] = CreateShopItemsString(LocationID.MegarockShop1, LocationID.MegarockShop2, LocationID.MegarockShop3);
            shopDataBase.cellData[17][24][1][0] = CreateShopItemsString(LocationID.BargainDuckShop1, LocationID.BargainDuckShop2, LocationID.BargainDuckShop3);
            shopDataBase.cellData[18][24][1][0] = CreateShopItemsString(LocationID.KeroShop1, LocationID.KeroShop2, LocationID.KeroShop3);
            shopDataBase.cellData[19][24][1][0] = CreateShopItemsString(LocationID.VenomShop1, LocationID.VenomShop2, LocationID.VenomShop3);
            shopDataBase.cellData[20][24][1][0] = CreateShopItemsString(LocationID.FairyLanShop1, LocationID.FairyLanShop2, LocationID.FairyLanShop3);
        }

        private string CreateShopItemsString(LocationID firstSpot, LocationID secondSpot, LocationID thirdSpot)
        {
            return String.Format("{0}\n{1}\n{2}", CreateSetItemString(firstSpot), CreateSetItemString(secondSpot), CreateSetItemString(thirdSpot));
        }

        private string CreateSetItemString(LocationID locationID)
        {
            if (locationToItemMap.TryGetValue((int)locationID, out int id))
            {
                ItemID newItemID = (ItemID)id;
                ItemInfo newItemInfo = ItemFlags.GetItemInfo(newItemID);
                return String.Format("[@sitm,{0},{1},{2},{3}]", newItemInfo.shopType, newItemInfo.shopName, newItemInfo.shopPrice, newItemInfo.shopAmount);
            }

            return String.Empty;
        }

        private void ChangeShopThanks()
        {
            //so for items that need extra flags set just gonna change the mojiscript corresponding to the thank you for that slot
            //changing this in code seems like have to change a giant method in the shop script to set the flags id rather do this atm

            ChangeThanksStrings(LocationID.SidroShop1, LocationID.SidroShop2, LocationID.SidroShop3,0,9);
            ChangeThanksStrings(LocationID.ModroShop1, LocationID.ModroShop2, LocationID.ModroShop3,1,9,2,3);
            ChangeThanksStrings(LocationID.NeburShop1, LocationID.NeburShop2, LocationID.NeburShop3,2,8);
            ChangeThanksStrings(LocationID.HinerShop1, LocationID.HinerShop2, LocationID.HinerShop3,3,9);
            ChangeThanksStrings(LocationID.HinerShop1, LocationID.HinerShop2, LocationID.HinerShop4,4,8);
            ChangeThanksStrings(LocationID.KorobokShop1, LocationID.KorobokShop2, LocationID.KorobokShop3,5,8);
            ChangeThanksStrings(LocationID.PymShop1, LocationID.PymShop2, LocationID.PymShop3,6,8);
            ChangeThanksStrings(LocationID.PiebalusaShop1, LocationID.PiebalusaShop2, LocationID.PiebalusaShop3,7,8);
            ChangeThanksStrings(LocationID.HiroRoderickShop1, LocationID.HiroRoderickShop2, LocationID.HiroRoderickShop3,8,8);
            ChangeThanksStrings(LocationID.BtkShop1, LocationID.BtkShop2, LocationID.BtkShop3,9,8);
            ChangeThanksStrings(LocationID.BtkShop1, LocationID.BtkShop2, LocationID.BtkShop3,10,8);
            ChangeThanksStrings(LocationID.MinoShop1, LocationID.MinoShop2, LocationID.MinoShop3,11,8);
            ChangeThanksStrings(LocationID.ShuhokaShop1, LocationID.ShuhokaShop2, LocationID.ShuhokaShop3,12,8);
            ChangeThanksStrings(LocationID.HydlitShop1, LocationID.HydlitShop2, LocationID.HydlitShop3,13,8);
            ChangeThanksStrings(LocationID.AytumShop1, LocationID.AytumShop2, LocationID.AytumShop3,14,8);
            ChangeThanksStrings(LocationID.AshGeenShop1, LocationID.AshGeenShop2, LocationID.AshGeenShop3,15,8);
            ChangeThanksStrings(LocationID.MegarockShop1, LocationID.MegarockShop2, LocationID.MegarockShop3,16,8);
            ChangeThanksStrings(LocationID.BargainDuckShop1, LocationID.BargainDuckShop2, LocationID.BargainDuckShop3,17,8);
            ChangeThanksStrings(LocationID.KeroShop1, LocationID.KeroShop2, LocationID.KeroShop3,18,8);
            ChangeThanksStrings(LocationID.VenomShop1, LocationID.VenomShop2, LocationID.VenomShop3,19,8);
            ChangeThanksStrings(LocationID.FairyLanShop1, LocationID.FairyLanShop2, LocationID.FairyLanShop3,20,8);
        }

        private void ChangeThanksStrings(LocationID firstSpot, LocationID secondSpot, LocationID thirdSpot, int seet, int first, int secondOffset = 1, int thirdOffset = 2)
        {
            shopDataBase.cellData[seet][first][1][0] += CreateGetFlagString(firstSpot);
            shopDataBase.cellData[seet][first + secondOffset][1][0] += CreateGetFlagString(secondSpot);
            shopDataBase.cellData[seet][first + thirdOffset][1][0] += CreateGetFlagString(thirdSpot);
        }

        private void ChangeDialogueItems()
        {
            //could do this by changing stuff in the mojicript stuff but atm id rather do it this way, also fairly simple 
            //to add the extra flags that need to be set, sadly can't rely modify the set item 

            //Neburu map
            talkDataBase.cellData[0][11][1][0] = ChangeTalkString(LocationID.MapfromNebur,
                "[@anim,thanks,1]\n{0}[@setf,2,127,=,1]\n[@setf,2,128,=,1]\n[@setf,2,129,=,1]\n[@setf,2,130,=,1]\n[@setf,5,3,=,1]\n[@out]");

            //Xelpud item
            talkDataBase.cellData[1][10][1][0] = ChangeTalkString(LocationID.XelpudItem,
                "{0}[@setf,3,31,=,1]\n[@setf,5,2,=,1]\n[@setf,5,20,=,2]\n[@p,lastC]");

            //if you say to neburs map xelpud gives it too you instead at some point, should never need to use as this isnt in logic
            talkDataBase.cellData[1][70][1][0] = ChangeTalkString(LocationID.MapfromNebur,
                "{0}[@setf,2,127,=,1]\n[@setf,5,4,=,2]\n[@anim,talk,1]");

            //Alsedana item
            talkDataBase.cellData[2][13][1][0] = ChangeTalkString(LocationID.AlsedanaItem,
                "{0}[@anim,talk,1]\n[@setf,1,54,=,1]\n[@p,2nd-6]");

            //Funeral item
            talkDataBase.cellData[3][5][1][0] = ChangeTalkString(LocationID.FuneralItem,
                "{0}[@setf,1,54,=,1]\n[@anim,talk,1]\n[@p,1st-3]");

            //check to see if you can get the funeral item
            talkDataBase.cellData[3][3][1][0] = ChangeTalkFlagCheck(LocationID.FuneralItem, COMPARISON.Greater, "[@iff,5,62,=,7,giltoriyo,9th]\n[@iff,2,{0},&gt;,{1},giltoriyo,8th]\n" +
                "[@iff,5,62,=,6,giltoriyo,7th]\n[@iff,5,62,=,5,giltoriyo,6th]\n[@iff,5,62,=,4,giltoriyo,5th]\n[@iff,5,62,=,3,giltoriyo,4th]\n[@iff,5,62,=,2,giltoriyo,2nd]\n" +
                "[@exit]\n[@anim,talk,1]\n[@p,1st]");
            
            //didnt talk to alsedana after vritra
            talkDataBase.cellData[3][7][1][0] = ChangeTalkStringAndFlagCheck(LocationID.AlsedanaItem,
                "[@iff,2,{0},&gt;,{1},giltoriyo,2nd]\n[@exit]\n{2}[@anim,talk,1]\n[@p,1st-5]");

            //Fobos after you break statue
            talkDataBase.cellData[6][9][1][0] = ChangeTalkString(LocationID.FobosItem,
                "[@setf,5,16,=,5]\n[@anim,talk,1]\n{0}[@p,3rd-2]");

            //Fobos skull reader
            talkDataBase.cellData[5][24][1][0] = ChangeTalkString(LocationID.FobosSkullItem, 
                "[@exit]\n[@anim,talk,1]\n[@setf,23,15,=,4]\n{0}[@p,lastC]");

            //Freya item
            talkDataBase.cellData[7][7][1][0] = ChangeTalkString(LocationID.FreyasItem,
                "[@anim,talk,1]\n{0}[@setf,5,67,=,1]\n[@p,lastC]");

            //add check too Freya's starting mojiscript so she gives the item if you havent got it yet
            talkDataBase.cellData[7][3][1][0] = ChangeTalkFlagCheck(LocationID.FreyasItem, COMPARISON.Less,"[@anifla,mfanim,wait2]\n[@iff,2,{0},&lt;,{1},freyja,1st-1]\n[@iff,3,95,&gt;,0,freyja,escape]\n" +
                "[@anifla,mfanim,wait]\n[@iff,3,35,&gt;,7,freyja,8th]\n[@iff,3,35,=,6,freyja,7th3]\n[@iff,3,35,&gt;,3,freyja,7th2]\n[@iff,3,35,=,3,freyja,ragna]\n[@iff,3,35,=,2,freyja,4th]\n" +
                "[@iff,3,35,=,1,freyja,3rd]\n[@iff,5,67,=,1,freyja,2nd]\n[@exit]\n[@anim,talk,1]\n[@p,1st-1]");

            //Mulbruk item
            talkDataBase.cellData[10][42][1][0] = ChangeTalkString(LocationID.MulbrukItem,
                "{0}[@setf,5,101,=,2]\n[@anim,talk,1]\n[@p,3rd-2]");

            //add check too Mulbruk so she just gives you the item
            talkDataBase.cellData[10][3][1][0] = ChangeTalkFlagCheck(LocationID.MulbrukItem, COMPARISON.Less, "[@iff,2,{0},&lt;,{1},mulbruk2,3rd]\n[@iff,5,61,=,1,mulbruk2,mirror]\n[@iff,5,86,=,1,mulbruk2,hint2]\n[@iff,5,87,=,1,mulbruk2,hint3]\n[@iff,5,88,=,1,mulbruk2,hint4]\n" +
                "[@iff,5,89,=,1,mulbruk2,hint5]\n[@iff,5,90,=,1,mulbruk2,hint6]\n[@iff,5,91,=,1,mulbruk2,hint7]\n[@iff,5,92,=,1,mulbruk2,hint8]\n[@iff,5,93,=,1,mulbruk2,hint9]\n[@iff,5,94,=,1,mulbruk2,hint10]\n" +
                "[@iff,5,95,=,1,mulbruk2,hint11]\n[@iff,3,33,&gt;,10,mulbruk2,5th]\n[@iff,5,0,=,2,mulbruk2,4th]\n[@anifla,mfanim,wait]\n[@iff,5,78,=,5,mulbruk2,hint1]\n[@anifla,mfanim,wait4]\n" +
                "[@iff,3,33,=,10,mulbruk2,3rdRnd]\n[@anifla,mfanim,wait]\n[@iff,5,78,=,5,mulbruk2,hint1]\n[@anifla,mfanim,wait3]\n[@iff,3,33,=,6,mulbruk2,rTalk2]\n[@anifla,mfanim,wait2]\n" +
                "[@iff,3,33,=,5,mulbruk2,rTalk1]\n[@anifla,mfanim,nochar]\n[@iff,3,33,=,4,mulbruk2,1st-8]\n[@iff,3,33,=,8,mulbruk2,1st-8]\n[@anifla,mfanim,wait]\n[@iff,3,33,&lt;,7,mulbruk2,1st]\n" +
                "[@iff,3,33,=,7,mulbruk2,2nd]\n");

            //Osiris L scythe
            talkDataBase.cellData[78][7][1][0] = ChangeTalkStringAndFlagCheck(LocationID.LightScytheItem,
                "[@iff,2,{0},&gt;,{1},f15-3,2nd]\n{2}[@anim,talk,1]\n[@p,lastC]");
        }

        private string ChangeTalkString(LocationID locationID, string original)
        {
            if (locationToItemMap.TryGetValue((int)locationID, out int id))
            {
                ItemID newItemID = (ItemID)id;

                //get the item data for the new item
                ItemInfo newItemInfo = ItemFlags.GetItemInfo(newItemID);

                //Sacred orbs might require some special work here if setting the orbcount flag doesnt give you the level up
                string take;
                if (newItemInfo.boxName.Equals("Crystal S") || newItemInfo.boxName.Equals("Sacred Orb") || newItemInfo.boxName.Equals("MSX3p"))
                {
                    take = String.Format("[@take,{0},02item,1]\n", newItemInfo.boxName);
                }
                else
                {
                    take = String.Format("[@take,{0},02item,1]\n", newItemInfo.shopName);
                }

                //if the item has more than just its set flags add the flags to the mojiscript string
                L2FlagBoxEnd[] getFLags = CreateGetFlags(newItemID, newItemInfo);
                if (getFLags != null)
                {
                    for (int i = 0; i < getFLags.Length; i++)
                    {
                        L2FlagBoxEnd flag = getFLags[i];
                        if (flag.calcu == CALCU.ADD)
                        {
                            take += String.Format("[@setf,{0},{1},+,{2}]\n", flag.seet_no1, flag.flag_no1, flag.data);
                        }
                        else if (flag.calcu == CALCU.EQR)
                        {
                            take += String.Format("[@setf,{0},{1},=,{2}]\n", flag.seet_no1, flag.flag_no1, flag.data);
                        }
                    }
                }

                return String.Format(original, take);
            }
            //should never get to here this will break this characters dialogue
            return String.Empty;
        }

        private string CreateGetFlagString(LocationID locationID)
        {
            string flagString = String.Empty;

            int id;
            if (locationToItemMap.TryGetValue((int)locationID, out id))
            {
                
                ItemID newItemID = (ItemID)id;

                ItemInfo newItemInfo = ItemFlags.GetItemInfo(newItemID);

                if (newItemInfo.boxName.Equals("Crystal S"))
                {
                    flagString = "\n[@take,Crystal S,02item,1]";
                }

                L2FlagBoxEnd[] getFLags = CreateGetFlags(newItemID, newItemInfo);
                if (getFLags != null)
                {
                    for (int i = 0; i < getFLags.Length; i++)
                    {
                        L2FlagBoxEnd flag = getFLags[i];
                        if (flag.calcu == CALCU.ADD)
                        {
                            flagString += String.Format("\n[@setf,{0},{1},+,{2}]", flag.seet_no1, flag.flag_no1, flag.data);
                        }
                        else if (flag.calcu == CALCU.EQR)
                        {
                            flagString += String.Format("\n[@setf,{0},{1},=,{2}]", flag.seet_no1, flag.flag_no1, flag.data);
                        }
                    }
                }
            }

            return flagString;
        }

        private string ChangeTalkFlagCheck(LocationID locationID, COMPARISON comp, string original)
        {
            int id;
            if (locationToItemMap.TryGetValue((int)locationID, out id))
            {
                ItemID newItemID = (ItemID)id;
                ItemInfo newItemInfo = ItemFlags.GetItemInfo(newItemID);

                int flagValue = 0;

                if (comp == COMPARISON.Greater)
                {
                    if (newItemID == ItemID.ChainWhip || newItemID == ItemID.SilverShield || newItemID == ItemID.MobileSuperx3P)
                    {
                        flagValue = 1;
                    }
                    else if (newItemID == ItemID.FlailWhip || newItemID == ItemID.AngelShield)
                    {
                        flagValue = 2;
                    }
                }
                else if(comp == COMPARISON.Less)
                {
                    flagValue = 1;

                    if (newItemID == ItemID.ChainWhip || newItemID == ItemID.SilverShield || newItemID == ItemID.MobileSuperx3P)
                    {
                        flagValue = 2;
                    }
                    else if (newItemID == ItemID.FlailWhip || newItemID == ItemID.AngelShield)
                    {
                        flagValue = 3;
                    }
                }

                return String.Format(original, newItemInfo.itemFlag, flagValue);
            }

            return original;
        }

        private string ChangeTalkStringAndFlagCheck(LocationID locationID, string original)
        {

            int id;
            if (locationToItemMap.TryGetValue((int)locationID, out id))
            {
                ItemID newItemID = (ItemID)id;
                ItemInfo newItemInfo = ItemFlags.GetItemInfo(newItemID);

                int flagValue = 0;

                if (newItemID == ItemID.ChainWhip || newItemID == ItemID.SilverShield || newItemID == ItemID.MobileSuperx3P)
                {
                    flagValue = 1;
                }
                else if (newItemID == ItemID.FlailWhip || newItemID == ItemID.AngelShield)
                {
                    flagValue = 2;
                }

                //TODO:put this is into a method to remove redundancy
                string takeString;
                if (newItemInfo.boxName.Equals("Crystal S") || newItemInfo.boxName.Equals("Sacred Orb") || newItemInfo.boxName.Equals("MSX3p"))
                {
                    takeString = String.Format("[@take,{0},02item,1]\n", newItemInfo.boxName);
                }
                else
                {
                    takeString = String.Format("[@take,{0},02item,1]\n", newItemInfo.shopName);
                }

                //if the item has more than just its set flags add the flags to the mojiscript string
                L2FlagBoxEnd[] getFLags = CreateGetFlags(newItemID, newItemInfo);
                if (getFLags != null)
                {
                    for (int i = 0; i < getFLags.Length; i++)
                    {
                        L2FlagBoxEnd flag = getFLags[i];
                        if (flag.calcu == CALCU.ADD)
                        {
                            takeString += String.Format("[@setf,{0},{1},+,{2}]\n", flag.seet_no1, flag.flag_no1, flag.data);
                        }
                        else if (flag.calcu == CALCU.EQR)
                        {
                            takeString += String.Format("[@setf,{0},{1},=,{2}]\n", flag.seet_no1, flag.flag_no1, flag.data);
                        }
                    }
                }

                return String.Format(original, newItemInfo.itemFlag, flagValue, takeString);
            }

            return original;
        }

        private void MojiScriptFixes()
        {
            //changes to Nebur's scripts so that she stays until you take her item or leave the surface
            shopDataBase.cellData[2][4][1][0] = "[@anim,smile,1]\n[@setf,5,27,+,1]";
            talkDataBase.cellData[0][10][1][0] = "[@anim,nejiru,1]\n[@out]";

            //chnage Fairy King to set flag to open endless even if you have the pendant
            talkDataBase.cellData[8][10][1][0] = "[@exit]\n[@anim,talk,1]\n[@setf,3,34,=,2]\n[@setf,5,12,=,1]\n[@p,2nd-2]";
        }
    }

    //only have to use the shop and npc dialogue enums now since the itemdatabase enum is used for the chests and free standing items
    public enum LocationID
    {
        None = 0,
        NeburShop1 =256,
        NeburShop2,
        NeburShop3,
        ModroShop1,
        ModroShop2,
        ModroShop3,
        SidroShop1,
        SidroShop2,
        SidroShop3,
        HinerShop1,
        HinerShop2,
        HinerShop3,
        HinerShop4,
        KorobokShop1,
        KorobokShop2,
        KorobokShop3,
        ShuhokaShop1,
        ShuhokaShop2,
        ShuhokaShop3,
        PymShop1,
        PymShop2,
        PymShop3,
        BtkShop1,
        BtkShop2,
        BtkShop3,
        MinoShop1,
        MinoShop2,
        MinoShop3,
        BargainDuckShop1,
        BargainDuckShop2,
        BargainDuckShop3,
        VenomShop1,
        VenomShop2,
        VenomShop3,
        PiebalusaShop1,
        PiebalusaShop2,
        PiebalusaShop3,
        HiroRoderickShop1,
        HiroRoderickShop2,
        HiroRoderickShop3,
        HydlitShop1,
        HydlitShop2,
        HydlitShop3,
        AytumShop1,
        AytumShop2,
        AytumShop3,
        KeroShop1,
        KeroShop2,
        KeroShop3,
        AshGeenShop1,
        AshGeenShop2,
        AshGeenShop3,
        FairyLanShop1,
        FairyLanShop2,
        FairyLanShop3,
        MegarockShop1,
        MegarockShop2,
        MegarockShop3,
        AlsedanaItem,
        FuneralItem,
        XelpudItem,
        MapfromNebur,
        FreyasItem,
        FobosItem,
        FobosSkullItem,
        MulbrukItem,
        LightScytheItem,
        Mantra1,
        Mantra2,
        Mantra3,
        Mantra4,
        Mantra5,
        Mantra6,
        Mantra7,
        Mantra8,
        Mantra9,
        Mantra10
    }
}