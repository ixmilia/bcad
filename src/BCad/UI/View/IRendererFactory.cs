using BCad.UI.Shared;

namespace BCad.UI
{
    public interface IRendererFactory
    {
        AbstractCadRenderer CreateRenderer(IViewControl viewControl, IWorkspace workspace);
    }
}
