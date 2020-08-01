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
            this.pl.setActionOder(PLAYERACTIONODER.getitem);
            this.pl.setGetItem(ref itemLabel);
            //Mantras don't have an icon so just use the mantra software icon
            if (itemLabel.Contains("Mantra"))
                this.pl.setGetItemIcon(L2SystemCore.getItemData("Mantra"));
            else
                this.pl.setGetItemIcon(L2SystemCore.getItemData(itemLabel));
        }
    }
}
