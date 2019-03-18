using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LM2Randomiser.RuleParsing
{
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
        True
    }

    public class Rule
    {
        public Rule(string rule, string value = null)
        {
            ruleType = (RuleType)Enum.Parse(typeof(RuleType), rule);
            this.value = value;
        }

        public RuleType ruleType;
        public string value;
    }
}
