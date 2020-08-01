using LaMulana2Randomizer.LogicParsing;
using LaMulana2RandomizerShared;

namespace LaMulana2Randomizer
{
    public class JsonExit
    {
        public string Name;
        public ExitID ID;
        public string ConnectingAreaName;
        public string Logic;
        public ExitType ConnectionType;
    }

    public class Exit
    {
        public string Name { get; private set; }
        public string ParentAreaName { get; private set; }
        public ExitID ID { get; private set; }
        public ExitType ExitType { get; private set; }
        public BinaryNode LogicTree { get; private set; }

        public string ConnectingAreaName;
        public bool Checking = false;

        private string logicString;

        public Exit(JsonExit jsonConnection, string parentAreaName) 
        {
            Name = jsonConnection.Name;
            ID = jsonConnection.ID;
            ConnectingAreaName = jsonConnection.ConnectingAreaName;
            logicString = jsonConnection.Logic;
            ExitType = jsonConnection.ConnectionType;
            ParentAreaName = parentAreaName;
            if (string.IsNullOrEmpty(Name))
                Name = $"{ParentAreaName} to {ConnectingAreaName}";
        }

        public bool CanReach(PlayerState state)
        {
            return LogicTree.Evaluate(state) && state.CanReach(ParentAreaName);
        }

        public void AppendLogicString(string append)
        {
            logicString = string.Format($"({logicString}){append}");
        }

        public void BuildLogicTree()
        {
            LogicTree = LogicParsing.LogicTree.ParseAndBuildLogic(logicString);
        }

        public bool IsInaccessible()
        {
            return ID == ExitID.fL08Right || ID == ExitID.f02Down || ID == ExitID.f03Down2 || ID == ExitID.fL05Up ||
                    ID == ExitID.f02GateYA || ID == ExitID.f06GateP0 || ID == ExitID.f12GateP0 || ID == ExitID.f13GateP0 ||
                    ID == ExitID.f03GateYC || ID == ExitID.f09In || ID == ExitID.f03In || ID == ExitID.fNibiru;
        }
    }

    public enum ExitType
    {
        LeftDoor,
        RightDoor,
        DownLadder,
        UpLadder,
        Gate,
        SoulGate,
        OneWay,
        Corridor,
        CorridorSealed,
        Internal,
        PrisonExit,
        Unique
    }
}
