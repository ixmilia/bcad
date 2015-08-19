namespace BCad.UI.View
{
    [ExportRendererFactory("Software")]
    internal class XamlRendererFactory : IRendererFactory
    {
        public IRenderer CreateRenderer(IViewControl viewControl, IWorkspace workspace)
        {
            return new XamlRenderer(viewControl, workspace);
        }
    }
}
