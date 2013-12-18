using System;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using BCad.Extensions;
using BCad.Helpers;

namespace BCad.Commands
{
    [ExportCommand("Zoom.Extents", "ZOOMEXTENTS", "ze")]
    internal class ZoomExtentsCommand : ICommand
    {
        private const int ZoomPixelBuffer = 20;

        [Import]
        public IWorkspace Workspace { get; set; }

        public Task<bool> Execute(object arg = null)
        {
            var result = Task.FromResult(true);
            var planeProjection = Workspace.DrawingPlane.ToXYPlaneProjection();
            var allPoints = Workspace.Drawing.GetEntities()
                .SelectMany(e => e.GetPrimitives())
                .SelectMany(p => p.GetInterestingPoints())
                .Select(p => planeProjection.Transform(p));

            if (allPoints.Count() < 2)
                return result;

            var first = allPoints.First();
            var minx = first.X;
            var miny = first.Y;
            var maxx = first.X;
            var maxy = first.Y;
            foreach (var point in allPoints.Skip(1))
            {
                if (point.X < minx)
                    minx = point.X;
                if (point.X > maxx)
                    maxx = point.X;
                if (point.Y < miny)
                    miny = point.Y;
                if (point.Y > maxy)
                    maxy = point.Y;
            }

            var deltaX = maxx - minx;
            var deltaY = maxy - miny;

            // translate back out of XY plane
            var unproj = planeProjection;
            unproj.Invert();
            var bottomLeft = unproj.Transform(new Point(minx, miny, 0));
            var topRight = unproj.Transform(new Point(minx + deltaX, miny + deltaY, 0));
            var drawingHeight = deltaY;
            var drawingWidth = deltaX;

            double viewHeight, drawingToViewScale;
            var viewRatio = (double)Workspace.ViewControl.DisplayWidth / Workspace.ViewControl.DisplayHeight;
            var drawingRatio = drawingWidth / drawingHeight;
            if (MathHelper.CloseTo(0.0, drawingHeight) || drawingRatio > viewRatio)
            {
                // fit to width
                var viewWidth = drawingWidth;
                viewHeight = Workspace.ViewControl.DisplayHeight * viewWidth / Workspace.ViewControl.DisplayWidth;
                drawingToViewScale = Workspace.ViewControl.DisplayWidth / viewWidth;

                // add a buffer of some amount of pixels
                var pixelWidth = drawingWidth * drawingToViewScale;
                var newPixelWidth = pixelWidth + (ZoomPixelBuffer * 2);
                viewHeight *= newPixelWidth / pixelWidth;
            }
            else
            {
                // fit to height
                viewHeight = drawingHeight;
                drawingToViewScale = Workspace.ViewControl.DisplayHeight / viewHeight;

                // add a buffer of some amount of pixels
                var pixelHeight = drawingHeight * drawingToViewScale;
                var newPixelHeight = pixelHeight + (ZoomPixelBuffer * 2);
                viewHeight *= newPixelHeight / pixelHeight;
            }

            // center viewport
            var tempViewport = Workspace.ActiveViewPort.Update(bottomLeft: bottomLeft, viewHeight: viewHeight);
            var pixelMatrix = tempViewport.GetTransformationMatrixWindowsStyle(Workspace.ViewControl.DisplayWidth, Workspace.ViewControl.DisplayHeight);
            var bottomLeftScreen = pixelMatrix.Transform(bottomLeft);
            var topRightScreen = pixelMatrix.Transform(topRight);

            // center horizontally
            var leftXGap = bottomLeftScreen.X;
            var rightXGap = Workspace.ViewControl.DisplayWidth - topRightScreen.X;
            var xAdjust = Math.Abs((rightXGap - leftXGap) / 2.0) / drawingToViewScale;

            // center vertically
            var topYGap = topRightScreen.Y;
            var bottomYGap = Workspace.ViewControl.DisplayHeight - bottomLeftScreen.Y;
            var yAdjust = Math.Abs((topYGap - bottomYGap) / 2.0) / drawingToViewScale;

            var newBottomLeft = new Point(bottomLeft.X - xAdjust, bottomLeft.Y - yAdjust, bottomLeft.Z);

            var newVp = Workspace.ActiveViewPort.Update(bottomLeft: newBottomLeft, viewHeight: viewHeight);
            Workspace.Update(activeViewPort: newVp);

            return result;
        }
    }
}
