using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace BCad
{
    [Export(typeof(IViewFactory))]
    internal class RenderedViewFactory : IViewFactory
    {
        public UserControl Generate()
        {
            var view = new WpfView();
            App.Container.SatisfyImportsOnce(view);
            return view;
        }
    }
}
