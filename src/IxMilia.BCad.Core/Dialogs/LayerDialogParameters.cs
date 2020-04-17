using System.Collections.Generic;
using System.Linq;

namespace IxMilia.BCad.Dialogs
{
    public partial class LayerDialogParameters
    {
        public List<Layer> Layers { get; } = new List<Layer>();

        public LayerDialogParameters(Drawing drawing)
        {
            foreach (var layer in drawing.GetLayers().OrderBy(l => l.Name))
            {
                Layers.Add(layer);
            }
        }
    }
}
