using System.ComponentModel.Composition;
using System.Windows.Input;

namespace BCad.Commands
{
    [ExportCommand("Draw.AngleSnap", ModifierKeys.None, Key.F8, "asnap")]
    internal class AngleSnapCommand : ICommand
    {
        [Import]
        private IWorkspace Workspace = null;

        [Import]
        private IInputService InputService = null;

        public bool Execute(object arg)
        {
            Workspace.SettingsManager.AngleSnap = !Workspace.SettingsManager.AngleSnap;
            InputService.WriteLine("Angle snap is {0}", Workspace.SettingsManager.AngleSnap ? "on" : "off");
            return true;
        }

        public string DisplayName { get { return "ASNAP"; } }
    }
}
