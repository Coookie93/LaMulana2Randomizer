using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using L2Word;
using L2Flag;
using L2MobTask;
using LM2RandomiserMod.Patches;
using LaMulana2RandomizerShared;

namespace LM2RandomiserMod
{
    public class ShopItem
    {
        public ItemID ID;
        public int Multiplier;

        public ShopItem(ItemID id, int multiplier)
        {
            ID = id;
            Multiplier = multiplier;
        }
    }

    public class L2Rando : MonoBehaviour
    {
        public bool StartingGame = false;

        private Dictionary<LocationID, ItemID> locationToItemMap;
        private Dictionary<LocationID, ShopItem> shopToItemMap;
        private List<LocationID> cursedChests;
        private Dictionary<ExitID, ExitID> exitToExitMap;
        private Dictionary<ExitID, int> soulGateValueMap;
        private bool randomSoulGates;
        private bool randomDissonance;
        private bool autoPlaceSkull;
        private bool easyEchidna;
        private int requiredGuardians;
        private int itemChestColour;
        private int weightChestColour;
        private string startFieldName;

        private patched_L2System sys;
        private L2ShopDataBase shopDataBase;
        private L2TalkDataBase talkDataBase;

        private Dictionary<string, GameObject> objects;

        private Font font = null;
        private GUIStyle style = null;
        private bool onTitle;
        private bool loading;
        private string message;

        public bool IsRandomising { get; private set; }
        public ItemID StartingWeapon { get; private set; }
        public AreaID StartingArea { get; private set; }
        public List<ItemID> StartingItems { get; private set; }
        public int RequiredSkulls { get; private set; }
        public bool AutoScanTablets { get; private set; }
        public bool RemoveITStatue { get; private set; }
        public int StartingMoney { get; private set; }
        public int StartingWeights { get; private set; }

        public void OnGUI()
        {
            if (onTitle || !string.IsNullOrEmpty(message))
            {
                if (font == null)
                    font = Font.CreateDynamicFontFromOSFont("Consolas", 14);

                if (style == null)
                {
                    style = new GUIStyle(GUI.skin.label);
                    style.normal.textColor = Color.white;
                    style.font = font;
                    style.fontStyle = FontStyle.Bold;
                }

                GUIContent verContent = new GUIContent("La-Mulana 2 Randomiser " + LaMulana2RandomizerShared.Version.version);
                style.fontSize = 14;
                Vector2 verSize = style.CalcSize(verContent);
                GUI.Label(new Rect(0, 0, verSize.x, verSize.y), verContent, style);
                style.fontSize = 10;
                GUI.Label(new Rect(0, verSize.y, 500, 50), message, style);
            }

            if (loading)
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Color.black, 0, 0);
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
            onTitle = scene.name.Equals("title");

            if (scene.name.Equals("fieldLast") || scene.name.Equals("title"))
                return;

            //using (StreamWriter sr = new StreamWriter(File.Open(Path.Combine(Directory.GetCurrentDirectory(), "log.txt"), FileMode.Open)))
            //{
                try
                {
                    message = string.Empty;
                    if (IsRandomising)
                    {
                        CreateStartingFieldObjects(scene.name);
                        StartCoroutine(ChangeTreasureChests());
                        ChangeEventItems();
                        StartCoroutine(ChangeEntrances(scene.name));
                        DissonanceChests(scene.name);
                        ChangeFlagWatchers(scene.name);
                        FieldSpecificChanges(scene.name);
                        AddAnchorPoints(scene.name);
                        ObjectChanges();
                    }
                   
                }        
                catch (Exception ex)
                {
                    message = ex.ToString();
                    //sr.WriteLine(ex.ToString());
                }
            //}
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
            List<L2FlagBoxEnd> getFlags = new List<L2FlagBoxEnd>();

            short data;
            if (itemID >= ItemID.SacredOrb0 && itemID <= ItemID.SacredOrb9)
            {
                getFlags.Add(new L2FlagBoxEnd { calcu = CALCU.ADD, seet_no1 = 0, flag_no1 = 2, data = 1 });
                getFlags.Add(new L2FlagBoxEnd { calcu = CALCU.EQR, seet_no1 = itemInfo.ItemSheet, flag_no1 = itemInfo.ItemFlag, data = 1 });
            }
            else if (itemID >= ItemID.CrystalSkull1 && itemID <= ItemID.CrystalSkull12)
            {
                getFlags.Add(new L2FlagBoxEnd { calcu = CALCU.ADD, seet_no1 = 0, flag_no1 = 32, data = 1 });
                getFlags.Add(new L2FlagBoxEnd { calcu = CALCU.ADD, seet_no1 = 3, flag_no1 = 30, data = 4 });
                if (autoPlaceSkull)
                {
                    getFlags.Add(new L2FlagBoxEnd { calcu = CALCU.ADD, seet_no1 = 5, flag_no1 = (int)itemID - 108, data = 1 });
                    getFlags.Add(new L2FlagBoxEnd { calcu = CALCU.ADD, seet_no1 = 5, flag_no1 = 47, data = 1 });
                }
                getFlags.Add(new L2FlagBoxEnd { calcu = CALCU.EQR, seet_no1 = itemInfo.ItemSheet, flag_no1 = itemInfo.ItemFlag, data = 1 });
            }
            else if ((itemID >= ItemID.AnkhJewel1 && itemID <= ItemID.AnkhJewel9) || Array.IndexOf(storyItems, itemID) > -1)
            {
                data = 4;
                if (itemID == ItemID.GrappleClaw || itemID == ItemID.HolyGrail)
                    data = 2;
                getFlags.Add(new L2FlagBoxEnd { calcu = CALCU.ADD, seet_no1 = 3, flag_no1 = 30, data = data });

                data = 1;
                if (itemID == ItemID.LampofTime)
                    data = 2;
                getFlags.Add(new L2FlagBoxEnd { calcu = CALCU.EQR, seet_no1 = itemInfo.ItemSheet, flag_no1 = itemInfo.ItemFlag, data = data });
            }
            else if (itemID >= ItemID.ProgressiveBeherit1 && itemID <= ItemID.ProgressiveBeherit7)
            {
                getFlags.Add(new L2FlagBoxEnd { calcu = CALCU.ADD, seet_no1 = 2, flag_no1 = 3, data = 1 });
                getFlags.Add(new L2FlagBoxEnd { calcu = CALCU.EQR, seet_no1 = itemInfo.ItemSheet, flag_no1 = itemInfo.ItemFlag, data = 1 });
            }
            else
            {
                data = 1;
                if (itemID == ItemID.MobileSuperx3P)
                    data = 2;

                getFlags.Add(new L2FlagBoxEnd { calcu = CALCU.EQR, seet_no1 = itemInfo.ItemSheet, flag_no1 = itemInfo.ItemFlag, data = data });
            }


            return getFlags.ToArray();
        }

        public void Initialise(L2ShopDataBase shopDataBase, L2TalkDataBase talkDataBase, patched_L2System system)
        {
            this.shopDataBase = shopDataBase;
            this.talkDataBase = talkDataBase;
            sys = system;
            gameObject.AddComponent<ItemTracker>();
#if DEV
            DevUI devUI = gameObject.AddComponent<DevUI>() as DevUI;
            devUI.Initialise(sys);
#endif
            StartCoroutine(InitalSetup());
        }

        private bool LoadSeedFile()
        {
            StartingItems = new List<ItemID>();
            locationToItemMap = new Dictionary<LocationID, ItemID>();
            shopToItemMap = new Dictionary<LocationID, ShopItem>();
            cursedChests = new List<LocationID>();
            exitToExitMap = new Dictionary<ExitID, ExitID>();
            soulGateValueMap = new Dictionary<ExitID, int>();
            try
            {
                using (BinaryReader br = new BinaryReader(File.Open(Path.Combine(Directory.GetCurrentDirectory(),
                    Path.Combine("LaMulana2Randomizer", Path.Combine("Seed", "seed.lm2r"))), FileMode.Open)))
                {
                    StartingWeapon = (ItemID)br.ReadInt32();
                    StartingArea = (AreaID)br.ReadInt32();
                    SetStartFieldName(StartingArea);
                    randomDissonance = br.ReadBoolean();
                    requiredGuardians = br.ReadInt32();
                    RequiredSkulls = br.ReadInt32();
                    RemoveITStatue = br.ReadBoolean();
                    easyEchidna = br.ReadBoolean();
                    AutoScanTablets = br.ReadBoolean();
                    autoPlaceSkull = br.ReadBoolean();
                    StartingMoney = br.ReadInt32();
                    StartingWeights = br.ReadInt32();
                    itemChestColour = br.ReadInt32();
                    weightChestColour = br.ReadInt32();

                    int startingItems = br.ReadInt32();
                    for (int i = 0; i < startingItems; i++)
                        StartingItems.Add((ItemID)br.ReadInt32());

                    int itemCount = br.ReadInt32();
                    for (int i = 0; i < itemCount; i++)
                        locationToItemMap.Add((LocationID)br.ReadInt32(), (ItemID)br.ReadInt32());

                    int shopItemCount = br.ReadInt32();
                    for (int i = 0; i < shopItemCount; i++)
                        shopToItemMap.Add((LocationID)br.ReadInt32(), new ShopItem((ItemID)br.ReadInt32(), br.ReadInt32()));

                    int cursedCount = br.ReadInt32();
                    for (int i = 0; i < cursedCount; i++)
                        cursedChests.Add((LocationID)br.ReadInt32());

                    int exitPairCount = br.ReadInt32();
                    for (int i = 0; i < exitPairCount; i++)
                    {
                        ExitID exit1 = (ExitID)br.ReadInt32();
                        ExitID exit2 = (ExitID)br.ReadInt32();
                        exitToExitMap.Add(exit1, exit2);
                        exitToExitMap.Add(exit2, exit1);
                    }

                    int soulGatePairs = br.ReadInt32();
                    randomSoulGates = soulGatePairs > 0;
                    for (int i = 0; i < soulGatePairs; i++)
                    {
                        ExitID gate1 = (ExitID)br.ReadInt32();
                        ExitID gate2 = (ExitID)br.ReadInt32();
                        exitToExitMap.Add(gate1, gate2);
                        exitToExitMap.Add(gate2, gate1);

                        int soulValue = br.ReadInt32();
                        soulGateValueMap.Add(gate1, soulValue);
                        soulGateValueMap.Add(gate2, soulValue);
                    }
                }
            }
            catch (Exception ex)
            {
                message = "Failed to load seed: " + ex.ToString();
                return false;
            }
            message = "Successfully loaded seed.";
            return true;
        }


        private void SetStartFieldName(AreaID startID)
        {
            switch (StartingArea)
            {
                case AreaID.RoY: startFieldName = "field00"; break;
                case AreaID.AnnwfnMain: startFieldName = "field02"; break;
                case AreaID.IBMain: startFieldName = "field03"; break;
                case AreaID.ITLeft: startFieldName = "field04"; break;
                case AreaID.DFMain: startFieldName = "field05"; break;
                case AreaID.ValhallaMain: startFieldName = "field10"; break;
                case AreaID.DSLMMain: startFieldName = "field11"; break;
                case AreaID.ACTablet: startFieldName = "field12"; break;
                default: startFieldName = string.Empty; break;
            }
        }


        private IEnumerator InitalSetup()
        {
            yield return new WaitForSeconds(0.1f);
            //stop the game from accepting inputs when we are doing the initial loading of scenes to get objects from them
            sys.setKeyBlock(true);
            yield return new WaitForSeconds(5f);
            loading = true;
            yield return StartCoroutine(GetGameObjects());
            ApplySeed();
            yield return new WaitForSeconds(1f);
            sys.setKeyBlock(false);
            loading = false;
        }

        private void ApplySeed()
        {
            if (LoadSeedFile())
            {
                ChangeShopItems();
                ChangeShopThanks();
                ChangeDialogueItems();
                MojiScriptFixes();
                IsRandomising = true;
            }
        }

        private IEnumerator GetGameObjects()
        {
            objects = new Dictionary<string, GameObject>();

            var ao = SceneManager.LoadSceneAsync("field04");
            while (!ao.isDone)
                yield return null;

            foreach (TreasureBoxScript box in FindObjectsOfType<TreasureBoxScript>())
            {
                if (box.closetMode)
                {
                    GameObject obj = Instantiate(box.gameObject);
                    obj.name = "Turquise Chest Prefab";
                    DontDestroyOnLoad(obj);
                    obj.SetActive(false);
                    objects.Add("turquiseChest", obj);
                    break;
                }
            }

            foreach (Animator animator in FindObjectsOfType<Animator>())
            {
                if (animator.name.Equals("Curse Tresure"))
                {
                    GameObject obj = Instantiate(animator.gameObject);
                    obj.name = "Curse Prefab";
                    obj.SetActive(false);
                    objects.Add("curse", obj);
                    DontDestroyOnLoad(obj);
                    break;
                }
            }

            foreach (AnimatorController controller in FindObjectsOfType<AnimatorController>())
            {
                if (controller.name.Equals("soul_gate"))
                {
                    if (controller.CheckFlags[0].BOX[0].flag_no1 == 5 && !objects.ContainsKey("threeSoulgate"))
                    {
                        GameObject obj = Instantiate(controller.gameObject);
                        obj.name = "Three Soul Gate Prefab";
                        DontDestroyOnLoad(obj);
                        obj.SetActive(false);
                        objects.Add("threeSoulGate", obj);
                    }
                    else if (controller.CheckFlags[0].BOX[0].flag_no1 == 8 && !objects.ContainsKey("fiveSoulGate"))
                    {
                        GameObject obj = Instantiate(controller.gameObject);
                        obj.name = "Five Soul Gate Prefab";
                        DontDestroyOnLoad(obj);
                        obj.SetActive(false);
                        objects.Add("fiveSoulGate", obj);
                    }
                }
                else if (controller.name.Equals("soul_cont"))
                {
                    if (controller.CheckFlags[0].BOX[0].flag_no1 == 23 && !objects.ContainsKey("threeSoul"))
                    {
                        GameObject obj = Instantiate(controller.gameObject);
                        obj.name = "Three Soul Prefab";
                        DontDestroyOnLoad(obj);
                        obj.SetActive(false);
                        objects.Add("threeSoul", obj);
                    }
                    else if (controller.CheckFlags[0].BOX[0].flag_no1 == 26 && !objects.ContainsKey("fiveSoul"))
                    {
                        GameObject obj = Instantiate(controller.gameObject);
                        obj.name = "Five Soul Prefab";
                        DontDestroyOnLoad(obj);
                        obj.SetActive(false);
                        objects.Add("fiveSoul", obj);
                    }
                }
                else if (controller.name.Equals("E0Ladder", StringComparison.Ordinal))
                {
                    GameObject obj = Instantiate(controller.gameObject);
                    obj.name = "Ladder Prefab";
                    DontDestroyOnLoad(obj);
                    obj.SetActive(false);
                    objects.Add("ladder", obj);
                }
            }

            sys.reInitSystem();

            ao = SceneManager.LoadSceneAsync("field05");
            while (!ao.isDone)
                yield return null;

            foreach (TreasureBoxScript box in FindObjectsOfType<TreasureBoxScript>())
            {
                GameObject obj = Instantiate(box.gameObject);
                obj.name = "Blue Chest Prefab";
                DontDestroyOnLoad(obj);
                obj.SetActive(false);
                objects.Add("blueChest", obj);
                break;
            }

            foreach (AnimatorController controller in FindObjectsOfType<AnimatorController>())
            {
                if (controller.name.Equals("soul_gate") && !objects.ContainsKey("oneSoulGate"))
                {
                    GameObject obj = Instantiate(controller.gameObject);
                    obj.name = "One Soul Gate Prefab";
                    DontDestroyOnLoad(obj);
                    obj.SetActive(false);
                    objects.Add("oneSoulGate", obj);
                }
                else if (controller.name.Equals("soul_cont") && !objects.ContainsKey("oneSoul"))
                {
                    GameObject obj = Instantiate(controller.gameObject);
                    obj.name = "One Soul Prefab";
                    DontDestroyOnLoad(obj);
                    obj.SetActive(false);
                    objects.Add("oneSoul", obj);
                }
            }

            sys.reInitSystem();

            ao = SceneManager.LoadSceneAsync("field13");
            while (!ao.isDone)
                yield return null;

            foreach (AnimatorController controller in FindObjectsOfType<AnimatorController>())
            {
                if (controller.name.Equals("soul_gate") && !objects.ContainsKey("nineSoulGate"))
                {
                    GameObject obj = Instantiate(controller.gameObject);
                    obj.name = "Nine Soul Gate Prefab";
                    DontDestroyOnLoad(obj);
                    obj.SetActive(false);
                    objects.Add("nineSoulGate", obj);
                }
                else if (controller.name.Equals("soul_cont") && !objects.ContainsKey("nineSoul"))
                {
                    GameObject obj = Instantiate(controller.gameObject);
                    obj.name = "Nine Soul Prefab";
                    DontDestroyOnLoad(obj);
                    obj.SetActive(false);
                    objects.Add("nineSoul", obj);
                }
            }

            sys.reInitSystem();

            ao = SceneManager.LoadSceneAsync("fieldP00");
            while (!ao.isDone)
                yield return null;

            foreach (TreasureBoxScript box in FindObjectsOfType<TreasureBoxScript>())
            {
                if (box.closetMode)
                {
                    GameObject obj = Instantiate(box.gameObject);
                    obj.name = "Red Chest Prefab";
                    DontDestroyOnLoad(obj);
                    obj.SetActive(false);
                    objects.Add("redChest", obj);
                    break;
                }
            }

            sys.reInitSystem();

            ao = SceneManager.LoadSceneAsync("field07");
            while (!ao.isDone)
                yield return null;

            foreach (TreasureBoxScript box in FindObjectsOfType<TreasureBoxScript>())
            {
                if (box.closetMode)
                {
                    GameObject obj = Instantiate(box.gameObject);
                    obj.name = "Pink Chest Prefab";
                    DontDestroyOnLoad(obj);
                    obj.SetActive(false);
                    objects.Add("pinkChest", obj);
                    break;
                }
            }

            foreach (AnimatorController controller in FindObjectsOfType<AnimatorController>())
            {
                if (controller.name.Equals("soul_gate") && !objects.ContainsKey("twoSoulGate"))
                {
                    GameObject obj = Instantiate(controller.gameObject);
                    obj.name = "Two Soul Gate Prefab";
                    DontDestroyOnLoad(obj);
                    obj.SetActive(false);
                    objects.Add("twoSoulGate", obj);
                }
                else if (controller.name.Equals("soul_cont") && !objects.ContainsKey("twoSoul"))
                {
                    GameObject obj = Instantiate(controller.gameObject);
                    obj.name = "Two Soul Prefab";
                    DontDestroyOnLoad(obj);
                    obj.SetActive(false);
                    objects.Add("twoSoul", obj);
                }
            }

            sys.reInitSystem();

            ao = SceneManager.LoadSceneAsync("field12");
            while (!ao.isDone)
                yield return null;

            foreach (TreasureBoxScript box in FindObjectsOfType<TreasureBoxScript>())
            {
                if (box.closetMode)
                {
                    GameObject obj = Instantiate(box.gameObject);
                    obj.name = "Yellow Chest Prefab";
                    DontDestroyOnLoad(obj);
                    obj.SetActive(false);
                    objects.Add("yellowChest", obj);
                    break;
                }
            }

            sys.reInitSystem();
        }

        private ItemData GetItemDataFromName(string objName)
        {
            if (objName.Contains("ItemSym "))
            {
                string name = objName.Substring(8);

                if (name.Contains("SacredOrb"))
                    name = name.Insert(6, " ");
                else if (name.Equals("MSX3p"))
                    name = "MSX";

                return L2SystemCore.getItemData(name);
            }
            return null;
        }

        private LocationID GetLocationIDForResearch(L2FlagBoxParent[] flags)
        {
            foreach (L2FlagBoxParent flagBoxParent in flags)
            {
                foreach (L2FlagBox flagBox in flagBoxParent.BOX)
                {
                    if (flagBox.seet_no1 == 6 && flagBox.flag_no1 == 43) return LocationID.ResearchAnnwfn;
                    else if (flagBox.seet_no1 == 8 && flagBox.flag_no1 == 45) return LocationID.ResearchIT;
                    else if (flagBox.seet_no1 == 7 && flagBox.flag_no1 == 78) return LocationID.ResearchIBTopLeft;
                    else if (flagBox.seet_no1 == 7 && flagBox.flag_no1 == 79) return LocationID.ResearchIBTopRight;
                    else if (flagBox.seet_no1 == 7 && flagBox.flag_no1 == 80) return LocationID.ResearchIBTent1;
                    else if (flagBox.seet_no1 == 7 && flagBox.flag_no1 == 81) return LocationID.ResearchIBPit;
                    else if (flagBox.seet_no1 == 7 && flagBox.flag_no1 == 83) return LocationID.ResearchIBLeft;
                    else if (flagBox.seet_no1 == 7 && flagBox.flag_no1 == 85) return LocationID.ResearchIBTent2;
                    else if (flagBox.seet_no1 == 7 && flagBox.flag_no1 == 86) return LocationID.ResearchIBTent3;
                    else if (flagBox.seet_no1 == 15 && flagBox.flag_no1 == 44) return LocationID.ResearchDSLM;
                }
            }
            return LocationID.None;
        }

        private bool IsLocationCursed(LocationID locationID)
        {
            foreach (LocationID cursedID in cursedChests)
            {
                if (locationID == cursedID)
                    return true;
            }
            return false;
        }

        private TreasureBoxScript CreateChest(int colour, Vector3 position, Quaternion rotation)
        {
            switch (colour)
            {
                case 0: return Instantiate(objects["blueChest"], position, rotation).GetComponent<TreasureBoxScript>();
                case 1: return Instantiate(objects["turquiseChest"], position, rotation).GetComponent<TreasureBoxScript>();
                case 2: return Instantiate(objects["redChest"], position, rotation).GetComponent<TreasureBoxScript>();
                case 3: return Instantiate(objects["pinkChest"], position, rotation).GetComponent<TreasureBoxScript>();
                case 4: return Instantiate(objects["yellowChest"], position, rotation).GetComponent<TreasureBoxScript>();
                default: return null;
            }
        }

        private IEnumerator ChangeTreasureChests()
        {
            List<GameObject> curses = new List<GameObject>();
            foreach (Animator animator in FindObjectsOfType<Animator>())
            {
                if (animator.name.Equals("Curse Tresure"))
                    curses.Add(animator.gameObject);
            }

            List<TreasureBoxScript> oldChests = new List<TreasureBoxScript>();
            foreach (TreasureBoxScript oldChest in FindObjectsOfType<TreasureBoxScript>())
            {
                ItemData oldItemData = GetItemDataFromName(oldChest.itemObj.name);
                if (oldItemData == null)
                    continue;

                LocationID locationID = (LocationID)oldItemData.getItemName();
                if (locationToItemMap.TryGetValue(locationID, out ItemID newItemID))
                {
                    TreasureBoxScript newChest;
                    if (newItemID >= ItemID.ChestWeight01)
                        newChest = CreateChest(weightChestColour, oldChest.transform.position, oldChest.transform.rotation);
                    else
                        newChest = CreateChest(itemChestColour, oldChest.transform.position, oldChest.transform.rotation);

                    if (IsLocationCursed(locationID))
                    {
                        GameObject curse = Instantiate(objects["curse"], oldChest.transform.position, oldChest.transform.rotation);
                        curse.SetActive(true);
                        curse.transform.SetParent(newChest.transform);
                        newChest.curseAnime = curse.GetComponent<Animator>();
                        newChest.curseParticle = curse.GetComponent<ParticleSystem>();
                        newChest.curseMode = true;
                    }
                    else
                    {
                        newChest.curseMode = false;
                    }

                    newChest.closetMode = false;
                    newChest.forceOpenFlags = oldChest.forceOpenFlags;
                    newChest.itemFlags = oldChest.itemFlags;
                    newChest.openActionFlags = oldChest.openActionFlags;
                    newChest.openFlags = oldChest.openFlags;
                    newChest.unlockFlags = oldChest.unlockFlags;
                    newChest.itemObj = oldChest.itemObj;
                    newChest.transform.SetParent(oldChest.transform.parent);

                    ChangeChestItemFlags(newChest, newItemID); 
                    newChest.gameObject.SetActive(true);
                    oldChests.Add(oldChest);
                }
            }

            yield return new WaitForEndOfFrame();
            foreach (var box in oldChests)
                box.gameObject.SetActive(false);

            foreach (var obj in curses)
                obj.SetActive(false);
        }

        private void ChangeChestItemFlags(TreasureBoxScript chest, ItemID itemID)
        {
            ItemInfo itemInfo = ItemDB.GetItemInfo(itemID);

            //Change the Treasure Boxs open flags to correspond to the new item
            //These flags are used to so the chest stays open after you get the item
            foreach (L2FlagBoxParent flagBoxParent in chest.openFlags)
            {
                foreach (L2FlagBox flagBox in flagBoxParent.BOX)
                {
                    if (flagBox.seet_no1 == 2)
                    {
                        flagBox.seet_no1 = itemInfo.ItemSheet;
                        flagBox.flag_no1 = itemInfo.ItemFlag;
                        flagBox.flag_no2 = 1;

                        //msx flag starts at 1 so have to check against 2 as 2 means we have the upgrade
                        if (itemID == ItemID.MobileSuperx3P)
                        {
                            flagBox.flag_no2 = 2;
                        }
                    }
                }
            }

            EventItemScript item = chest.itemObj.GetComponent<EventItemScript>();

            //Change the Event Items active flags to correspond to the new item
            //These flags are used to set the item inactive after you have got it
            foreach (L2FlagBoxParent flagBoxParent in item.itemActiveFlag)
            {
                foreach (L2FlagBox flagBox in flagBoxParent.BOX)
                {
                    if (flagBox.seet_no1 == 2)
                    {
                        flagBox.seet_no1 = itemInfo.ItemSheet;
                        flagBox.flag_no1 = itemInfo.ItemFlag;
                        flagBox.comp = COMPARISON.Equal;
                        flagBox.flag_no2 = 0;

                        //msx flag starts at 1 so have to check against 1 not 0
                        if (itemID == ItemID.MobileSuperx3P)
                        {
                            flagBox.flag_no2 = 1;
                            flagBox.comp = COMPARISON.LessEq;
                        }
                    }
                }
            }
            //Change the Event Items get flags to correspond to the new item
            //These are flags that are set when the item is gotten
            item.itemGetFlags = CreateGetFlags(itemID, itemInfo);

            //Change the name used when calling setitem to correspond to new item
            item.itemLabel = itemInfo.BoxName;

            //if item is a weight set item value for flag checking
            if (itemID >= ItemID.ChestWeight01)
                item.itemValue = itemInfo.ItemFlag;

            //Change the sprite to correspond to new item if its not a weight
            if (itemID < ItemID.ChestWeight01)
                item.gameObject.GetComponent<SpriteRenderer>().sprite = GetItemSprite(itemInfo.BoxName, itemID);
        }

        private void ChangeEventItems()
        {
            foreach (EventItemScript item in FindObjectsOfType<EventItemScript>())
            {
                LocationID locationID;
                if (item.name.Contains("Research"))
                {
                    locationID = GetLocationIDForResearch(item.itemActiveFlag);
                }
                else
                {
                    ItemData oldItemData = GetItemDataFromName(item.name);
                    if (oldItemData != null)
                        locationID = (LocationID)oldItemData.getItemName();
                    else
                        continue;
                }

                if (locationToItemMap.TryGetValue(locationID, out ItemID newItemID))
                {
                    ItemInfo newItemInfo = ItemDB.GetItemInfo(newItemID);
                    if (locationID >= LocationID.ResearchAnnwfn && locationID <= LocationID.ResearchDSLM)
                    {
                        //stop this item becoming inaccessable under the failing pit of time thing
                        if (locationID == LocationID.ResearchIBPit)
                            item.gameObject.transform.position = item.gameObject.transform.position + new Vector3(0, 70, 0);

                        List<L2FlagBox> flags = new List<L2FlagBox>();
                        L2FlagBox flagBox = new L2FlagBox()
                        {
                            seet_no1 = newItemInfo.ItemSheet,
                            flag_no1 = newItemInfo.ItemFlag,
                            seet_no2 = -1,
                            flag_no2 = 0,
                            comp = COMPARISON.Equal,
                            logic = LOGIC.AND
                        };

                        //MSX flag starts at 1 so have to check against 1 not 0
                        if (newItemID == ItemID.MobileSuperx3P)
                        {
                            flagBox.flag_no2 = 1;
                            flagBox.comp = COMPARISON.LessEq;
                        }

                        flags.Add(flagBox);

                        if (locationID == LocationID.ResearchIBTent2)
                        {
                            flags.Add(new L2FlagBox()
                            {
                                seet_no1 = 3,
                                flag_no1 = 0,
                                seet_no2 = -1,
                                flag_no2 = 7,
                                comp = COMPARISON.GreaterEq,
                                logic = LOGIC.AND
                            });
                        }
                        else if (locationID == LocationID.ResearchIBTent3)
                        {
                            flags.Add(new L2FlagBox()
                            {
                                seet_no1 = 3,
                                flag_no1 = 86,
                                seet_no2 = -1,
                                flag_no2 = 1,
                                comp = COMPARISON.Equal,
                                logic = LOGIC.AND
                            });
                        }

                        item.itemActiveFlag[0].BOX = flags.ToArray();
                    }
                    else
                    {
                        //Change the Event Items active flags to correspond to the new item
                        //These flags are used to set the item inactive after you have got it
                        foreach (L2FlagBoxParent flagBoxParent in item.itemActiveFlag)
                        {
                            foreach (L2FlagBox flagBox in flagBoxParent.BOX)
                            {
                                if (flagBox.seet_no1 == 2)
                                {
                                    flagBox.seet_no1 = newItemInfo.ItemSheet;
                                    flagBox.flag_no1 = newItemInfo.ItemFlag;
                                    flagBox.comp = COMPARISON.Equal;
                                    flagBox.flag_no2 = 0;

                                    //msx flag starts at 1 so have to check against 1 not 0
                                    if (newItemID == ItemID.MobileSuperx3P)
                                    {
                                        flagBox.flag_no2 = 1;
                                        flagBox.comp = COMPARISON.LessEq;
                                    }
                                }
                            }
                        }
                    }

                    if (newItemID < ItemID.ChestWeight01)
                    {
                        //Change the Event Items get flags to correspond to the new item
                        //These are flags that are set when the item is gotten
                        item.itemGetFlags = CreateGetFlags(newItemID, newItemInfo);

                        //Change the name used when calling setitem to correspond to new item
                        item.itemLabel = newItemInfo.BoxName;

                        //Change the sprite to correspond to new itemwhip"
                        item.gameObject.GetComponent<SpriteRenderer>().sprite = GetItemSprite(newItemInfo.BoxName, newItemID);
                    }
                    else
                    {
                        //make a fake item instead of the item
                        GameObject obj = new GameObject(newItemID.ToString());
                        obj.transform.position = item.transform.position;
                        obj.transform.SetParent(item.transform.parent);

                        FakeItem fakeItem = obj.AddComponent<FakeItem>();
                        fakeItem.Init(sys, item.itemActiveFlag, newItemInfo.ItemFlag);

                        //random sprite
                        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
                        renderer.sprite = GetRandomSprite();
                        renderer.enabled = false;

                        //set old item to inactive
                        item.gameObject.SetActive(false);
                    }
                }
            }
        }

        private Sprite GetItemSprite(string itemName, ItemID itemID)
        {
            if (itemID == ItemID.Whip1 || itemID == ItemID.Whip2 || itemID == ItemID.Whip3)
            {
                //since whips are progressive we need to determine what level of whip the player currently has to show the next one
                short data = 0;
                string name = string.Empty;
                sys.getFlag(2, "Whip", ref data);
                if (data == 0) name = "Whip";
                else if (data == 1) name = "Whip2";
                else if (data >= 2) name = "Whip3";
                return L2SystemCore.getMapIconSprite(L2SystemCore.getItemData(name));
            }
            else if (itemID == ItemID.Shield1 || itemID == ItemID.Shield2 || itemID == ItemID.Shield3)
            {
                //since shields are progressive we need to determine what level of shield the player currently has to show the next one
                short data = 0;
                string name = string.Empty;
                sys.getFlag(2, 196, ref data);
                if (data == 0) name = "Shield";
                else if (data == 1) name = "Shield2";
                else if (data >= 2) name = "Shield3";
                return L2SystemCore.getMapIconSprite(L2SystemCore.getItemData(name));
            }
            else if (itemID >= ItemID.Research1 && itemID <= ItemID.Research10)
            {
                return L2SystemCore.getMapIconSprite(L2SystemCore.getItemData("Research"));
            }
            else if (itemID >= ItemID.ProgressiveBeherit1 && itemID <= ItemID.ProgressiveBeherit7)
            {
                return L2SystemCore.getMapIconSprite(L2SystemCore.getItemData("Beherit"));
            }
            else if (itemID >= ItemID.Heaven && itemID <= ItemID.Night)
            {
                //Mantras don't have an icon so use the Mantra software icon 
                return L2SystemCore.getMapIconSprite(L2SystemCore.getItemData("Mantra"));
            }
            else
            {
                return L2SystemCore.getMapIconSprite(L2SystemCore.getItemData(itemName));
            }
        }

        private Sprite GetRandomSprite()
        {
            ItemID itemID = (ItemID)UnityEngine.Random.Range(1, (int)ItemID.Research10);
            ItemInfo itemInfo = ItemDB.GetItemInfo(itemID);
            return GetItemSprite(itemInfo.BoxName, itemID);
        }

        private void DissonanceChests(string field)
        {
            if (!randomDissonance || field.Equals("lastBoss"))
                return;

            //destory the objects that give dissonance by using the beherit 
            GameObject toDestroy = null;
            foreach (var useItem in FindObjectsOfType<UsingItemTargetScript>())
            {
                foreach (var target in useItem.itemTargets)
                {
                    if (target.targetItem == L2Hit.USEITEM.USE_BEHERIT)
                    {
                        foreach (var flags in target.effectFlags)
                        {
                            if (flags.seet_no1 == 2 && flags.flag_no1 == 3)
                            {
                                toDestroy = useItem.gameObject;
                                break;
                            }
                        }
                    }
                }
            }
            Destroy(toDestroy);

            //turn off the dissonance effects
            foreach (var animator in FindObjectsOfType<Animator>())
            {
                if (animator.name.Equals("MinusSmoke")){
                    animator.gameObject.SetActive(false);
                    break;
                }
            }

            //set up data for the chest depending on the field 
            LocationID locationID = LocationID.None;
            Vector3 position = new Vector3(0,0,0);
            int sheet = 0, flag = 0;
            switch (field)
            {
                case "fieldL02":
                {
                    locationID = LocationID.DissonanceMoG;
                    position = new Vector3(926, -166, 8);
                    sheet = 5;
                    flag = 59;
                    break;
                }
                case "field09":
                {
                    locationID = LocationID.DissonanceHL;
                    position = new Vector3(-55, -270, 8);
                    sheet = 13;
                    flag = 39;
                    break; 
                }
                case "field10":
                {
                    locationID = LocationID.DissonanceValhalla;
                    position = new Vector3(1000, -6, 8);
                    sheet = 14;
                    flag = 42;
                    break; 
                }
                case "field11":
                {
                    locationID = LocationID.DissonanceDSLM;
                    position = new Vector3(175, -870, 8);
                    sheet = 15;
                    flag = 45;
                    break; 
                }
                case "field15":
                {
                    locationID = LocationID.DissonanceEPG;
                    position = new Vector3(-990, 138, 8);
                    sheet = 18;
                    flag = 55;
                    break;
                }
                case "fieldSpace":
                {
                    locationID = LocationID.DissonanceNibiru;
                    position = new Vector3(50, 138, 8);
                    sheet = 5;
                    flag = 60;
                    break;
                }
                default:
                    break;
            }

            if(locationToItemMap.TryGetValue(locationID, out ItemID itemID))
            {
                TreasureBoxScript chest;
                if (itemID >= ItemID.ChestWeight01)
                    chest = CreateChest(weightChestColour, position, objects["blueChest"].transform.rotation);
                else
                    chest = CreateChest(itemChestColour, position, objects["blueChest"].transform.rotation);

                if (IsLocationCursed(locationID))
                {
                    GameObject curse = Instantiate(objects["curse"], chest.transform.position, chest.transform.rotation);
                    curse.SetActive(true);
                    chest.curseAnime = curse.GetComponent<Animator>();
                    chest.curseParticle = curse.GetComponent<ParticleSystem>();
                    chest.curseMode = true;
                }
                else
                {
                    chest.curseMode = false;
                }

                chest.closetMode = false;
                chest.unlockFlags = new L2FlagBoxParent[]
                {
                    new L2FlagBoxParent()
                    {
                        logoc = LOGIC.AND,
                        BOX = new L2FlagBox[]
                        {
                            new L2FlagBox()
                            {
                                seet_no1 = sheet,
                                flag_no1 = flag,
                                seet_no2 = -1,
                                flag_no2 = 1,
                                logic = LOGIC.AND,
                                comp = COMPARISON.GreaterEq
                            },
                            new L2FlagBox()
                            {
                                seet_no1 = 2,
                                flag_no1 = 3,
                                seet_no2 = -1,
                                flag_no2 = 0,
                                logic = LOGIC.AND,
                                comp = COMPARISON.Greater
                            }
                        }
                    }
                };
                chest.openFlags = new L2FlagBoxParent[]
                {
                    new L2FlagBoxParent()
                    {
                        BOX = new L2FlagBox[]
                        {
                            new L2FlagBox()
                            {
                                seet_no1 = 2,
                                flag_no1 = -1,
                                seet_no2 = -1,
                                flag_no2 = 1,
                                logic = LOGIC.NON,
                                comp = COMPARISON.GreaterEq
                            }
                        }
                    }
                };

                ChangeChestItemFlags(chest, itemID);
                chest.gameObject.SetActive(true);
            }
        }

        private void FieldSpecificChanges(string fieldName)
        {
            if (fieldName.Equals("field01-2"))
            {
                //move the pots on the starting screen
                foreach (ItemPotScript pot in FindObjectsOfType<ItemPotScript>())
                    pot.transform.position = new Vector3(pot.transform.position.x - 100, pot.transform.position.y, pot.transform.position.z);
            }
            else if (fieldName.Equals("field00"))
            {
                //remove the pillar that falls infront of Roots main gate during escape
                GameObject obj = GameObject.Find("endPiller");
                if (obj != null)
                    obj.SetActive(false);
            }
            else if (fieldName.Equals("field04"))
            {
                foreach (ShopGateScript shopGate in FindObjectsOfType<ShopGateScript>())
                {
                    //stop the first BTK shop from being unenterable after certain flags and turn the second version off
                    if (shopGate.name.Equals("ShopGate (1)"))
                        shopGate.shdowtask = null;
                    else if (shopGate.name.Equals("ShopGate (2)"))
                        shopGate.gameObject.SetActive(false);
                }
            }
            else if (fieldName.Equals("field10"))
            {
                //i hate this stupid trapdoor's hitbox
                StartCoroutine(FixTrapDoor());

                CorridorSealerFlagWatcher(new Vector3(48, 208, 0));
            }
            else if (fieldName.Equals("field11"))
            {
                CorridorSealerFlagWatcher(new Vector3(28, 504, 0));
            }
            else if (fieldName.Equals("field12"))
            {
                CorridorSealerFlagWatcher(new Vector3(210, 168, 0));
            }
            else if (fieldName.Equals("field13"))
            {
                CorridorSealerFlagWatcher(new Vector3(824, -544, 0));
            }
            else if (fieldName.Equals("field14"))
            {
                CorridorSealerFlagWatcher(new Vector3(-8, 48, 0));
            }
            else if (fieldName.Equals("field06-2"))
            {
                CorridorSealerFlagWatcher(new Vector3(572, -16, 0));
            }
            else if (fieldName.Equals("fieldSpace"))
            {
                //disable the object that stop the holy grail working in space
                foreach (HolyGrailCancellerScript grailCanceller in FindObjectsOfType<HolyGrailCancellerScript>())
                    grailCanceller.gameObject.SetActive(false);
            }
            else if (fieldName.Equals("fieldL08"))
            {
                //change the shadowtask that makes Freya stop talking in endless corridor to only happen after you get the item from her
                foreach (ShopGateScript talkGate in FindObjectsOfType<ShopGateScript>())
                {
                    if (talkGate.shdowtask != null)
                    {
                        foreach (L2FlagBoxParent flagBoxParent in talkGate.shdowtask.startflag)
                        {
                            foreach (L2FlagBox flagBox in flagBoxParent.BOX)
                            {
                                ItemID itemID = GetItemIDForLocation(LocationID.FreyasItem);
                                ItemInfo itemInfo = ItemDB.GetItemInfo(itemID);
                                flagBox.comp = COMPARISON.Less;
                                flagBox.seet_no1 = itemInfo.ItemSheet;
                                flagBox.flag_no1 = itemInfo.ItemFlag;
                                flagBox.flag_no2 = 1;
                                if (itemID == ItemID.MobileSuperx3P)
                                    flagBox.flag_no2 = 2;
                            }
                        }
                    }
                }
            }
            else if (fieldName.Equals("lastBoss"))
            {
                foreach (var useItem in FindObjectsOfType<UsingItemTargetScript>())
                {
                    foreach (var target in useItem.itemTargets)
                    {
                        if (target.targetItem == L2Hit.USEITEM.USE_BEHERIT)
                        {
                            foreach (var flags in target.effectFlags)
                            {
                                if (flags.seet_no1 == 2 && flags.flag_no1 == 3)
                                    flags.data = 2;
                            }
                        }
                    }
                }
            }
        }

        private void CorridorSealerFlagWatcher(Vector3 position)
        {
            //setup the flagwatcher to drop the spiral boat
            GameObject obj = new GameObject("CorridorSealerFlagWatcher");
            obj.transform.position = position;
            FlagWatcherScript flagWatcher = obj.AddComponent<FlagWatcherScript>();
            flagWatcher.setTaskSystemName(L2Base.TASKSYSNAME.SCENE);
            flagWatcher.actionWaitFrames = 90;
            flagWatcher.autoFinish = false;
            flagWatcher.characterEfxType = MoveCharacterBase.CharacterEffectType.NONE;
            flagWatcher.startAreaMode = MoveCharacterBase.ActionstartAreaMode.VIEW;
            flagWatcher.taskLayerNo = 2;
            flagWatcher.AnimeData = new GameObject[0];
            flagWatcher.ResetFlags = new L2FlagBoxEnd[0];
            flagWatcher.CheckFlags = new L2FlagBoxParent[1];
            flagWatcher.CheckFlags[0] = new L2FlagBoxParent();
            List<L2FlagBox> flagBoxes = new List<L2FlagBox>
            {
                new L2FlagBox()
                {
                    seet_no1 = 3,
                    flag_no1 = 93,
                    seet_no2 = -1,
                    flag_no2 = 0,
                    logic = LOGIC.AND,
                    comp = COMPARISON.Equal
                },
                new L2FlagBox()
                {
                    seet_no1 = 2,
                    flag_no1 = 3,
                    seet_no2 = -1,
                    flag_no2 = 7,
                    logic = LOGIC.AND,
                    comp = COMPARISON.Equal
                }
            };

            if (randomDissonance)
            {
                flagBoxes.Add(new L2FlagBox()
                {
                    seet_no1 = 3,
                    flag_no1 = 0,
                    seet_no2 = -1,
                    flag_no2 = requiredGuardians,
                    logic = LOGIC.AND,
                    comp = COMPARISON.GreaterEq
                });
            }
            else
            {
                flagBoxes.Add(new L2FlagBox()
                {
                    seet_no1 = 3,
                    flag_no1 = 15,
                    seet_no2 = -1,
                    flag_no2 = 4,
                    logic = LOGIC.AND,
                    comp = COMPARISON.GreaterEq
                });
            }

            flagWatcher.CheckFlags[0].BOX = flagBoxes.ToArray();
            flagWatcher.ActionFlags = new L2FlagBoxEnd[]
            {
                new L2FlagBoxEnd(){ seet_no1 = 3, flag_no1 = 93, data = 2, calcu = CALCU.EQR },
            };
            flagWatcher.finishFlags = new L2FlagBoxParent[1];
            flagWatcher.finishFlags[0] = new L2FlagBoxParent
            {
                BOX = new L2FlagBox[]
                {
                    new L2FlagBox()
                    {
                        seet_no1 = 3,
                        flag_no1 = 93,
                        seet_no2 = -1,
                        flag_no2 = 1,
                        logic = LOGIC.NON,
                        comp = COMPARISON.Equal
                    }
                }
            };
            obj.SetActive(true);
        }

        private void ObjectChanges()
        {
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

            //change it so the corridor of blood can be entered even after sealing it
            foreach (AnchorGateZ anchorGate in FindObjectsOfType<AnchorGateZ>())
            {
                if (anchorGate.shdowtask != null)
                {
                    foreach (L2FlagBoxParent flagBoxParent in anchorGate.shdowtask.startflag)
                    {
                        foreach (L2FlagBox flagBox in flagBoxParent.BOX)
                        {
                            if (flagBox.seet_no1 == 3 && flagBox.flag_no1 == 93 && flagBox.flag_no2 == 0)
                                flagBox.comp = COMPARISON.GreaterEq;
                        }
                    }
                }
            }

            //make it so fake items always play the wrong puzzle solve noise
            foreach (AnimatorController animatorController in FindObjectsOfType<AnimatorController>())
            {
                if (animatorController.name.Equals("WrongCall"))
                {
                    foreach (L2FlagBoxParent flagBoxParent in animatorController.CheckFlags)
                    {
                        foreach (L2FlagBox flagBox in flagBoxParent.BOX)
                        {
                            if (flagBox.seet_no1 == 2 && flagBox.flag_no1 == 16 && flagBox.flag_no2 == 1)
                            {
                                flagBox.flag_no2 = 0;
                                flagBox.comp = COMPARISON.GreaterEq;
                            }
                        }
                    }
                }
            }
        }

        private void ChangeFlagWatchers(string fieldName) 
        {
            foreach (FlagWatcherScript flagWatcher in FindObjectsOfType<FlagWatcherScript>())
            {
                if (flagWatcher.name.Equals("sougiOn"))
                {
                    //Change funeral event start conditions so they can reliably be in logic
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
                    //Change the timer so don't have to wait 2 minutes for the funeral event flags to be set
                    flagWatcher.actionWaitFrames = 60;
                }
                else if (flagWatcher.name.Equals("ragnarok"))
                {
                    //add a check to avoid soflock of triggering the rescue event without first summoning Jormangund's Ankh
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

                if (fieldName.Equals("fieldL00"))
                {
                    foreach (L2FlagBoxParent flagBoxParent in flagWatcher.CheckFlags)
                    {
                        foreach (L2FlagBox flagBox in flagBoxParent.BOX)
                        {
                            if (flagBox.seet_no1 == 5 && flagBox.flag_no1 == 3 && flagBox.flag_no2 == 2)
                            {
                                //stops Nebur from disappearing on the surface
                                flagBox.flag_no2 = 1;
                                flagBox.comp = COMPARISON.GreaterEq;
                            }
                            else if(flagBox.seet_no1 == 2 && flagBox.flag_no1 == 131)
                            {
                                //this is used to trigger the change of Hiner's shop, need to change it so it checks for the actual item in slot 3
                                if (shopToItemMap.TryGetValue(LocationID.HinerShop3, out ShopItem item))
                                {
                                    ItemInfo info = ItemDB.GetItemInfo(item.ID);
                                    flagBox.seet_no1 = info.ItemSheet;
                                    flagBox.flag_no1 = info.ItemFlag;
                                    flagBox.flag_no2 = 1;
                                    if(item.ID == ItemID.MobileSuperx3P)
                                        flagBox.flag_no2 = 2;
                                }
                            }
                        }
                    }
                }
                else if (fieldName.Equals("field02"))
                {
                    //stop Freya moving to Annwfn
                    foreach (L2FlagBoxParent flagBoxParent in flagWatcher.CheckFlags)
                    {
                        foreach (L2FlagBox flagBox in flagBoxParent.BOX)
                        {
                            if (flagBox.seet_no1 == 3 && flagBox.flag_no1 == 30 && flagBox.flag_no2 == 80)
                                flagBox.flag_no2 = 255;
                        }
                    }
                }
                else if (fieldName.Equals("fieldL08"))
                {
                    //disable this FlagWatcher as it messes with the elevator when this Endless Corridor leads to the Gate of Guidance gate
                    if (flagWatcher.name.Equals("FlagWatcher (8)"))
                        flagWatcher.gameObject.SetActive(false);
                }
                else if (fieldName.Equals("field13"))
                {
                    //disable these the Flagwatchers tht change the Echidna fight if easy Echidna option is on
                    if (easyEchidna && (flagWatcher.name.Equals("FlagWatcherTime1") || flagWatcher.name.Equals("FlagWatcherTime2") || flagWatcher.name.Equals("FlagWatcherTime3")))
                        flagWatcher.gameObject.SetActive(false);

                    RemoveCorridorSealers(flagWatcher);
                }
                else if (fieldName.Equals("field06-2") || fieldName.Equals("field10") || fieldName.Equals("field11") || 
                            fieldName.Equals("field12") || fieldName.Equals("field15"))
                {
                    RemoveCorridorSealers(flagWatcher);
                }
            }
        }

        private void RemoveCorridorSealers(FlagWatcherScript flagWatcher)
        {
            //Don't want the corridor to ever seal 
            bool isCorridorSealer = false;
            foreach (L2FlagBoxParent flagBoxParent in flagWatcher.CheckFlags)
            {
                foreach (L2FlagBox flagBox in flagBoxParent.BOX)
                {
                    if (flagBox.seet_no1 == 5 && flagBox.flag_no1 == 48 && flagBox.flag_no2 == 1)
                    {
                        isCorridorSealer = true;
                        break;
                    }
                }
            }

            if (isCorridorSealer)
                Destroy(flagWatcher.gameObject);
        }

        private void AddAnchorPoints(string fieldName)
        {
            BGScrollSystem bgScroll = FindObjectOfType<BGScrollSystem>();
            if (fieldName.Equals("field02"))
            {
                GameObject obj = new GameObject
                {
                    name = "PlayerStart f02Bifrost"
                };
                obj.transform.SetParent(bgScroll.transform);
                PlayerAnchor2 playerAnchor = obj.AddComponent<PlayerAnchor2>();
                playerAnchor.transform.position = new Vector3(-480, -460, 0);
                bgScroll.WarpAnchors.Add(playerAnchor);

            }
            else if (fieldName.Equals("field03"))
            {
                GameObject obj = new GameObject
                {
                    name = "PlayerStart f03Up"
                };
                obj.transform.SetParent(bgScroll.transform);
                PlayerAnchor2 playerAnchor = obj.AddComponent<PlayerAnchor2>();
                playerAnchor.transform.position = new Vector3(488, 756, 0);
                bgScroll.WarpAnchors.Add(playerAnchor);
            }
            else if (fieldName.Equals("field04"))
            {
                GameObject obj = new GameObject
                {
                    name = "PlayerStart f04Up3"
                };
                obj.transform.SetParent(bgScroll.transform);
                PlayerAnchor2 playerAnchor = obj.AddComponent<PlayerAnchor2>();
                playerAnchor.transform.position = new Vector3(840, 640, 0);
                bgScroll.WarpAnchors.Add(playerAnchor);
            }
            else if (fieldName.Equals("field08"))
            {
                GameObject obj = new GameObject
                {
                    name = "PlayerStart f08Neck"
                };
                obj.transform.SetParent(bgScroll.transform);
                PlayerAnchor2 playerAnchor = obj.AddComponent<PlayerAnchor2>();
                playerAnchor.transform.position = new Vector3(910, 170, 0);
                bgScroll.WarpAnchors.Add(playerAnchor);
            }
        }

        private void CreateStartingFieldObjects(string field)
        {
            //surface has a shop that can be used already so it doesnt matter
            //if its not the start field nothing needs to be done
            if (StartingArea == AreaID.VoD || !field.Equals(startFieldName))
                return;

            Vector3 tabletPosition = new Vector3();
            foreach(HolyTabretScript holyTablet in FindObjectsOfType<HolyTabretScript>())
            {
                if (holyTablet.name.Equals("TabletH"))
                {
                    tabletPosition = holyTablet.transform.position;
                    var hotSpring = holyTablet.gameObject.AddComponent<HotSpring>();
                    hotSpring.Init(sys);
                }
            }

            StartInfo startInfo = StartDB.GetStartInfo(StartingArea);
            foreach (ShopGateScript shopGate in FindObjectsOfType<ShopGateScript>())
            {
                if (shopGate.name.Equals("ShopGate"))
                {
                    //make a copy of a shop for the start shop
                    Vector3 shopPosition = tabletPosition + startInfo.ShopOffset;
                    shopPosition.z = 0;
                    GameObject shop = Instantiate(shopGate.gameObject, shopPosition, Quaternion.identity, shopGate.transform.parent);
                    shop.name = "Start Shop";
                    shop.SetActive(true);
                    ShopGateScript gateScript = shop.GetComponent<ShopGateScript>();
                    gateScript.sheetName = "f04-1e";
                    gateScript.shdowtask = null;

                    //add a white rectangle so the shop entrance is visible
                    GameObject shopVisual = new GameObject("Start Shop Entrance");
                    shopVisual.transform.position = new Vector3(shopPosition.x - 10, shopPosition.y, 1);
                    shopVisual.transform.localScale = new Vector3(5, 7, 1);
                    shopVisual.SetActive(true);
                    SpriteRenderer spriteRenderer = shopVisual.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height), new Vector2(0, 0), 1);
                    spriteRenderer.color = Color.white;
                    break;
                }
            }
        }

        private ExitID GetExitIDFromAnchorName(string anchorName, string field)
        {
            if (anchorName.Equals("PlayerStart"))
            {
                if (field.Equals("fieldL02")) return ExitID.fL02Left;
                else if (field.Equals("field03")) return ExitID.f03Right;
                else if (field.Equals("fieldP00")) return ExitID.fP00Right;
                else if (field.Equals("field01")) return ExitID.f01Down;
                else if (field.Equals("field11")) return ExitID.f11Pyramid;
            }
            else if (field.Equals("field01-2") && anchorName.Equals("PlayerStart f01Right"))
            {
                return ExitID.fStart;
            }
            else if (field.Equals("field01") && anchorName.Equals("PlayerStart f01Right"))
            {
                return ExitID.f01Start;
            }
            return ExitDB.AnchorNameToExitID(anchorName);
        }

        private IEnumerator ChangeEntrances(string field)
        {
            if(StartingGame && field.Equals("field01-2"))
            {
                var gate = FindObjectOfType<AnchorGateZ>();
                var startInfo = StartDB.GetStartInfo(StartingArea);
                gate.AnchorName = startInfo.AnchorName;
                gate.FieldNo = startInfo.FieldNo;
                gate.AnchorID = -1;

                StartingGame = false;

                yield return true;
            }

            List<AnimatorController> yugGates = new List<AnimatorController>(); 
            foreach (AnimatorController animator in FindObjectsOfType<AnimatorController>())
            {
                if (animator.name.Equals("YugGateDoor"))
                    yugGates.Add(animator);
            }

            List<GameObject> objectsToRemove = new List<GameObject>();
            if (randomSoulGates)
            {
                foreach (AnimatorController controller in FindObjectsOfType<AnimatorController>())
                {
                    if (controller.name.Equals("soul_gate") || controller.name.Equals("soul_cont"))
                        objectsToRemove.Add(controller.gameObject);
                }
            }

            foreach(AnchorGateZ gate in FindObjectsOfType<AnchorGateZ>())
            {
                ExitID exitID = GetExitIDFromAnchorName(gate.AnchorName, field);
                if (exitID == ExitID.None)
                    continue;

                if (exitToExitMap.TryGetValue(exitID, out ExitID destinationID))
                {
                    ExitInfo destinationInfo = ExitDB.GetExitInfo(destinationID);
                    gate.AnchorName = destinationInfo.AnchorName;
                    gate.FieldNo = destinationInfo.FieldNo;
                    gate.AnchorID = -1;

                    ExitInfo exitInfo = ExitDB.GetExitInfo(exitID);

                    gate.bgmFadeOut = false;

                    if (exitID >= ExitID.f00GateY0 && exitID <= ExitID.fL11GateN)
                    {
                        L2FlagBoxParent[] boxParents = new L2FlagBoxParent[1];
                        boxParents[0] = new L2FlagBoxParent();
                        List<L2FlagBox> flagBoxes = new List<L2FlagBox>();

                        AnimatorController gateDoor = null;
                        foreach (var door in yugGates)
                        {
                            Vector3 position = gate.transform.position;
                            if (position.x - 30 < door.transform.position.x && position.x + 30 > door.transform.position.x &&
                                position.y - 30 < door.transform.position.y && position.y + 30 > door.transform.position.y)
                            {
                                gateDoor = door;
                                break;
                            }
                        }

                        flagBoxes.Add(new L2FlagBox()
                        {
                            seet_no1 = exitInfo.SheetNo,
                            flag_no1 = exitInfo.FlagNo,
                            seet_no2 = -1,
                            flag_no2 = exitInfo.SheetNo == 0 ? -1 : 0,
                            logic = LOGIC.AND,
                            comp = COMPARISON.Greater
                        });

                        if(exitID == ExitID.f07GateP0)
                        {
                            flagBoxes.Add(new L2FlagBox()
                            {
                                seet_no1 = 11,
                                flag_no1 = 24,
                                seet_no2 = -1,
                                flag_no2 = 0,
                                logic = LOGIC.AND,
                                comp = COMPARISON.Equal
                            });
                        }

                        boxParents[0].BOX = flagBoxes.ToArray();
                        if (gate.shdowtask != null)
                            gate.shdowtask.startflag = boxParents;

                        if (gateDoor != null)
                            gateDoor.CheckFlags = boxParents;

                    }
                    else if(exitID >= ExitID.f00GateN1 && exitID <= ExitID.f14GateN6)
                    {
                        if (soulGateValueMap.TryGetValue(exitID, out int soulValue))
                        {
                            L2FlagBoxParent[] boxParents = new L2FlagBoxParent[] 
                            {
                                new L2FlagBoxParent()
                            };
                            List<L2FlagBox> flagBoxes = new List<L2FlagBox>();

                            AnimatorController soulGateDoor = null;
                            AnimatorController soul = null;

                            switch (soulValue)
                            {
                                case 1:
                                {
                                    soulGateDoor = Instantiate(objects["oneSoulGate"], gate.transform.position, Quaternion.identity).GetComponent<AnimatorController>();
                                    soul = Instantiate(objects["oneSoul"], gate.transform.position, Quaternion.identity).GetComponent<AnimatorController>();
                                    break;
                                }
                                case 2:
                                {
                                    soulGateDoor = Instantiate(objects["twoSoulGate"], gate.transform.position, Quaternion.identity).GetComponent<AnimatorController>();
                                    soul = Instantiate(objects["twoSoul"], gate.transform.position, Quaternion.identity).GetComponent<AnimatorController>();
                                    break;
                                }
                                case 3:
                                {
                                    soulGateDoor = Instantiate(objects["threeSoulGate"], gate.transform.position, Quaternion.identity).GetComponent<AnimatorController>();
                                    soul = Instantiate(objects["threeSoul"], gate.transform.position, Quaternion.identity).GetComponent<AnimatorController>();
                                    break;
                                }
                                case 5:
                                {
                                    soulGateDoor = Instantiate(objects["fiveSoulGate"], gate.transform.position, Quaternion.identity).GetComponent<AnimatorController>();
                                    soul = Instantiate(objects["fiveSoul"], gate.transform.position, Quaternion.identity).GetComponent<AnimatorController>();
                                    break;
                                }
                                case 9:
                                {
                                    soulGateDoor = Instantiate(objects["nineSoulGate"], gate.transform.position, Quaternion.identity).GetComponent<AnimatorController>();
                                    soul = Instantiate(objects["nineSoul"], gate.transform.position, Quaternion.identity).GetComponent<AnimatorController>();
                                    break;
                                }
                            }

                            soulGateDoor.transform.SetParent(gate.transform.parent);
                            soul.transform.SetParent(gate.transform.parent);

                            flagBoxes.Add(new L2FlagBox()
                            {
                                seet_no1 = 3,
                                flag_no1 = 0,
                                seet_no2 = -1,
                                flag_no2 = soulValue,
                                logic = LOGIC.AND,
                                comp = COMPARISON.GreaterEq
                            });

                            boxParents[0].BOX = flagBoxes.ToArray();
                            if (gate.shdowtask != null)
                                gate.shdowtask.startflag = boxParents;

                            if (soulGateDoor != null)
                            {
                                soulGateDoor.gameObject.SetActive(true);
                                soulGateDoor.CheckFlags = boxParents;
                            }

                            if (soul != null)
                            {
                                soul.gameObject.SetActive(true);
                                soul.CheckFlags = boxParents;
                            }
                        }
                    }

                    List<L2FlagBoxEnd> gateFlags = new List<L2FlagBoxEnd>();
                    if (destinationID == ExitID.fL02Left || destinationID == ExitID.fL02Up)
                    {
                        gateFlags.Add(new L2FlagBoxEnd()
                        {
                            seet_no1 = 5,
                            flag_no1 = 73,
                            data = 2,
                            calcu = CALCU.EQR
                        });
                        gateFlags.Add(new L2FlagBoxEnd()
                        {
                            seet_no1 = 5,
                            flag_no1 = 22,
                            data = 1,
                            calcu = CALCU.EQR
                        });
                    }
                    else if(destinationID == ExitID.f14GateN6)
                    {
                        gateFlags.Add(new L2FlagBoxEnd()
                        {
                            seet_no1 = 18,
                            flag_no1 = 0,
                            data = 1,
                            calcu = CALCU.EQR
                        });
                    }

                    if(gateFlags.Count > 0)
                    {
                        if(gate.gateFlags != null)
                            gateFlags.AddRange(gate.gateFlags.ToList());

                        gate.gateFlags = gateFlags.ToArray();
                    }


                    if(destinationID == ExitID.fLGate)
                    {
                        GameObject obj = new GameObject();
                        obj.transform.position = gate.transform.position;
                        FlagWatcherScript flagWatcher = obj.AddComponent<FlagWatcherScript>();
                        flagWatcher.actionWaitFrames = 60;
                        flagWatcher.autoFinish = false;
                        flagWatcher.characterEfxType = MoveCharacterBase.CharacterEffectType.NONE;
                        flagWatcher.startAreaMode = MoveCharacterBase.ActionstartAreaMode.VIEW;
                        flagWatcher.taskLayerNo = 2;
                        flagWatcher.AnimeData = new GameObject[0];
                        flagWatcher.ResetFlags = new L2FlagBoxEnd[0];
                        flagWatcher.CheckFlags = new L2FlagBoxParent[] 
                        {
                            new L2FlagBoxParent
                            {
                                BOX = new L2FlagBox[]
                                {
                                    new L2FlagBox()
                                    {
                                        seet_no1 = 5,
                                        flag_no1 = 73,
                                        seet_no2 = -1,
                                        flag_no2 = 2,
                                        logic = LOGIC.NON,
                                        comp = COMPARISON.Equal
                                    }
                                }
                            }
                        };
                        flagWatcher.ActionFlags = new L2FlagBoxEnd[]
                        {
                            new L2FlagBoxEnd()
                            {
                                seet_no1 = 5,
                                flag_no1 = 73,
                                data = 1,
                                calcu = CALCU.EQR
                            }
                        };
                        flagWatcher.finishFlags = new L2FlagBoxParent[] 
                        {
                            new L2FlagBoxParent
                            {
                                BOX = new L2FlagBox[]
                                {
                                    new L2FlagBox()
                                    {
                                        seet_no1 = 5,
                                        flag_no1 = 73,
                                        seet_no2 = -1,
                                        flag_no2 = 1,
                                        logic = LOGIC.NON,
                                        comp = COMPARISON.Equal
                                    }
                                }
                            } 
                        };
                    }
                }
                else if(exitID == ExitID.fLDown)
                {
                    //fix for rando start since samaranta wont appear to unlock the elevator sometimes
                    List<L2FlagBoxEnd> gateFlags = new List<L2FlagBoxEnd>
                    {
                        new L2FlagBoxEnd()
                        {
                            seet_no1 = 5,
                            flag_no1 = 73,
                            data = 2,
                            calcu = CALCU.EQR
                        },
                        new L2FlagBoxEnd()
                        {
                            seet_no1 = 5,
                            flag_no1 = 22,
                            data = 1,
                            calcu = CALCU.EQR
                        }
                    };

                    if (gate.gateFlags != null)
                        gateFlags.AddRange(gate.gateFlags.ToList());

                    gate.gateFlags = gateFlags.ToArray();
                }
            }

            yield return new WaitForEndOfFrame();
            foreach (var obj in objectsToRemove)
                obj.SetActive(false);
        }

        private IEnumerator FixTrapDoor()
        {
            yield return new WaitForEndOfFrame();

            foreach (patched_TrapFloor trapFloor in FindObjectsOfType<patched_TrapFloor>())
            {
                if (trapFloor.transform.position.x == 550 && trapFloor.transform.position.y == 388)
                {
                    trapFloor.width = 50;
                    trapFloor.whalf = 25;
                }
            }
        }

        #region Dialogue

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
            talkDataBase.cellData[3][4][1][0] = ChangeTalkFlagCheck(LocationID.GiltoriyoItem, COMPARISON.Greater, "[@iff,{0},{1},&gt;,{2},giltoriyo,1st-3]\n[@anim,talk,1]\n[@p,1st-2]");

            //Alsedana's itemf from Giltoriyo if didn't talk to Alsedana after vritra or vritra was after 6 guardians
            talkDataBase.cellData[3][7][1][0] = ChangeTalkStringAndFlagCheck(LocationID.AlsedanaItem,
                "[@iff,{0},{1},&gt;,{2},giltoriyo,2nd]\n[@exit]\n{3}[@anim,talk,1]\n[@p,1st-5]");

            //Fobos' 1st item
            talkDataBase.cellData[6][9][1][0] = ChangeTalkString(LocationID.FobosItem, "[@setf,5,16,=,5]\n[@anim,talk,1]\n{0}[@p,3rd-2]");

            //Fobos' 1st item check to see if you don't have the item
            talkDataBase.cellData[5][3][1][0] = ChangeTalkFlagCheck(LocationID.FobosItem, COMPARISON.Less, "[@iff,5,16,=,0,fobos,1st]\n[@iff,{0},{1},&lt;,{2},fobos,2nd]\n[@p,gS1]");
            
            //Fobos' 1st item
            talkDataBase.cellData[5][16][1][0] = ChangeTalkString(LocationID.FobosItem, "[@exit]\n[@anim,talk,1]\n[@setf,5,17,=,1]\n{0}[@p,lastC]");

            //Fobos' 2nd item check
            talkDataBase.cellData[5][22][1][0] = ChangeTalkFlagCheck(LocationID.FobosItem2, COMPARISON.Less,
                "[@setf,5,17,=,1]\n[@iff,{0},{1},&lt;,{2},fobos,gS2]\n[@anim,stalk2,1]\n[@setf,23,15,=,2]\n[@anifla,mnext,swait]\n[@out]");

            //Fobos' 2nd item
            talkDataBase.cellData[5][24][1][0] = ChangeTalkString(LocationID.FobosItem2, "[@exit]\n[@anim,talk,1]\n[@setf,23,15,=,4]\n{0}[@p,lastC]");

            //Freya's item
            talkDataBase.cellData[7][7][1][0] = ChangeTalkString(LocationID.FreyasItem,
                "[@anim,talk,1]\n{0}[@setf,5,67,=,1]\n[@p,lastC]");

            //Add check too Freya's starting mojiscript so she gives the item if you havent got it yet
            talkDataBase.cellData[7][3][1][0] = ChangeTalkFlagCheck(LocationID.FreyasItem, COMPARISON.Less, "[@anifla,mfanim,wait2]\n[@iff,{0},{1},&lt;,{2},freyja,1st-1]\n[@iff,3,95,&gt;,0,freyja,escape]\n" +
                "[@anifla,mfanim,wait]\n[@iff,3,35,&gt;,7,freyja,8th]\n[@iff,3,35,=,6,freyja,7th3]\n[@iff,3,35,&gt;,3,freyja,7th2]\n[@iff,3,35,=,3,freyja,ragna]\n[@iff,3,35,=,2,freyja,4th]\n" +
                "[@iff,3,35,=,1,freyja,3rd]\n[@iff,5,67,=,1,freyja,2nd]\n[@exit]\n[@anim,talk,1]\n[@p,2nd]");

            //Mulbruk's item
            talkDataBase.cellData[10][42][1][0] = ChangeTalkString(LocationID.MulbrukItem,
                "{0}[@setf,5,101,=,2]\n[@anim,talk,1]\n[@p,3rd-2]");

            //Add check too Mulbruk to see if you have her item
            talkDataBase.cellData[10][3][1][0] = ChangeTalkFlagCheck(LocationID.MulbrukItem, COMPARISON.Less, "[@iff,{0},{1},&lt;,{2},mulbruk2,3rd]\n[@iff,5,61,=,1,mulbruk2,mirror]\n" +
                "[@iff,5,86,=,1,mulbruk2,hint2]\n[@iff,5,87,=,1,mulbruk2,hint3]\n[@iff,5,88,=,1,mulbruk2,hint4]\n[@iff,5,89,=,1,mulbruk2,hint5]\n[@iff,5,90,=,1,mulbruk2,hint6]\n" +
                "[@iff,5,91,=,1,mulbruk2,hint7]\n[@iff,5,92,=,1,mulbruk2,hint8]\n[@iff,5,93,=,1,mulbruk2,hint9]\n[@iff,5,94,=,1,mulbruk2,hint10]\n[@iff,5,95,=,1,mulbruk2,hint11]\n" +
                "[@iff,3,33,&gt;,10,mulbruk2,5th]\n[@iff,5,0,=,2,mulbruk2,4th]\n[@anifla,mfanim,wait]\n[@iff,5,78,=,5,mulbruk2,hint1]\n[@anifla,mfanim,wait4]\n" +
                "[@iff,3,33,=,10,mulbruk2,3rdRnd]\n[@anifla,mfanim,wait]\n[@iff,5,78,=,5,mulbruk2,hint1]\n[@anifla,mfanim,wait3]\n[@iff,3,33,=,6,mulbruk2,rTalk2]\n" +
                "[@anifla,mfanim,wait2]\n[@iff,3,33,=,5,mulbruk2,rTalk1]\n[@anifla,mfanim,nochar]\n[@iff,3,33,=,4,mulbruk2,1st-8]\n[@iff,3,33,=,8,mulbruk2,1st-8]\n" +
                "[@anifla,mfanim,wait]\n[@iff,3,33,&lt;,7,mulbruk2,1st]\n[@iff,3,33,=,7,mulbruk2,2nd]\n");

            //Osiris' item
            talkDataBase.cellData[78][7][1][0] = ChangeTalkStringAndFlagCheck(LocationID.OsirisItem,
                "[@iff,{0},{1},&gt;,{2},f15-3,2nd]\n{3}[@anim,talk,1]\n[@p,lastC]");
        }

        private string ChangeTalkString(LocationID locationID, string original)
        {
            if (locationToItemMap.TryGetValue(locationID, out ItemID newItemID))
            {
                ItemInfo newItemInfo = ItemDB.GetItemInfo(newItemID);
                
                string itemString;
                if (newItemInfo.BoxName.Equals("Crystal S") || newItemInfo.BoxName.Equals("Sacred Orb") || newItemInfo.BoxName.Equals("MSX3p"))
                    itemString = string.Format("[@take,{0},02item,1]\n", newItemInfo.BoxName);
                else if (newItemInfo.BoxName.Equals("Money"))
                    itemString = "[@setfd,0,1,+,30]\n";
                else
                    itemString = string.Format("[@take,{0},02item,1]\n", newItemInfo.ShopName);

                //if the item has additional flags to be set add the flags to the mojiscript string
                foreach (L2FlagBoxEnd flag in CreateGetFlags(newItemID, newItemInfo))
                {
                    if (flag.calcu == CALCU.ADD)
                        itemString += string.Format("[@setf,{0},{1},+,{2}]\n", flag.seet_no1, flag.flag_no1, flag.data);
                    else if (flag.calcu == CALCU.EQR)
                        itemString += string.Format("[@setf,{0},{1},=,{2}]\n", flag.seet_no1, flag.flag_no1, flag.data);
                }

                return string.Format(original, itemString);
            }
            return string.Empty;
        }

        private string ChangeTalkFlagCheck(LocationID locationID, COMPARISON comp, string original)
        {
            if (locationToItemMap.TryGetValue(locationID, out ItemID newItemID))
            {
                ItemInfo newItemInfo = ItemDB.GetItemInfo(newItemID);

                int flagValue = 0;
                if (comp == COMPARISON.Less)
                    flagValue = 1;

                if (newItemID == ItemID.MobileSuperx3P)
                    flagValue++;

                return string.Format(original, newItemInfo.ItemSheet, newItemInfo.ItemFlag, flagValue);
            }
            return string.Empty;
        }

        private string ChangeTalkStringAndFlagCheck(LocationID locationID, string original)
        {
            if (locationToItemMap.TryGetValue(locationID, out ItemID newItemID))
            {
                ItemInfo newItemInfo = ItemDB.GetItemInfo(newItemID);

                int flagValue = 0;
                if (newItemID == ItemID.MobileSuperx3P)
                    flagValue = 1;

                string itemString;
                if (newItemInfo.BoxName.Equals("Crystal S") || newItemInfo.BoxName.Equals("Sacred Orb") || newItemInfo.BoxName.Equals("MSX3p"))
                    itemString = string.Format("[@take,{0},02item,1]\n", newItemInfo.BoxName);
                else if (newItemInfo.BoxName.Equals("Money"))
                    itemString = "[@setfd,0,1,+,30]\n";
                else
                    itemString = string.Format("[@take,{0},02item,1]\n", newItemInfo.ShopName);

                //if the item has additional flags to be set add the flags to the mojiscript string
                foreach (L2FlagBoxEnd flag in CreateGetFlags(newItemID, newItemInfo))
                {
                    if (flag.calcu == CALCU.ADD)
                        itemString += string.Format("[@setf,{0},{1},+,{2}]\n", flag.seet_no1, flag.flag_no1, flag.data);
                    else if (flag.calcu == CALCU.EQR)
                        itemString += string.Format("[@setf,{0},{1},=,{2}]\n", flag.seet_no1, flag.flag_no1, flag.data);
                }

                return string.Format(original, newItemInfo.ItemSheet, newItemInfo.ItemFlag, flagValue, itemString);
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

            //Fobos Dialogue
            talkDataBase.cellData[5][3][3][1] = "Hmmm.";
            talkDataBase.cellData[5][16][3][1] = "Here take this.";

            //Change Fairy King to set flag to open endless even if you have the pendant
            talkDataBase.cellData[8][10][1][0] = "[@exit]\n[@anim,talk,1]\n[@setf,3,34,=,2]\n[@setf,5,12,=,1]\n[@p,2nd-2]";

            //Change the Fairy King check on Freya's Pendant
            talkDataBase.cellData[8][3][1][0] = "[@iff,3,34,&gt;,3,freyr,5th]\n[@iff,3,34,=,3,freyr,4th]\n[@iff,3,34,=,2,freyr,3rd]\n[@iff,2,31,&gt;,0,freyr,2nd]\n" +
                "[@iff,3,34,=,1,freyr,1stEnd]\n[@iff,3,34,=,0,freyr,1st]";

            //Make Freya open left door in Mausoleum of Giants
            talkDataBase.cellData[7][14][1][0] = "[@anim,talk,1]\n[@setf,5,12,=,1]\n[@setf,3,29,=,1]\n[@setf,3,39,=,1]\n[@setf,9,38,=,1]";

            //Add check to see if you have beaten 4 guardians so mulbruuk can give you the item
            talkDataBase.cellData[10][41][1][0] = "[@exit]\n[@anim,talk,1]\n[@setf,3,33,=,10]\n[@iff,3,0,&gt;,3,mulbruk2,3rd-1]\n[@p,lastC]";

            //remove giltoriyo check on his item
            talkDataBase.cellData[3][3][1][0] = "[@iff,5,62,=,7,giltoriyo,9th]\n[@iff,5,62,=,6,giltoriyo,7th]\n[@iff,5,62,=,5,giltoriyo,6th]\n" +
                "[@iff,5,62,=,4,giltoriyo,5th]\n[@iff,5,62,=,3,giltoriyo,4th]\n[@iff,5,62,=,2,giltoriyo,2nd]\n[@exit]\n[@anim,talk,1]\n[@p,1st]";

            //fix giltoriyo early dialogue exit
            talkDataBase.cellData[3][6][1][0] = "[@setf,5,62,=,2]\n[@setf,1,7,=,0]\n[@anim,talk,1]\n[@p,1st-4]";
        }
        #endregion

        #region Shops

        private void ChangeShopItems()
        {
            shopDataBase.cellData[0][25][1][0] = CreateShopItemsString(LocationID.SidroShop1, LocationID.SidroShop2, LocationID.SidroShop3);
            shopDataBase.cellData[1][26][1][0] = CreateShopItemsString(LocationID.ModroShop1, LocationID.ModroShop2, LocationID.ModroShop3);
            shopDataBase.cellData[2][24][1][0] = CreateShopItemsString(LocationID.NeburShop1, LocationID.NeburShop2, LocationID.NeburShop3);
            shopDataBase.cellData[3][25][1][0] = CreateShopItemsString(LocationID.HinerShop1, LocationID.HinerShop2, LocationID.HinerShop3);
            shopDataBase.cellData[4][24][1][0] = CreateShopItemsString(LocationID.HinerShop1, LocationID.HinerShop2, LocationID.HinerShop4);
            shopDataBase.cellData[5][24][1][0] = CreateShopItemsString(LocationID.KorobockShop1, LocationID.KorobockShop2, LocationID.KorobockShop3);
            shopDataBase.cellData[6][24][1][0] = CreateShopItemsString(LocationID.PymShop1, LocationID.PymShop2, LocationID.PymShop3);
            shopDataBase.cellData[7][24][1][0] = CreateShopItemsString(LocationID.PeibalusaShop1, LocationID.PeibalusaShop2, LocationID.PeibalusaShop3);
            shopDataBase.cellData[8][24][1][0] = CreateShopItemsString(LocationID.HiroRoderickShop1, LocationID.HiroRoderickShop2, LocationID.HiroRoderickShop3);
            shopDataBase.cellData[9][24][1][0] = CreateShopItemsString(LocationID.BTKShop1, LocationID.BTKShop2, LocationID.BTKShop3);
            shopDataBase.cellData[10][24][1][0] = CreateShopItemsString(LocationID.StartingShop1, LocationID.StartingShop2, LocationID.StartingShop3);
            shopDataBase.cellData[11][24][1][0] = CreateShopItemsString(LocationID.MinoShop1, LocationID.MinoShop2, LocationID.MinoShop3);
            shopDataBase.cellData[12][24][1][0] = CreateShopItemsString(LocationID.ShuhokaShop1, LocationID.ShuhokaShop2, LocationID.ShuhokaShop3);
            shopDataBase.cellData[13][24][1][0] = CreateShopItemsString(LocationID.HydlitShop1, LocationID.HydlitShop2, LocationID.HydlitShop3);
            shopDataBase.cellData[14][24][1][0] = CreateShopItemsString(LocationID.AytumShop1, LocationID.AytumShop2, LocationID.AytumShop3);
            shopDataBase.cellData[15][24][1][0] = CreateShopItemsString(LocationID.AshGeenShop1, LocationID.AshGeenShop2, LocationID.AshGeenShop3);
            shopDataBase.cellData[16][24][1][0] = CreateShopItemsString(LocationID.MegarockShop1, LocationID.MegarockShop2, LocationID.MegarockShop3);
            shopDataBase.cellData[17][24][1][0] = CreateShopItemsString(LocationID.BargainDuckShop1, LocationID.BargainDuckShop2, LocationID.BargainDuckShop3);
            shopDataBase.cellData[18][24][1][0] = CreateShopItemsString(LocationID.KeroShop1, LocationID.KeroShop2, LocationID.KeroShop3);
            shopDataBase.cellData[19][24][1][0] = CreateShopItemsString(LocationID.VenumShop1, LocationID.VenumShop2, LocationID.VenumShop3);
            shopDataBase.cellData[20][24][1][0] = CreateShopItemsString(LocationID.FairylanShop1, LocationID.FairylanShop2, LocationID.FairylanShop3);
        }

        private string CreateShopItemsString(LocationID firstSpot, LocationID secondSpot, LocationID thirdSpot)
        {
            return string.Format("{0}\n{1}\n{2}", CreateSetItemString(firstSpot), CreateSetItemString(secondSpot), CreateSetItemString(thirdSpot));
        }

        private string CreateSetItemString(LocationID locationID)
        {
            if (shopToItemMap.TryGetValue(locationID, out ShopItem shopItem))
            {
                ItemInfo newItemInfo = ItemDB.GetItemInfo(shopItem.ID);
                if (shopItem.Multiplier < 5)
                    shopItem.Multiplier = 10;

                return string.Format("[@sitm,{0},{1},{2},{3}]", newItemInfo.ShopType, newItemInfo.ShopName, 
                                        IsStartWeaponAmmo(shopItem.ID) ? 0 : newItemInfo.ShopPrice * shopItem.Multiplier, 
                                        IsStartWeaponAmmo(shopItem.ID) ? newItemInfo.MaxShopAmount : newItemInfo.ShopAmount);
            }
            return string.Empty;
        }

        private bool IsStartWeaponAmmo(ItemID ammoID) 
        {
            switch (ammoID)
            {
                case ItemID.ShurikenAmmo: return StartingWeapon == ItemID.Shuriken;
                case ItemID.RollingShurikenAmmo: return StartingWeapon == ItemID.RollingShuriken;
                case ItemID.EarthSpearAmmo: return StartingWeapon == ItemID.EarthSpear;
                case ItemID.FlareAmmo: return StartingWeapon == ItemID.Flare;
                case ItemID.CaltropsAmmo: return StartingWeapon == ItemID.Caltrops;
                case ItemID.ChakramAmmo: return StartingWeapon == ItemID.Chakram;
                case ItemID.BombAmmo: return StartingWeapon == ItemID.Bomb;
                case ItemID.PistolAmmo: return StartingWeapon == ItemID.Pistol;
                default: return false;
            }
        }

        private void ChangeShopThanks()
        {
            //change these strings beforehand because they do stuff usually that is unwanted like increase score
            //Modro's thank1, remove check for shield
            shopDataBase.cellData[1][9][1][0] = "[@anim,thank,1]\n[@animp,buyF0121,1]";

            //Hiner's thank3
            shopDataBase.cellData[3][11][1][0] = "[@anim,thank,1]\n[@animp,buyF0142,1]";

            //Hiner's thank4
            shopDataBase.cellData[4][10][1][0] = "[@anim,thank,1]\n[@animp,buyF0142,1]";

            //Hiro Roderick thank3
            shopDataBase.cellData[8][10][1][0] = "[@anim,thank,1]\n[@animp,buyF032,1]";

            //Hydlit thank3
            shopDataBase.cellData[13][10][1][0] = "[@anim,wait,1]\n[@animp,buyF06,1]";

            ChangeThanksStrings(LocationID.SidroShop1, LocationID.SidroShop2, LocationID.SidroShop3, 0, 9);
            ChangeThanksStrings(LocationID.ModroShop1, LocationID.ModroShop2, LocationID.ModroShop3, 1, 9, 2, 3);
            ChangeThanksStrings(LocationID.NeburShop1, LocationID.NeburShop2, LocationID.NeburShop3, 2, 8);
            ChangeThanksStrings(LocationID.HinerShop1, LocationID.HinerShop2, LocationID.HinerShop3, 3, 9);
            ChangeThanksStrings(LocationID.HinerShop1, LocationID.HinerShop2, LocationID.HinerShop4, 4, 8);
            ChangeThanksStrings(LocationID.KorobockShop1, LocationID.KorobockShop2, LocationID.KorobockShop3, 5, 8);
            ChangeThanksStrings(LocationID.PymShop1, LocationID.PymShop2, LocationID.PymShop3, 6, 8);
            ChangeThanksStrings(LocationID.PeibalusaShop1, LocationID.PeibalusaShop2, LocationID.PeibalusaShop3, 7, 8);
            ChangeThanksStrings(LocationID.HiroRoderickShop1, LocationID.HiroRoderickShop2, LocationID.HiroRoderickShop3, 8, 8);
            ChangeThanksStrings(LocationID.BTKShop1, LocationID.BTKShop2, LocationID.BTKShop3, 9, 8);
            ChangeThanksStrings(LocationID.StartingShop1, LocationID.StartingShop2, LocationID.StartingShop3, 10, 8);
            ChangeThanksStrings(LocationID.MinoShop1, LocationID.MinoShop2, LocationID.MinoShop3, 11, 8);
            ChangeThanksStrings(LocationID.ShuhokaShop1, LocationID.ShuhokaShop2, LocationID.ShuhokaShop3, 12, 8);
            ChangeThanksStrings(LocationID.HydlitShop1, LocationID.HydlitShop2, LocationID.HydlitShop3, 13, 8);
            ChangeThanksStrings(LocationID.AytumShop1, LocationID.AytumShop2, LocationID.AytumShop3, 14, 8);
            ChangeThanksStrings(LocationID.AshGeenShop1, LocationID.AshGeenShop2, LocationID.AshGeenShop3, 15, 8);
            ChangeThanksStrings(LocationID.MegarockShop1, LocationID.MegarockShop2, LocationID.MegarockShop3, 16, 8);
            ChangeThanksStrings(LocationID.BargainDuckShop1, LocationID.BargainDuckShop2, LocationID.BargainDuckShop3, 17, 8);
            ChangeThanksStrings(LocationID.KeroShop1, LocationID.KeroShop2, LocationID.KeroShop3, 18, 8);
            ChangeThanksStrings(LocationID.VenumShop1, LocationID.VenumShop2, LocationID.VenumShop3, 19, 8);
            ChangeThanksStrings(LocationID.FairylanShop1, LocationID.FairylanShop2, LocationID.FairylanShop3, 20, 8);
        }

        private void ChangeThanksStrings(LocationID firstSlot, LocationID secondSlot, LocationID thirdSlot, int sheet, int first, int secondOffset = 1, int thirdOffset = 2)
        {
            shopDataBase.cellData[sheet][first][1][0] += CreateGetFlagString(firstSlot);
            shopDataBase.cellData[sheet][first + secondOffset][1][0] += CreateGetFlagString(secondSlot);
            shopDataBase.cellData[sheet][first + thirdOffset][1][0] += CreateGetFlagString(thirdSlot);
        }

        private string CreateGetFlagString(LocationID locationID)
        {
            string flagString = string.Empty;

            if (shopToItemMap.TryGetValue(locationID, out ShopItem shopItem))
            {
                ItemInfo newItemInfo = ItemDB.GetItemInfo(shopItem.ID);

                if (newItemInfo.BoxName.Equals("Crystal S"))
                    flagString = "\n[@take,Crystal S,02item,1]";

                foreach (L2FlagBoxEnd flag in CreateGetFlags(shopItem.ID, newItemInfo))
                {
                    if (flag.calcu == CALCU.ADD)
                        flagString += string.Format("\n[@setf,{0},{1},+,{2}]", flag.seet_no1, flag.flag_no1, flag.data);
                    else if (flag.calcu == CALCU.EQR)
                        flagString += string.Format("\n[@setf,{0},{1},=,{2}]", flag.seet_no1, flag.flag_no1, flag.data);
                }
            }
            return flagString;
        }

        #endregion

    }
}