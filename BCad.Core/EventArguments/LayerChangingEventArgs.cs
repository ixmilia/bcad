using System;

namespace BCad.EventArguments
{
    public class LayerChangingEventArgs : EventArgs
    {
        public Layer OldLayer { get; private set; }
        public Layer NewLayer { get; private set; }
        public bool Cancel { get; set; }

        public LayerChangingEventArgs(Layer oldLayer, Layer newLayer)
        {
            Cancel = false;
            OldLayer = oldLayer;
            NewLayer = newLayer;
        }
    }
}
