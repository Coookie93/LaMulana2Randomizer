using L2Base;
using MonoMod;
using UnityEngine;

#pragma warning disable 0626, 0649, 0414, 0108
namespace LM2RandomiserMod.Patches
{
    public class HolyTabretScript
    {
        [MonoModPatch("global::HolyTabretScript")]
        public class patched_HolyTabretScript : global::HolyTabretScript
        {
            [MonoModIgnore]
            private L2System sys;

            [MonoModIgnore]
            private L2SystemCore sysCore;

            [MonoModIgnore]
            public ParticleSystem memsaveParticle;

            [MonoModReplace]
            public bool memorySave()
            {
                short num = 0;
                if (!this.sys.getFlag(this.sheetNo, this.flagNo, ref num))
                {
                    return false;
                }
                if (num < 1)
                {
                    L2Rando rando = GameObject.FindObjectOfType<L2Rando>();
                    if (rando == null)
                    {
                        return false;
                    }

                    if (rando.AutoScanTablets())
                    {
                        this.sys.setFlagData(this.sheetNo, this.flagNo, 1);
                    }
                    else
                    {
                        return false;
                    }
                }
                if (this.sys.getItemNum("Holy Grail") <= 0)
                {
                    return false;
                }
                this.sysCore.seManager.playSE(base.gameObject, 143);
                this.sys.memSave(base.transform.position.x, base.transform.position.y, this.warpPointNo);
                this.clearSaveParticle();
                this.memsaveParticle.Play();
                return true;
            }
        }
    }
}
