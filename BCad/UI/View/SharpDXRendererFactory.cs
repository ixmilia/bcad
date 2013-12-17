using System.Windows.Controls;
using BCad.Services;
using SharpDX.Toolkit;

namespace BCad.UI
{
    [ExportRendererFactory("SharpDX")]
    internal class SharpDXRendererFactory : IRendererFactory
    {
        public IRenderer CreateRenderer(IViewControl viewControl, IWorkspace workspace, IInputService inputService)
        {
            var element = new SharpDXElement();
            element.SendResizeToGame = true;
            element.LowPriorityRendering = true;
            var game = new SharpDXRenderer(workspace, inputService, viewControl);
            game.Run(element);
            var container = new RenderContainer();
            container.Content = element;
            return container;
        }

        private class RenderContainer : UserControl, IRenderer
        {
            public void RenderRubberBandLines()
            {
                // SharpDXRenderer always renders, anyways
            }
        }
    }
}
