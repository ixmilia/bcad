using System.Collections.Generic;
using System.IO;

namespace BCad.FileHandlers
{
    public interface IFileHandler
    {
        bool ReadDrawing(string fileName, Stream fileStream, out Drawing drawing, out ViewPort viewPort, out Dictionary<string, object> propertyBag);

        bool WriteDrawing(string fileName, Stream fileStream, Drawing drawing, ViewPort viewPort, Dictionary<string, object> propertyBag);
    }
}
