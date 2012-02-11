using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.Objects;

namespace BCad.EventArguments
{
    public class AbstractObjectEventArgs : EventArgs
    {
        public IObject Object { get; protected set; }

        public AbstractObjectEventArgs(IObject obj)
        {
            Object = obj;
        }
    }
}
