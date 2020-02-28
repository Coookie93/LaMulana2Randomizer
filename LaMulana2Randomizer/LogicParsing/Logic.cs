using System;

namespace LaMulana2Randomizer.LogicParsing
{
    public class Logic
    {
        public LogicType logicType;
        public string value;

        public Logic(string logic, string value = null)
        {
            if(!Enum.TryParse(logic, out logicType))
            {
                throw new InvalidLogicTypeException($"Failed to parse logic type, type of logic \"{logic}\" does not exist.");
            }
            this.value = value;
        }
    }

    public enum LogicType
    {
        CanReach,
        CanChant,
        CanWarp,
        CanSpinCorridor,
        Has,
        CanUse,
        IsDead,
        OrbCount,
        GuardianKills,
        PuzzleFinished,
        AnkhCount,
        Dissonance,
        SkullCount,
        HasWeaponUpgrade,
        True
    }

    public class InvalidLogicTypeException : Exception
    {
        public InvalidLogicTypeException(string message) : base(message) { }
    }
}
