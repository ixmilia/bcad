namespace BCad.UI.View
{
    [ExportRendererFactory("Hardware")]
    internal class SharpDXRendererFactory : IRendererFactory
    {
        public IRenderer CreateRenderer(IViewControl viewControl, IWorkspace workspace)
        {
            return new SharpDXRenderer(viewControl, workspace);
        }
    }
}
