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

        L2Base.L2System sys;

        TreasureBoxScript[] cachedBoxes;
        EventItemScript[] cachedItems;

        Dictionary<int, int> locationToItemMap;
        bool randomising = false;

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
                GUI.Label(new Rect(0, Screen.height - 100f, 500f, 25f), randomising.ToString());
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
            if(cachedBoxes != null)
            {
                //loop over all the boxes in the current scene and change their item and flags
                foreach (var box in cachedBoxes)
                {
                    //this should never fail currently, more of a sanity check
                    if (box.itemObj.name.Contains("ItemSym "))
                    {
                        string itemName = box.itemObj.name.Remove(0, 8).RemoveWhitespace();

                        //if the name off the item is one we care about
                        if (Enum.IsDefined(typeof(LocationID), itemName))
                        {
                            ChangeBoxFlags(box, (LocationID)Enum.Parse(typeof(LocationID), itemName));
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

        public void Initialise(L2ShopDataBase shopDataBase, L2System system)
        {
            this.shopDataBase = shopDataBase;
            this.sys = system;

            //load the locationToItemMap from seed.lm2
            locationToItemMap = LoadSeedFile();
            //if we successfully loaded and the seed has the right amount of locations
            if (locationToItemMap != null && locationToItemMap.Count == 172)
            {
                randomising = true;
            }
        }
        
        private Dictionary<int, int> LoadSeedFile()
        {
            Dictionary<int, int> itemLocations = null;
            FileStream fs = null;
            BinaryFormatter formatter;
            try
            {
                fs = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "Release\\Seed\\seed.lm2"), FileMode.Open);
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
        
        private void ChangeBoxFlags(TreasureBoxScript box, LocationID locationID)
        {
            int id;
            if (locationToItemMap.TryGetValue((int)locationID, out id))
            {
                ItemID newItemID = (ItemID)id;

                //the flags the box uses to check whether you have that item already if true the box will
                //be open if the boxes unlock condition is true, otherwise it will be in the unlocked state.
                //Not super important as if these are incorrect, as only the box may look like you havent opened it before 
                //if the area is reloaded
                box.openFlags = ItemFlags.GetBoxOpenFlags(newItemID);

                AbstractItemBase item = box.itemObj.GetComponent<AbstractItemBase>();

                //name used when calling setitem
                item.itemLabel = name;

                //flags the item uses to check to see if it should be active and visible to the user, important that these are
                //changed because if you only change the label the it will use the original items flags to check. This means that 
                //if you change another item to what was this items original is and collect it when it comes to collecting the item 
                //this has been changed too if won't be active as it thinks you already have it
                item.itemActiveFlag = ItemFlags.GetActiveItemFlags(newItemID);

                //flags that the item sets when you collect it, important to change otherwise the original item will also be collected
                //when you pick up the item because by default it sets the original items flags again, also other flags can be set here
                //usually items that add to flags that are used as a type of counter eg.Sacred Orbs orb count
                item.itemGetFlags = ItemFlags.GetItemGetFlags(newItemID);

                //change the sprite to match the new item
                Sprite sprite = L2SystemCore.getMapIconSprite(L2SystemCore.getItemData(name));
                item.gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
            }
        }

        private string CreateShopItemsString(string item1, string item2, string item3)
        {
            return String.Format("{0}\n{1}\n{2}", CreateSetItemString(item1), CreateSetItemString(item2), CreateSetItemString(item3));
        }

        private string CreateSetItemString(string item)
        {
            return String.Format("[@sitem,{0},{1},{2},{3}]", "item", item, 100, 1);
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
        SacredOrb5,
        SacredOrb2,
        SacredOrb4,
        SacredOrb3,
        SacredOrb6,
        SacredOrb7,
        SacredOrb8,
        SacredOrb9,

        Map1,
        Map5,
        Map2,
        Map4,
        Map10,
        Map3,
        Map6,
        Map7,
        Map11,
        Map8,
        Map12,
        Map13,
        Map9,
        Map15,
        Map14,

        CrystalS1,
        CrystalS2,
        CrystalS4,
        CrystalS5,
        CrystalS3,
        CrystalS6,
        CrystalS8,
        CrystalS7,
        CrystalS9,
        CrystalS11,
        CrystalS10,
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
        ItemSymBMirror,
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
        Bomb

        //R-Shuriken,ESpear,La-Mulana,La-Mulana2
    }
}