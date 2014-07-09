using System;
using System.Collections.Generic;
using System.Linq;
using BCad.Entities;
using BCad.Extensions;
using BCad.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BCad.Test
{
    [TestClass]
    public class TrimExtendTests : AbstractDrawingTests
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
            Assert.AreEqual(expectTrim, removed.Any());
            if (expectTrim)
            {
                Assert.AreEqual(1, removed.Count());
                Assert.IsTrue(removed.Single().EquivalentTo(entityToTrim));
            }

            // verify added
            Assert.AreEqual(expectedAdded.Count(), added.Count());
            Assert.IsTrue(expectedAdded.Zip(added, (a, b) => a.EquivalentTo(b)).All(b => b));
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
            Assert.AreEqual(expectExtend, removed.Any());
            if (expectExtend)
            {
                Assert.AreEqual(1, removed.Count());
                Assert.IsTrue(removed.Single().EquivalentTo(entityToExtend));
            }

            // verify added
            Assert.AreEqual(expectedAdded.Count(), added.Count());
            Assert.IsTrue(expectedAdded.Zip(added, (a, b) => a.EquivalentTo(b)).All(b => b));
        }

        #endregion

        [TestMethod]
        public void SimpleLineTrimTest()
        {
            var line = new Line(new Point(0, 0, 0), new Point(2, 0, 0), IndexedColor.Auto);
            DoTrim(new[]
                {
                    new Line(new Point(1.0, -1.0, 0.0), new Point(1.0, 1.0, 0.0), IndexedColor.Auto)
                },
                line,
                Point.Origin,
                true,
                new[]
                {
                    new Line(new Point(1, 0, 0), new Point(2, 0, 0), IndexedColor.Auto)
                });
        }

        [TestMethod]
        public void TrimWholeLineBetweenTest()
        {
            DoTrim(
                new[]
                {
                    new Line(new Point(0.0, 0.0, 0.0), new Point(0.0, 1.0, 0.0), IndexedColor.Auto),
                    new Line(new Point(1.0, 0.0, 0.0), new Point(1.0, 1.0, 0.0), IndexedColor.Auto)
                },
                new Line(new Point(0.0, 0.0, 0.0), new Point(1.0, 0.0, 0.0), IndexedColor.Auto),
                new Point(0.5, 0, 0),
                false,
                null);
        }

        [TestMethod]
        public void TrimCircleAtZeroAngleTest()
        {
            DoTrim(
                new[]
                {
                    new Line(new Point(-1.0, 0.0, 0.0), new Point(1.0, 0.0, 0.0), IndexedColor.Auto),
                },
                new Circle(Point.Origin, 1.0, Vector.ZAxis, IndexedColor.Auto),
                new Point(0.0, -1.0, 0.0),
                true,
                new[]
                {
                    new Arc(Point.Origin, 1.0, 0.0, 180.0, Vector.ZAxis, IndexedColor.Auto)
                });
        }

        [TestMethod]
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
                    new Line(new Point(0.0, 0.0, 0.0), new Point(0.0, 1.0, 0.0), IndexedColor.Auto)
                },
                new Arc(Point.Origin, 1.0, 0.0, 180.0, Vector.ZAxis, IndexedColor.Auto),
                new Point(-sqrt2 / 2.0, sqrt2 / 2.0, 0.0),
                true,
                new[]
                {
                    new Arc(Point.Origin, 1.0, 0.0, 90.0, Vector.ZAxis, IndexedColor.Auto)
                });
        }

        [TestMethod]
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
                    new Line(new Point(-0.612372435695796, -1.0606617177983, 0.0), new Point(0.6123724356958, 1.06066017177983, 0.0), IndexedColor.Auto)
                },
                new Ellipse(Point.Origin, new Vector(0.6123724356958, 1.06066017177983, 0.0), 0.577350269189626, 0, 360, Vector.ZAxis, IndexedColor.Auto),
                new Point(0.612372435695806, -0.353553390593266, 0.0),
                true,
                new[]
                {
                    new Ellipse(Point.Origin, new Vector(0.6123724356958, 1.06066017177983, 0.0), 0.577350269189626, 0, 180.000062635721, Vector.ZAxis, IndexedColor.Auto)
                });
        }

        [TestMethod]
        public void SimpleExtendTest()
        {
            //          |  =>           |
            // ----o    |      ---------|
            //          |               |
            DoExtend(
                new[]
                {
                    new Line(new Point(2.0, -1.0, 0.0), new Point(2.0, 1.0, 0.0), IndexedColor.Auto)
                },
                new Line(new Point(0.0, 0.0, 0.0), new Point(1.0, 0.0, 0.0), IndexedColor.Auto),
                new Point(1.0, 0.0, 0.0),
                true,
                new[]
                {
                    new Line(new Point(0.0, 0.0, 0.0), new Point(2.0, 0.0, 0.0), IndexedColor.Auto)
                });
        }

        [TestMethod]
        public void NoExtendFromFurtherPointTest()
        {
            //          |  =>           |
            // -o---    |      ---------|
            //          |               |
            DoExtend(
                new[]
                {
                    new Line(new Point(2.0, -1.0, 0.0), new Point(2.0, 1.0, 0.0), IndexedColor.Auto)
                },
                new Line(new Point(0.0, 0.0, 0.0), new Point(1.0, 0.0, 0.0), IndexedColor.Auto),
                new Point(0.1, 0.0, 0.0),
                false,
                null);
        }

        [TestMethod]
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
                    new Line(new Point(0.0, 0.0, 0.0), new Point(1.0, 1.0, 0.0), IndexedColor.Auto)
                },
                new Arc(new Point(0.0, 0.0, 0.0), 1.0, 90.0, 360.0, Vector.ZAxis, IndexedColor.Auto),
                new Point(0.0, 1.0, 0.0),
                true,
                new[]
                {
                    new Arc(new Point(0.0, 0.0, 0.0), 1.0, 45.0, 360.0, Vector.ZAxis, IndexedColor.Auto)
                });
        }

        [TestMethod]
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
                    new Line(new Point(1.0, 1.0, 0.0), new Point(1.0, 0.0, 0.0), IndexedColor.Auto)
                },
                new Arc(new Point(1.0, 1.0, 0.0), 1.0, 90.0, 180.0, Vector.ZAxis, IndexedColor.Auto),
                new Point(0.0, 1.0, 0.0),
                true,
                new[]
                {
                    new Arc(new Point(1.0, 1.0, 0.0), 1.0, 90.0, 270.0, Vector.ZAxis, IndexedColor.Auto)
                });
        }
    }
}
