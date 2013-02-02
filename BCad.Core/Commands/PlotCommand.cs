using System.ComponentModel.Composition;

namespace BCad.Commands
{
    [ExportCommand("File.Plot", "plot")]
    public class PlotCommand : ICommand
    {
        [Import]
        private IWorkspace Workspace = null;

        [Import]
        private IDialogFactory DialogFactory = null;

        public bool Execute(object arg)
        {
            var result = DialogFactory.ShowDialog("Plot", Workspace.SettingsManager.PlotDialogId);
            return result == true;
        }

        public string DisplayName
        {
            get { return "PLOT"; }
        }
    }
}
