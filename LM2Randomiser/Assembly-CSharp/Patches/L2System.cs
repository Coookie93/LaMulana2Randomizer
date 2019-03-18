using System;
using MonoMod;
using UnityEngine;
using L2Word;

#pragma warning disable 0649, 0414
namespace LM2RandomiserMod
{
    [MonoModPatch("L2Base.L2System")]
    public class L2System : L2Base.L2System
    {
        [MonoModIgnore]
        private L2ShopDataBase l2sdb;

        private void orig_Start() { }
        private void Start()
        {
            orig_Start();
            GameObject obj = new GameObject();
            L2Rando component = obj.AddComponent<L2Rando>() as L2Rando;
            component.Initialise(this.l2sdb, this);
            DontDestroyOnLoad(obj);
        }
    }
}
