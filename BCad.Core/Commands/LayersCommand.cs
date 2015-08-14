using System.Threading.Tasks;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCadCommand("Edit.Layers", "LAYERS", ModifierKeys.Control, Key.L, "layers", "layer", "la")]
    public class LayersCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var dialogFactoryService = workspace.GetService<IDialogFactoryService>();
            var result = await dialogFactoryService.ShowDialog("Layer", workspace.SettingsManager.LayerDialogId);
            return result == true;
        }
    }
}
