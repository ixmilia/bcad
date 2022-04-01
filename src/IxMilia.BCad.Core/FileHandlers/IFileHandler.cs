using System;
using System.IO;
using System.Threading.Tasks;

namespace IxMilia.BCad.FileHandlers
{
    public interface IFileHandler
    {
        Task<ReadDrawingResult> ReadDrawing(string fileName, Stream fileStream, Func<string, Task<byte[]>> contentResolver);

        Task<bool> WriteDrawing(string fileName, Stream fileStream, Drawing drawing, ViewPort viewPort, object fileSettings);

        object GetFileSettingsFromDrawing(Drawing drawing);
    }
}
