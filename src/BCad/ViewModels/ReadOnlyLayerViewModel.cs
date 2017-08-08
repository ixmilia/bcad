// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace IxMilia.BCad.ViewModels
{
    public class ReadOnlyLayerViewModel
    {
        public Layer Layer { get; private set; }

        public ReadOnlyLayerViewModel(Layer layer)
        {
            Layer = layer;
        }

        public string Name
        {
            get { return Layer.Name; }
        }

        public bool IsVisible
        {
            get { return Layer.IsVisible; }
        }

        public CadColor? Color
        {
            get { return Layer.Color; }
        }

        public static bool operator ==(ReadOnlyLayerViewModel a, ReadOnlyLayerViewModel b)
        {
            if ((object)a != null)
                return a.Equals(b);
            return (object)b == null;
        }

        public static bool operator !=(ReadOnlyLayerViewModel a, ReadOnlyLayerViewModel b)
        {
            if ((object)a != null)
                return !a.Equals(b);
            return (object)b != null;
        }

        public override bool Equals(object obj)
        {
            if (obj is ReadOnlyLayerViewModel)
            {
                var rl = (ReadOnlyLayerViewModel)obj;
                return this.Layer == rl.Layer;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Layer.GetHashCode();
        }
    }
}
