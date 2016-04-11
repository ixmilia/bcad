using BCad.UI.Shared;

namespace BCad.UI.View
{
    /// <summary>
    /// Interaction logic for XamlRenderer.xaml
    /// </summary>
    public partial class XamlRenderer : AbstractCadRenderer
    {
        private IViewControl ViewControl;

        public XamlRenderer()
        {
            InitializeComponent();
        }

        public XamlRenderer(IViewControl viewControl, IWorkspace workspace)
            : this()
        {
            Initialize(workspace, viewControl);
            ViewControl = viewControl;
        }
    }
}
