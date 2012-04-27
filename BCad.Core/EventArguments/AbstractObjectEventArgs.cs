using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.Objects;

namespace BCad.EventArguments
{
    public class AbstractObjectEventArgs : EventArgs
    {
        public Entity Object { get; protected set; }

        public AbstractObjectEventArgs(Entity obj)
        {
            Object = obj;
        }
    }
}
