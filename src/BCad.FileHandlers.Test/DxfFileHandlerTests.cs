// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using BCad.Entities;
using BCad.FileHandlers.Extensions;
using Xunit;

namespace BCad.FileHandlers.Test
{
    public class DxfFileHandlerTests : FileHandlerTestsBase
    {
        protected override Entity RoundTripEntity(Entity entity)
        {
            return entity.ToDxfEntity(new Layer("layer")).ToEntity();
        }

        [Fact]
        public void RoundTripArcTest()
        {
            VerifyRoundTrip(new Arc(new Point(1.0, 2.0, 3.0), 4.0, 5.0, 6.0, Vector.YAxis, CadColor.Yellow, thickness: 1.2345));
        }

        [Fact]
        public void RoundTripCircleTest()
        {
            VerifyRoundTrip(new Circle(new Point(1.0, 2.0, 3.0), 4.0, Vector.XAxis, CadColor.Red, thickness: 1.2345));
        }

        [Fact]
        public void RoundTripLineTest()
        {
            VerifyRoundTrip(new Line(new Point(1.0, 2.0, 3.0), new Point(4.0, 5.0, 6.0), CadColor.Green, thickness: 1.2345));
        }

        [Fact]
        public void RoundTripPolylineTest()
        {
            // 90 degree arc from (0,0) to (1,1) representing the fourth quadrant
            VerifyRoundTrip(new Polyline(new[]
            {
                new Vertex(Point.Origin),
                new Vertex(new Point(1.0, 1.0, 0.0), 90.0, VertexDirection.CounterClockwise)
            }));

            // 90 degree arc from (0,0) to (1,1) representing the second quadrant
            VerifyRoundTrip(new Polyline(new[]
            {
                new Vertex(Point.Origin),
                new Vertex(new Point(1.0, 1.0, 0.0), 90.0, VertexDirection.Clockwise)
            }));
        }
    }
}
