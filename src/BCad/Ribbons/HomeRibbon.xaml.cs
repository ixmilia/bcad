using System.Composition;
using BCad.ViewModels;

namespace BCad.Ribbons
{
    /// <summary>
    /// Interaction logic for HomeRibbon.xaml
    /// </summary>
    [ExportRibbonTab("home")]
    public partial class HomeRibbon : CadRibbonTab
    {
        private HomeRibbonViewModel viewModel;

        [ImportingConstructor]
        public HomeRibbon(IWorkspace workspace)
            : base(workspace)
        {
            InitializeComponent();
            viewModel = new HomeRibbonViewModel(Workspace);
            DataContext = viewModel;
        }
    }
}
