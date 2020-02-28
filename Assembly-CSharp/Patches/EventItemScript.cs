using MonoMod;
using L2Base;

namespace LM2RandomiserMod.Patches
{
    [MonoModPatch("global::EventItemScript")]
    public class patched_EventItemScript : global::EventItemScript
    {
        [MonoModReplace]
        protected override void itemGetAction()
        {
            int slotNo = base.getL2Core().seManager.playSE(null, 39);
            base.getL2Core().seManager.releaseGameObjectFromPlayer(slotNo);
            this.pl.setActionOder(PLAYERACTIONODER.getitem);
            this.pl.setGetItem(ref this.itemLabel);
            //Mantras don't have an icon so just use the mantra software icon
            if (this.itemLabel.Contains("Mantra"))
            {
                this.pl.setGetItemIcon(L2SystemCore.getItemData("Mantra"));
            }
            else
            {
                this.pl.setGetItemIcon(L2SystemCore.getItemData(this.itemLabel));
            }
        }
    }
}
