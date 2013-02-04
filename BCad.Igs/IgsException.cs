using System;

namespace BCad.Igs
{
    public class IgsException : Exception
    {
        public IgsException()
            : base()
        {
        }

        public IgsException(string message)
            : base(message)
        {
        }

        public IgsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
