using System.Composition;

namespace BCad.Ribbons
{
    /// <summary>
    /// Interaction logic for DrawingSettingsRibbon.xaml
    /// </summary>
    [ExportRibbonTab("settings")]
    public partial class SettingsRibbon : CadRibbonTab
    {
        private SettingsRibbonViewModel viewModel = null;

        [ImportingConstructor]
        public SettingsRibbon(IWorkspace workspace)
            : base(workspace)
        {
            InitializeComponent();
            viewModel = new SettingsRibbonViewModel(Workspace);
            DataContext = viewModel;
            Workspace.WorkspaceChanged += WorkspaceChanged;
        }

        void WorkspaceChanged(object sender, EventArguments.WorkspaceChangeEventArgs e)
        {
            viewModel.UpdateProperty();
        }
    }
}
