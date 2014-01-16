using System.Windows.Controls;
using BCad.Services;

namespace BCad.UI.View
{
    /// <summary>
    /// Interaction logic for XamlRenderer.xaml
    /// </summary>
    public partial class XamlRenderer : UserControl, IRenderer
    {
        private IViewControl ViewControl;
        private IInputService InputService;

        public XamlRenderer()
        {
            InitializeComponent();
        }

        public XamlRenderer(IViewControl viewControl, IWorkspace workspace, IInputService inputService)
            : this()
        {
            Initialize(workspace);
            ViewControl = viewControl;
            InputService = inputService;
        }        

        public void RenderRubberBandLines()
        {
            this.RubberBandCanvas.Children.Clear();
            var generator = InputService.PrimitiveGenerator;
            if (generator != null)
            {
                var primitives = generator(ViewControl.GetCursorPoint());
                if (primitives != null)
                {
                    foreach (var prim in primitives)
                    {
                        AddPrimitive(this.RubberBandCanvas, prim, IndexedColor.Auto);
                    }
                }
            }
        }
    }
}
