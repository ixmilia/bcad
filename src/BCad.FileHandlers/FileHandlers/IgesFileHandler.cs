// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.IO;
using BCad.Collections;
using BCad.Extensions;
using BCad.FileHandlers.Extensions;
using IxMilia.Iges;

namespace BCad.FileHandlers
{
    [ExportFileHandler(DisplayName, true, true, FileExtension1, FileExtension2)]
    public class IgesFileHandler: IFileHandler
    {
        public const string DisplayName = "IGES Files (" + FileExtension1 + ", " + FileExtension2 + ")";
        public const string FileExtension1 = ".igs";
        public const string FileExtension2 = ".iges";

        public INotifyPropertyChanged GetFileSettingsFromDrawing(Drawing drawing)
        {
            return null;
        }

        public bool ReadDrawing(string fileName, Stream fileStream, out Drawing drawing, out ViewPort viewPort)
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

            drawing = new Drawing(
                new DrawingSettings(fileName, UnitFormat.Architectural, 8),
                new ReadOnlyTree<string, Layer>().Insert(layer.Name, layer),
                layer.Name,
                file.Author);
            drawing.Tag = file;

            viewPort = null; // auto-set it later

            return true;
        }

        public bool WriteDrawing(string fileName, Stream fileStream, Drawing drawing, ViewPort viewPort, INotifyPropertyChanged fileSettings)
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
            return true;
        }
    }
}
