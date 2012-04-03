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

        public bool? ShowDialog(Type type)
        {
            var factory = ControlFactories.First(f => f.Metadata.Type == type).Value;
            return (bool?)Application.Current.Dispatcher.Invoke(new Func<bool?>(() =>
            {
                var control = factory.Generate();
                var window = new BCadDialog(control);
                return window.ShowDialog();
            }));
        }
    }
}
