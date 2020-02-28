using System;
using System.Collections.Generic;
using LaMulana2Randomizer;
using LaMulana2Randomizer.Utils;

namespace LaMulana2Randomizer.LogicParsing
{
    public abstract class LogicTree
    {
        public static BinaryNode ParseAndBuildLogic(string logicString)
        {
            try
            {
                IList<Token> tokens = new Tokeniser(logicString).Tokenise();
                IList<Token> polish = ShuntingYard.Sort(tokens);
                IEnumerator<Token> enumerator = polish.GetEnumerator();
                enumerator.MoveNext();

                return BuildLogicTree(enumerator);
            }
            catch(Exception ex)
            {
                Logger.Log($"Failed to parse or build logic string, {logicString}.");
                Logger.LogAndFlush(ex.Message);
                throw new LogicParsingExcpetion("Failed to parse logic.");
            }
        }

        private static BinaryNode BuildLogicTree(IEnumerator<Token> tokens)
        {
            if (tokens.Current.Type == TokenType.RuleToken)
            {
                LogicNode node = new LogicNode(tokens.Current.Logic, tokens.Current.Value);
                tokens.MoveNext();
                return node;
            }
            else if (tokens.Current.Type == TokenType.AndOperator)
            {
                tokens.MoveNext();
                AndNode node = new AndNode
                {
                    left = BuildLogicTree(tokens),
                    right = BuildLogicTree(tokens)
                };
                return node;
            }
            else if (tokens.Current.Type == TokenType.OrOperator)
            {
                tokens.MoveNext();
                OrNode node = new OrNode
                {
                    left = BuildLogicTree(tokens),
                    right = BuildLogicTree(tokens)
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

    public class LogicNode : BinaryNode
    {
        public Logic logic;

        public LogicNode(string logicType, string value = null)
        {
            logic = new Logic(logicType, value);
        }

        public override bool Evaluate(PlayerState state)
        {
            return state.Evaluate(logic);
        }
    }
}
