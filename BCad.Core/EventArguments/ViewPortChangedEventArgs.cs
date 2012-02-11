using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace BCad.EventArguments
{
    public class ViewPortChangedEventArgs : EventArgs
    {
        public IView View { get; private set; }

        public ViewPortChangedEventArgs(IView view)
        {
            View = view;
        }
    }
}
