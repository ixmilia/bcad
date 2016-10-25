using System.Composition;

namespace BCad.Ribbons
{
    /// <summary>
    /// Interaction logic for ViewRibbon.xaml
    /// </summary>
    [ExportRibbonTab("view")]
    public partial class ViewRibbon : CadRibbonTab
    {
        [ImportingConstructor]
        public ViewRibbon(IWorkspace workspace)
            : base(workspace)
        {
            InitializeComponent();
        }
    }
}
