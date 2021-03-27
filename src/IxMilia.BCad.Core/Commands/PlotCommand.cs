using System.Threading.Tasks;

namespace IxMilia.BCad.Commands
{
    public class PlotCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var result = (bool)(await workspace.DialogService.ShowDialog("plot", null));
            return result == true;
        }
    }
}
