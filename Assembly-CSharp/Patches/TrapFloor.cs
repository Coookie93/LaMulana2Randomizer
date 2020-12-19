using MonoMod;

namespace LM2RandomiserMod.Patches
{
    [MonoModPatch("global::TrapFloor")]
    public class patched_TrapFloor : global::TrapFloor
    {
        public float whalf;
        public float width;
    }
}
