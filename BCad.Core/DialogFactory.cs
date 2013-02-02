using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using BCad.UI;

namespace BCad
{
    [Export(typeof(IDialogFactory))]
    internal class DialogFactory : IDialogFactory
    {
        [ImportMany]
        public IEnumerable<Lazy<BCadControl, IControlMetadata>> Controls { get; set; }

        public Task<bool?> ShowDialog(string type, string id)
        {
            var lazyControl = Controls.FirstOrDefault(c => c.Metadata.ControlType == type && c.Metadata.ControlId == id);
            if (lazyControl == null)
            {
                // TODO: throw
                MessageBox.Show("Unable to find specified dialog.");
                return Task.FromResult<bool?>(false);
            }

            var control = lazyControl.Value;
            using (var window = new BCadDialog(control))
            {
                window.Title = lazyControl.Metadata.Title;
                window.Owner = Application.Current.MainWindow;
                return Task.FromResult<bool?>(window.ShowDialog());
            }
        }
    }
}
