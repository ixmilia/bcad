using System.Composition;
using System.Threading.Tasks;
using BCad.Entities;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCommand("Draw.Point", "POINT", "point", "p")]
    public class DrawPointCommand : ICommand
    {
        [Import]
        public IInputService InputService { get; set; }

        [Import]
        public IWorkspace Workspace { get; set; }

        public async Task<bool> Execute(object arg)
        {
            var location = await InputService.GetPoint(new UserDirective("Location"));
            if (location.Cancel) return false;
            if (!location.HasValue) return true;
            Workspace.AddToCurrentLayer(new Location(location.Value, null));
            return true;
        }
    }
}
