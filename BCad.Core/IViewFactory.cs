using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace BCad
{
    public interface IViewFactory
    {
        UserControl Generate();
    }
}
