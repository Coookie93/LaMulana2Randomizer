using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using ExtensionMethods;
using UnityEngine;
using UnityEngine.SceneManagement;
using L2Word;
using L2Flag;

namespace LM2RandomiserMod
{
    public class L2Rando : MonoBehaviour
    {
        bool showText = true;
        
        L2ShopDataBase shopDataBase;
        L2TalkDataBase talkDataBase;
        L2Base.L2System sys;

        TreasureBoxScript[] cachedBoxes;
        EventItemScript[] cachedItems;
        
        Dictionary<int, int> locationToItemMap;
        bool randomising = false;
        string error;
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
                GUI.Label(new Rect(0, Screen.height - 75f, 500f, 25f), error);
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
            cachedBoxes = UnityEngine.Object.FindObjectsOfType<TreasureBoxScript>();
            cachedItems = UnityEngine.Object.FindObjectsOfType<EventItemScript>();
            if (randomising)
            {
                if (cachedBoxes != null)
                {
                    //loop over all the boxes in the current scene and change their item and flags
                    foreach (var box in cachedBoxes)
                    {
                        string itemName = box.itemObj.name;
                        //this should never fail currently, more of a sanity check
                        if (itemName.Contains("ItemSym "))
                        {
                            itemName = itemName.Remove(0, 8).RemoveWhitespace();

                            //theres 4 items that contain '-' E-Spear, R-Shuriken, La-Mulana, La-Mulana2
                            if (itemName.Contains("-"))
                            {
                                itemName = itemName.Replace("-", "");
                            }

                            //if the name off the item is one we care about
                            if (Enum.IsDefined(typeof(LocationID), itemName))
                            {
                                ChangeBox(box, (LocationID)Enum.Parse(typeof(LocationID), itemName));
                            }
                        }
                    }
                }

                if (cachedItems != null)
                {
                    //loop over all the event items in the current scene and change their flags
                    foreach (var eventItem in cachedItems)
                    {
                        string itemName = eventItem.name;

                        //there is 2 bronze mirrors for some reason just disable one of them
                        if (itemName.Equals("ItemSym B Mirror2"))
                        {
                            eventItem.gameObject.SetActive(false);
                            continue;
                        }


                        //this should never fail currently, more of a sanity check
                        if (itemName.Contains("ItemSym "))
                        {
                            itemName = itemName.Remove(0, 8).RemoveWhitespace();

                            //theres 4 items that contain '-' E-Spear, R-Shuriken, La-Mulana, La-Mulana2
                            if (itemName.Contains("-"))
                            {
                                itemName = itemName.Replace("-", "");
                            }

                            //if the name off the item is one we care about
                            if (Enum.IsDefined(typeof(LocationID), itemName))
                            {
                                ChangeEventItem(eventItem, (LocationID)Enum.Parse(typeof(LocationID), itemName));
                            }
                        }
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
        
        public void Initialise(L2ShopDataBase shopDataBase, L2TalkDataBase talkDataBase, L2System system)
        {
            this.shopDataBase = shopDataBase;
            this.talkDataBase = talkDataBase;
            this.sys = system;

            //load the locationToItemMap from seed.lm2
            locationToItemMap = LoadSeedFile();
            //if we successfully loaded and the seed has the right amount of locations
            if (locationToItemMap != null && locationToItemMap.Count == 173)
            {
                randomising = true;
                ChangeShopItems();
                ChangeShopThanks();
                ChangeDialogueItems();
            }
            else
            {
                if (locationToItemMap != null)
                {
                    error += "total items randomised: " + locationToItemMap.Count;
                }
            }
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
        
        private void ChangeBox(TreasureBoxScript box, LocationID locationID)
        {
            int id;
            if (locationToItemMap.TryGetValue((int)locationID, out id))
            {
                ItemID newItemID = (ItemID)id;

                //get the item data for the new item, only really need the
                ItemData newItemData = ItemFlags.GetItemData(newItemID);

                //the flags the box uses to check whether you have that item already if true the box will be open
                //im pretty sure that it wont spawn the item if it this check is true on intialisation therefore if
                //you collect the item that was originally in the box you now cant get the item in the box since it
                //only spawns the item on box open
                box.openFlags = ItemFlags.GetBoxOpenFlags(newItemID);

                AbstractItemBase item = box.itemObj.GetComponent<AbstractItemBase>();
                
                //flags the item uses to check to see if it should be active and visible to the user, important that these are
                //changed because if you only change the label the it will use the original items flags to check. This means that 
                //if you change another item to what was this items original is and collect it when it comes to collecting the item 
                //this has been changed too if won't be active as it thinks you already have it
                item.itemActiveFlag = ItemFlags.GetActiveItemFlags(newItemID);

                //flags that the item sets when you collect it, important to change otherwise the original item will also be collected
                //when you pick up the item because by default it sets the original items flags again, also other flags can be set here
                //usually items that add to flags that are used as a type of counter eg.Sacred Orbs orb count
                item.itemGetFlags = ItemFlags.GetItemGetFlags(newItemID);

                //name used when calling setitem
                item.itemLabel = newItemData.boxName;

                //change the sprite to match the new item
                Sprite sprite = L2SystemCore.getMapIconSprite(L2SystemCore.getItemData(newItemData.boxName));
                item.gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
            }
        }

        private void ChangeEventItem(EventItemScript eventItem, LocationID locationID)
        {
            int id;
            if (locationToItemMap.TryGetValue((int)locationID, out id))
            {
                ItemID newItemID = (ItemID)id;

                //get the item data for the new item, only really need the name here
                ItemData newItemData = ItemFlags.GetItemData(newItemID);

                //flags the item uses to check to see if it should be active and visible to the user, important that these are
                //changed because if you only change the label the it will use the original items flags to check. This means that 
                //if you change another item to what was this items original is and collect it when it comes to collecting the item 
                //this has been changed too if won't be active as it thinks you already have it
                eventItem.itemActiveFlag = ItemFlags.GetActiveItemFlags(newItemID);

                //flags that the item sets when you collect it, important to change otherwise the original item will also be collected
                //when you pick up the item because by default it sets the original items flags again, also other flags can be set here
                //usually items that add to flags that are used as a type of counter eg.Sacred Orbs orb count
                eventItem.itemGetFlags = ItemFlags.GetItemGetFlags(newItemID);

                //name used when calling setitem
                eventItem.itemLabel = newItemData.boxName;

                //change the sprite to match the new item
                Sprite sprite = L2SystemCore.getMapIconSprite(L2SystemCore.getItemData(newItemData.boxName));
                eventItem.gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
            }
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
            int id;
            if (locationToItemMap.TryGetValue((int)locationID, out id))
            {
                ItemID newItemID = (ItemID)id;
                //get the item data for the new item, only really need the

                ItemData newItemData = ItemFlags.GetItemData(newItemID);
                return String.Format("[@sitm,{0},{1},{2},{3}]", newItemData.shopType, newItemData.shopName, newItemData.shopPrice, newItemData.shopAmount);
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
            //"[@anim,thanks,1]\n[@take,Map16,02item,1]\n[@setf,2,126,=,1]\n[@setf,2,127,=,1]\n[@setf,2,128,=,1]\n[@setf,2,129,=,1]\n[@setf,2,130,=,1]\n[@setf,5,3,=,1]\n[@out]"
            talkDataBase.cellData[0][11][1][0] = ChangeTalkString(LocationID.MapfromNebur,
                "[@anim,thanks,1]\n{0}[@setf,2,126,=,1]\n[@setf,2,127,=,1]\n[@setf,2,128,=,1]\n[@setf,2,129,=,1]\n[@setf,2,130,=,1]\n[@setf,5,3,=,1]\n[@out]");

            //xelpud item
            //"[@take,Xelputter,02item,1]\n[@setf,3,31,=,1]\n[@setf,5,2,=,1]\n[@setf,5,20,=,2]\n[@p,lastC]"
            talkDataBase.cellData[1][10][1][0] = ChangeTalkString(LocationID.XelpudItem,
                "{0}[@setf,3,31,=,1]\n[@setf,5,2,=,1]\n[@setf,5,20,=,2]\n[@p,lastC]");

            //if you say to neburs map xelpud gives it too you instead at some point, should never need to use this as it breaks logic
            //"[@take,Map16,02item,1]\n[@setf,2,126,=,1]\n[@setf,2,127,=,1]\n[@setf,5,4,=,2]\n[@anim,talk,1]"
            talkDataBase.cellData[1][70][1][0] = ChangeTalkString(LocationID.MapfromNebur,
                "{0}[@setf,2,126,=,1]\n[@setf,2,127,=,1]\n[@setf,5,4,=,2]\n[@anim,talk,1]");

            //alsedana item
            //"[@take,Beherit,02item,1]\n[@anim,talk,1]\n[@setf,1,54,=,1]\n[@p,2nd-6]"
            talkDataBase.cellData[2][13][1][0] = ChangeTalkString(LocationID.AlsedanaItem,
                "{0}[@anim,talk,1]\n[@setf,1,54,=,1]\n[@p,2nd-6]");

            //funeral item
            //"[@take,M Talisman,02item,1]\n[@setf,1,54,=,1]\n[@anim,talk,1]\n[@p,1st-3]"
            talkDataBase.cellData[3][5][1][0] = ChangeTalkString(LocationID.FuneralItem,
                "{0}[@setf,1,54,=,1]\n[@anim,talk,1]\n[@p,1st-3]");

            //if you are mean and never visited a dying man, this also isnt in logic
            //"[@iff,2,3,&gt;,0,giltoriyo,2nd]\n[@exit]\n[@take,Beherit,02item,1]\n[@anim,talk,1]\n[@p,1st-5]"
            talkDataBase.cellData[3][7][1][0] = ChangeTalkString(LocationID.AlsedanaItem,
                "[@iff,2,3,&gt;,0,giltoriyo,2nd]\n[@exit]\n{0}[@anim,talk,1]\n[@p,1st-5]");

            //fobos after you break statue
            //"[@setf,2,81,=,1]\n[@setf,5,16,=,5]\n[@anim,talk,1]\n[@take,R Book,02item,1]\n[@p,3rd-2]"
            talkDataBase.cellData[6][9][1][0] = ChangeTalkString(LocationID.FobosItem,
                "[@setf,5,16,=,5]\n[@anim,talk,1]\n{0}[@p,3rd-2]");

            //freya item
            //"[@anim,talk,1]\n[@take,F Pendant,2,1]\n[@setf,5,67,=,1]\n[@p,lastC]"
            talkDataBase.cellData[7][7][1][0] = ChangeTalkString(LocationID.FreyasItem,
                "[@anim,talk,1]\n{0}[@setf,5,67,=,1]\n[@p,lastC]");

            //mulbruk item
            //"[@take,Snow Shoes,02item,1]\n[@setf,5,101,=,2]\n[@anim,talk,1]\n[@p,3rd-2]"
            talkDataBase.cellData[10][42][1][0] = ChangeTalkString(LocationID.MulbrukItem,
                "{0}[@setf,5,101,=,2]\n[@anim,talk,1]\n[@p,3rd-2]");

            //anubis L scythe
            //"[@iff,2,48,&gt;,0,f15-3,2nd]\n[@take,L Scythe,02Items,1]\n[@anim,talk,1]\n[@p,lastC]"
            talkDataBase.cellData[78][7][1][0] = ChangeTalkString(LocationID.LightScytheItem,
                "[@iff,2,48,&gt;,0,f15-3,2nd]\n{0}[@anim,talk,1]\n[@p,lastC]");

        }

        private string ChangeTalkString(LocationID locationID, string original)
        {
            int id;
            if (locationToItemMap.TryGetValue((int)locationID, out id))
            {
                ItemID newItemID = (ItemID)id;
                //get the item data for the new item, only really need the

                ItemData newItemData = ItemFlags.GetItemData(newItemID);

                //Sacred orbs might require some special work here if setting the orbcount flag doesnt give you the level up
                string take;
                if (newItemData.boxName.Equals("Crystal S") || newItemData.boxName.Equals("Sacred Orb")) {
                    take= String.Format("[@take,{0},02item,1]\n", newItemData.boxName);
                }
                else
                {
                    take = String.Format("[@take,{0},02item,1]\n", newItemData.shopName);
                }

                //if the item has more than just its set flags add the flags to the mojiscript string
                L2FlagBoxEnd[] getFLags = ItemFlags.GetItemGetFlags(newItemID);
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

                ItemData newItemData = ItemFlags.GetItemData(newItemID);

                if (newItemData.boxName.Equals("Crystal S"))
                {
                    flagString = "\n[@take,Crystal S,02item,1]";
                }

                L2FlagBoxEnd[] getFLags = ItemFlags.GetItemGetFlags(newItemID);
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
    }

    //this is pretty bad but it works, use the name of the item in chests and free standing items since we can parse it to an enum which is
    //easier than doing other checks to figure out which box is currently being modified, this is a direct mapping to the locations in the generators 
    //LocationID 
    public enum LocationID
    {
        None = 0,
        NeburShop1,
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

        SacredOrb0,
        SacredOrb1,
        SacredOrb2,
        SacredOrb3,
        SacredOrb4,
        SacredOrb5,
        SacredOrb6,
        SacredOrb7,
        SacredOrb8,
        SacredOrb9,

        Map1,
        Map2,
        Map3,
        Map4,
        Map5,
        Map6,
        Map7,
        Map8,
        Map9,
        Map10,
        Map11,
        Map12,
        Map13,
        Map14,
        Map15,
        
        CrystalS1,
        CrystalS2,
        CrystalS3,
        CrystalS4,
        CrystalS5,
        CrystalS6,
        CrystalS7,
        CrystalS8,
        CrystalS9,
        CrystalS10,
        CrystalS11,
        CrystalS12,

        AnkhJewel2,
        AnkhJewel3,
        AnkhJewel4,
        AnkhJewel5,
        AnkhJewel6,
        AnkhJewel7,
        AnkhJewel9,

        AlsedanaItem,
        FuneralItem,
        XelpudItem,
        MapfromNebur,
        ShellHorn,
        HolyGrail,
        FreyasItem,
        FobosItem,
        BMirror,
        CrystalP,
        Shuriken,
        Knife,
        OriginSeal,
        FShip,
        DeathVillage,
        Glove,
        RShuriken,
        Shield2,
        DjedPillar,
        GClaw,
        LifeSeal,
        FTorque,
        FPass,
        Scalesphere,
        Caltrops,
        Whip3,
        GBand,
        ESpear,
        MulbrukItem,
        IceCape,
        TPole,
        Lamp,
        Mjolnir,
        Whip2,
        Chakram,
        Battery,
        DFigure,
        BirthSeal,
        PKey,
        Rapier,
        ClayDoll,
        Gauntlet,
        GStreet,
        Vajra,
        Anchor,
        Katana,
        Feather,
        LaMulana,
        Vessel,
        Crucifix,
        MFeather,
        Ring,
        Egg,
        FlareGun,
        PowerBand,
        DestinyTablet,
        Scriptures,
        Fur,
        GPipe,
        MiracleWitch,
        Perfume,
        Axe,
        MSX3p,
        Gear,
        LaMulana2,
        Spaulder,
        LightScytheItem,
        Book,
        DeathSeal,
        Bomb,
        SecretTreasure

        //R-Shuriken,E-Spear,La-Mulana,La-Mulana2
    }
}