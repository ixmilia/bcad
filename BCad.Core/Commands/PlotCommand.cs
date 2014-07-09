using System.Composition;
using System.Threading.Tasks;

namespace BCad.Commands
{
    [ExportCommand("File.Plot", "PLOT")]
    public class PlotCommand : ICommand
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
