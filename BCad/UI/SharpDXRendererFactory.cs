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
            grid.Children.Add(element);
            return grid;
        }

        private class RendererGrid : Grid, IRenderer
        {
            public void ForceRendering()
            {
                // SharpDXRenderer always renders, anyways
            }
        }
    }
}
