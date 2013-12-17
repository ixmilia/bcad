using System.Windows;
using BCad.Services;

namespace BCad.UI
{
    [ExportRendererFactory("SlimDX")]
    internal class SlimDXRendererFactory : IRendererFactory
    {
        public IRenderer CreateRenderer(IViewControl viewControl, IWorkspace workspace, IInputService inputService)
        {
            var element = new SlimDXControl();
            var engine = new SlimDXRenderEngine(element, viewControl, workspace, inputService);
            return element;
        }
    }
}
