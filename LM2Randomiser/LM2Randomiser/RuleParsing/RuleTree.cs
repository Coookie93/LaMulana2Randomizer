using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LM2Randomiser.RuleParsing
{
    public class RuleTree
    {
        public static BinaryNode ParseAndBuildRules(string ruleString)
        {
            //NOTE:maybe validate these afterwards
            var tokens = new Tokeniser(ruleString).Tokenise();

            var polish = ShuntingYard.Sort(tokens);

            var enumerator = polish.GetEnumerator();
            enumerator.MoveNext();
            
            return RuleTree.BuildRuleTree(enumerator);
        }

        internal static BinaryNode BuildRuleTree(IEnumerator<Token> tokens)
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
                AndNode node = new AndNode();
                node.left = BuildRuleTree(tokens);
                node.right = BuildRuleTree(tokens);
                return node;
            }
            else if (tokens.Current.type == TokenType.OrOperator)
            {
                tokens.MoveNext();
                OrNode node = new OrNode();
                node.left = BuildRuleTree(tokens);
                node.right = BuildRuleTree(tokens);
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
        public RuleNode(string ruleType, string value = null)
        {
            rule = new Rule(ruleType, value);
        }

        public Rule rule;

        public override bool Evaluate(PlayerState state)
        {
            return state.Evaluate(rule);
        }
    }
}
