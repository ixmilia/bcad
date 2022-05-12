using System.Collections.Generic;
using System.Linq;

namespace IxMilia.BCad.Dialogs
{
    public partial class LineTypeDialogParameters
    {
        public List<LineType> LineTypes { get; } = new List<LineType>();

        public LineTypeDialogParameters(Drawing drawing)
        {
            foreach (var lineType in drawing.GetLineTypes().OrderBy(l => l.Name))
            {
                LineTypes.Add(lineType);
            }
        }
    }
}
