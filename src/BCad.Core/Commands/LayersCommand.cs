// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace IxMilia.BCad.Commands
{
    [ExportCadCommand("Edit.Layers", "LAYERS", ModifierKeys.Control, Key.L, "layers", "layer", "la")]
    public class LayersCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var result = await workspace.DialogFactoryService.ShowDialog("Layer", workspace.SettingsService.GetValue<string>("UI.LayerDialogId"));
            return result == true;
        }
    }
}
