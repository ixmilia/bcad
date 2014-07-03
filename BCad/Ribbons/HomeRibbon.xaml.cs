using System.Composition;
using System.Diagnostics;
using System.Windows.Input;
using BCad.Services;
using BCad.ViewModels;
using Microsoft.Windows.Controls.Ribbon;

namespace BCad.Ribbons
{
    /// <summary>
    /// Interaction logic for HomeRibbon.xaml
    /// </summary>
    [ExportRibbonTab("home")]
    public partial class HomeRibbon : RibbonTab
    {
        private IWorkspace workspace;
        private IInputService inputService;
        private HomeRibbonViewModel viewModel;

        public HomeRibbon()
        {
            InitializeComponent();
        }

        [ImportingConstructor]
        public HomeRibbon(IWorkspace workspace, IInputService inputService)
            : this()
        {
            this.workspace = workspace;
            this.inputService = inputService;
            viewModel = new HomeRibbonViewModel(this.workspace, this.inputService);
            DataContext = viewModel;
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = workspace == null ? false : workspace.CanExecute();
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.Assert(workspace != null, "Workspace should not have been null");
            var command = e.Parameter as string;
            Debug.Assert(command != null, "Command string should not have been null");
            workspace.ExecuteCommand(command);
        }
    }
}
