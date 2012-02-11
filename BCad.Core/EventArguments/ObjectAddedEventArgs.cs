using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.Objects;

namespace BCad.EventArguments
{
    public class ObjectAddedEventArgs : AbstractObjectEventArgs
    {
        public ObjectAddedEventArgs(IObject obj)
            : base(obj)
        {
        }
    }
}
