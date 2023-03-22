using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IxMilia.BCad.Collections;
using IxMilia.BCad.Dialogs;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Extensions;

namespace IxMilia.BCad.Commands
{
    public class DimensionStylesCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg = null)
        {
            var parameter = DimensionStylesDialogParameters.FromDrawing(workspace.Drawing);
            var dimensionStyleChanges = await workspace.DialogService.ShowDialog("dimension-styles", parameter) as DimensionStylesDialogParameters;
            if (dimensionStyleChanges == null)
            {
                return false;
            }

            var updatedDrawing = ApplyDimensionStyleChanges(workspace.Drawing, dimensionStyleChanges);
            workspace.Update(drawing: updatedDrawing);
            return true;
        }

        public static Drawing ApplyDimensionStyleChanges(Drawing drawing, DimensionStylesDialogParameters dimensionStyleChanges)
        {
            // build new collection of dimension styles
            var currentDimensionStyleName = dimensionStyleChanges.CurrentDimensionStyleName;
            var newDimensionStyles = dimensionStyleChanges.DimensionStyles.Where(d => !d.IsDeleted).Select(d => d.ToDimensionStyle());
            var newDimensionStyleCollection = DimensionStyleCollection.FromEnumerable(newDimensionStyles);
            if (!newDimensionStyleCollection.Any())
            {
                // ensure there's _something_
                var dimensionStyle = DimensionStyle.CreateDefault();
                newDimensionStyleCollection = newDimensionStyleCollection.Add(dimensionStyle);
                currentDimensionStyleName = dimensionStyle.Name;
            }

            // remap entities based on renames or deletions
            var dimStyleNameMap = dimensionStyleChanges.DimensionStyles.Where(d => !d.IsDeleted).ToDictionary(d => d.OriginalName, d => d.Name);
            var newLayers = new List<Layer>();
            foreach (var layer in drawing.Layers.GetValues())
            {
                var newEntities = layer.GetEntities().Select(e =>
                {
                    switch (e)
                    {
                        case AbstractDimension dim:
                            if (dimStyleNameMap.TryGetValue(dim.DimensionStyleName, out var updatedDimensionStyleName))
                            {
                                return dim.WithDimensionStyleName(updatedDimensionStyleName);
                            }
                            else
                            {
                                // couldn't find a matching style, so fall back to the default
                                return dim.WithDimensionStyleName(currentDimensionStyleName);
                            }
                        default:
                            return e;
                    }
                });

                var updatedLayer = layer.Update(entities: ReadOnlyTree<uint, Entity>.FromEnumerable(newEntities, e => e.Id));
                newLayers.Add(updatedLayer);
            }

            var updatedLayers = ReadOnlyTree<string, Layer>.FromEnumerable(newLayers, l => l.Name);
            var updatedSettings = drawing.Settings.Update(currentDimensionStyleName: currentDimensionStyleName, dimStyles: newDimensionStyleCollection);
            var updatedDrawing = drawing.Update(layers: updatedLayers, settings: updatedSettings);
            return updatedDrawing;
        }
    }
}
