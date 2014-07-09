using System;
using System.Collections.Generic;
using System.IO;
using BCad.Entities;

namespace BCad.FileHandlers
{
    [ExportFileHandler(AscFileHandler.DisplayName, true, false, AscFileHandler.FileExtension)]
    public class AscFileHandler : IFileHandler
    {
        public const string DisplayName = "Point Cloud Files (" + FileExtension + ")";
        public const string FileExtension = ".asc";

        public bool ReadDrawing(string fileName, Stream fileStream, out Drawing drawing, out ViewPort viewPort, out Dictionary<string, object> propertyBag)
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
                            points.Add(new Location(new Point(x, y, z), IndexedColor.Auto));
                        }
                    }
                }
            }

            var layer = new Layer("ASC", IndexedColor.Auto, points);
            drawing = new Drawing().Add(layer);
            propertyBag = null;
            viewPort = null;

            return true;
        }

        public bool WriteDrawing(string fileName, Stream fileStream, Drawing drawing, ViewPort viewPort, Dictionary<string, object> propertyBag)
        {
            throw new NotImplementedException();
        }
    }
}
