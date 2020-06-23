using System.ComponentModel;
using System.IO;

namespace IxMilia.BCad.FileHandlers
{
    public interface IFileHandler
    {
        bool ReadDrawing(string fileName, Stream fileStream, out Drawing drawing, out ViewPort viewPort);

        bool WriteDrawing(string fileName, Stream fileStream, Drawing drawing, ViewPort viewPort, object fileSettings);

        object GetFileSettingsFromDrawing(Drawing drawing);
    }
}
