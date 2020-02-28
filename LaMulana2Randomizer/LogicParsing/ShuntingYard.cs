using System;
using System.Linq;
using System.Collections.Generic;

namespace LaMulana2Randomizer.LogicParsing
{
    public abstract class ShuntingYard
    {
        public static IList<Token> Sort(IList<Token> tokens)
        {
            Queue<Token> outputQueue = new Queue<Token>();
            Stack<Token> stack = new Stack<Token>();

            int index = 0;

            while (tokens.Count > index)
            {
                Token token = tokens[index];

                switch (token.Type)
                {
                    case TokenType.RuleToken:
                        outputQueue.Enqueue(token);
                        break;

                    case TokenType.AndOperator:
                        while (stack.Count > 0 && stack.Peek().Type == TokenType.AndOperator)
                        {
                            outputQueue.Enqueue(stack.Pop());
                        }
                        stack.Push(token);
                        break;

                    case TokenType.OrOperator:
                        while (stack.Count > 0 && (stack.Peek().Type == TokenType.AndOperator || stack.Peek().Type == TokenType.OrOperator))
                        {
                            outputQueue.Enqueue(stack.Pop());
                        }
                        stack.Push(token);
                        break;

                    case TokenType.OpenParentheses:
                        stack.Push(token);
                        break;

                    case TokenType.ClosedParentheses:
                        while (stack.Count > 0 && stack.Peek().Type != TokenType.OpenParentheses)
                        {
                            outputQueue.Enqueue(stack.Pop());
                        }
                        if (stack.Count == 0 || stack.Peek().Type != TokenType.OpenParentheses)
                        {
                            //NOTE:Should never hit this is theory
                            throw new Exception("Mismatched parentheses.");
                        }
                        else
                        {
                            stack.Pop();
                        }
                        break;

                    default:
                        break;
                }
                index++;
            }

            while (stack.Count > 0)
            {
                outputQueue.Enqueue(stack.Pop());
            }

            return outputQueue.Reverse().ToList();
        }
    }
}
