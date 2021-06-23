using LaMulana2Randomizer.LogicParsing;
using LaMulana2RandomizerShared;

namespace LaMulana2Randomizer
{
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
        Internal,
        PrisonExit,
        PrisonGate,
        Start,
        Elevator,
        SpiralGate
    }

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
        public AreaID ConnectingAreaID;
        public bool Checking = false;

        private string logicString;
        private BinaryNode logicTree;

        public string Name { get; private set; }
        public AreaID ParentAreaID { get; private set; }
        public ExitID ID { get; private set; }
        public ExitType ExitType { get; private set; }

        public bool IsInaccessible {
            get => ID == ExitID.fStart || ID == ExitID.fL05Up || ID == ExitID.fL08Right || ID == ExitID.f02GateYA || 
                    ID == ExitID.f03GateYC || ID == ExitID.f03Down2 || ID == ExitID.f06GateP0 || ID == ExitID.f09In || 
                    ID == ExitID.f13GateP0 || ID == ExitID.fNibiru;
        }

        public bool IsDeadEnd {
            get => ID == ExitID.fStart || ID == ExitID.fL05Up || ID == ExitID.fL08Right || ID == ExitID.fLGate || 
                    ID == ExitID.f00Down || ID == ExitID.f00GateYA || ID == ExitID.f01Down || ID == ExitID.f03Down1 || 
                    ID == ExitID.f03Down3 || ID == ExitID.f04Up3 || ID == ExitID.f06GateP0 || ID == ExitID.f06_2GateP0 || 
                    ID == ExitID.f09In || ID == ExitID.f09GateP0 || ID == ExitID.f11Pyramid || ID == ExitID.f12GateP0 || 
                    ID == ExitID.f13GateP0 || ID == ExitID.fNibiru;
        }

        public bool IsOneWay {
            get => ID == ExitID.f02Down || ID == ExitID.f03Down2 || ID == ExitID.f03In || ID == ExitID.f09In;
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
                Name = $"{ParentAreaID} to {ConnectingAreaID}";
        }

        public bool CanReach(PlayerState state)
        {
            return logicTree.Evaluate(state) && state.CanReach(ParentAreaID);
        }

        public void AppendLogicString(string str)
        {
            logicString = string.Format($"({logicString}) {str}");
            BuildLogicTree();
        }

        public void BuildLogicTree()
        {
            logicTree = LogicTree.ParseAndBuildLogic(logicString);
        }
    }
}
