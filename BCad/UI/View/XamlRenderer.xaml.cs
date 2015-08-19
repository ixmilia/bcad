using System.Windows.Controls;

namespace BCad.UI.View
{
    /// <summary>
    /// Interaction logic for XamlRenderer.xaml
    /// </summary>
    public partial class XamlRenderer : UserControl, IRenderer
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
