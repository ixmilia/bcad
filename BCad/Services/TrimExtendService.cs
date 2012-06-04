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
        public void Trim(Drawing drawing, SelectedEntity entityToTrim, IEnumerable<IPrimitive> boundaryPrimitives, out IEnumerable<Entity> removed, out IEnumerable<Entity> added)
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
                        TrimLine(drawing, (Line)entityToTrim.Entity, entityToTrim.SelectionPoint, intersectionPoints, out removed, out added);
                        break;
                    default:
                        Debug.Fail("only lines are supported");
                        removed = new List<Entity>();
                        added = new List<Entity>();
                        break;
                }
            }
            else
            {
                removed = new List<Entity>();
                added = new List<Entity>();
            }
        }

        private static void TrimLine(Drawing drawing, Line lineToTrim, Point pivot, IEnumerable<Point> intersectionPoints, out IEnumerable<Entity> removed, out IEnumerable<Entity> added)
        {
            var removedList = new List<Entity>();
            var addedList = new List<Entity>();

            // split intersection points based on which side of the selection point they are
            var left = new List<Point>();
            var right = new List<Point>();
            var pivotDist = (pivot - lineToTrim.P1).LengthSquared;
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
            var leftPoint = left.OrderBy(p => (p - pivot).LengthSquared).FirstOrDefault();
            var rightPoint = right.OrderBy(p => (p - pivot).LengthSquared).FirstOrDefault();

            if (leftPoint != null || rightPoint != null)
            {
                // remove the original line
                removedList.Add(lineToTrim);

                // add the new shorted lines where appropriate
                if (leftPoint != null)
                {
                    addedList.Add(lineToTrim.Update(p1: lineToTrim.P1, p2: leftPoint));
                }
                if (rightPoint != null)
                {
                    addedList.Add(lineToTrim.Update(p1: rightPoint, p2: lineToTrim.P2));
                }
            }

            removed = removedList;
            added = addedList;
        }
    }
}
