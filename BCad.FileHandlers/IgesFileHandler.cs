using System.IO;
using BCad.Core;
using BCad.FileHandlers.Converters;
using BCad.FileHandlers.DrawingFiles;
using BCad.Iges;

namespace BCad.FileHandlers
{
    [ExportFileHandler(IgesFileHandler.DisplayName, true, true, IgesFileHandler.FileExtension1, IgesFileHandler.FileExtension2)]
    internal class IgesFileHandler: IFileHandler
    {
        public const string DisplayName = "IGES Files (" + FileExtension1 + ", " + FileExtension2 + ")";
        public const string FileExtension1 = ".igs";
        public const string FileExtension2 = ".iges";
        private static IgesConverter converter = new IgesConverter();

        public IDrawingConverter GetConverter()
        {
            return converter;
        }

        public IDrawingFile Load(Stream stream)
        {
            var dxfFile = IgesFile.Load(stream);
            return new IgesDrawingFile(dxfFile);
        }
    }
}
