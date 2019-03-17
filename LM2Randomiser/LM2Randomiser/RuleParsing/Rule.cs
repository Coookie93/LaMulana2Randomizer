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
            ruleType = GetRuleType(rule);
            this.value = value;
        }

        public RuleType ruleType;
        public string value;
        
        private RuleType GetRuleType(string rule)
        {
            switch (rule)
            {
                case "True":
                    return RuleType.True;
                case "CanReach":
                    return RuleType.CanReach;
                case "CanChant":
                    return RuleType.CanChant;
                case "CanWarp":
                    return RuleType.CanWarp;
                case "CanSpinCorridor":
                    return RuleType.CanSpinCorridor;
                case "Has":
                    return RuleType.Has;
                case "CanUse":
                    return RuleType.CanUse;
                case "OrbCount":
                    return RuleType.OrbCount;
                case "IsDead":
                    return RuleType.IsDead;
                case "GuardianKills":
                    return RuleType.GuardianKills;
                case "AnkhCount":
                    return RuleType.AnkhCount;
                case "PuzzleFinished":
                    return RuleType.PuzzleFinished;
                case "SkullCount":
                    return RuleType.SkullCount;
                case "Dissonance":
                    return RuleType.Dissonance;
                default:
                    throw new Exception();
            }
        }
    }
}
