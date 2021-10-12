using System;
using System.Collections.Generic;
using System.Linq;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Helpers;
using IxMilia.BCad.Primitives;
using IxMilia.BCad.Utilities;
using Xunit;

namespace IxMilia.BCad.Core.Test
{
    public class TrimExtendTests : TestBase
    {

        #region Helpers

        private void DoTrim(IEnumerable<Entity> existingEntities,
            Entity entityToTrim,
            Point selectionPoint,
            bool expectTrim,
            IEnumerable<Entity> expectedAdded)
        {
            expectedAdded = expectedAdded ?? new Entity[0];

            // prepare the drawing
            foreach (var ent in existingEntities)
            {
                Workspace.AddToCurrentLayer(ent);
            }
            var boundary = Workspace.Drawing.GetEntities().SelectMany(e => e.GetPrimitives());
            Workspace.AddToCurrentLayer(entityToTrim);

            // trim
            IEnumerable<Entity> removed;
            IEnumerable<Entity> added;
            EditUtilities.Trim(
                new SelectedEntity(entityToTrim, selectionPoint),
                boundary,
                out removed,
                out added);

            // verify deleted
            Assert.Equal(expectTrim, removed.Any());
            if (expectTrim)
            {
                Assert.Equal(1, removed.Count());
                Assert.True(removed.Single().EquivalentTo(entityToTrim));
            }

            // verify added
            Assert.Equal(expectedAdded.Count(), added.Count());
            var expectedText = string.Join("\n  ", expectedAdded.Select(e => e.ToString()));
            var actualText = string.Join("\n  ", added.Select(e => e.ToString()));
            Assert.True(expectedAdded.Zip(added, (a, b) => a.EquivalentTo(b)).All(b => b), $"Expected:\n  {expectedText}\nActual:\n  {actualText}");
        }

        private void DoExtend(IEnumerable<Entity> existingEntities,
            Entity entityToExtend,
            Point selectionPoint,
            bool expectExtend,
            IEnumerable<Entity> expectedAdded)
        {
            expectedAdded = expectedAdded ?? new Entity[0];

            // prepare the drawing
            foreach (var ent in existingEntities)
            {
                Workspace.AddToCurrentLayer(ent);
            }
            var boundary = Workspace.Drawing.GetEntities().SelectMany(e => e.GetPrimitives());
            Workspace.AddToCurrentLayer(entityToExtend);

            // extend
            IEnumerable<Entity> removed;
            IEnumerable<Entity> added;
            EditUtilities.Extend(
                new SelectedEntity(entityToExtend, selectionPoint),
                boundary,
                out removed,
                out added);

            // verify deleted
            Assert.Equal(expectExtend, removed.Any());
            if (expectExtend)
            {
                Assert.Equal(1, removed.Count());
                Assert.True(removed.Single().EquivalentTo(entityToExtend));
            }

            // verify added
            Assert.Equal(expectedAdded.Count(), added.Count());
            Assert.True(expectedAdded.Zip(added, (a, b) => a.EquivalentTo(b)).All(b => b));
        }

        #endregion

        [Fact]
        public void SimpleLineTrimTest()
        {
            var line = new Line(new Point(0, 0, 0), new Point(2, 0, 0));
            DoTrim(new[]
                {
                    new Line(new Point(1.0, -1.0, 0.0), new Point(1.0, 1.0, 0.0))
                },
                line,
                Point.Origin,
                true,
                new[]
                {
                    new Line(new Point(1, 0, 0), new Point(2, 0, 0))
                });
        }

        [Fact]
        public void TrimWholeLineBetweenTest()
        {
            DoTrim(
                new[]
                {
                    new Line(new Point(0.0, 0.0, 0.0), new Point(0.0, 1.0, 0.0)),
                    new Line(new Point(1.0, 0.0, 0.0), new Point(1.0, 1.0, 0.0))
                },
                new Line(new Point(0.0, 0.0, 0.0), new Point(1.0, 0.0, 0.0)),
                new Point(0.5, 0, 0),
                false,
                null);
        }

        [Fact]
        public void TrimCircleAtZeroAngleTest()
        {
            DoTrim(
                new[]
                {
                    new Line(new Point(-1.0, 0.0, 0.0), new Point(1.0, 0.0, 0.0)),
                },
                new Circle(Point.Origin, 1.0, Vector.ZAxis),
                new Point(0.0, -1.0, 0.0),
                true,
                new[]
                {
                    new Arc(Point.Origin, 1.0, 0.0, 180.0, Vector.ZAxis)
                });
        }

        [Fact]
        public void TrimHalfArcTest()
        {
            //      _________            ____
            //     /    |    \           |   \
            //   o/     |     \    =>    |    \
            //   |      |      |         |     |
            //   |      |      |         |     |
            var sqrt2 = Math.Sqrt(2.0);
            DoTrim(
                new[]
                {
                    new Line(new Point(0.0, 0.0, 0.0), new Point(0.0, 1.0, 0.0))
                },
                new Arc(Point.Origin, 1.0, 0.0, 180.0, Vector.ZAxis),
                new Point(-sqrt2 / 2.0, sqrt2 / 2.0, 0.0),
                true,
                new[]
                {
                    new Arc(Point.Origin, 1.0, 0.0, 90.0, Vector.ZAxis)
                });
        }

        [Fact]
        public void IsometricCircleTrimTest()
        {
            //      ____             ___
            //     /  / \           /  /
            //    /  /   |         /  /
            //   /  /    |   =>   /  /
            //  |  /    /        |  /
            //  | /    /o        | /
            //   \____/           \
            DoTrim(
                new[]
                {
                    new Line(new Point(-0.612372435695796, -1.0606617177983, 0.0), new Point(0.6123724356958, 1.06066017177983, 0.0))
                },
                new Ellipse(Point.Origin, new Vector(0.6123724356958, 1.06066017177983, 0.0), 0.577350269189626, 0, 360, Vector.ZAxis),
                new Point(0.612372435695806, -0.353553390593266, 0.0),
                true,
                new[]
                {
                    new Ellipse(Point.Origin, new Vector(0.6123724356958, 1.06066017177983, 0.0), 0.577350269189626, 0, 180.000062635721, Vector.ZAxis)
                });
        }

        [Fact]
        public void TrimLineOnSplineTest1()
        {
            // ___   /      ___
            //    \ /          \  o
            //     \    =>      \
            //    / |          / |
            //   /  |         /  |
            DoTrim(
                existingEntities: new[]
                {
                    Spline.FromBezier(new PrimitiveBezier(
                        new Point(1.0, 0.0, 0.0),
                        new Point(1.0, PrimitiveEllipse.BezierConstant, 0.0),
                        new Point(PrimitiveEllipse.BezierConstant, 1.0, 0.0),
                        new Point(0.0, 1.0, 0.0)))
                },
                entityToTrim: new Line(new Point(0.0, 0.0, 0.0), new Point(1.0, 1.0, 0.0)),
                selectionPoint: new Point(0.9, 0.9, 0.0),
                expectTrim: true,
                expectedAdded: new[]
                {
                    new Line(new Point(0.0, 0.0, 0.0), new Point(0.70696813418525, 0.70696813418525, 0.0))
                });
        }

        [Fact]
        public void TrimLineOnSplineTest2()
        {
            // taken from a real-world example
            var trimLine = new Line(new Point(55.0, 30.0, 0.0), new Point(115.0, 85.0, 0.0));
            DoTrim(
                existingEntities: new[]
                {
                    new Spline(
                        3,
                        new[]
                        {
                            new Point(59.1, 66.8, 0.0),
                            new Point(63.1, 81.7, 0.0),
                            new Point(127.2, 93.7, 0.0),
                            new Point(100.1, 12.9, 0.0),
                            new Point(55.4, 52.8, 0.0),
                            new Point(59.1, 66.8, 0.0)
                        },
                        new[] { 0.0, 0.0, 0.0, 0.0, 0.36, 0.65, 1.0, 1.0, 1.0, 1.0 })
                },
                entityToTrim: trimLine,
                selectionPoint: trimLine.MidPoint(),
                expectTrim: true,
                expectedAdded: new[]
                {
                    new Line(trimLine.P1, new Point(70.38849858212603, 44.10612370028221, 0.0)),
                    new Line(new Point(106.77307628734086, 77.45865326339579, 0.0), trimLine.P2)
                });
        }

        [Fact]
        public void TrimSplineOnLineTest1()
        {
            // ___   /      ___   /
            //    \ /          \ /
            //     \    =>      /
            //    / |          / o
            //   /  |         /
            DoTrim(
                existingEntities: new[]
                {
                    new Line(new Point(0.0, 0.0, 0.0), new Point(1.0, 1.0, 0.0))
                },
                entityToTrim: Spline.FromBezier(new PrimitiveBezier(
                    new Point(1.0, 0.0, 0.0),
                    new Point(1.0, PrimitiveEllipse.BezierConstant, 0.0),
                    new Point(PrimitiveEllipse.BezierConstant, 1.0, 0.0),
                    new Point(0.0, 1.0, 0.0))),
                selectionPoint: new Point(Math.Cos(30.0 * MathHelper.DegreesToRadians), Math.Sin(30.0 * MathHelper.DegreesToRadians), 0.0),
                expectTrim: true,
                expectedAdded: new[]
                {
                    Spline.FromBezier(new PrimitiveBezier(
                        new Point(0.7069681341852515, 0.7069681341852482, 0.0),
                        new Point(0.5259575122470016, 0.8879787561234993, 0.0),
                        new Point(0.2759575122470009, 1.0, 0.0),
                        new Point(0.0, 1.0, 0.0)))
                });
        }

        [Fact]
        public void TrimSplineOnLineTest2()
        {
            //      _______|_              _______|
            //     /       | \            /       | \
            //    /        |  \          /        |
            //   |         |   |o       |         |
            //   |         |   |   =>   |         |   |
            //    \        |  /          \        |  /
            //     \       | /            \       | /
            //      -------|-              -------|_
            var sqrt2over2 = Math.Sqrt(2.0) / 2.0;
            var unitCircle = new PrimitiveEllipse(Point.Origin, 1.0, Vector.ZAxis);
            var circleSpline = Spline.FromBeziers(unitCircle.AsBezierCurves());
            var trimLine = new Line(new Point(sqrt2over2, -1.0, 0.0), new Point(sqrt2over2, 1.0, 0.0));

            // 45-90 degrees
            var q1trimBezier = new PrimitiveBezier(
                new Point(0.70696813418525, 0.70696813418525, 0.0),
                new Point(0.525957512247, 0.8879787561235, 0.0),
                new Point(0.275957512247, 1.0, 0.0),
                new Point(0.0, 1.0, 0.0));

            // 270-315 degrees
            var q4trimBezier = new PrimitiveBezier(
                new Point(0.0, -1.0, 0.0),
                new Point(0.275957512247, -1.0, 0.0),
                new Point(0.525957512247, -0.8879787561235, 0.0),
                new Point(0.70696813418525, -0.70696813418525, 0.0));

            var selectionPoint = new Point(Math.Cos(1.0 * MathHelper.DegreesToRadians), Math.Sin(1.0 * MathHelper.DegreesToRadians), 0.0); // one degree up

            var resultSpline = Spline.FromBeziers(new[]
            {
                new PrimitiveBezier(new Point(1.0, 0.0, 0.0), new Point(1.0, -0.551915024494, 0.0), new Point(0.551915024494, -1.0, 0.0), new Point(0.0, -1.0, 0.0)),
                new PrimitiveBezier(new Point(0.0, 1.0, 0.0), new Point(0.27602797672647056, 1.0, 0.0), new Point(0.5260851832511362, 0.8879215406024443, 0.0), new Point(0.7071067813752437, 0.7068294600060859, 0.0)),
            });

            DoTrim(
                existingEntities: new[] { trimLine },
                entityToTrim: circleSpline,
                selectionPoint: selectionPoint,
                expectTrim: true,
                expectedAdded: new[] { resultSpline });
        }

        [Fact]
        public void SimpleExtendTest()
        {
            //          |  =>           |
            // ----o    |      ---------|
            //          |               |
            DoExtend(
                new[]
                {
                    new Line(new Point(2.0, -1.0, 0.0), new Point(2.0, 1.0, 0.0))
                },
                new Line(new Point(0.0, 0.0, 0.0), new Point(1.0, 0.0, 0.0)),
                new Point(1.0, 0.0, 0.0),
                true,
                new[]
                {
                    new Line(new Point(0.0, 0.0, 0.0), new Point(2.0, 0.0, 0.0))
                });
        }

        [Fact]
        public void NoExtendFromFurtherPointTest()
        {
            //          |  =>           |
            // -o---    |      ---------|
            //          |               |
            DoExtend(
                new[]
                {
                    new Line(new Point(2.0, -1.0, 0.0), new Point(2.0, 1.0, 0.0))
                },
                new Line(new Point(0.0, 0.0, 0.0), new Point(1.0, 0.0, 0.0)),
                new Point(0.1, 0.0, 0.0),
                false,
                null);
        }

        [Fact]
        public void SimpleArcExtendTest()
        {
            //    o   /        --\  /
            //  /   /         /   /
            // |  /      =>  |  /
            //  \   /         \   /
            //   ---           ---
            DoExtend(
                new[]
                {
                    new Line(new Point(0.0, 0.0, 0.0), new Point(1.0, 1.0, 0.0))
                },
                new Arc(new Point(0.0, 0.0, 0.0), 1.0, 90.0, 360.0, Vector.ZAxis),
                new Point(0.0, 1.0, 0.0),
                true,
                new[]
                {
                    new Arc(new Point(0.0, 0.0, 0.0), 1.0, 45.0, 360.0, Vector.ZAxis)
                });
        }

        [Fact]
        public void SimpleExtendArcNotAtOriginTest()
        {
            //   /           /
            //  /           /
            // o   |   =>  |   |
            //     |        \  |
            //     |         \_|
            DoExtend(
                new[]
                {
                    new Line(new Point(1.0, 1.0, 0.0), new Point(1.0, 0.0, 0.0))
                },
                new Arc(new Point(1.0, 1.0, 0.0), 1.0, 90.0, 180.0, Vector.ZAxis),
                new Point(0.0, 1.0, 0.0),
                true,
                new[]
                {
                    new Arc(new Point(1.0, 1.0, 0.0), 1.0, 90.0, 270.0, Vector.ZAxis)
                });
        }
    }
}
