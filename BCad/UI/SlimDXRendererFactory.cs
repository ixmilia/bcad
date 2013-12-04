using System.Windows;
using BCad.Services;

namespace BCad.UI
{
    [ExportRendererFactory("SlimDX")]
    internal class SlimDXRendererFactory : IRendererFactory
    {
        public FrameworkElement CreateRenderer(IViewHost viewHost, IWorkspace workspace, IInputService inputService)
        {
            var element = new SlimDXControl();
            var engine = new SlimDXRenderEngine(element, viewHost, workspace, inputService);
            element.SetRenderEngine(engine);
            return element;
        }
    }
}
