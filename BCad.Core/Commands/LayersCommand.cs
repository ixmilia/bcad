using System.Threading.Tasks;

namespace BCad.Commands
{
    [ExportCadCommand("Edit.Layers", "LAYERS", ModifierKeys.Control, Key.L, "layers", "layer", "la")]
    public class LayersCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var result = await workspace.DialogFactoryService.ShowDialog("Layer", workspace.SettingsManager.LayerDialogId);
            return result == true;
        }
    }
}
