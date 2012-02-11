using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCad.EventArguments
{
    public class LayerAddedEventArgs : AbstractLayerEventArgs
    {
        public LayerAddedEventArgs(Layer layer)
            : base(layer)
        {
        }
    }
}
