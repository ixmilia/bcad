using System.Diagnostics;
using System.Threading.Tasks;

namespace IxMilia.BCad.Commands
{
    public class DebuggerAttachCommand : ICadCommand
    {
        public Task<bool> Execute(IWorkspace workspace, object arg = null)
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
            else
            {
                Debugger.Launch();
            }

            return Task.FromResult(true);
        }
    }
}
