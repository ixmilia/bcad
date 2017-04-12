// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using BCad.Entities;
using BCad.FileHandlers.Extensions;
using Xunit;

namespace BCad.FileHandlers.Test
{
    public class IgesFileHandlerTests : FileHandlerTestsBase
    {
        protected override Entity RoundTripEntity(Entity entity)
        {
            return entity.ToIgesEntity().ToEntity();
        }

        [Fact]
        public void RoundTripColorTest()
        {
            VerifyRoundTrip(new Line(Point.Origin, Point.Origin, color: null));
            VerifyRoundTrip(new Line(Point.Origin, Point.Origin, CadColor.Red));
            VerifyRoundTrip(new Line(Point.Origin, Point.Origin, CadColor.FromArgb(255, 1, 2, 5)));
        }

        [Fact]
        public void RoundTripArcTest()
        {
            VerifyRoundTrip(new Arc(new Point(1.0, 2.0, 3.0), 4.0, 5.0, 6.0, Vector.ZAxis));
        }

        [Fact]
        public void RoundTripCircleTest()
        {
            VerifyRoundTrip(new Circle(new Point(1.0, 2.0, 3.0), 4.0, Vector.ZAxis));
        }

        [Fact]
        public void RoundTripLineTest()
        {
            VerifyRoundTrip(new Line(new Point(1.0, 2.0, 3.0), new Point(1.0, 2.0, 3.0)));
        }
    }
}
