using System.Composition;
using System.Threading.Tasks;

namespace BCad.Commands
{
    [ExportCommand("Edit.Layers", "LAYERS", ModifierKeys.Control, Key.L, "layers", "layer", "la")]
    public class LayersCommand : ICommand
    {
        [Import]
        public IWorkspace Workspace { get; set; }

        [Import]
        public IDialogFactory DialogFactory { get; set; }

        public async Task<bool> Execute(object arg)
        {
            var result = await DialogFactory.ShowDialog("Layer", Workspace.SettingsManager.LayerDialogId);
            return result == true;
        }
    }
}
