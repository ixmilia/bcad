using System;

namespace BCad.Dxf
{
    public class DxfReadException : Exception
    {
        public DxfReadException()
            : base()
        {
        }

        public DxfReadException(string message)
            : base(message)
        {
        }

        public DxfReadException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
