using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCad.EventArguments
{
    public class LayerRemovedEventArgs : AbstractLayerEventArgs
    {
        public LayerRemovedEventArgs(Layer layer)
            : base(layer)
        {
        }
    }
}
