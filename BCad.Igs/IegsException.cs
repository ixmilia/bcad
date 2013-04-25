﻿using System;

namespace BCad.Igs
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
