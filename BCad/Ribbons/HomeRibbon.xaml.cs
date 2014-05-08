using System.Composition;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Windows.Controls.Ribbon;
using BCad.ViewModels;

namespace BCad.Ribbons
{
    /// <summary>
    /// Interaction logic for HomeRibbon.xaml
    /// </summary>
    [ExportRibbonTab("home")]
    public partial class HomeRibbon : RibbonTab
    {
        private IWorkspace workspace;
        private HomeRibbonViewModel viewModel;

        public HomeRibbon()
        {
            InitializeComponent();
        }

        [ImportingConstructor]
        public HomeRibbon(IWorkspace workspace)
            : this()
        {
            this.workspace = workspace;
            viewModel = new HomeRibbonViewModel(this.workspace);
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

        private void CurrentLayerSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var layer = (Layer)this.currentLayer.SelectedItem;
            workspace.Update(drawing: workspace.Drawing.Update(currentLayerName: layer.Name));
        }
    }
}
