using System.Composition;
using System.Windows.Controls;

namespace BCad.UI
{
    /// <summary>
    /// Interaction logic for SharpDXViewControl.xaml
    /// </summary>
    [ExportViewControl("SharpDX")]
    public partial class SharpDXViewControl : UserControl, IViewControl
    {
        private readonly CadRendererGame game;
        private readonly IWorkspace workspace;

        public SharpDXViewControl()
        {
            InitializeComponent();
        }

        [ImportingConstructor]
        public SharpDXViewControl(IWorkspace workspace)
            : this()
        {
            this.workspace = workspace;
            game = new CadRendererGame(workspace);
            game.Run(surface);
        }

        public Point GetCursorPoint()
        {
            var transform = workspace.ActiveViewPort.GetTransformationMatrix(ActualWidth, ActualHeight);
            transform.Invert();
            var mouse = System.Windows.Input.Mouse.GetPosition(this);
            return transform.Transform(new Point(mouse.X, mouse.Y, 0));
        }
    }
}
