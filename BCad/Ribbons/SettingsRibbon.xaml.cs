using System.Composition;
using Microsoft.Windows.Controls.Ribbon;

namespace BCad.Ribbons
{
    /// <summary>
    /// Interaction logic for DrawingSettingsRibbon.xaml
    /// </summary>
    [ExportRibbonTab("settings")]
    public partial class SettingsRibbon : RibbonTab
    {
        private IWorkspace workspace = null;
        private SettingsRibbonViewModel viewModel = null;

        public SettingsRibbon()
        {
            InitializeComponent();
        }

        [ImportingConstructor]
        public SettingsRibbon(IWorkspace workspace)
            : this()
        {
            this.workspace = workspace;
            viewModel = new SettingsRibbonViewModel(workspace);
            DataContext = viewModel;
            workspace.WorkspaceChanged += WorkspaceChanged;
        }

        void WorkspaceChanged(object sender, EventArguments.WorkspaceChangeEventArgs e)
        {
            viewModel.UpdateProperty();
        }
    }
}
