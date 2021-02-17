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
            { AreaID.RoY,           new StartInfo(0,  "PlayerStart f00Holy",  new Vector2(0.0f, 96.0f),     new Vector2(30.0f, 96.0f))},
            { AreaID.VoD,           new StartInfo(1,  "PlayerStart f01Holy",  new Vector2(-96.0f, -240.0f), new Vector2())},
            { AreaID.AnnwfnMain,    new StartInfo(2,  "PlayerStart f02Holy",  new Vector2(-328.0f, -16.0f), new Vector2(-348.0f, -16.0f))},
            { AreaID.IBMain,        new StartInfo(3,  "PlayerStart f03Holy",  new Vector2(944.0f, 208.0f),  new Vector2(974.0f, 208.0f))},
            { AreaID.ITLeft,        new StartInfo(4,  "PlayerStart f04Holy",  new Vector2(-669.0f, 96.0f),  new Vector2(-699.0f, 96.0f))},
            { AreaID.DFMain,        new StartInfo(5,  "PlayerStart f05Holy",  new Vector2(0.0f, -200.0f),   new Vector2(30.0f, -200.0f))},
            { AreaID.ValhallaMain,  new StartInfo(10, "PlayerStart f10RHoly", new Vector2(-480.0f, -56.0f), new Vector2(-450.0f, -56.0f))},
            { AreaID.DSLMMain,      new StartInfo(11, "PlayerStart f11RHoly", new Vector2(-592.0f, 56.0f),  new Vector2(-502.0f, 56.0f))},
            { AreaID.ACTablet,      new StartInfo(12, "PlayerStart f12RHoly", new Vector2(-208.0f, 64.0f),  new Vector2(-288.0f, 64.0f))}
        };
    }

    public class StartInfo
    {
        public int FieldNo;
        public string AnchorName;
        public Vector2 TabletPosition;
        public Vector2 ShopPosition;

        public StartInfo(int fieldNo, string anchorName, Vector2 tabletPosition, Vector2 shopPosition)
        {
            FieldNo = fieldNo;
            AnchorName = anchorName;
            TabletPosition = tabletPosition;
            ShopPosition = shopPosition;
        }
    }
}
