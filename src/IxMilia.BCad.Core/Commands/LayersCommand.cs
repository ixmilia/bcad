// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IxMilia.BCad.Dialogs;

namespace IxMilia.BCad.Commands
{
    [ExportCadCommand("Edit.Layers", "LAYERS", ModifierKeys.Control, Key.L, "layers", "layer", "la")]
    public class LayersCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var parameter = new LayerDialogParameters(workspace.Drawing);
            var result = (LayerDialogResult)await workspace.DialogService.ShowDialog("layer", parameter);
            if (result == null)
            {
                return false;
            }

            var layers = workspace.Drawing.Layers;
            var finalOldLayerNames = new HashSet<string>(result.Layers.Select(l => l.OldLayerName));
            var layersToDelete = layers.GetValues().Where(l => !finalOldLayerNames.Contains(l.Name));
            foreach (var layerToDelete in layersToDelete)
            {
                layers = layers.Delete(layerToDelete.Name);
            }

            foreach (var layerResult in result.Layers)
            {
                if (layers.TryFind(layerResult.OldLayerName, out var layer))
                {
                    // update
                    layer = layer.Update(
                        name: layerResult.NewLayerName,
                        color: layerResult.Color,
                        isVisible: layerResult.IsVisible);
                }
                else
                {
                    // add
                    layer = new Layer(
                        layerResult.NewLayerName,
                        color: layerResult.Color,
                        isVisible: layerResult.IsVisible);
                }

                layers = layers.Insert(layer.Name, layer);
            }

            workspace.Update(drawing: workspace.Drawing.Update(layers: layers));
            return true;
        }
    }
}
