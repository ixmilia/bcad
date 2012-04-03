using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using BCad.UI;

namespace BCad
{
    [Export(typeof(IDialogFactory))]
    internal class DialogFactory : IDialogFactory
    {
        [ImportMany]
        public IEnumerable<Lazy<IControlFactory, IControlFactoryMetadata>> ControlFactories { get; set; }

        public bool? ShowDialog(string id)
        {
            var factory = ControlFactories.First(f => f.Metadata.ControlId == id).Value;
            return (bool?)Application.Current.Dispatcher.Invoke(new Func<bool?>(() =>
            {
                var control = factory.Generate();
                var window = new BCadDialog(control);
                window.Owner = Application.Current.MainWindow;
                return window.ShowDialog();
            }));
        }
    }
}
