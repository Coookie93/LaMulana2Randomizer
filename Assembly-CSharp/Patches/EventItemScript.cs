using MonoMod;
using L2Base;

namespace LM2RandomiserMod.Patches
{
    [MonoModPatch("global::EventItemScript")]
    public class patched_EventItemScript : global::EventItemScript
    {
#if ONEPOINTZERO
        protected NewPlayer pl;
#endif
        [MonoModReplace]
        protected override void itemGetAction()
        {
#if ONEPOINTZERO
            pl = sys.getPlayer();
#endif
            int slotNo = getL2Core().seManager.playSE(null, 39);
            getL2Core().seManager.releaseGameObjectFromPlayer(slotNo);
            pl.setActionOder(PLAYERACTIONODER.getitem);
            if (itemLabel.Contains("Whip"))
            {
                short data = 0;
                string trueItemName = string.Empty;
                sys.getFlag(2, "Whip", ref data);
                if (data == 0) trueItemName = "Whip";
                else if (data == 1) trueItemName = "Whip2";
                else if (data >= 2) trueItemName = "Whip3";

                pl.setGetItem(ref trueItemName);
                pl.setGetItemIcon(L2SystemCore.getItemData(trueItemName));
            }
            else if (itemLabel.Contains("Shield"))
            {
                short data = 0;
                string trueItemName = string.Empty;
                sys.getFlag(2, 196, ref data);
                if (data == 0) trueItemName = "Shield";
                else if (data == 1) trueItemName = "Shield2";
                else if (data >= 2) trueItemName = "Shield3";

                pl.setGetItem(ref trueItemName);
                pl.setGetItemIcon(L2SystemCore.getItemData(trueItemName));
            }
            else
            {
                pl.setGetItem(ref itemLabel);
                //Mantras don't have an icon so just use the mantra software icon
                if (itemLabel.Contains("Mantra"))
                    pl.setGetItemIcon(L2SystemCore.getItemData("Mantra"));
                else if(itemLabel.Contains("Research"))
                    pl.setGetItemIcon(L2SystemCore.getItemData("Research"));
                else
                    pl.setGetItemIcon(L2SystemCore.getItemData(itemLabel));
            }
        }
    }
}
