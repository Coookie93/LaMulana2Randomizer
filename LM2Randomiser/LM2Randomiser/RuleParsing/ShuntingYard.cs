using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LM2Randomiser.Logging;

namespace LM2Randomiser.RuleParsing
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

                switch (token.type)
                {
                    case TokenType.RuleToken:
                        outputQueue.Enqueue(token);
                        break;

                    case TokenType.AndOperator:
                        while (stack.Count > 0 && stack.Peek().type == TokenType.AndOperator)
                        {
                            outputQueue.Enqueue(stack.Pop());
                        }
                        stack.Push(token);
                        break;

                    case TokenType.OrOperator:
                        while (stack.Count > 0 && (stack.Peek().type == TokenType.AndOperator || stack.Peek().type == TokenType.OrOperator))
                        {
                            outputQueue.Enqueue(stack.Pop());
                        }
                        stack.Push(token);
                        break;

                    case TokenType.OpenParentheses:
                        stack.Push(token);
                        break;

                    case TokenType.ClosedParentheses:
                        while (stack.Count > 0 && stack.Peek().type != TokenType.OpenParentheses)
                        {
                            outputQueue.Enqueue(stack.Pop());
                        }
                        if (stack.Peek().type != TokenType.OpenParentheses)
                        {
                            Logger.GetLogger.Log("Mismatched Parenthesis");
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
