using System.Windows;
using BCad.Services;

namespace BCad.UI
{
    public interface IRendererFactory
    {
        FrameworkElement CreateRenderer(IViewHost viewHost, IWorkspace workspace, IInputService inputService);
    }
}
