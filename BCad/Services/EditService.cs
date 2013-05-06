using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using BCad.Entities;
using BCad.Extensions;
using BCad.Helpers;
using BCad.Primitives;
using System;

namespace BCad.Services
{
    [Export(typeof(IEditService))]
    internal class EditService : IEditService
    {
        public void Trim(SelectedEntity entityToTrim, IEnumerable<IPrimitive> boundaryPrimitives, out IEnumerable<Entity> removed, out IEnumerable<Entity> added)
        {
            var selectionPrimitives = entityToTrim.Entity.GetPrimitives();

            // find all intersection points
            var intersectionPoints = boundaryPrimitives
                .SelectMany(b => selectionPrimitives.Select(sel => b.IntersectionPoints(sel)))
                .WhereNotNull()
                .SelectMany(b => b)
                .WhereNotNull();

            if (intersectionPoints.Any())
            {
                // perform the trim operation
                switch (entityToTrim.Entity.Kind)
                {
                    case EntityKind.Line:
                        TrimLine((Line)entityToTrim.Entity, entityToTrim.SelectionPoint, intersectionPoints, out removed, out added);
                        break;
                    case EntityKind.Arc:
                    case EntityKind.Circle:
                    case EntityKind.Ellipse:
                        TrimEllipse(entityToTrim.Entity, entityToTrim.SelectionPoint, intersectionPoints, out removed, out added);
                        break;
                    default:
                        Debug.Fail("unsupported trim entity: " + entityToTrim.Entity.Kind);
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

        public void Extend(SelectedEntity entityToExtend, IEnumerable<IPrimitive> boundaryPrimitives, out IEnumerable<Entity> removed, out IEnumerable<Entity> added)
        {
            var selectionPrimitives = entityToExtend.Entity.GetPrimitives();

            // find all intersection points on boundary primitives but not on the entity to extend
            var intersectionPoints = boundaryPrimitives
                .SelectMany(b => selectionPrimitives.Select(sel => b.IntersectionPoints(sel, withinBounds: false)))
                .WhereNotNull()
                .SelectMany(b => b)
                .WhereNotNull()
                .Where(p => boundaryPrimitives.Any(b => b.ContainsPoint(p)) && !selectionPrimitives.Any(b => b.ContainsPoint(p)));

            if (intersectionPoints.Any())
            {
                switch (entityToExtend.Entity.Kind)
                {
                    case EntityKind.Line:
                        ExtendLine((Line)entityToExtend.Entity, entityToExtend.SelectionPoint, intersectionPoints, out removed, out added);
                        break;
                    default:
                        Debug.Fail("unsupported extend entity: " + entityToExtend.Entity.Kind);
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

        public bool CanOffsetEntity(Entity entityToOffset)
        {
            switch (entityToOffset.Kind)
            {
                case EntityKind.Arc:
                case EntityKind.Circle:
                case EntityKind.Ellipse:
                case EntityKind.Line:
                    return true;
                case EntityKind.Polyline:
                case EntityKind.Text:
                    return false;
                default:
                    throw new ArgumentException("entityToOffset.Kind");
            }
        }

        public IPrimitive Offset(Plane drawingPlane, IPrimitive primitive, Point offsetDirection, double offsetDistance)
        {
            if (!drawingPlane.Contains(offsetDirection))
                return null;

            IPrimitive result;
            switch (primitive.Kind)
            {
                case PrimitiveKind.Ellipse:
                    var el = (PrimitiveEllipse)primitive;
                    var projection = el.FromUnitCircleProjection();
                    projection.Invert();
                    var isInside = ((Vector)(offsetDirection).Transform(projection))
                        .LengthSquared <= 1.0;
                    var majorLength = el.MajorAxis.Length;
                    if (isInside && (offsetDistance > majorLength * el.MinorAxisRatio)
                        || (offsetDistance >= majorLength))
                    {
                        result = null;
                    }
                    else
                    {
                        Vector newMajor;
                        if (isInside)
                        {
                            newMajor = el.MajorAxis.Normalize() * (majorLength - offsetDistance);
                        }
                        else
                        {
                            newMajor = el.MajorAxis.Normalize() * (majorLength + offsetDistance);
                        }
                        result = new PrimitiveEllipse(
                            center: el.Center,
                            majorAxis: newMajor,
                            normal: el.Normal,
                            minorAxisRatio: el.MinorAxisRatio,
                            startAngle: el.StartAngle,
                            endAngle: el.EndAngle,
                            color: el.Color);
                    }
                    break;
                case PrimitiveKind.Line:
                    // find what side the offset occured on and move both end points
                    var line = (PrimitiveLine)primitive;
                    // normalize to XY plane
                    var picked = drawingPlane.ToXYPlane(offsetDirection);
                    var p1 = drawingPlane.ToXYPlane(line.P1);
                    var p2 = drawingPlane.ToXYPlane(line.P2);
                    var pline = new PrimitiveLine(p1, p2);
                    var perpendicular = new PrimitiveLine(picked, pline.PerpendicularSlope());
                    var intersection = pline.IntersectionPoint(perpendicular, false);
                    if (intersection != null && intersection != picked)
                    {
                        var offsetVector = (picked - intersection).Normalize() * offsetDistance;
                        offsetVector = drawingPlane.FromXYPlane(offsetVector);
                        result = new PrimitiveLine(
                            p1: line.P1 + offsetVector,
                            p2: line.P2 + offsetVector,
                            color: line.Color);
                    }
                    else
                    {
                        // the selected point was directly on the line
                        result = null;
                    }
                    break;
                case PrimitiveKind.Text:
                    result = null;
                    break;
                default:
                    throw new ArgumentException("primitive.Kind");
            }

            return result;
        }

        public Entity Offset(IWorkspace workspace, Entity entityToOffset, Point offsetDirection, double offsetDistance)
        {
            switch (entityToOffset.Kind)
            {
                case EntityKind.Arc:
                case EntityKind.Circle:
                case EntityKind.Ellipse:
                case EntityKind.Line:
                    var offset = Offset(
                        workspace.DrawingPlane,
                        entityToOffset.GetPrimitives().Single(),
                        offsetDirection,
                        offsetDistance);
                    return offset == null ? null : offset.ToEntity();
                case EntityKind.Polyline:
                case EntityKind.Text:
                    return null;
                default:
                    throw new ArgumentException("entityToOffset.Kind");
            }
        }

        public PrimitiveEllipse Ttr(Plane drawingPlane, SelectedEntity firstEntity, SelectedEntity secondEntity, double radius)
        {
            var first = firstEntity.Entity;
            var second = secondEntity.Entity;
            if (!CanOffsetEntity(first) || !CanOffsetEntity(second))
                return null;

            if (!drawingPlane.Contains(first) || !drawingPlane.Contains(second))
                return null;

            // offset each entity both possible directions, intersect everything, and take the closest
            // point to be the center
            var firstOffsets = OffsetBothDirections(
                drawingPlane: drawingPlane,
                primitive: first.GetPrimitives().First(),
                distance: radius);
            var secondOffsets = OffsetBothDirections(
                drawingPlane: drawingPlane,
                primitive: second.GetPrimitives().First(),
                distance: radius);

            var candidatePoints = (from f in firstOffsets
                                   from s in secondOffsets
                                   where f != null && s != null
                                   select f.IntersectionPoints(s, false))
                                  .SelectMany(x => x)
                                  .Where(x => x != null);

            var center = candidatePoints.OrderBy(x =>
                {
                    return (x - firstEntity.SelectionPoint).LengthSquared
                        * (x - secondEntity.SelectionPoint).LengthSquared;
                })
                .FirstOrDefault();

            if (center == null)
                return null;

            return new PrimitiveEllipse(center, radius, drawingPlane.Normal, Color.Auto);
        }

        public Entity Move(Entity entity, Vector offset)
        {
            switch (entity.Kind)
            {
                case EntityKind.Arc:
                    var arc = (Arc)entity;
                    return arc.Update(center: arc.Center + offset);
                case EntityKind.Circle:
                    var circle = (Circle)entity;
                    return circle.Update(center: circle.Center + offset);
                case EntityKind.Ellipse:
                    var el = (Ellipse)entity;
                    return el.Update(center: el.Center + offset);
                case EntityKind.Line:
                    var line = (Line)entity;
                    return line.Update(p1: line.P1 + offset, p2: line.P2 + offset);
                case EntityKind.Polyline:
                    var poly = (Polyline)entity;
                    return poly.Update(points: poly.Points.Select(p => p + offset));
                case EntityKind.Text:
                    var text = (Text)entity;
                    return text.Update(location: text.Location + offset);
                default:
                    throw new ArgumentException("entity.Kind");
            }
        }

        private IEnumerable<IPrimitive> OffsetBothDirections(Plane drawingPlane, IPrimitive primitive, double distance)
        {
            switch (primitive.Kind)
            {
                case PrimitiveKind.Ellipse:
                    var el = (PrimitiveEllipse)primitive;
                    return new[]
                        {
                            Offset(drawingPlane, el, el.Center, distance),
                            Offset(drawingPlane, el, el.Center + (el.MajorAxis * 2.0), distance)
                        };
                case PrimitiveKind.Line:
                    var line = (PrimitiveLine)primitive;
                    var lineVector = line.P2 - line.P1;
                    if (lineVector.IsZeroVector)
                    {
                        return Enumerable.Empty<IPrimitive>();
                    }

                    var offsetVector = lineVector.Cross(drawingPlane.Normal);
                    return new[]
                        {
                            Offset(drawingPlane, line, line.P1 + offsetVector, distance),
                            Offset(drawingPlane, line, line.P1 - offsetVector, distance)
                        };
                case PrimitiveKind.Text:
                    return Enumerable.Empty<IPrimitive>();
                default:
                    throw new ArgumentException("primitive.Kind");
            }
        }

        private static void TrimLine(Line lineToTrim, Point pivot, IEnumerable<Point> intersectionPoints, out IEnumerable<Entity> removed, out IEnumerable<Entity> added)
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

        private static void TrimEllipse(Entity entityToTrim, Point pivot, IEnumerable<Point> intersectionPoints, out IEnumerable<Entity> removed, out IEnumerable<Entity> added)
        {
            var addedList = new List<Entity>();
            var removedList = new List<Entity>();

            // prep common variables
            double startAngle, endAngle;
            bool isClosed;
            if (entityToTrim.Kind == EntityKind.Arc)
            {
                startAngle = ((Arc)entityToTrim).StartAngle;
                endAngle = ((Arc)entityToTrim).EndAngle;
                isClosed = false;
            }
            else if (entityToTrim.Kind == EntityKind.Ellipse)
            {
                startAngle = ((Ellipse)entityToTrim).StartAngle;
                endAngle = ((Ellipse)entityToTrim).EndAngle;
                isClosed = false;
            }
            else
            {
                startAngle = 0.0;
                endAngle = 360.0;
                isClosed = true;
            }

            // convert points to angles
            var inverse = entityToTrim.GetUnitCircleProjection();
            inverse.Invert();

            var angles = intersectionPoints
                .Select(p => p.Transform(inverse))
                .Select(p => (Math.Atan2(p.Y, p.X) * MathHelper.RadiansToDegrees).CorrectAngleDegrees())
                .Where(a => isClosed || (!MathHelper.CloseTo(a, startAngle) && !MathHelper.CloseTo(a, endAngle)))
                .OrderBy(a => a)
                .ToList();
            var unitPivot = pivot.Transform(inverse);
            var selectionAngle = (Math.Atan2(unitPivot.Y, unitPivot.X) * MathHelper.RadiansToDegrees).CorrectAngleDegrees();
            var selectionAfterIndex = angles.Where(a => a < selectionAngle).Count() - 1;
            if (selectionAfterIndex < 0)
                selectionAfterIndex = angles.Count - 1;
            var previousIndex = selectionAfterIndex;
            var nextIndex = previousIndex + 1;
            if (nextIndex >= angles.Count)
                nextIndex = 0;

            // prepare normalized angles (for arc trimming)
            List<double> normalizedAngles;
            double normalizedSelectionAngle;
            if (startAngle > endAngle)
            {
                normalizedAngles = angles.Select(a => a >= startAngle ? a - 360.0 : a).ToList();
                normalizedSelectionAngle = selectionAngle >= startAngle ? selectionAngle - 360.0 : selectionAngle;
            }
            else
            {
                normalizedAngles = angles;
                normalizedSelectionAngle = selectionAngle;
            }

            var lesserAngles = normalizedAngles.Where(a => a < normalizedSelectionAngle).ToList();
            var greaterAngles = normalizedAngles.Where(a => a > normalizedSelectionAngle).ToList();

            switch (entityToTrim.Kind)
            {
                case EntityKind.Arc:
                    var arc = (Arc)entityToTrim;
                    if (lesserAngles.Any() || greaterAngles.Any())
                    {
                        removedList.Add(entityToTrim);
                        if (lesserAngles.Any())
                        {
                            addedList.Add(arc.Update(endAngle: lesserAngles.Max().CorrectAngleDegrees()));
                        }
                        if (greaterAngles.Any())
                        {
                            addedList.Add(arc.Update(startAngle: greaterAngles.Min().CorrectAngleDegrees()));
                        }
                    }
                    break;
                case EntityKind.Circle:
                    var circle = (Circle)entityToTrim;
                    if (angles.Count >= 2)
                    {
                        // 2 cutting edges required
                        removedList.Add(entityToTrim);
                        addedList.Add(new Arc(
                            circle.Center,
                            circle.Radius,
                            angles[nextIndex],
                            angles[previousIndex],
                            circle.Normal,
                            circle.Color));
                    }
                    break;
                case EntityKind.Ellipse:
                    var el = (Ellipse)entityToTrim;
                    if (el.StartAngle == 0.0 && el.EndAngle == 360.0)
                    {
                        // treat like a circle
                        if (angles.Count >= 2)
                        {
                            removedList.Add(entityToTrim);
                            addedList.Add(el.Update(startAngle: angles[nextIndex], endAngle: angles[previousIndex]));
                        }
                    }
                    else
                    {
                        // tread like an arc
                        if (lesserAngles.Any() || greaterAngles.Any())
                        {
                            removedList.Add(entityToTrim);
                            if (lesserAngles.Any())
                            {
                                addedList.Add(el.Update(endAngle: lesserAngles.Max().CorrectAngleDegrees()));
                            }
                            if (greaterAngles.Any())
                            {
                                addedList.Add(el.Update(startAngle: greaterAngles.Min().CorrectAngleDegrees()));
                            }
                        }
                    }
                    break;
                default:
                    throw new ArgumentException("This should never happen", "entityToTrim.Kind");
            }

            added = addedList;
            removed = removedList;
        }

        private static void ExtendLine(Line lineToExtend, Point selectionPoint, IEnumerable<Point> intersectionPoints, out IEnumerable<Entity> removed, out IEnumerable<Entity> added)
        {
            var removedList = new List<Entity>();
            var addedList = new List<Entity>();

            // find closest intersection point to the selection
            var closestRealPoint = intersectionPoints.OrderBy(p => (p - selectionPoint).LengthSquared).FirstOrDefault();
            if (closestRealPoint != null)
            {
                // find closest end point to the selection
                var closestEndPoint = (lineToExtend.P1 - selectionPoint).LengthSquared < (lineToExtend.P2 - selectionPoint).LengthSquared
                    ? lineToExtend.P1
                    : lineToExtend.P2;

                // if closest intersection point and closest end point are on the same side of the midpoint, do extend
                var midPoint = lineToExtend.MidPoint();
                var selectionVector = (closestRealPoint - midPoint).Normalize();
                var endVector = (closestEndPoint - midPoint).Normalize();
                if (selectionVector.CloseTo(endVector))
                {
                    removedList.Add(lineToExtend);
                    if (closestEndPoint.CloseTo(lineToExtend.P1))
                    {
                        // replace p1
                        addedList.Add(lineToExtend.Update(p1: closestRealPoint));
                    }
                    else
                    {
                        // replace p2
                        addedList.Add(lineToExtend.Update(p2: closestRealPoint));
                    }
                }
            }

            removed = removedList;
            added = addedList;
        }
    }
}
