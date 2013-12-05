using System.Windows.Controls;
using BCad.Services;
using SharpDX.Toolkit;

namespace BCad.UI
{
    [ExportRendererFactory("SharpDX")]
    internal class SharpDXRendererFactory : IRendererFactory
    {
        public IRenderer CreateRenderer(IViewHost viewHost, IWorkspace workspace, IInputService inputService)
        {
            var element = new SharpDXElement();
            element.SendResizeToGame = true;
            element.LowPriorityRendering = true;
            var game = new SharpDXRenderer(workspace, inputService, viewHost);
            game.Run(element);
            var grid = new RendererGrid();
            grid.Content = element;
            return grid;
        }

        private class RendererGrid : UserControl, IRenderer
        {
            public void RenderRubberBandLines()
            {
                // SharpDXRenderer always renders, anyways
            }
        }
    }
}
