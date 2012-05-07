using System.ComponentModel.Composition;
using System.Windows.Input;

namespace BCad.Commands
{
    [ExportCommand("Edit.Layers", ModifierKeys.Control, Key.L, "layers", "lay", "la")]
    internal class LayersCommand : ICommand
    {
        [Import]
        public IWorkspace Workspace { get; set; }

        [Import]
        public IDialogFactory DialogFactory { get; set; }

        public bool Execute(object arg)
        {
            var result = DialogFactory.ShowDialog(Workspace.SettingsManager.LayerDialogId);
            return result == true;
        }

        public string DisplayName
        {
            get { return "LAYERS"; }
        }
    }
}
