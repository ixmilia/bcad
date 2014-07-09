using System;
using System.Collections.Generic;
using System.IO;
using BCad.Dwg;

namespace BCad.FileHandlers
{
    [ExportFileHandler(DwgFileHandler.DisplayName, true, true, DwgFileHandler.FileExtension)]
    public class DwgFileHandler : IFileHandler
    {
        public const string DisplayName = "DWG Files (" + FileExtension + ")";
        public const string FileExtension = ".dwg";

        public bool ReadDrawing(string fileName, Stream fileStream, out Drawing drawing, out ViewPort viewPort, out Dictionary<string, object> propertyBag)
        {
            var file = DwgFile.Load(fileStream);
            throw new NotImplementedException();
        }

        public bool WriteDrawing(string fileName, Stream fileStream, Drawing drawing, ViewPort viewPort, Dictionary<string, object> propertyBag)
        {
            throw new NotImplementedException();
        }
    }
}
