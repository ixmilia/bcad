// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using BCad.Entities;
using BCad.Extensions;
using BCad.Utilities;
using Xunit;

namespace BCad.Test
{
    public class RotateTests : AbstractDrawingTests
    {
        private void DoRotate(Entity entityToRotate, Vector origin, double angleInDegrees, Entity expectedResult)
        {
            var actual = EditUtilities.Rotate(entityToRotate, origin, angleInDegrees);
            Assert.True(expectedResult.EquivalentTo(actual));
        }

        [Fact]
        public void OriginRotateTest()
        {
            DoRotate(new Line(new Point(0, 0, 0), new Point(1, 0, 0), null),
                Point.Origin,
                90,
                new Line(new Point(0, 0, 0), new Point(0, 1, 0), null));
        }

        [Fact]
        public void NonOriginRotateTest()
        {
            DoRotate(new Line(new Point(2, 2, 0), new Point(3, 2, 0), null),
                new Point(1, 1, 0),
                90,
                new Line(new Point(0, 2, 0), new Point(0, 3, 0), null));
        }
    }
}
