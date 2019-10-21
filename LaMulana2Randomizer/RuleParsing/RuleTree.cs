using System;
using System.Collections.Generic;

namespace LM2Randomizer.RuleParsing
{
    public abstract class RuleTree
    {
        public static BinaryNode ParseAndBuildRules(string ruleString)
        {
            try
            {
                IList<Token> tokens = new Tokeniser(ruleString).Tokenise();
                IList<Token> polish = ShuntingYard.Sort(tokens);
                IEnumerator<Token> enumerator = polish.GetEnumerator();
                enumerator.MoveNext();

                return BuildRuleTree(enumerator);
            }
            catch(Exception)
            {
                throw new Exception($"Failed to parse or build rule string, {ruleString}.");
            }
        }

        private static BinaryNode BuildRuleTree(IEnumerator<Token> tokens)
        {
            if (tokens.Current.type == TokenType.RuleToken)
            {
                RuleNode node = new RuleNode(tokens.Current.rule, tokens.Current.value);
                tokens.MoveNext();
                return node;
            }
            else if (tokens.Current.type == TokenType.AndOperator)
            {
                tokens.MoveNext();
                AndNode node = new AndNode
                {
                    left = BuildRuleTree(tokens),
                    right = BuildRuleTree(tokens)
                };
                return node;
            }
            else if (tokens.Current.type == TokenType.OrOperator)
            {
                tokens.MoveNext();
                OrNode node = new OrNode
                {
                    left = BuildRuleTree(tokens),
                    right = BuildRuleTree(tokens)
                };
                return node;
            }

            return null;
        }
    }

    public abstract class BinaryNode
    {
        public BinaryNode left;
        public BinaryNode right;

        public abstract bool Evaluate(PlayerState state);
    }

    public class AndNode : BinaryNode
    {
        public override bool Evaluate(PlayerState state)
        {
            return left.Evaluate(state) && right.Evaluate(state);
        }
    }

    public class OrNode : BinaryNode
    {
        public override bool Evaluate(PlayerState state)
        {
            return left.Evaluate(state) || right.Evaluate(state);
        }
    }

    public class RuleNode : BinaryNode
    {
        public Rule rule;

        public RuleNode(string ruleType, string value = null)
        {
            rule = new Rule(ruleType, value);
        }

        public override bool Evaluate(PlayerState state)
        {
            return state.Evaluate(rule);
        }
    }
}
