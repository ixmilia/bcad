using System.Linq;
using BCad.Entities;
using BCad.Extensions;
using Xunit;
using System.Collections.Generic;

namespace BCad.Test
{
    public class TrimExtendTests : AbstractDrawingTests
    {

        #region Helpers

        private void DoLineTrim(IEnumerable<Line> existingLines,
            Line lineToTrim,
            Point selectionPoint,
            bool expectTrim,
            IEnumerable<Line> expectedAdded)
        {
            expectedAdded = expectedAdded ?? new Line[0];

            // prepare the drawing
            foreach (var line in existingLines)
            {
                Workspace.AddToCurrentLayer(line);
            }
            var boundary = Workspace.Drawing.GetEntities().SelectMany(e => e.GetPrimitives());
            Workspace.AddToCurrentLayer(lineToTrim);

            // trim
            IEnumerable<Entity> removed;
            IEnumerable<Entity> added;
            TrimExtendService.Trim(Workspace.Drawing,
                new SelectedEntity(lineToTrim, selectionPoint),
                boundary,
                out removed,
                out added);

            // verify deleted
            Assert.Equal(expectTrim, removed.Any());
            if (expectTrim)
            {
                Assert.Equal(1, removed.Count());
                Assert.True(removed.Single().EquivalentTo(lineToTrim));
            }

            // verify added
            Assert.Equal(expectedAdded.Count(), added.Count());
            Assert.True(expectedAdded.Zip(added, (a, b) => a.EquivalentTo(b)).All(b => b));
        }

        #endregion

        [Fact]
        public void SimpleLineTrimTest()
        {
            var line = new Line(new Point(0, 0, 0), new Point(2, 0, 0), Color.Auto);
            DoLineTrim(new[]
                {
                    new Line(new Point(1.0, -1.0, 0.0), new Point(1.0, 1.0, 0.0), Color.Auto)
                },
                line,
                Point.Origin,
                true,
                new[]
                {
                    new Line(new Point(1, 0, 0), new Point(2, 0, 0), Color.Auto)
                });
        }

        [Fact]
        public void TrimWholeLineBetweenTest()
        {
            DoLineTrim(
                new[]
                {
                    new Line(new Point(0.0, 0.0, 0.0), new Point(0.0, 1.0, 0.0), Color.Auto),
                    new Line(new Point(1.0, 0.0, 0.0), new Point(1.0, 1.0, 0.0), Color.Auto)
                },
                new Line(new Point(0.0, 0.0, 0.0), new Point(1.0, 0.0, 0.0), Color.Auto),
                new Point(0.5, 0, 0),
                false,
                null);
        }
    }
}
