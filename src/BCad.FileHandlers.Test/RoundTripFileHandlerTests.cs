// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using BCad.Entities;
using BCad.Extensions;
using Xunit;

namespace BCad.FileHandlers.Test
{
    public class RoundTripFileHandlerTests
    {
        public static Drawing RoundTripDrawing(IFileHandler fileHandler, Drawing drawing)
        {
            using (var ms = new MemoryStream())
            {
                Assert.True(fileHandler.WriteDrawing("filename", ms, drawing, ViewPort.CreateDefaultViewPort()));
                ms.Seek(0, SeekOrigin.Begin);
                fileHandler.ReadDrawing("filename", ms, out var result, out var viewPort);
                return result;
            }
        }

        public static TEntity RoundTripEntity<TEntity>(IFileHandler fileHandler, TEntity entity) where TEntity : Entity
        {
            var layer = new Layer("name", null).Add(entity);
            var drawing = new Drawing().Add(layer);
            drawing = RoundTripDrawing(fileHandler, drawing);
            return (TEntity)drawing.GetEntities().Single();
        }

        public static readonly IFileHandler[] FileHandlers = new IFileHandler[]
        {
            // TODO: add more handlers here
            new DxfFileHandler()
        };

        public static void RoundTripEntity<TEntity>(TEntity entity, Action<TEntity> validator) where TEntity : Entity
        {
            foreach (var fileHandler in FileHandlers)
            {
                var result = RoundTripEntity(fileHandler, entity);
                validator(result);
            }
        }

        public static void RoundTripEntity<TEntity>(TEntity entity) where TEntity : Entity
        {
            RoundTripEntity(entity, result => Assert.True(entity.EquivalentTo(result)));
        }

        [Fact]
        public void RoundTripEntitiesTest()
        {
            RoundTripEntity(new Arc(new Point(1.0, 2.0, 3.0), 4.0, 5.0, 6.0, Vector.YAxis, CadColor.Yellow, thickness: 1.2345));
            RoundTripEntity(new Circle(new Point(1.0, 2.0, 3.0), 4.0, Vector.XAxis, CadColor.Red, thickness: 1.2345));
            RoundTripEntity(new Line(new Point(1.0, 2.0, 3.0), new Point(4.0, 5.0, 6.0), CadColor.Green, thickness: 1.2345));
        }
    }
}
