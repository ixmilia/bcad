using BCad.Services;

namespace BCad.UI
{
    public interface IRendererFactory
    {
        IRenderer CreateRenderer(IViewControl viewControl, IWorkspace workspace, IInputService inputService);
    }
}
