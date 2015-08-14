using System.Threading.Tasks;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCadCommand("File.Plot", "PLOT", ModifierKeys.Control, Key.P, "plot")]
    public class PlotCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var dialogFactoryService = workspace.GetService<IDialogFactoryService>();
            var result = await dialogFactoryService.ShowDialog("Plot", workspace.SettingsManager.PlotDialogId);
            return result == true;
        }
    }
}
