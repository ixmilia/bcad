﻿using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BCad.Commands
{
    [ExportCommand("Edit.Layers", ModifierKeys.Control, Key.L, "layers", "lay", "la")]
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

        public string DisplayName
        {
            get { return "LAYERS"; }
        }
    }
}
