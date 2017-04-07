// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using BCad.Entities;
using Xunit;

namespace BCad.FileHandlers.Test
{
    public abstract class FileHandlerTestsBase
    {
        public static IFileHandler DxfFileHandler = new DxfFileHandler();
        public static IFileHandler IgesFileHandler = new IgesFileHandler();

        public static TFile WriteEntityToFile<TFile>(Entity entity, IFileHandler fileHandler, Func<Stream, TFile> fileReader)
        {
            var layer = new Layer("layer", null).Add(entity);
            var drawing = new Drawing().Add(layer);
            using (var ms = new MemoryStream())
            {
                Assert.True(fileHandler.WriteDrawing("filename", ms, drawing, ViewPort.CreateDefaultViewPort()));
                ms.Seek(0, SeekOrigin.Begin);
                var file = fileReader(ms);
                return file;
            }
        }

        public static Entity ReadEntityFromFile(IFileHandler fileHandler, Action<Stream> fileWriter)
        {
            using (var ms = new MemoryStream())
            {
                fileWriter(ms);
                ms.Seek(0, SeekOrigin.Begin);
                Assert.True(fileHandler.ReadDrawing("filename", ms, out var drawing, out var viewPort));
                return drawing.GetEntities().Single();
            }
        }
    }
}
