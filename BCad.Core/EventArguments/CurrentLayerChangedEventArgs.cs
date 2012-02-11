using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCad.EventArguments
{
    public class CurrentLayerChangedEventArgs : AbstractLayerEventArgs
    {
        public CurrentLayerChangedEventArgs(Layer layer)
            : base(layer)
        {
        }
    }
}
