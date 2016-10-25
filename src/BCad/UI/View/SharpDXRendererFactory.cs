using BCad.UI.Shared;

namespace BCad.UI.View
{
    [ExportRendererFactory("Hardware")]
    internal class SharpDXRendererFactory : IRendererFactory
    {
        public AbstractCadRenderer CreateRenderer(IViewControl viewControl, IWorkspace workspace)
        {
            return new SharpDXRenderer(viewControl, workspace);
        }
    }
}
