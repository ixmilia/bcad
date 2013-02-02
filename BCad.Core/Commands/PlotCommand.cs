using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace BCad.Commands
{
    [ExportCommand("File.Plot", "plot")]
    public class PlotCommand : ICommand
    {
        [Import]
        private IWorkspace Workspace = null;

        [Import]
        private IDialogFactory DialogFactory = null;

        public async Task<bool> Execute(object arg)
        {
            var result = await DialogFactory.ShowDialog("Plot", Workspace.SettingsManager.PlotDialogId);
            return result == true;
        }

        public string DisplayName
        {
            get { return "PLOT"; }
        }
    }
}
