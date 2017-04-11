// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Linq;
using BCad.Entities;
using BCad.Helpers;
using IxMilia.Dxf;
using IxMilia.Dxf.Entities;
using Xunit;

namespace BCad.FileHandlers.Test
{
    public class DxfFileHandlerTests : FileHandlerTestsBase
    {
        private static DxfEntity ToDxfEntity(Entity entity)
        {
            var dxfFile = WriteEntityToFile(entity, DxfFileHandler, DxfFile.Load);
            return dxfFile.Entities.Last();
        }

        private static Entity ToEntity(DxfEntity entity)
        {
            var file = new DxfFile();
            file.Entities.Add(entity);
            return ReadEntityFromFile(DxfFileHandler, stream => file.Save(stream, asText: true));
        }

        private static DxfEntity RoundTripEntity(DxfEntity entity)
        {
            return ToDxfEntity(ToEntity(entity));
        }

        private void AssertVertex(DxfVertex expected, DxfVertex actual)
        {
            Assert.Equal(expected.Location, actual.Location);
            Assert.True(MathHelper.CloseTo(expected.Bulge, actual.Bulge), $"Expected: {expected.Bulge}{Environment.NewLine}Actual: {actual.Bulge}");
        }

        private void AssertPolyline(DxfPolyline expected, DxfPolyline actual)
        {
            Assert.Equal(expected.Vertices.Count, actual.Vertices.Count);
            for (int i = 0; i < expected.Vertices.Count; i++)
            {
                AssertVertex(expected.Vertices[i], actual.Vertices[i]);
            }
        }

        [Fact]
        public void RoundTripPolylineTest()
        {
            // 90 degree arc from (0,0) to (1,1) representing the fourth quadrant
            var bulge = Math.Sqrt(2.0) - 1.0;
            var polyline1 = new DxfPolyline(new[]
            {
                new DxfVertex(new DxfPoint(0.0, 0.0, 0.0)),
                new DxfVertex(new DxfPoint(1.0, 1.0, 0.0)) { Bulge = bulge }
            });
            var polyline2 = (DxfPolyline)RoundTripEntity(polyline1);
            AssertPolyline(polyline1, polyline2);

            // 90 degree arc from (0,0) to (1,1) representing the second quadrant
            polyline1.Vertices.Last().Bulge = -bulge;
            polyline2 = (DxfPolyline)RoundTripEntity(polyline1);
            AssertPolyline(polyline1, polyline2);
        }
    }
}
