using System;

namespace LaMulana2Randomizer
{
    public class RandomiserException : Exception
    {
        public RandomiserException(string message) : base(message) { }
    }

    public class LogicParsingExcpetion : RandomiserException
    {
        public LogicParsingExcpetion(string message) : base(message) { }
    }

    public class InvalidLocationException : RandomiserException
    {
        public InvalidLocationException(string message) : base(message) { }
    }

    public class InvalidAreaException : RandomiserException
    {
        public InvalidAreaException(string message) : base(message) { }
    }
}
