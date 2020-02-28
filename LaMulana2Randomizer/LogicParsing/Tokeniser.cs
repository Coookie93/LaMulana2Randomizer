using System;
using System.IO;
using System.Collections.Generic;

namespace LaMulana2Randomizer.LogicParsing
{
    public enum TokenType
    {
        OpenParentheses,
        ClosedParentheses,
        OrOperator,
        AndOperator,
        RuleToken
    }

    public class Token
    {
        public Token(TokenType type, string logic = null, string value = null)
        {
            Type = type;
            Logic = logic;
            Value = value;
        }

        public TokenType Type;
        public string Logic;
        public string Value;
    }

    public class Tokeniser
    {
        public Tokeniser(string line)
        {
            reader = new StringReader(line);
            tokens = new List<Token>();
        }

        char next;
        int parenthesCheck = 0;

        StringReader reader;
        IList<Token> tokens;

        public IList<Token> Tokenise()
        {
            while (reader.Peek() != -1)
            {
                next = (char)reader.Peek();
                if (next.Equals('('))
                {
                    if (tokens.Count != 0)
                    {
                        Token previousToken = tokens[tokens.Count - 1];
                        if (previousToken.Type == TokenType.ClosedParentheses || previousToken.Type == TokenType.RuleToken)
                        {
                            throw new TokeniserException("An open parentheses can only follow an \"and\" expression, an \"or\" expression or an parenthesis.");
                        }
                    }

                    tokens.Add(new Token(TokenType.OpenParentheses));
                    parenthesCheck++;
                    reader.Read();
                }
                else if (next.Equals(')'))
                {
                    if (tokens.Count == 0)
                    {
                        throw new TokeniserException("Logic string can't start with a closed parentheses.");
                    }
                    else
                    {
                        Token previousToken = tokens[tokens.Count - 1];
                        if (previousToken.Type == TokenType.OpenParentheses || previousToken.Type == TokenType.AndOperator || previousToken.Type == TokenType.OrOperator)
                        {
                            throw new TokeniserException("A closed parentheses can only follow a logic expression or a closed parenthesis.");
                        }
                    }

                    tokens.Add(new Token(TokenType.ClosedParentheses));
                    parenthesCheck--;
                    reader.Read();
                }
                else if (char.IsLetter(next))
                {
                    Expression();
                }
                else if (char.IsWhiteSpace(next))
                {
                    reader.Read();
                }
                else
                {
                    throw new TokeniserException($"Failed to parse character \"{next}\" when tokenising logic string.");
                }
            }

            if(parenthesCheck != 0)
            {
                throw new TokeniserException("Mismatched amount of open and closed parentheses in logic string.");
            }

            return tokens;
        }

        void Expression()
        {
            string s = GeString();

            if (s.Equals("or"))
            {
                if (tokens.Count == 0)
                {
                    throw new TokeniserException("Logic string can't start with \"or\" expression.");
                }

                Token previousToken = tokens[tokens.Count - 1];
                if (previousToken.Type == TokenType.ClosedParentheses || previousToken.Type == TokenType.RuleToken)
                {
                    tokens.Add(new Token(TokenType.OrOperator));
                }
                else
                {
                    throw new TokeniserException("An \"or\" expression can only follow a closed parentheses or logic expression in a logic string.");
                }
            }
            else if (s.Equals("and"))
            {
                if (tokens.Count == 0)
                {
                    throw new TokeniserException("Rule string can't start with \"and\" expression.");
                }

                Token previousToken = tokens[tokens.Count - 1];
                if (previousToken.Type == TokenType.ClosedParentheses || previousToken.Type == TokenType.RuleToken)
                {
                    tokens.Add(new Token(TokenType.AndOperator));
                }
                else
                {
                    throw new TokeniserException("An \"and\" expression can only follow a closed parentheses or a logic expression in a logic string.");
                }
            }
            else
            {
                if (tokens.Count != 0)
                {
                    Token previousToken = tokens[tokens.Count - 1];
                    if (previousToken.Type == TokenType.ClosedParentheses || previousToken.Type == TokenType.RuleToken)
                    {
                        throw new TokeniserException("A rule expression can only follow and open parentheses, an \"and\" expression or an \"or\" expression in a logic string.");
                    }
                }

                if (next.Equals('('))
                {
                    reader.Read();
                    string value = GetValueString();
                    tokens.Add(new Token(TokenType.RuleToken, s, value));
                    reader.Read();
                }
                else
                {
                    tokens.Add(new Token(TokenType.RuleToken, s));
                }
            }
        }

        string GeString()
        {
            next = (char)reader.Peek();
            List<char> chars = new List<char>();
            while (char.IsLetter(next))
            {
                chars.Add(next);
                reader.Read();
                next = (char)reader.Peek();
            }

            return new string(chars.ToArray());
        }

        string GetValueString()
        {
            next = (char)reader.Peek();
            List<char> chars = new List<char>();
            while (!next.Equals(')'))
            {
                chars.Add(next);
                reader.Read();
                next = (char)reader.Peek();
            }

            return new string(chars.ToArray());
        }
    }

    public class TokeniserException : Exception
    {
        public TokeniserException(string message) : base(message) { }
    }
}
