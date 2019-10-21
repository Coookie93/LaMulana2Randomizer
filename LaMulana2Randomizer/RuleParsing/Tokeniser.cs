using System;
using System.IO;
using System.Collections.Generic;

namespace LM2Randomizer.RuleParsing
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
        public Token(TokenType type, string rule = null, string value = null)
        {
            this.type = type;
            this.rule = rule;
            this.value = value;

        }

        public TokenType type;
        public string rule;
        public string value;
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
                        if (previousToken.type == TokenType.ClosedParentheses || previousToken.type == TokenType.RuleToken)
                        {
                            throw new Exception("An opne parentheses can only follow an \"and\" expression, an \"or\" expression or an parenthesis.");
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
                        throw new Exception("Rule string can't start with a closed parentheses.");
                    }
                    else
                    {
                        Token previousToken = tokens[tokens.Count - 1];
                        if (previousToken.type == TokenType.OpenParentheses || previousToken.type == TokenType.AndOperator || previousToken.type == TokenType.OrOperator)
                        {
                            throw new Exception("A closed parentheses can only follow a rule expression or a closed parenthesis.");
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
                    throw new Exception($"Failed to parse character \"{next}\" when tokenising rule string.");
                }
            }

            if(parenthesCheck != 0)
            {
                throw new Exception("Mismatched amount of open and closed parentheses in rule string.");
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
                    throw new Exception("Rule string can't start with \"or\" expression.");
                }

                Token previousToken = tokens[tokens.Count - 1];
                if (previousToken.type == TokenType.ClosedParentheses || previousToken.type == TokenType.RuleToken)
                {
                    tokens.Add(new Token(TokenType.OrOperator));
                }
                else
                {
                    throw new Exception("An \"or\" expression can only follow a closed parentheses or a rule in a rule string.");
                }
            }
            else if (s.Equals("and"))
            {
                if (tokens.Count == 0)
                {
                    throw new Exception("Rule string can't start with \"and\" expression.");
                }

                Token previousToken = tokens[tokens.Count - 1];
                if (previousToken.type == TokenType.ClosedParentheses || previousToken.type == TokenType.RuleToken)
                {
                    tokens.Add(new Token(TokenType.AndOperator));
                }
                else
                {
                    throw new Exception("An \"and\" expression can only follow a closed parentheses or a rule expression in a rule string.");
                }
            }
            else
            {
                if (tokens.Count != 0)
                {
                    Token previousToken = tokens[tokens.Count - 1];
                    if (previousToken.type == TokenType.ClosedParentheses || previousToken.type == TokenType.RuleToken)
                    {
                        throw new Exception("A rule expression can only follow and open parentheses, an \"and\" expression or an \"or\" expression in a rule string.");
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

}
