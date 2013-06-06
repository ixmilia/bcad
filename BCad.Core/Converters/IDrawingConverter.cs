using BCad.DrawingFiles;

namespace BCad.Converters
{
    public interface IDrawingConverter
    {
        bool ConvertToDrawing(string fileName, IDrawingFile drawingFile, out Drawing drawing, out ViewPort viewPort);

        bool ConvertFromDrawing(string fileName, Drawing drawing, ViewPort viewPort, out IDrawingFile drawingFile);
    }
}
