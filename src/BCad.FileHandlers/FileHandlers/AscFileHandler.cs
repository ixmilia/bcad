// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using BCad.Entities;

namespace BCad.FileHandlers
{
    [ExportFileHandler(DisplayName, true, false, FileExtension)]
    public class AscFileHandler : IFileHandler
    {
        public const string DisplayName = "Point Cloud Files (" + FileExtension + ")";
        public const string FileExtension = ".asc";

        public INotifyPropertyChanged GetFileSettingsFromDrawing(Drawing drawing)
        {
            throw new NotImplementedException();
        }

        public bool ReadDrawing(string fileName, Stream fileStream, out Drawing drawing, out ViewPort viewPort)
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
            drawing = new Drawing().Add(layer);
            viewPort = null;

            return true;
        }

        public bool WriteDrawing(string fileName, Stream fileStream, Drawing drawing, ViewPort viewPort, INotifyPropertyChanged fileSettings)
        {
            throw new NotImplementedException();
        }
    }
}
