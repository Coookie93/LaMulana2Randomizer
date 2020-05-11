using System.Collections.Generic;
using LaMulana2RandomizerShared;


namespace LM2RandomiserMod
{
    public abstract class ExitDB
    {
        public static ExitInfo GetExitInfo(ExitID id)
        {
            connectionData.TryGetValue(id, out ExitInfo result);
            return result;
        }

        private static readonly Dictionary<ExitID, ExitInfo> connectionData = new Dictionary<ExitID, ExitInfo>(){
            //left doors
            { ExitID.fLLeft,           new ExitInfo("PlayerStart fLLeft",        17, -1, -1)},
            { ExitID.fL02Left,         new ExitInfo("PlayerStart fL02Left",      18, -1, -1)},
            { ExitID.fP00Left,         new ExitInfo("PlayerStart",               23, -1, -1)},
            { ExitID.fP02Left,         new ExitInfo("PlayerStart",               25, -1, -1)},

            //right doors
            { ExitID.f01Right,         new ExitInfo("PlayerStart f01Right",      1, -1, -1)},
            { ExitID.f03Right,         new ExitInfo("PlayerStart f03Right",      3, -1, -1)},
            { ExitID.fL08Right,        new ExitInfo("PlayerStart",               21, -1, -1)},
            { ExitID.fP00Right,        new ExitInfo("PlayerStart2",              23, -1, -1)},

            //down ladders
            { ExitID.f00Down,          new ExitInfo("PlayerStart f00Down",       0, -1, -1)},
            { ExitID.f01Down,          new ExitInfo("PlayerStart f01Down",       1, -1, -1)},
            { ExitID.f02Down,          new ExitInfo("PlayerStart f02Down",       2, -1, -1)},
            { ExitID.f03Down1,         new ExitInfo("PlayerStart f03Down1",      3, -1, -1)},
            { ExitID.f03Down2,         new ExitInfo("PlayerStart f03Down2",      3, -1, -1)},
            { ExitID.f03Down3,         new ExitInfo("PlayerStart f03Down3",      3, -1, -1)},
            { ExitID.fLDown,           new ExitInfo("PlayerStart fLDown",        17, -1, -1)},

            //up ladders
            { ExitID.f02Up,            new ExitInfo("PlayerStart f02Up",         2, -1, -1)},
            { ExitID.f03Up,            new ExitInfo("PlayerStart f03Up",         3, -1, -1)}, //
            { ExitID.f04Up,            new ExitInfo("PlayerStart f04Up",         4, -1, -1)},
            { ExitID.f04Up2,           new ExitInfo("PlayerStart f04Up2",        4, -1, -1)},
            { ExitID.f04Up3,           new ExitInfo("PlayerStart f04Up3",        4, -1, -1)}, //
            { ExitID.fL02Up,           new ExitInfo("PlayerStart fL02Up",        18, -1, -1)},
            { ExitID.fL05Up,           new ExitInfo("PlayerStart",               20, -1, -1)},

            //gates
            { ExitID.f00GateY0,       new ExitInfo("PlayerStart f00GateY0",       0, -1, -1)},
            { ExitID.f00GateYA,       new ExitInfo("PlayerStart f00GateYA",       0,  4,  0)},
            { ExitID.f00GateYB,       new ExitInfo("PlayerStart f00GateYB",       0,  4,  3)},
            { ExitID.f00GateYC,       new ExitInfo("PlayerStart f00GateYC",       0,  4,  8)},
            { ExitID.f02GateYA,       new ExitInfo("PlayerStart f02GateYA",       2, -1, -1)},
            { ExitID.f03GateYC,       new ExitInfo("PlayerStart f03GateYC",       3, -1, -1)},
            { ExitID.f04GateYB,       new ExitInfo("PlayerStart f04GateYB",       4, -1, -1)},
            { ExitID.f05GateP1,       new ExitInfo("PlayerStart f05GateP1",       5,  9,  4)},
            { ExitID.f06GateP0,       new ExitInfo("PlayerStart f06GateP0",       6, -1, -1)},
            { ExitID.f06_2GateP0,     new ExitInfo("PlayerStart f06-2GateP0",    16, 10,  4)},
            { ExitID.f07GateP0,       new ExitInfo("PlayerStart f07GateP0",       7, 11, 16)},
            { ExitID.f08GateP0,       new ExitInfo("PlayerStart f08GateP0",       8, 12, 50)},
            { ExitID.f09GateP0,       new ExitInfo("PlayerStart f09GateP0",       9, 13,  3)},
            { ExitID.f10GateP0,       new ExitInfo("PlayerStart f10GateP0",      10,  9,  4)},
            { ExitID.f11GateP0,       new ExitInfo("PlayerStart f11GateP0",      11, -1, -1)},
            { ExitID.f12GateP0,       new ExitInfo("PlayerStart f12GateP0",      12, -1, -1)},
            { ExitID.f13GateP0,       new ExitInfo("PlayerStart f13GateP0",      13, -1, -1)},
            { ExitID.fLGate,          new ExitInfo("PlayerStart fLGate",         17, -1, -1)},
            { ExitID.fL11GateY0,      new ExitInfo("PlayerStart fL11GateY0",     22,  5, 17)},
            { ExitID.fL11GateN,       new ExitInfo("PlayerStart fL11GateN",      22, -1, -1)},

            //soul gates
            { ExitID.f00GateN1,       new ExitInfo("PlayerStart f00GateN1",       0, -1, -1)},
            { ExitID.f02GateN2,       new ExitInfo("PlayerStart f02GateN2",       2, -1, -1)},
            { ExitID.f03GateN3,       new ExitInfo("PlayerStart f03GateN3",       3, -1, -1)},
            { ExitID.f03GateN4,       new ExitInfo("PlayerStart f03GateN4",       3, -1, -1)},
            { ExitID.f03GateN9,       new ExitInfo("PlayerStart f03GateN9",       3, -1, -1)},
            { ExitID.f04GateN5,       new ExitInfo("PlayerStart f04GateN5",       4, -1, -1)},
            { ExitID.f04GateN6,       new ExitInfo("PlayerStart f04GateN62",      4, -1, -1)},
            { ExitID.f05GateN1,       new ExitInfo("PlayerStart f05GateN1",       5, -1, -1)},
            { ExitID.f06GateN2,       new ExitInfo("PlayerStart f06GateN2",       6, -1, -1)},
            { ExitID.f06GateN7,       new ExitInfo("PlayerStart f06GateN7",       6, -1, -1)},
            { ExitID.f07GateN3,       new ExitInfo("PlayerStart f07GateN3",       7, -1, -1)},
            { ExitID.f08GateN4,       new ExitInfo("PlayerStart f08GateN4",       8, -1, -1)},
            { ExitID.f08GateN8,       new ExitInfo("PlayerStart f08GateN8",       8, -1, -1)},
            { ExitID.f09GateN5,       new ExitInfo("PlayerStart f09GateN5",       9, -1, -1)},
            { ExitID.f10GateN7,       new ExitInfo("PlayerStart f10GateN7",      10, -1, -1)},
            { ExitID.f12GateN8,       new ExitInfo("PlayerStart f12GateN8",      12, -1, -1)},
            { ExitID.f13GateN9,       new ExitInfo("PlayerStart f13GateN9",      13, -1, -1)},
            { ExitID.f14GateN6,       new ExitInfo("PlayerStart f14GateN6",      14, -1, -1)},
        };


        public static ExitID AnchorNameToExitID(string anchorName)
        {
            anchorNameToExitID.TryGetValue(anchorName, out ExitID result);
            return result;
        }

        private static readonly Dictionary<string, ExitID> anchorNameToExitID = new Dictionary<string, ExitID>()
        {
            //left doors
            {"PlayerStart f01Right",       ExitID.fLLeft },
            {"PlayerStart f03Right",       ExitID.fP00Left },
            {"PlayerStart2",               ExitID.fP02Left },
                                           
            //right doors                  
            {"PlayerStart fLLeft",         ExitID.f01Right },
            {"PlayerStart fL02Left",       ExitID.fL08Right },
                                           
            //down ladders                 
            {"PlayerStart f02Up",          ExitID.f00Down },
            {"PlayerStart f04Up",          ExitID.f03Down1 },
            {"PlayerStart f04Up2",         ExitID.f03Down3 },
            {"PlayerStart fL02Up",         ExitID.fLDown },
                                           
            //up ladders                   
            {"PlayerStart f00Down",        ExitID.f02Up },
            {"PlayerStart f02Down",        ExitID.f03Up },
            {"PlayerStart f03Down1",       ExitID.f04Up },
            {"PlayerStart f03Down2",       ExitID.f04Up3 },
            {"PlayerStart f03Down3",       ExitID.f04Up2 },
            {"PlayerStart fLDown",         ExitID.fL02Up },

            //gates
            {"PlayerStart fL11GateY0",     ExitID.f00GateY0 },
            {"PlayerStart f02GateYA",      ExitID.f00GateYA },
            {"PlayerStart f03GateYC",      ExitID.f00GateYC },
            {"PlayerStart f04GateYB",      ExitID.f00GateYB },
            {"PlayerStart f00GateYA",      ExitID.f02GateYA },
            {"PlayerStart f00GateYC",      ExitID.f03GateYC },
            {"PlayerStart f00GateYB",      ExitID.f04GateYB },
            {"PlayerStart f10GateP0",      ExitID.f05GateP1 },
            {"PlayerStart f06-2GateP0",    ExitID.f06GateP0 },
            {"PlayerStart f06GateP0",      ExitID.f06_2GateP0 },
            {"PlayerStart f11GateP0",      ExitID.f07GateP0 },
            {"PlayerStart f12GateP0",      ExitID.f08GateP0 },
            {"PlayerStart f13GateP0",      ExitID.f09GateP0 },
            {"PlayerStart f05GateP1",      ExitID.f10GateP0 },
            {"PlayerStart f07GateP0",      ExitID.f11GateP0 },
            {"PlayerStart f08GateP0",      ExitID.f12GateP0 },
            {"PlayerStart f09GateP0",      ExitID.f13GateP0 },
            {"PlayerStart fL11GateN",      ExitID.fLGate },
            {"PlayerStart f00GateY0",      ExitID.fL11GateY0 },
            {"PlayerStart fLGate",         ExitID.fL11GateN },

            //soul gates
            {"PlayerStart f05GateN1",      ExitID.f00GateN1 },
            {"PlayerStart f06GateN2",      ExitID.f02GateN2 },
            {"PlayerStart f07GateN3",      ExitID.f03GateN3 },
            {"PlayerStart f08GateN4",      ExitID.f03GateN4 },
            {"PlayerStart f13GateN9",      ExitID.f03GateN9 },
            {"PlayerStart f09GateN5",      ExitID.f04GateN5 },
            {"PlayerStart f14GateN6",      ExitID.f04GateN6 },
            {"PlayerStart f00GateN1",      ExitID.f05GateN1 },
            {"PlayerStart f02GateN2",      ExitID.f06GateN2 },
            {"PlayerStart f10GateN7",      ExitID.f06GateN7 },
            {"PlayerStart f03GateN3",      ExitID.f07GateN3 },
            {"PlayerStart f03GateN4",      ExitID.f08GateN4 },
            {"PlayerStart f12GateN8",      ExitID.f08GateN8 },
            {"PlayerStart f04GateN5",      ExitID.f09GateN5 },
            {"PlayerStart f06GateN7",      ExitID.f10GateN7 },
            {"PlayerStart f08GateN8",      ExitID.f12GateN8 },
            {"PlayerStart f03GateN9",      ExitID.f13GateN9 },
            {"PlayerStart f04GateN6",      ExitID.f14GateN6 },
            {"PlayerStart f04GateN62",     ExitID.f14GateN6 }    
        };
    }

    public class ExitInfo
    {
        public string AnchorName;
        public int FieldNo;
        public int SheetNo;
        public int FlagNo;

        public ExitInfo(string anchorName, int fieldNo, int sheetNo, int flagNo)
        {
            AnchorName = anchorName;
            FieldNo = fieldNo;
            SheetNo = sheetNo;
            FlagNo = flagNo;
        }
    }
}
