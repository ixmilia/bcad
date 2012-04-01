using System;

namespace BCad.EventArguments
{
    public class LayerChangedEventArgs : EventArgs
    {
        public Layer Layer { get; private set; }

        public LayerChangedEventArgs(Layer layer)
        {
            this.Layer = layer;
        }
    }
}
