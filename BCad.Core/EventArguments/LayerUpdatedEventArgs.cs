using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCad.EventArguments
{
    public class LayerUpdatedEventArgs : EventArgs
    {
        public Layer OldLayer { get; private set; }
        public Layer NewLayer { get; private set; }

        public LayerUpdatedEventArgs(Layer oldLayer, Layer newLayer)
        {
            OldLayer = oldLayer;
            NewLayer = newLayer;
        }
    }
}
