using System;
using System.IO;
using System.Threading.Tasks;
using IxMilia.BCad.Extensions;
using IxMilia.Converters;
using IxMilia.Dwg;
using IxMilia.Dxf;

namespace IxMilia.BCad.FileHandlers
{
    public class DwgFileHandler : IFileHandler
    {
        public object GetFileSettingsFromDrawing(Drawing drawing)
        {
            var settings = new DwgFileSettings()
            {
                FileVersion = DwgFileVersion.R14,
            };
            var dwgDrawing = drawing.Tag as DwgDrawing;
            if (dwgDrawing != null)
            {
                settings.FileVersion = dwgDrawing.FileHeader.Version.ToFileVersion();
            }

            return settings;
        }

        public async Task<ReadDrawingResult> ReadDrawing(string fileName, Stream fileStream, Func<string, Task<byte[]>> contentResolver)
        {
            var dwgDrawing = DwgDrawing.Load(fileStream);
            var options = new DwgToDxfConverterOptions(DxfAcadVersion.R14);
            var converter = new DwgToDxfConverter();
            var dxf = await converter.Convert(dwgDrawing, options);
            return await DxfFileHandler.FromDxfFile(Path.ChangeExtension(fileName, ".dxf"), dxf, contentResolver);
        }

        public async Task<bool> WriteDrawing(string fileName, Stream fileStream, Drawing drawing, ViewPort viewPort, object fileSettings)
        {
            var dxf = DxfFileHandler.ToDxfFile(drawing, viewPort, new DxfFileSettings() { FileVersion = DxfFileVersion.R14 });
            var dwgConverterOptions = new DxfToDwgConverterOptions()
            {
                TargetVersion = DwgVersionId.R14,
            };
            if (fileSettings is DwgFileSettings dwgSettings)
            {
                dwgConverterOptions.TargetVersion = dwgSettings.FileVersion.ToDwgVersion();
            }

            var converter = new DxfToDwgConverter();
            var dwgDrawing = await converter.Convert(dxf, dwgConverterOptions);

            dwgDrawing.Save(fileStream);
            return true;
        }
    }
}
