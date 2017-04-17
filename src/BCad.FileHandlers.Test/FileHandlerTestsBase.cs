// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using BCad.Core.Test;
using BCad.Entities;
using BCad.Extensions;
using Xunit;

namespace BCad.FileHandlers.Test
{
    public abstract class FileHandlerTestsBase : TestBase
    {
        public abstract IFileHandler FileHandler { get; }

        protected virtual Entity RoundTripEntity(Entity entity)
        {
            var drawing = new Drawing().Add(new Layer("layer").Add(entity));
            var result = RoundTripDrawing(drawing);
            return result.GetLayers().Single(l => l.Name == "layer").GetEntities().Single();
        }

        protected void VerifyRoundTrip(Entity entity)
        {
            var afterRoundTrip = RoundTripEntity(entity);
            Assert.True(entity.EquivalentTo(afterRoundTrip));
        }

        public void VerifyRoundTrip(Layer layer)
        {
            var afterRoundTrip = RoundTripLayer(layer);
            Assert.Equal(layer.Name, afterRoundTrip.Name);
            Assert.Equal(layer.Color, afterRoundTrip.Color);
            Assert.Equal(layer.IsVisible, afterRoundTrip.IsVisible);
            Assert.Equal(layer.EntityCount, afterRoundTrip.EntityCount);
        }

        public Drawing RoundTripDrawing(Drawing drawing)
        {
            using (var stream = WriteToStream(drawing))
            {
                return ReadFromStream(stream);
            }
        }

        public Stream WriteToStream(Drawing drawing)
        {
            var ms = new MemoryStream();
            var fileSettings = FileHandler.GetFileSettingsFromDrawing(drawing);
            Assert.True(FileHandler.WriteDrawing("filename", ms, drawing, ViewPort.CreateDefaultViewPort(), fileSettings));
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        public Drawing ReadFromStream(Stream stream)
        {
            Assert.True(FileHandler.ReadDrawing("filename", stream, out var result, out var viewPort));
            return result;
        }

        public Layer RoundTripLayer(Layer layer)
        {
            var drawing = new Drawing().Add(layer);
            var result = RoundTripDrawing(drawing);
            return result.GetLayers().Single(l => l.Name == layer.Name);
        }
    }
}
