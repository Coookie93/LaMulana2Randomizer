using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using L2Word;
using L2Flag;
using LM2RandomiserMod.Patches;
using LaMulana2RandomizerShared;

namespace LM2RandomiserMod
{
    public class L2Rando : MonoBehaviour
    {
        private patched_L2System sys;
        private L2ShopDataBase shopDataBase;
        private L2TalkDataBase talkDataBase;

        private GameObject cursedChest;

        public bool Randomising { get; private set; } = false;
        public bool AutoScanTablets { get; private set; } = false;
        private Dictionary<LocationID,ItemID> locationToItemMap;
        private List<LocationID> cursedChests;

        private Font currentFont = null;
        private bool showText = false;
        private string message;

        public void OnGUI()
        {
            if (showText)
            {
                if (currentFont == null)
                    currentFont = Font.CreateDynamicFontFromOSFont("Consolas", 14);

                GUIStyle guistyle = new GUIStyle(GUI.skin.label);
                guistyle.normal.textColor = Color.white;
                guistyle.font = currentFont;
                guistyle.fontStyle = FontStyle.Bold;
                guistyle.fontSize = 14;

                GUIContent verContent = new GUIContent(LaMulana2RandomizerShared.Version.version);
                Vector2 verSize = guistyle.CalcSize(verContent);
                GUI.Label(new Rect(0, 0, verSize.x, verSize.y), verContent, guistyle);

                guistyle.fontSize = 10;
                GUI.Label(new Rect(0, verSize.y, 500, 50), message, guistyle);
            }
        }

        public void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            showText = scene.name.Equals("title");
            
            if (Randomising)
            {
                StartCoroutine(ChangeTreasureBoxes());
                ChangeEventItems();

                //these cause the message to popup when you record a mantra for the first time, so just deactivate
                //the gameobject so they don't appear
                foreach (FlagDialogueScript flagDialogue in FindObjectsOfType<FlagDialogueScript>())
                {
                    if (flagDialogue.cellName.Contains("mantraDialog"))
                        flagDialogue.gameObject.SetActive(false);
                }

                //Change the snapshot type to software so as this behaves like getting an item instead of a mantra
                foreach (SnapShotTargetScript snapTarget in FindObjectsOfType<SnapShotTargetScript>())
                {
                    LocationID locationID = GetLocationIDForMural(snapTarget);
                    if (locationID != LocationID.None)
                        snapTarget.mode = SnapShotTargetScript.SnapShotMode.SOFTWARE;
                }

                foreach (FlagWatcherScript flagWatcher in FindObjectsOfType<FlagWatcherScript>())
                {
                    //Change funeral event start conditions so they can reliably be in logic
                    if (flagWatcher.name.Equals("sougiOn"))
                    {
                        //Change the timer so don't have to wait 2 minutes for the funeral event flags to be set
                        flagWatcher.actionWaitFrames = 60;

                        foreach (L2FlagBoxParent flagBoxParent in flagWatcher.CheckFlags)
                        {
                            foreach (L2FlagBox flagBox in flagBoxParent.BOX)
                            {
                                if (flagBox.seet_no1 == 3 && flagBox.flag_no1 == 30)
                                {
                                    //Change these flags so it is now checking if guardians killed is >=6
                                    flagBox.flag_no1 = 0;
                                    flagBox.flag_no2 = 6;
                                    flagBox.comp = COMPARISON.GreaterEq;
                                }
                            }
                        }
                    }
                    //add a check to avoid soflock of triggering the rescue event without first summoning Jormangund's Ankh
                    else if (flagWatcher.name.Equals("ragnarok"))
                    {
                        foreach (L2FlagBoxParent flagBoxParent in flagWatcher.CheckFlags)
                        {
                            L2FlagBox[] flagBoxes = new L2FlagBox[3];
                            flagBoxes[0] = flagBoxParent.BOX[0];
                            flagBoxes[1] = flagBoxParent.BOX[1];

                            //this checks to see if Jormangund's Ankh has atleast been summoned
                            flagBoxes[2] = new L2FlagBox()
                            {
                                seet_no1 = 3,
                                flag_no1 = 14,
                                seet_no2 = -1,
                                flag_no2 = 2,
                                comp = COMPARISON.GreaterEq,
                                logic = LOGIC.AND
                            };
                            flagBoxParent.BOX = flagBoxes;
                        }
                    }

                    foreach (L2FlagBoxParent flagBoxParent in flagWatcher.CheckFlags)
                    {
                        foreach (L2FlagBox flagBox in flagBoxParent.BOX)
                        {
                            if (flagBox.seet_no1 == 5 && flagBox.flag_no1 == 3 && flagBox.flag_no2 == 2)
                            {
                                flagBox.flag_no2 = 1;
                                flagBox.comp = COMPARISON.GreaterEq;
                            }
                        }
                    }
                }
            }
        }

        public ItemID GetItemIDForLocation(LocationID locationID)
        {
            locationToItemMap.TryGetValue(locationID, out ItemID id);
            return id;
        }

        public LocationID GetLocationIDForMural(SnapShotTargetScript snapTarget)
        {
            if (snapTarget.itemName == ItemDatabaseSystem.ItemNames.BeoEgLana)
            {
                return (LocationID)snapTarget.itemName;
            }
            else if (snapTarget.itemName == ItemDatabaseSystem.ItemNames.Mantra)
            {
                switch (snapTarget.cellName)
                {
                    case "": return LocationID.MantraMural;
                    case "mantra1": return LocationID.HeavenMantraMural;
                    case "mantra2": return LocationID.EarthMantraMural;
                    case "mantra3": return LocationID.SunMantraMural;
                    case "mantra4": return LocationID.MoonMantraMural;
                    case "mantra5": return LocationID.SeaMantraMural;
                    case "mantra6": return LocationID.FireMantraMural;
                    case "mantra7": return LocationID.WindMantraMural;
                    case "mantra8": return LocationID.MotherMantraMural;
                    case "mantra9": return LocationID.ChildMantraMural;
                    case "mantra10": return LocationID.NightMantraMural;
                    default: return LocationID.None;
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
            else if (itemID >= ItemID.CrystalSkull1 && itemID <= ItemID.CrystalSkull12)
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
                if (itemID == ItemID.ChainWhip || itemID == ItemID.SilverShield || itemID == ItemID.MobileSuperx3P)
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

        public void Initialise(L2ShopDataBase shopDataBase, L2TalkDataBase talkDataBase, patched_L2System system)
        {
            this.shopDataBase = shopDataBase;
            this.talkDataBase = talkDataBase;
            sys = system;
#if DEV
            DevUI devUI = gameObject.AddComponent<DevUI>() as DevUI;
            devUI.Initialise(this, sys);
#endif
            StartCoroutine(Setup());
        }

        bool LoadSeed()
        {
            locationToItemMap = new Dictionary<LocationID, ItemID>();
            cursedChests = new List<LocationID>();
            try
            {
                using (BinaryReader br = new BinaryReader(File.Open(Path.Combine(Directory.GetCurrentDirectory(),
                    "LaMulana2Randomizer\\Seed\\seed.lm2r"), FileMode.Open)))
                {
                    AutoScanTablets = br.ReadBoolean();
                    int itemCount = br.ReadInt32();
                    for (int i = 0; i < itemCount; i++)
                    {
                        locationToItemMap.Add((LocationID)br.ReadInt32(), (ItemID)br.ReadInt32());
                    }
                    for(int i = 0; i < 4; i++)
                    {
                        cursedChests.Add((LocationID)br.ReadInt32());
                    }
                    if (itemCount != locationToItemMap.Count)
                    {
                        message = "Seed Failed to load, mismatch between the expected and actual item count.";
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
            message = "Successfully loaded seed.";
            return true;
        }

        public IEnumerator Setup()
        {
            yield return new WaitForSeconds(0.1f);
            sys.setKeyBlock(true);
            yield return new WaitForSeconds(5f);
            //Check to seed is seed was successfully loaded
            if (LoadSeed())
            {
                ChangeShopItems();
                ChangeShopThanks();
                ChangeDialogueItems();
                MojiScriptFixes();
                yield return StartCoroutine(GetCursedChest());
                Randomising = true;
            }
            sys.setKeyBlock(false);
        }

        private IEnumerator GetCursedChest()
        {
            var ao = SceneManager.LoadSceneAsync("field04");
            while (!ao.isDone)
                yield return null;

            foreach (TreasureBoxScript box in FindObjectsOfType<TreasureBoxScript>())
            {
                if (box.curseMode && cursedChest == null)
                {
                    cursedChest = Instantiate(box.gameObject);
                    cursedChest.name = "Cursed Chest Prefab";
                    DontDestroyOnLoad(cursedChest);
                    cursedChest.SetActive(false);
                }
            }
            sys.reInitSystem();
        }

        private IEnumerator ChangeTreasureBoxes()
        {
            List<TreasureBoxScript> oldboxes = new List<TreasureBoxScript>();
            foreach (TreasureBoxScript box in FindObjectsOfType<TreasureBoxScript>())
            {
                ItemData oldItemData = GetItemDataFromName(box.itemObj.name);
                if (oldItemData == null)
                    continue;

                LocationID locationID = (LocationID)oldItemData.getItemName();

                if (oldItemData != null && locationToItemMap.TryGetValue(locationID, out ItemID newItemID))
                {
                    ItemInfo newItemInfo = ItemDB.GetItemInfo(newItemID);
                    TreasureBoxScript newBox;
                    if (IsLocationCursed(locationID))
                    {
                        GameObject obj = Instantiate(cursedChest, box.transform.position, box.transform.rotation);
                        newBox = obj.GetComponent<TreasureBoxScript>();
                        newBox.curseMode = true;
                        newBox.forceOpenFlags = box.forceOpenFlags;
                        newBox.itemFlags = box.itemFlags;
                        newBox.openActionFlags = box.openActionFlags;
                        newBox.openFlags = box.openFlags;
                        newBox.unlockFlags = box.unlockFlags;
                        newBox.itemObj = box.itemObj;
                        obj.SetActive(true);
                        obj.transform.SetParent(box.transform.parent);
                        oldboxes.Add(box);
                    }
                    else
                    {
                        newBox = box;
                        newBox.curseMode = false;
                    }

                    //Change the Treasure Boxs open flags to correspond to the new item
                    //These flags are used to so the chest stays open after you get the item
                    foreach (L2FlagBoxParent flagBoxParent in newBox.openFlags)
                    {
                        foreach (L2FlagBox flagBox in flagBoxParent.BOX)
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

                    EventItemScript item = newBox.itemObj.GetComponent<EventItemScript>();

                    //Change the Event Items active flags to correspond to the new item
                    //These flags are used to set the item inactive after you have got it
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
                    //Change the Event Items get flags to correspond to the new item
                    //These are flags that are set when the item is gotten
                    item.itemGetFlags = CreateGetFlags(newItemID, newItemInfo);

                    //Change the name used when calling setitem to correspond to new item
                    item.itemLabel = newItemInfo.boxName;

                    //Change the sprite to correspond to new item
                    Sprite sprite;
                    //Mantras don't have an icon so use the Mantra software icon
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
            yield return new WaitForEndOfFrame();
            foreach(var box in oldboxes)
                box.gameObject.SetActive(false);
        }

        private void ChangeEventItems()
        {
            foreach (EventItemScript item in FindObjectsOfType<EventItemScript>())
            {
                ItemData oldItemData = GetItemDataFromName(item.name);
                if (oldItemData == null)
                    continue;

                if (oldItemData != null && locationToItemMap.TryGetValue((LocationID)oldItemData.getItemName(), out ItemID newItemID))
                {
                    ItemInfo newItemInfo = ItemDB.GetItemInfo(newItemID);

                    //Change the Event Items active flags to correspond to the new item
                    //These flags are used to set the item inactive after you have got it
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
                    //Change the Event Items get flags to correspond to the new item
                    //These are flags that are set when the item is gotten
                    item.itemGetFlags = CreateGetFlags(newItemID, newItemInfo);

                    //Change the name used when calling setitem to correspond to new item
                    item.itemLabel = newItemInfo.boxName;

                    //Change the sprite to correspond to new item
                    Sprite sprite;
                    //Mantras don't have an icon so use the Mantra software icon
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
        
        private bool IsLocationCursed(LocationID locationID)
        {
            foreach(LocationID cursedID in cursedChests)
            {
                if(locationID == cursedID)
                    return true;
            }
            return false;
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
            shopDataBase.cellData[7][24][1][0] = CreateShopItemsString(LocationID.PeibalusaShop1, LocationID.PeibalusaShop2, LocationID.PeibalusaShop3);
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
            return string.Format("{0}\n{1}\n{2}", CreateSetItemString(firstSpot), CreateSetItemString(secondSpot), CreateSetItemString(thirdSpot));
        }

        private string CreateSetItemString(LocationID locationID)
        {
            if (locationToItemMap.TryGetValue(locationID, out ItemID newItemID))
            {
                ItemInfo newItemInfo = ItemDB.GetItemInfo(newItemID);
                return string.Format("[@sitm,{0},{1},{2},{3}]", newItemInfo.shopType, newItemInfo.shopName, newItemInfo.shopPrice, newItemInfo.shopAmount);
            }
            return string.Empty;
        }

        private void ChangeShopThanks()
        {
            ChangeThanksStrings(LocationID.SidroShop1, LocationID.SidroShop2, LocationID.SidroShop3,0,9);
            ChangeThanksStrings(LocationID.ModroShop1, LocationID.ModroShop2, LocationID.ModroShop3,1,9,2,3);
            ChangeThanksStrings(LocationID.NeburShop1, LocationID.NeburShop2, LocationID.NeburShop3,2,8);
            ChangeThanksStrings(LocationID.HinerShop1, LocationID.HinerShop2, LocationID.HinerShop3,3,9);
            ChangeThanksStrings(LocationID.HinerShop1, LocationID.HinerShop2, LocationID.HinerShop4,4,8);
            ChangeThanksStrings(LocationID.KorobokShop1, LocationID.KorobokShop2, LocationID.KorobokShop3,5,8);
            ChangeThanksStrings(LocationID.PymShop1, LocationID.PymShop2, LocationID.PymShop3,6,8);
            ChangeThanksStrings(LocationID.PeibalusaShop1, LocationID.PeibalusaShop2, LocationID.PeibalusaShop3,7,8);
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

        private string CreateGetFlagString(LocationID locationID)
        {
            string flagString = string.Empty;

            if (locationToItemMap.TryGetValue(locationID, out ItemID newItemID))
            {
                ItemInfo newItemInfo = ItemDB.GetItemInfo(newItemID);

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
                            flagString += string.Format("\n[@setf,{0},{1},+,{2}]", flag.seet_no1, flag.flag_no1, flag.data);
                        }
                        else if (flag.calcu == CALCU.EQR)
                        {
                            flagString += string.Format("\n[@setf,{0},{1},=,{2}]", flag.seet_no1, flag.flag_no1, flag.data);
                        }
                    }
                }
            }
            return flagString;
        }

        private void ChangeDialogueItems()
        {
            //Xelpud's item
            talkDataBase.cellData[1][10][1][0] = ChangeTalkString(LocationID.XelpudItem,
                "{0}[@setf,3,31,=,1]\n[@setf,5,2,=,1]\n[@setf,5,20,=,2]\n[@p,lastC]");

            //Nebur's item
            talkDataBase.cellData[0][11][1][0] = ChangeTalkString(LocationID.NeburItem,
                "[@anim,thanks,1]\n{0}[@setf,2,127,=,1]\n[@setf,2,128,=,1]\n[@setf,2,129,=,1]\n[@setf,2,130,=,1]\n[@setf,5,3,=,1]\n[@out]");
            
            //If you say to nebur's map xelpud gives it too you instead at some point, should never need to use as this isnt in logic
            talkDataBase.cellData[1][70][1][0] = ChangeTalkString(LocationID.NeburItem,
                "{0}[@setf,2,127,=,1]\n[@setf,5,4,=,2]\n[@anim,talk,1]");

            //Alsedana's item
            talkDataBase.cellData[2][13][1][0] = ChangeTalkString(LocationID.AlsedanaItem,
                "{0}[@anim,talk,1]\n[@setf,1,54,=,1]\n[@p,2nd-6]");

            //Giltoriyo's item
            talkDataBase.cellData[3][5][1][0] = ChangeTalkString(LocationID.GiltoriyoItem,
                "{0}[@setf,1,54,=,1]\n[@anim,talk,1]\n[@p,1st-3]");

            //Check to see if you can get Giltoriyo's item
            talkDataBase.cellData[3][3][1][0] = ChangeTalkFlagCheck(LocationID.GiltoriyoItem, COMPARISON.Greater, "[@iff,5,62,=,7,giltoriyo,9th]\n[@iff,2,{0},&gt;,{1},giltoriyo,8th]\n" +
                "[@iff,5,62,=,6,giltoriyo,7th]\n[@iff,5,62,=,5,giltoriyo,6th]\n[@iff,5,62,=,4,giltoriyo,5th]\n[@iff,5,62,=,3,giltoriyo,4th]\n[@iff,5,62,=,2,giltoriyo,2nd]\n" +
                "[@exit]\n[@anim,talk,1]\n[@p,1st]");
            
            //Didnt talk to Alsedana after vritra
            talkDataBase.cellData[3][7][1][0] = ChangeTalkStringAndFlagCheck(LocationID.AlsedanaItem,
                "[@iff,2,{0},&gt;,{1},giltoriyo,2nd]\n[@exit]\n{2}[@anim,talk,1]\n[@p,1st-5]");

            //Fobos' 1st item
            talkDataBase.cellData[6][9][1][0] = ChangeTalkString(LocationID.FobosItem,
                "[@setf,5,16,=,5]\n[@anim,talk,1]\n{0}[@p,3rd-2]");

            //Fobo' 2nd item
            talkDataBase.cellData[5][24][1][0] = ChangeTalkString(LocationID.FobosItem2, 
                "[@exit]\n[@anim,talk,1]\n[@setf,23,15,=,4]\n{0}[@p,lastC]");

            //Fobos' 2nd item check to see if you don't have the item
            talkDataBase.cellData[5][3][1][0] = ChangeTalkFlagCheck(LocationID.FobosItem2, COMPARISON.Less, "[@iff,5,16,=,0,fobos,1st]\n[@iff,2,{0},&lt;,{1},fobos,gS2]\n[@anifla,mfanim,wait]\n" +
                "[@iff,23,15,=,1,fobos,getSkull]\n[@iff,5,68,=,1,fobos,skullFull]\n[@iff,5,69,=,1,fobos,mirror]\n[@iff,5,78,=,4,fobos,hint1]\n[@iff,5,79,=,1,fobos,hint2]\n[@iff,5,80,=,1,fobos,hint3]\n" +
                "[@iff,5,81,=,1,fobos,hint4]\n[@iff,5,82,=,1,fobos,hint5]\n[@iff,5,83,=,1,fobos,hint6]\n[@iff,5,84,=,1,fobos,hint7]\n[@iff,5,85,=,1,fobos,hint8]\n[@iff,5,16,&gt;,12,fobos,8th]\n" +
                "[@iff,5,16,=,12,fobos,7th]\n[@iff,5,16,=,10,fobos,6th]\n[@iff,5,16,=,9,fobos,5th]\n[@iff,5,16,=,8,fobos,4th]\n[@iff,5,16,=,7,fobos,3rd]\n[@iff,5,16,=,6,fobos,2nd]\n[@iff,5,16,=,5,fobos,2nd]\n" +
                "[@anifla,mfanim,nochar]\n[@nochar]");

            //Freya's item
            talkDataBase.cellData[7][7][1][0] = ChangeTalkString(LocationID.FreyasItem,
                "[@anim,talk,1]\n{0}[@setf,5,67,=,1]\n[@p,lastC]");

            //Add check too Freya's starting mojiscript so she gives the item if you havent got it yet
            talkDataBase.cellData[7][3][1][0] = ChangeTalkFlagCheck(LocationID.FreyasItem, COMPARISON.Less,"[@anifla,mfanim,wait2]\n[@iff,2,{0},&lt;,{1},freyja,1st-1]\n[@iff,3,95,&gt;,0,freyja,escape]\n" +
                "[@anifla,mfanim,wait]\n[@iff,3,35,&gt;,7,freyja,8th]\n[@iff,3,35,=,6,freyja,7th3]\n[@iff,3,35,&gt;,3,freyja,7th2]\n[@iff,3,35,=,3,freyja,ragna]\n[@iff,3,35,=,2,freyja,4th]\n" +
                "[@iff,3,35,=,1,freyja,3rd]\n[@iff,5,67,=,1,freyja,2nd]\n[@exit]\n[@anim,talk,1]\n[@p,1st-1]");

            //Mulbruk's item
            talkDataBase.cellData[10][42][1][0] = ChangeTalkString(LocationID.MulbrukItem,
                "{0}[@setf,5,101,=,2]\n[@anim,talk,1]\n[@p,3rd-2]");

            //Add check too Mulbruk to see if you have her item
            talkDataBase.cellData[10][3][1][0] = ChangeTalkFlagCheck(LocationID.MulbrukItem, COMPARISON.Less, "[@iff,2,{0},&lt;,{1},mulbruk2,3rd]\n[@iff,5,61,=,1,mulbruk2,mirror]\n" +
                "[@iff,5,86,=,1,mulbruk2,hint2]\n[@iff,5,87,=,1,mulbruk2,hint3]\n[@iff,5,88,=,1,mulbruk2,hint4]\n[@iff,5,89,=,1,mulbruk2,hint5]\n[@iff,5,90,=,1,mulbruk2,hint6]\n" +
                "[@iff,5,91,=,1,mulbruk2,hint7]\n[@iff,5,92,=,1,mulbruk2,hint8]\n[@iff,5,93,=,1,mulbruk2,hint9]\n[@iff,5,94,=,1,mulbruk2,hint10]\n[@iff,5,95,=,1,mulbruk2,hint11]\n" +
                "[@iff,3,33,&gt;,10,mulbruk2,5th]\n[@iff,5,0,=,2,mulbruk2,4th]\n[@anifla,mfanim,wait]\n[@iff,5,78,=,5,mulbruk2,hint1]\n[@anifla,mfanim,wait4]\n" +
                "[@iff,3,33,=,10,mulbruk2,3rdRnd]\n[@anifla,mfanim,wait]\n[@iff,5,78,=,5,mulbruk2,hint1]\n[@anifla,mfanim,wait3]\n[@iff,3,33,=,6,mulbruk2,rTalk2]\n" +
                "[@anifla,mfanim,wait2]\n[@iff,3,33,=,5,mulbruk2,rTalk1]\n[@anifla,mfanim,nochar]\n[@iff,3,33,=,4,mulbruk2,1st-8]\n[@iff,3,33,=,8,mulbruk2,1st-8]\n" +
                "[@anifla,mfanim,wait]\n[@iff,3,33,&lt;,7,mulbruk2,1st]\n[@iff,3,33,=,7,mulbruk2,2nd]\n");

            //Osiris' item
            talkDataBase.cellData[78][7][1][0] = ChangeTalkStringAndFlagCheck(LocationID.OsirisItem,
                "[@iff,2,{0},&gt;,{1},f15-3,2nd]\n{2}[@anim,talk,1]\n[@p,lastC]");
        }

        private string ChangeTalkString(LocationID locationID, string original)
        {
            if (locationToItemMap.TryGetValue(locationID, out ItemID newItemID))
            {
                ItemInfo newItemInfo = ItemDB.GetItemInfo(newItemID);
                
                string take;
                if (newItemInfo.boxName.Equals("Crystal S") || newItemInfo.boxName.Equals("Sacred Orb") || newItemInfo.boxName.Equals("MSX3p"))
                {
                    take = string.Format("[@take,{0},02item,1]\n", newItemInfo.boxName);
                }
                else
                {
                    take = string.Format("[@take,{0},02item,1]\n", newItemInfo.shopName);
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
                            take += string.Format("[@setf,{0},{1},+,{2}]\n", flag.seet_no1, flag.flag_no1, flag.data);
                        }
                        else if (flag.calcu == CALCU.EQR)
                        {
                            take += string.Format("[@setf,{0},{1},=,{2}]\n", flag.seet_no1, flag.flag_no1, flag.data);
                        }
                    }
                }

                return string.Format(original, take);
            }
            return string.Empty;
        }

        private string ChangeTalkFlagCheck(LocationID locationID, COMPARISON comp, string original)
        {
            if (locationToItemMap.TryGetValue(locationID, out ItemID newItemID))
            {
                ItemInfo newItemInfo = ItemDB.GetItemInfo(newItemID);

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
                else if (comp == COMPARISON.Less)
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

                return string.Format(original, newItemInfo.itemFlag, flagValue);
            }
            return string.Empty;
        }

        private string ChangeTalkStringAndFlagCheck(LocationID locationID, string original)
        {
            if (locationToItemMap.TryGetValue(locationID, out ItemID newItemID))
            {
                ItemInfo newItemInfo = ItemDB.GetItemInfo(newItemID);

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
                    takeString = string.Format("[@take,{0},02item,1]\n", newItemInfo.boxName);
                }
                else
                {
                    takeString = string.Format("[@take,{0},02item,1]\n", newItemInfo.shopName);
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
                            takeString += string.Format("[@setf,{0},{1},+,{2}]\n", flag.seet_no1, flag.flag_no1, flag.data);
                        }
                        else if (flag.calcu == CALCU.EQR)
                        {
                            takeString += string.Format("[@setf,{0},{1},=,{2}]\n", flag.seet_no1, flag.flag_no1, flag.data);
                        }
                    }
                }

                return string.Format(original, newItemInfo.itemFlag, flagValue, takeString);
            }
            return string.Empty;
        }

        private void MojiScriptFixes()
        {
            //Changes to Nebur's scripts so that she stays until you take her item or leave the surface
            shopDataBase.cellData[2][4][1][0] = "[@anim,smile,1]\n[@setf,5,27,+,1]";
            talkDataBase.cellData[0][10][1][0] = "[@anim,nejiru,1]\n[@out]";

            //Fobos's 2nd item check if you have a skull
            talkDataBase.cellData[5][23][1][0] = "[@iff,0,32,&gt;,0,fobos,gS3]\n[@anim,stalk,1]\n[@anifla,mnext,swait]";

            //Change Fairy King to set flag to open endless even if you have the pendant
            talkDataBase.cellData[8][10][1][0] = "[@exit]\n[@anim,talk,1]\n[@setf,3,34,=,2]\n[@setf,5,12,=,1]\n[@p,2nd-2]";

            //Change the Fairy King check on Freya's Pendant
            talkDataBase.cellData[8][3][1][0] = "[@iff,3,34,&gt;,3,freyr,5th]\n[@iff,3,34,=,3,freyr,4th]\n[@iff,3,34,=,2,freyr,3rd]\n[@iff,2,31,&gt;,0,freyr,2nd]\n[@iff,3,34,=,1,freyr,1stEnd]\n[@iff,3,34,=,0,freyr,1st]";

            //Add check to see if you have beaten 4 guardians so mulbruuk can give you the item
            talkDataBase.cellData[10][41][1][0] = "[@exit]\n[@anim,talk,1]\n[@setf,3,33,=,10]\n[@iff,3,0,&gt;,3,mulbruk2,3rd-1]\n[@p,lastC]";

            //fix giltoriyo early dialogue exit
            talkDataBase.cellData[3][6][1][0] = "[@setf,5,62,=,2]\n[@setf,1,7,=,0]\n[@anim,talk,1]\n[@p,1st-4]";
        }
    }
}