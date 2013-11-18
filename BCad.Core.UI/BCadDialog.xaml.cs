using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BCad.UI
{
    /// <summary>
    /// Interaction logic for BCadDialog.xaml
    /// </summary>
    public partial class BCadDialog : Window, IDisposable
    {
        public BCadControl Control { get; private set; }

        private TaskCompletionSource<bool?> completionAwaiter = new TaskCompletionSource<bool?>();

        public BCadDialog(BCadControl control)
        {
            InitializeComponent();

            this.Control = control;
            this.Control.SetWindowParent(this);
            this.controlSurface.Content = this.Control;
        }

        public void Dispose()
        {
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (this.Control.Validate())
            {
                this.Control.Commit();
                this.Control = null;
                this.Close();
                completionAwaiter.SetResult(true);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Cancel();
        }

        private void Cancel()
        {
            this.Control.Cancel();
            this.Close();
            completionAwaiter.SetResult(false);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Cancel();
            }
        }

        public Task<bool?> ShowHideableDialog()
        {
            this.Show();
            return completionAwaiter.Task;
        }
    }
}
