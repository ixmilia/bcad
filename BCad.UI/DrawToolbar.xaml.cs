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
using System.ComponentModel.Composition;

namespace BCad.UI
{
    /// <summary>
    /// Interaction logic for DrawToolbar.xaml
    /// </summary>
    [ExportToolbar]
    public partial class DrawToolbar : ToolBar
    {
        [Import]
        public IWorkspace Workspace { get; set; }

        public DrawToolbar()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var command = button.Tag as string;
                if (command != null)
                {
                    Workspace.ExecuteCommand(command);
                }
            }
        }
    }
}
