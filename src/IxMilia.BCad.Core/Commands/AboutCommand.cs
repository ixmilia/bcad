using System.Threading.Tasks;

namespace IxMilia.BCad.Commands
{
    internal class AboutCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg = null)
        {
            await workspace.DialogService.ShowDialog("about", null);
            return true;
        }
    }
}
