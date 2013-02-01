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
        public IEnumerable<Lazy<BCadControl, IControlMetadata>> Controls { get; set; }

        public bool? ShowDialog(string type, string id)
        {
            var lazyControl = Controls.FirstOrDefault(c => c.Metadata.ControlType == type && c.Metadata.ControlId == id);
            if (lazyControl == null)
            {
                // TODO: throw
                MessageBox.Show("Unable to find specified dialog.");
                return false;
            }
            return (bool?)Application.Current.Dispatcher.Invoke(new Func<bool?>(() =>
                {
                    var control = lazyControl.Value;
                    using (var window = new BCadDialog(control))
                    {
                        window.Title = lazyControl.Metadata.Title;
                        window.Owner = Application.Current.MainWindow;
                        return window.ShowDialog();
                    }
                }));
        }
    }
}
