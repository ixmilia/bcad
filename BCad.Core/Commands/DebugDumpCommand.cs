using System.Composition;
using System.Threading.Tasks;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCommand("Debug.Dump", "DUMP", "dump")]
    internal class DebugDumpCommand : ICommand
    {
        [Import]
        public IWorkspace Workspace { get; set; }

        [Import]
        public IDebugService DebugService { get; set; }

        [Import]
        public IInputService InputService { get; set; }

        public Task<bool> Execute(object arg = null)
        {
            InputService.WriteLine("input service entries:");
            var entries = DebugService.GetLog();
            foreach (var entry in entries)
            {
                InputService.WriteLine("  " + entry.ToString());
            }

            InputService.WriteLine("drawing structure:");
            foreach (var layer in Workspace.Drawing.Layers.GetValues())
            {
                InputService.WriteLine("  layer={0}", layer.Name);
                foreach (var entity in layer.GetEntities())
                {
                    InputService.WriteLine("    entity id={0}, detail={1}", entity.Id, entity);
                }
            }

            return Task.FromResult<bool>(true);
        }
    }
}
