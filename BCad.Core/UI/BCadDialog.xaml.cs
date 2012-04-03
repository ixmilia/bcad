using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public partial class BCadDialog : Window
    {
        public BCadControl Control { get; private set; }

        public BCadDialog(BCadControl control)
        {
            InitializeComponent();

            this.Control = control;
            this.Control.Initialize();
            this.controlSurface.Content = this.Control;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            this.Control.Commit();
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Cancel();
        }

        private void Cancel()
        {
            this.Control.Cancel();
            this.DialogResult = false;
            this.Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Cancel();
            }
        }
    }
}
