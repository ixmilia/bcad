using System.Threading.Tasks;

namespace IxMilia.BCad.Commands
{
    [ExportCadCommand("View.Pan", "PAN")]
    public class PanCommand : ICadCommand
    {
        public bool IsPanning { get; private set; }

        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            IsPanning = true;
            await workspace.InputService.GetNone();
            IsPanning = false;
            return true;
        }
    }
}
