using System;

namespace LM2Randomizer.RuleParsing
{
    public class Rule
    {
        public RuleType ruleType;
        public string value;

        public Rule(string rule, string value = null)
        {
            if(!Enum.TryParse(rule, out ruleType))
            {
                throw new Exception($"Failed to parse rule type, type of rule \"{rule}\" does not exist.");
            }
            this.value = value;
        }
    }

    public enum RuleType
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
}
