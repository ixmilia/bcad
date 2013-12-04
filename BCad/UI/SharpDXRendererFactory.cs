using System.Windows;
using BCad.Services;
using SharpDX.Toolkit;

namespace BCad.UI
{
    [ExportRendererFactory("SharpDX")]
    internal class SharpDXRendererFactory : IRendererFactory
    {
        public FrameworkElement CreateRenderer(IViewHost viewHost, IWorkspace workspace, IInputService inputService)
        {
            var element = new SharpDXElement();
            element.SendResizeToGame = true;
            element.LowPriorityRendering = false;
            var game = new SharpDXRenderer(workspace, inputService, viewHost);
            game.Run(element);
            return element;
        }
    }
}
