using System;

namespace BCad.Iegs
{
    public class IegsException : Exception
    {
        public IegsException()
            : base()
        {
        }

        public IegsException(string message)
            : base(message)
        {
        }

        public IegsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
