using System.Composition;
using System.Threading.Tasks;
using BCad.Services;
using System.Threading;

namespace BCad.Commands
{
    [ExportCommand("Debug.Dump", "DUMP")]
    internal class DebugDumpCommand : ICommand
    {
        [Import]
        public IWorkspace Workspace { get; set; }

        [Import]
        public IDebugService DebugService { get; set; }

        [Import]
        public IOutputService OutputService { get; set; }

        public Task<bool> Execute(object arg = null)
        {
            OutputService.WriteLine("input service entries:");
            var entries = DebugService.GetLog();
            foreach (var entry in entries)
            {
                OutputService.WriteLine("  " + entry.ToString());
            }

            OutputService.WriteLine("drawing structure:");
            foreach (var layer in Workspace.Drawing.Layers.GetValues())
            {
                OutputService.WriteLine("  layer={0}", layer.Name);
                foreach (var entity in layer.GetEntities())
                {
                    OutputService.WriteLine("    entity id={0}, detail={1}", entity.Id, entity);
                }
            }

            return Task.FromResult<bool>(true);
        }
    }
}
