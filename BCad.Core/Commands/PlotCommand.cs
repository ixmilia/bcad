using System.Composition;
using System.Threading.Tasks;

namespace BCad.Commands
{
    [ExportCadCommand("File.Plot", "PLOT", ModifierKeys.Control, Key.P, "plot")]
    public class PlotCommand : ICadCommand
    {
        [Import]
        public IWorkspace Workspace { get; set; }

        [Import]
        public IDialogFactory DialogFactory { get; set; }

        public async Task<bool> Execute(object arg)
        {
            var result = await DialogFactory.ShowDialog("Plot", Workspace.SettingsManager.PlotDialogId);
            return result == true;
        }
    }
}
