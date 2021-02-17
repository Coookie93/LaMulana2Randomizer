using LaMulana2Randomizer.LogicParsing;
using LaMulana2RandomizerShared;

namespace LaMulana2Randomizer
{
    public class JsonExit
    {
        public string Name;
        public ExitID ID;
        public AreaID ConnectingAreaID;
        public string Logic;
        public ExitType ConnectionType;
    }

    public class Exit
    {
        public string Name { get; private set; }
        public AreaID ParentAreaID { get; private set; }
        public ExitID ID { get; private set; }
        public ExitType ExitType { get; private set; }
        public BinaryNode LogicTree { get; private set; }

        public AreaID ConnectingAreaID;
        public bool Checking = false;

        private string logicString;

        public bool IsInaccessible
        {
            get => ID == ExitID.fStart || ID == ExitID.fL05Up || ID == ExitID.fL08Right || ID == ExitID.f02Down || 
                    ID == ExitID.f02GateYA || ID == ExitID.f03In || ID == ExitID.f03GateYC || ID == ExitID.f03Down2 || 
                    ID == ExitID.f06GateP0 || ID == ExitID.f09In || ID == ExitID.f11Pyramid || ID == ExitID.f12GateP0 || 
                    ID == ExitID.f13GateP0 || ID == ExitID.fNibiru;
        }

        public bool IsDeadEnd 
        {
            get => ID == ExitID.f00Down || ID == ExitID.f00GateYA || ID == ExitID.f01Down || ID == ExitID.fStart || 
                    ID == ExitID.fL05Up || ID == ExitID.fL08Right || ID == ExitID.fLGate || ID == ExitID.f03Down3 || 
                    ID == ExitID.f04Up2 || ID == ExitID.f06_2GateP0 || ID == ExitID.f09In || ID == ExitID.f11Pyramid || 
                    ID == ExitID.f13GateP0 || ID == ExitID.fNibiru;
        }

        public Exit(JsonExit jsonConnection, AreaID parentAreaID) 
        {
            Name = jsonConnection.Name;
            ID = jsonConnection.ID;
            ConnectingAreaID = jsonConnection.ConnectingAreaID;
            logicString = jsonConnection.Logic;
            ExitType = jsonConnection.ConnectionType;
            ParentAreaID = parentAreaID;
            if (string.IsNullOrEmpty(Name))
                Name = $"{parentAreaID} to {ConnectingAreaID}";
        }

        public bool CanReach(PlayerState state)
        {
            return LogicTree.Evaluate(state) && state.CanReach(ParentAreaID);
        }

        public void AppendLogicString(string append)
        {
            logicString = string.Format($"({logicString}){append}");
        }

        public void BuildLogicTree()
        {
            LogicTree = LogicParsing.LogicTree.ParseAndBuildLogic(logicString);
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
        Pyramid,
        Corridor,
        CorridorSealed,
        Internal,
        PrisonExit,
        PrisonGate,
        Start,
        Unique
    }
}
