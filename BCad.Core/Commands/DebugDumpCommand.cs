using System.Threading.Tasks;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCadCommand("Debug.Dump", "DUMP", "dump")]
    internal class DebugDumpCommand : ICadCommand
    {
        public Task<bool> Execute(IWorkspace workspace, object arg = null)
        {
            var outputService = workspace.GetService<IOutputService>();
            var debugService = workspace.GetService<IDebugService>();
            outputService.WriteLine("input service entries:");
            var entries = debugService.GetLog();
            foreach (var entry in entries)
            {
                outputService.WriteLine("  " + entry.ToString());
            }

            outputService.WriteLine("drawing structure:");
            foreach (var layer in workspace.Drawing.Layers.GetValues())
            {
                outputService.WriteLine("  layer={0}", layer.Name);
                foreach (var entity in layer.GetEntities())
                {
                    outputService.WriteLine("    entity id={0}, detail={1}", entity.Id, entity);
                }
            }

            return Task.FromResult(true);
        }
    }
}
