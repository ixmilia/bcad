using System.Threading.Tasks;

namespace BCad.Commands
{
    [ExportCadCommand("File.Plot", "PLOT", ModifierKeys.Control, Key.P, "plot")]
    public class PlotCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var result = await workspace.DialogFactoryService.ShowDialog("Plot", workspace.SettingsManager.PlotDialogId);
            return result == true;
        }
    }
}
