using System.Linq;

namespace IxMilia.BCad.Dialogs
{
    public class DimensionStylesDialogParameters
    {
        public string CurrentDimensionStyleName { get; }
        public DimensionStylesDialogEntry[] DimensionStyles { get; }

        public DimensionStylesDialogParameters(string currentDimensionStyleName, DimensionStylesDialogEntry[] dimensionStyles)
        {
            CurrentDimensionStyleName = currentDimensionStyleName;
            DimensionStyles = dimensionStyles;
        }

        internal static DimensionStylesDialogParameters FromDrawing(Drawing drawing)
        {
            var entries = drawing.Settings.DimensionStyles.Select(DimensionStylesDialogEntry.FromDimensionStyle).ToArray();
            return new DimensionStylesDialogParameters(drawing.Settings.CurrentDimensionStyleName, entries);
        }
    }
}
