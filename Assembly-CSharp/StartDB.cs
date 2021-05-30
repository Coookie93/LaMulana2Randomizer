using System.Collections.Generic;
using UnityEngine;
using LaMulana2RandomizerShared;

namespace LM2RandomiserMod
{
    public abstract class StartDB
    {
        public static StartInfo GetStartInfo(AreaID areaID)
        {
            startData.TryGetValue(areaID, out StartInfo startInfo);
            return startInfo;
        }

        private static readonly Dictionary<AreaID, StartInfo> startData = new Dictionary<AreaID, StartInfo>()
        {
            { AreaID.RoY,           new StartInfo(0,  "PlayerStart f00Holy",  new Vector3( 30, -10, 0))},
            { AreaID.VoD,           new StartInfo(1,  "PlayerStart f01Holy",  new Vector3(  0,   0, 0))},
            { AreaID.AnnwfnMain,    new StartInfo(2,  "PlayerStart f02Holy",  new Vector3(-20, -10, 0))},
            { AreaID.IBMain,        new StartInfo(3,  "PlayerStart f03Holy",  new Vector3( 30, -10, 0))},
            { AreaID.ITLeft,        new StartInfo(4,  "PlayerStart f04Holy",  new Vector3(-30, -10, 0))},
            { AreaID.DFMain,        new StartInfo(5,  "PlayerStart f05Holy",  new Vector3( 30, -10, 0))},
            { AreaID.ValhallaMain,  new StartInfo(10, "PlayerStart f10RHoly", new Vector3( 30, -10, 0))},
            { AreaID.DSLMMain,      new StartInfo(11, "PlayerStart f11RHoly", new Vector3( 80, -10, 0))},
            { AreaID.ACTablet,      new StartInfo(12, "PlayerStart f12RHoly", new Vector3(-80, -10, 0))}
        };
    }

    public class StartInfo
    {
        public int FieldNo;
        public string AnchorName;
        public Vector3 ShopOffset;

        public StartInfo(int fieldNo, string anchorName, Vector2 shopOffset)
        {
            FieldNo = fieldNo;
            AnchorName = anchorName;
            ShopOffset = shopOffset;
        }
    }
}
