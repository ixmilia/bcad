using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCad.EventArguments
{
    public abstract class AbstractLayerEventArgs : EventArgs
    {
        public Layer Layer { get; protected set; }

        public AbstractLayerEventArgs(Layer layer)
        {
            Layer = layer;
        }
    }
}
