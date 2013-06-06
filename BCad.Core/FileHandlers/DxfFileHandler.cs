using System.IO;
using BCad.Converters;
using BCad.DrawingFiles;
using BCad.Dxf;

namespace BCad.FileHandlers
{
    [ExportFileHandler(DxfFileHandler.DisplayName, true, true, DxfFileHandler.FileExtension)]
    internal class DxfFileHandler : IFileHandler
    {
        public const string DisplayName = "DXF Files (" + FileExtension + ")";
        public const string FileExtension = ".dxf";
        private static DxfConverter converter = new DxfConverter();

        public IDrawingConverter GetConverter()
        {
            return converter;
        }

        public IDrawingFile Load(Stream stream)
        {
            var dxfFile = DxfFile.Load(stream);
            return new DxfDrawingFile(dxfFile);
        }
    }
}
