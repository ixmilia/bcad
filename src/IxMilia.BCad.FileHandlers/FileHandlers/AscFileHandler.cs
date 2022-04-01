using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using IxMilia.BCad.Entities;

namespace IxMilia.BCad.FileHandlers
{
    public class AscFileHandler : IFileHandler
    {
        public object GetFileSettingsFromDrawing(Drawing drawing)
        {
            throw new NotImplementedException();
        }

        public Task<ReadDrawingResult> ReadDrawing(string fileName, Stream fileStream, Func<string, Task<byte[]>> contentResolver)
        {
            var points = new List<Location>();
            using (var reader = new StreamReader(fileStream))
            {
                for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    if (!line.StartsWith("#"))
                    {
                        var parts = line.Split(' ');
                        if (parts.Length == 3)
                        {
                            var x = double.Parse(parts[0]);
                            var y = double.Parse(parts[1]);
                            var z = double.Parse(parts[2]);
                            points.Add(new Location(new Point(x, y, z)));
                        }
                    }
                }
            }

            var layer = new Layer("ASC", points);
            var drawing = new Drawing().Add(layer);
            return Task.FromResult(ReadDrawingResult.Succeeded(drawing, null));
        }

        public Task<bool> WriteDrawing(string fileName, Stream fileStream, Drawing drawing, ViewPort viewPort, object fileSettings)
        {
            throw new NotImplementedException();
        }
    }
}
