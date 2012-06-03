using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using BCad.Entities;
using BCad.Extensions;
using BCad.Helpers;
using BCad.Primitives;

namespace BCad.Services
{
    [Export(typeof(ITrimExtendService))]
    internal class TrimExtendService : ITrimExtendService
    {
        public Drawing Trim(Drawing drawing, SelectedEntity entityToTrim, IEnumerable<IPrimitive> boundaryPrimitives)
        {
            var selectionPrimitives = entityToTrim.Entity.GetPrimitives();

            // find all intersection points
            var intersectionPoints = boundaryPrimitives
                .SelectMany(b => selectionPrimitives.Select(sel => b.IntersectionPoints(sel)))
                .Where(p => p != null)
                .SelectMany(b => b)
                .Where(p => p != null);

            if (intersectionPoints.Any())
            {
                // perform the trim operation
                switch (entityToTrim.Entity.Kind)
                {
                    case EntityKind.Line:
                        drawing = TrimLine(drawing, (Line)entityToTrim.Entity, entityToTrim.SelectionPoint, intersectionPoints);
                        break;
                    default:
                        Debug.Fail("only lines are supported");
                        break;
                }
            }

            return drawing;
        }

        private static Drawing TrimLine(Drawing drawing, Line lineToTrim, Point selectionPoint, IEnumerable<Point> intersectionPoints)
        {
            // split intersection points based on which side of the selection point they are
            var left = new List<Point>();
            var right = new List<Point>();
            var pivotDist = (selectionPoint - lineToTrim.P1).LengthSquared;
            var fullDist = (lineToTrim.P2 - lineToTrim.P1).LengthSquared;
            foreach (var point in intersectionPoints)
            {
                var isectDist = (point - lineToTrim.P1).LengthSquared;
                if (MathHelper.BetweenNarrow(0.0, pivotDist, isectDist))
                {
                    left.Add(point);
                }
                else if (MathHelper.BetweenNarrow(pivotDist, fullDist, isectDist))
                {
                    right.Add(point);
                }
            }

            // find the closest points on each side.  these are the new endpoints
            var leftPoint = left.OrderBy(p => (p - selectionPoint).LengthSquared).FirstOrDefault();
            var rightPoint = right.OrderBy(p => (p - selectionPoint).LengthSquared).FirstOrDefault();

            // remove the original line
            if (leftPoint != null || rightPoint != null)
            {
                var layer = drawing.ContainingLayer(lineToTrim).Name;
                drawing = drawing.Remove(lineToTrim);
                if (leftPoint != null)
                {
                    drawing = drawing.Add(drawing.Layers[layer], lineToTrim.Update(p1: lineToTrim.P1, p2: leftPoint));
                }
                if (rightPoint != null)
                {
                    drawing = drawing.Add(drawing.Layers[layer], lineToTrim.Update(p1: rightPoint, p2: lineToTrim.P2));
                }
            }

            return drawing;
        }
    }
}
