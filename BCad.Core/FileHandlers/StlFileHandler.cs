using System.IO;
using BCad.Converters;
using BCad.DrawingFiles;
using BCad.Stl;

namespace BCad.FileHandlers
{
    [ExportFileHandler(StlFileHandler.DisplayName, true, false, StlFileHandler.FileExtension)]
    internal class StlFileHandler: IFileHandler
    {
        public const string DisplayName = "STL Files (" + FileExtension + ")";
        public const string FileExtension = ".stl";
        private static StlConverter converter = new StlConverter();

        public IDrawingConverter GetConverter()
        {
            return converter;
        }

        public IDrawingFile Load(Stream stream)
        {
            var dxfFile = StlFile.Load(stream);
            return new StlDrawingFile(dxfFile);
        }
    }
}
