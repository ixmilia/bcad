using BCad.Services;

namespace BCad.UI
{
    public interface IRendererFactory
    {
        IRenderer CreateRenderer(IViewHost viewHost, IWorkspace workspace, IInputService inputService);
    }
}
