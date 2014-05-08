using System.Collections.Generic;

namespace BCad.Core
{
    public interface IDrawingConverter
    {
        bool ConvertToDrawing(string fileName, IDrawingFile drawingFile, out Drawing drawing, out ViewPort viewPort, out Dictionary<string, object> propertyBag);

        bool ConvertFromDrawing(string fileName, Drawing drawing, ViewPort viewPort, Dictionary<string, object> propertyBag, out IDrawingFile drawingFile);
    }
}
