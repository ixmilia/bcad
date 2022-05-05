using System;
using System.IO;
using System.Threading.Tasks;
using IxMilia.BCad.Collections;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.FileHandlers.Extensions;
using IxMilia.Iges;

namespace IxMilia.BCad.FileHandlers
{
    public class IgesFileHandler : IFileHandler
    {
        public object GetFileSettingsFromDrawing(Drawing drawing)
        {
            return null;
        }

        public Task<ReadDrawingResult> ReadDrawing(string fileName, Stream fileStream, Func<string, Task<byte[]>> contentResolver)
        {
            var file = IgesFile.Load(fileStream);
            var layer = new Layer("igs");
            foreach (var entity in file.Entities)
            {
                var cadEntity = entity.ToEntity();
                if (cadEntity != null)
                {
                    layer = layer.Add(cadEntity);
                }
            }

            var drawing = new Drawing(
                new DrawingSettings(fileName, UnitFormat.Architectural, 8, 0),
                new ReadOnlyTree<string, Layer>().Insert(layer.Name, layer),
                layer.Name,
                file.Author);
            drawing.Tag = file;
            return Task.FromResult(ReadDrawingResult.Succeeded(drawing, null));
        }

        public Task<bool> WriteDrawing(string fileName, Stream fileStream, Drawing drawing, ViewPort viewPort, object fileSettings)
        {
            var file = new IgesFile();
            var oldFile = drawing.Tag as IgesFile;
            if (oldFile != null)
            {
                // preserve settings from original file
                file.TimeStamp = oldFile.TimeStamp;
            }

            file.Author = drawing.Author;
            file.FullFileName = fileName;
            file.Identification = Path.GetFileName(fileName);
            file.Identifier = Path.GetFileName(fileName);
            file.ModelUnits = drawing.Settings.UnitFormat.ToIgesUnits();
            file.ModifiedTime = DateTime.Now;
            file.SystemIdentifier = "BCad";
            file.SystemVersion = "1.0";
            foreach (var entity in drawing.GetEntities())
            {
                var igesEntity = entity.ToIgesEntity();
                if (igesEntity != null)
                    file.Entities.Add(igesEntity);
            }

            file.Save(fileStream);
            return Task.FromResult(true);
        }
    }
}
