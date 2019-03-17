using System;
using System.IO;
using System.Collections.Generic;

namespace LM2Randomiser.RuleParsing
{
    public enum TokenType
    {
        OpenParentheses,
        ClosedParentheses,
        OrOperator,
        AndOperator,
        AssignmentOperator,
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

        StringReader reader;
        IList<Token> tokens;

        public IList<Token> Tokenise()
        {
            while (reader.Peek() != -1)
            {
                next = (char)reader.Peek();
                if (next.Equals('('))
                {
                    tokens.Add(new Token(TokenType.OpenParentheses));
                    reader.Read();
                }
                else if (next.Equals(')'))
                {
                    tokens.Add(new Token(TokenType.ClosedParentheses));
                    reader.Read();
                }
                else if (Char.IsLetter(next))
                {
                    Expression();
                }
                else if (Char.IsWhiteSpace(next))
                {
                    reader.Read();
                }
                else
                {
                    throw new Exception("Unable to parse character: " + next);
                }
            }

            return tokens;
        }

        void Expression()
        {
            string s = GeString();

            if (s.Equals("or"))
            {
                tokens.Add(new Token(TokenType.OrOperator));
            }
            else if (s.Equals("and"))
            {
                tokens.Add(new Token(TokenType.AndOperator));
            }
            else
            {
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
            while (Char.IsLetter(next))
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
