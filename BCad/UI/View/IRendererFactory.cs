namespace BCad.UI
{
    public interface IRendererFactory
    {
        IRenderer CreateRenderer(IViewControl viewControl, IWorkspace workspace);
    }
}
