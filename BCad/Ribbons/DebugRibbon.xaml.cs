using System;
using System.Composition;
using Microsoft.Windows.Controls.Ribbon;

namespace BCad.Ribbons
{
    /// <summary>
    /// Interaction logic for DebugRibbon.xaml
    /// </summary>
    [ExportRibbonTab("debug")]
    public partial class DebugRibbon : RibbonTab
    {
        private IWorkspace workspace = null;

        public DebugRibbon()
        {
            InitializeComponent();
        }

        [ImportingConstructor]
        public DebugRibbon(IWorkspace workspace)
            : this()
        {
            this.workspace = workspace;
        }

        private void DebugButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            workspace.SettingsManager.ColorMap = ColorMap.AllBlack;
        }
    }
}
