using System;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using BCad.Extensions;

namespace BCad.Commands
{
    [ExportCommand("Zoom.Extents", "ZOOMEXTENTS", "ze")]
    internal class ZoomExtentsCommand : ICommand
    {
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

            // translate back out of XY plane
            var unproj = planeProjection;
            unproj.Invert();
            var bottomLeft = unproj.Transform(new Point(minx, miny, 0));
            var height = Math.Abs(maxy - miny);

            var newVp = Workspace.ActiveViewPort.Update(bottomLeft: bottomLeft, viewHeight: height);
            Workspace.Update(activeViewPort: newVp);

            return result;
        }
    }
}
