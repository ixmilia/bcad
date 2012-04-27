using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.Objects;

namespace BCad.EventArguments
{
    public class ObjectRemovedEventArgs : AbstractObjectEventArgs
    {
        public ObjectRemovedEventArgs(Entity obj)
            : base(obj)
        {
        }
    }
}
