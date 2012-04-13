using System.ComponentModel.Composition;
using System.Windows.Input;

namespace BCad.Commands
{
    [ExportCommand("Draw.Ortho", ModifierKeys.None, Key.F8, "ortho")]
    internal class OrthoCommand : ICommand
    {
        [Import]
        private IWorkspace Workspace = null;

        [Import]
        private IInputService InputService = null;

        public bool Execute(params object[] parameters)
        {
            Workspace.SettingsManager.OrthoganalLines = !Workspace.SettingsManager.OrthoganalLines;
            InputService.WriteLine("Ortho is {0}", Workspace.SettingsManager.OrthoganalLines ? "on" : "off");
            return true;
        }

        public string DisplayName { get { return "ORTHO"; } }
    }
}
