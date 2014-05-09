using BCad.Services;

namespace BCad.UI.View
{
    [ExportRendererFactory("Default")]
    internal class XamlRendererFactory : IRendererFactory
    {
        public IRenderer CreateRenderer(IViewControl viewControl, IWorkspace workspace, IInputService inputService)
        {
            return new XamlRenderer(viewControl, workspace, inputService);
        }
    }
}
