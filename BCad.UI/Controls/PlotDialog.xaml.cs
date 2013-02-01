using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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

namespace BCad.UI.Controls
{
    /// <summary>
    /// Interaction logic for PlotDialog.xaml
    /// </summary>
    [ExportControl("Plot", "Default", "Plot")]
    public partial class PlotDialog : BCadControl
    {
        private IWorkspace workspace = null;

        [Obsolete("The default constructor is only for the designer.  MEF import this item instead.")]
        public PlotDialog()
        {
            InitializeComponent();
        }

        [ImportingConstructor]
        public PlotDialog(IWorkspace workspace)
        {
            this.workspace = workspace;

            InitializeComponent();
        }
    }
}
