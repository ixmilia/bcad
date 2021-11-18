using System;
using System.Linq;
using IxMilia.BCad.Collections;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Primitives;
using Xunit;

namespace IxMilia.BCad.Core.Test
{
    public class RectTests
    {
        [Fact]
        public void RectContainsPartial()
        {
            //         /    <- line
            //        /
            // +-----/--+   <- selection rectangle
            // +    /   +
            // +   /    +
            // +        +
            // +--------+
            var rect = new Rect(left: 0.0, top: 0.0, width: 1.0, height: 1.0);
            var line = new PrimitiveLine(new Point(0.5, 0.5, 0.0), new Point(2.0, 2.0, 0.0));
            var points = new[] { line.P1, line.P2 };
            var isContained = rect.Contains(points, includePartial: true);
            Assert.True(isContained);
        }

        [Fact]
        public void RectDoesNotContainPartial()
        {
            //                  /    <- line
            //                 /
            // +--------+     /
            // +        +    /
            // +        +   /
            // +        +
            // +--------+  <- selection rectangle
            var rect = new Rect(left: 0.0, top: 0.0, width: 1.0, height: 1.0);
            var line = new PrimitiveLine(new Point(1.5, 0.5, 0.0), new Point(3.0, 2.0, 0.0));
            var points = new[] { line.P1, line.P2 };
            var isContained = rect.Contains(points, includePartial: true);
            Assert.False(isContained);
        }
    }
}
