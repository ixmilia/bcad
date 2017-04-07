// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using BCad.Entities;
using BCad.Helpers;
using IxMilia.Iges;
using IxMilia.Iges.Entities;
using Xunit;

namespace BCad.FileHandlers.Test
{
    public class IgesFileHandlerTests : FileHandlerTestsBase
    {
        private static IgesEntity ToIgesEntity(Entity entity)
        {
            var igesFile = WriteEntityToFile(entity, IgesFileHandler, IgesFile.Load);
            return igesFile.Entities.Last();
        }

        private static Entity ToEntity(IgesEntity entity)
        {
            var file = new IgesFile();
            file.Entities.Add(entity);
            return ReadEntityFromFile(IgesFileHandler, file.Save);
        }

        private static void AssertColor(CadColor cadColor, IgesColorDefinition igesColor)
        {
            Assert.True(MathHelper.CloseTo(cadColor.R / 255.0, igesColor.RedIntensity));
            Assert.True(MathHelper.CloseTo(cadColor.G / 255.0, igesColor.GreenIntensity));
            Assert.True(MathHelper.CloseTo(cadColor.B / 255.0, igesColor.BlueIntensity));
        }

        [Fact]
        public void WriteEntityColorTest()
        {
            Assert.Equal(IgesColorNumber.Default, ToIgesEntity(new Line(Point.Origin, Point.Origin, null)).Color);
            Assert.Equal(IgesColorNumber.Red, ToIgesEntity(new Line(Point.Origin, Point.Origin, CadColor.Red)).Color);

            var customColor = CadColor.FromArgb(255, 1, 2, 5);
            var line = ToIgesEntity(new Line(Point.Origin, Point.Origin, customColor));
            Assert.Equal(IgesColorNumber.Custom, line.Color);
            AssertColor(customColor, line.CustomColor);
        }

        [Fact]
        public void ReadEntityColorTest()
        {
            Assert.Null(ToEntity(new IgesLine() { Color = IgesColorNumber.Default }).Color);
            Assert.Equal(CadColor.Red, ToEntity(new IgesLine() { Color = IgesColorNumber.Red }).Color);
            Assert.Equal(CadColor.FromArgb(255, 1, 2, 5), ToEntity(new IgesLine() { CustomColor = new IgesColorDefinition(1.0 / 255.0, 2.0 / 255.0, 5.0 / 255.0) }).Color);
        }
    }
}
