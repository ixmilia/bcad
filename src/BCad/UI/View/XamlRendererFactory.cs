using BCad.UI.Shared;

namespace BCad.UI.View
{
    [ExportRendererFactory("Software")]
    internal class XamlRendererFactory : IRendererFactory
    {
        public AbstractCadRenderer CreateRenderer(IViewControl viewControl, IWorkspace workspace)
        {
            return new XamlRenderer(viewControl, workspace);
        }
    }
}
