using System;
using IxMilia.BCad.Display;
using IxMilia.BCad.Entities;
using Xunit;

namespace IxMilia.BCad.Core.Test
{
    public class DisplayInteractionManagerTests : TestBase
    {
        private DisplayInteractionManager CreateDisplayWithSize(double width, double height)
        {
            Workspace.Update(activeViewPort: new ViewPort(Point.Origin, Vector.ZAxis, Vector.YAxis, 480));
            var dim = new DisplayInteractionManager(Workspace, ProjectionStyle.OriginBottomLeft);
            dim.Resize(640.0, 480.0);
            return dim;
        }

        [Fact]
        public void GetHitEntity_ImageSelectsOnEdge()
        {
            var dim = CreateDisplayWithSize(640.0, 480.0);
            var image = new Image(new Point(0.0, 0.0, 0.0), "path", Array.Empty<byte>(), 640.0, 480.0, 0.0);
            Workspace.AddToCurrentLayer(image);
            var middleOfBottomEdge = image.Location + new Vector(image.Width / 2.0, 0.0, 0.0);
            var selected = dim.GetHitEntity(middleOfBottomEdge);
            Assert.True(ReferenceEquals(image, selected.Entity));
        }

        [Fact]
        public void GetHitEntity_ImageDoesNotSelectsInMiddle()
        {
            var dim = CreateDisplayWithSize(640.0, 480.0);
            var image = new Image(new Point(0.0, 0.0, 0.0), "path", Array.Empty<byte>(), 640.0, 480.0, 0.0);
            Workspace.AddToCurrentLayer(image);
            var middleOfImage = image.Location + new Vector(image.Width / 2.0, image.Height / 2.0, 0.0);
            var selected = dim.GetHitEntity(middleOfImage);
            Assert.Null(selected);
        }
    }
}
