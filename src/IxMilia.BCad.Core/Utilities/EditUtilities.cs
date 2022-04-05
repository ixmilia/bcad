using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IxMilia.BCad.Collections;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Helpers;
using IxMilia.BCad.Primitives;

namespace IxMilia.BCad.Utilities
{
    public static class EditUtilities
    {
        private static object rotateCacheLock = new object();
        private static Vector cachedRotateOffset;
        private static double cachedRotateAngle;
        private static Matrix4 cachedRotateMatrix;
        private static Matrix4 GetRotateMatrix(Vector offset, double angleInDegrees)
        {
            lock (rotateCacheLock)
            {
                // it will be common to re-use a specific rotation multiple times in a row
                if (offset != cachedRotateOffset || angleInDegrees != cachedRotateAngle)
                {
                    cachedRotateOffset = offset;
                    cachedRotateAngle = angleInDegrees;
                    cachedRotateMatrix = Matrix4.CreateTranslate(offset)
                        * Matrix4.RotateAboutZ(-angleInDegrees)
                        * Matrix4.CreateTranslate(-offset);
                }

                return cachedRotateMatrix;
            }
        }

        public static IPrimitive Rotate(IPrimitive primitive, Vector offset, double angleInDegrees)
        {
            var transform = GetRotateMatrix(offset, angleInDegrees);
            var inSitu = GetRotateMatrix(Vector.Zero, angleInDegrees);
            return primitive.MapPrimitive<IPrimitive>(
                ellipse => ellipse.Update(
                    center: transform.Transform(ellipse.Center),
                    majorAxis: inSitu.Transform(ellipse.MajorAxis)),
                line => line.Update(
                    p1: transform.Transform(line.P1),
                    p2: transform.Transform(line.P2)),
                point => point.Update(
                    location: transform.Transform(point.Location)),
                text => text.Update(
                    location: transform.Transform(text.Location),
                    rotation: text.Rotation + angleInDegrees),
                bezier => bezier.Update(
                    p1: transform.Transform(bezier.P1),
                    p2: transform.Transform(bezier.P2),
                    p3: transform.Transform(bezier.P3),
                    p4: transform.Transform(bezier.P4)),
                image => image.Update(
                    location: transform.Transform(image.Location),
                    rotation: image.Rotation + angleInDegrees)
            );
        }

        public static Entity Rotate(Entity entity, Vector offset, double angleInDegrees)
        {
            var transform = GetRotateMatrix(offset, angleInDegrees);
            var inSitu = GetRotateMatrix(Vector.Zero, angleInDegrees);
            return entity.MapEntity<Entity>(
                aggregate => aggregate.Update(
                    location: transform.Transform(aggregate.Location),
                    children: ReadOnlyList<Entity>.Create(aggregate.Children.Select(c => Rotate(entity, offset, angleInDegrees)))),
                arc => arc.Update(
                    center: transform.Transform(arc.Center),
                    startAngle: MathHelper.CorrectAngleDegrees(arc.StartAngle + angleInDegrees),
                    endAngle: MathHelper.CorrectAngleDegrees(arc.EndAngle + angleInDegrees)),
                circle => circle.Update(
                    center: transform.Transform(circle.Center)),
                ellipse => ellipse.Update(
                    center: transform.Transform(ellipse.Center),
                    majorAxis: inSitu.Transform(ellipse.MajorAxis)),
                image => image.Update(
                    location: transform.Transform(image.Location),
                    rotation: image.Rotation + angleInDegrees),
                line => line.Update(
                    p1: transform.Transform(line.P1),
                    p2: transform.Transform(line.P2)),
                location => location.Update(
                    point: transform.Transform(location.Point)),
                polyline => polyline.Update(
                    vertices: polyline.Vertices.Select(v => new Vertex(transform.Transform(v.Location), v.IncludedAngle, v.Direction))),
                spline => spline.Update(
                    controlPoints: spline.ControlPoints.Select(cp => transform.Transform(cp))),
                text => text.Update(
                    location: transform.Transform(text.Location),
                    rotation: text.Rotation + angleInDegrees)
            );
        }

        public static void Trim(SelectedEntity entityToTrim, IEnumerable<IPrimitive> boundaryPrimitives, out IEnumerable<Entity> removed, out IEnumerable<Entity> added)
        {
            var selectionPrimitives = entityToTrim.Entity.GetPrimitives();

            // find all intersection points
            var intersectionPoints = boundaryPrimitives
                .SelectMany(b => selectionPrimitives.Select(sel => b.IntersectionPoints(sel)))
                .WhereNotNull()
                .SelectMany(b => b)
                .WhereNotNull()
                .ToArray();

            if (intersectionPoints.Length > 0)
            {
                // perform the trim operation
                (removed, added) = entityToTrim.Entity.MapEntity<(IEnumerable<Entity>, IEnumerable<Entity>)>(
                    aggregate => (null, null),
                    arc => DoTrimEllipse(),
                    circle => DoTrimEllipse(),
                    ellipse => DoTrimEllipse(),
                    image => (null, null),
                    line =>
                    {
                        TrimLine(line, entityToTrim.SelectionPoint, intersectionPoints, out var removedX, out var addedX);
                        return (removedX, addedX);
                    },
                    location => (null, null),
                    polyline => (null, null),
                    spline =>
                    {
                        TrimSpline(spline, entityToTrim.SelectionPoint, intersectionPoints, out var removedX, out var addedX);
                        return (removedX, addedX);
                    },
                    text => (null, null)
                );
                removed ??= new List<Entity>();
                added ??= new List<Entity>();
            }
            else
            {
                removed = new List<Entity>();
                added = new List<Entity>();
            }

            (IEnumerable<Entity>, IEnumerable<Entity>) DoTrimEllipse()
            {
                TrimEllipse(entityToTrim.Entity, entityToTrim.SelectionPoint, intersectionPoints, out var removedX, out var addedX);
                return (removedX, addedX);
            }
        }

        public static void Extend(SelectedEntity entityToExtend, IEnumerable<IPrimitive> boundaryPrimitives, out IEnumerable<Entity> removed, out IEnumerable<Entity> added)
        {
            var selectionPrimitives = entityToExtend.Entity.GetPrimitives();

            // find all intersection points on boundary primitives but not on the entity to extend
            var intersectionPoints = boundaryPrimitives
                .SelectMany(b => selectionPrimitives.Select(sel => b.IntersectionPoints(sel, withinBounds: false)))
                .WhereNotNull()
                .SelectMany(b => b)
                .WhereNotNull()
                .Where(p => boundaryPrimitives.Any(b => b.IsPointOnPrimitive(p)) && !selectionPrimitives.Any(b => b.IsPointOnPrimitive(p)));

            if (intersectionPoints.Any())
            {
                (removed, added) = entityToExtend.Entity.MapEntity<(IEnumerable<Entity>, IEnumerable<Entity>)>(
                    aggregate => (null, null),
                    arc => DoExtendEllipse(),
                    circle => DoExtendEllipse(),
                    ellipse => DoExtendEllipse(),
                    image => (null, null),
                    line =>
                    {
                        ExtendLine(line, entityToExtend.SelectionPoint, intersectionPoints, out var removedX, out var addedX);
                        return (removedX, addedX);
                    },
                    location => (null, null),
                    polyline => (null, null),
                    spline => (null, null),
                    text => (null, null)
                );

                removed ??= new List<Entity>();
                added ??= new List<Entity>();
            }
            else
            {
                removed = new List<Entity>();
                added = new List<Entity>();
            }

            (IEnumerable<Entity>, IEnumerable<Entity>) DoExtendEllipse()
            {
                ExtendEllipse(entityToExtend.Entity, entityToExtend.SelectionPoint, intersectionPoints, out var removedX, out var addedX);
                return (removedX, addedX);
            }
        }

        public static bool CanOffsetEntity(Entity entityToOffset)
        {
            return entityToOffset.MapEntity<bool>(
                aggregate => false,
                arc => true,
                circle => true,
                ellipse => true,
                image => false,
                line => true,
                location => false,
                polyline => false,
                spline => false,
                text => false
            );
        }

        public static IPrimitive Offset(Plane drawingPlane, IPrimitive primitive, Point offsetDirection, double offsetDistance)
        {
            if (!drawingPlane.Contains(offsetDirection))
                return null;

            return primitive.MapPrimitive<IPrimitive>(
                ellipse =>
                {
                    var projection = ellipse.FromUnitCircle.Inverse();
                    var isInside = projection.Transform((Vector)offsetDirection).LengthSquared <= 1.0;
                    var majorLength = ellipse.MajorAxis.Length;
                    if (isInside && (offsetDistance > majorLength * ellipse.MinorAxisRatio)
                        || (offsetDistance >= majorLength))
                    {
                        return null;
                    }
                    else
                    {
                        Vector newMajor;
                        if (isInside)
                        {
                            newMajor = ellipse.MajorAxis.Normalize() * (majorLength - offsetDistance);
                        }
                        else
                        {
                            newMajor = ellipse.MajorAxis.Normalize() * (majorLength + offsetDistance);
                        }
                        return new PrimitiveEllipse(
                            center: ellipse.Center,
                            majorAxis: newMajor,
                            normal: ellipse.Normal,
                            minorAxisRatio: ellipse.MinorAxisRatio,
                            startAngle: ellipse.StartAngle,
                            endAngle: ellipse.EndAngle,
                            color: ellipse.Color);
                    }
                },
                line =>
                {
                    // find what side the offset occured on and move both end points
                    // normalize to XY plane
                    var picked = drawingPlane.ToXYPlane(offsetDirection);
                    var p1 = drawingPlane.ToXYPlane(line.P1);
                    var p2 = drawingPlane.ToXYPlane(line.P2);
                    var pline = new PrimitiveLine(p1, p2);
                    var perpendicular = new PrimitiveLine(picked, pline.PerpendicularSlope());
                    var intersection = pline.IntersectionPoint(perpendicular, false);
                    if (intersection.HasValue && intersection.Value != picked)
                    {
                        var offsetVector = (picked - intersection.Value).Normalize() * offsetDistance;
                        offsetVector = drawingPlane.FromXYPlane(offsetVector);
                        return new PrimitiveLine(
                            p1: line.P1 + offsetVector,
                            p2: line.P2 + offsetVector,
                            color: line.Color);
                    }
                    else
                    {
                        // the selected point was directly on the line
                        return null;
                    }
                },
                point =>
                {
                    var pointOffsetVector = (offsetDirection - point.Location).Normalize() * offsetDistance;
                    return new PrimitivePoint(point.Location + pointOffsetVector, point.Color);
                },
                text => null,
                bezier => null,
                image => null
            );
        }

        public static Entity Offset(IWorkspace workspace, Entity entityToOffset, Point offsetDirection, double offsetDistance)
        {
            return entityToOffset.MapEntity<Entity>(
                aggregate => null,
                arc => DoOffset(),
                circle => DoOffset(),
                ellipse => DoOffset(),
                image => null,
                line => DoOffset(),
                location => null,
                polyline => null,
                spline => null,
                text => null
            );

            Entity DoOffset()
            {
                var primitive = entityToOffset.GetPrimitives().Single();
                var thickness = primitive.GetThickness();
                var offset = Offset(
                    workspace.DrawingPlane,
                    primitive,
                    offsetDirection,
                    offsetDistance);
                var entity = offset?.ToEntity();
                if (entity != null)
                {
                    entity = entity.WithThickness(thickness);
                }

                return entity;
            }
        }

        public static PrimitiveEllipse Ttr(Plane drawingPlane, SelectedEntity firstEntity, SelectedEntity secondEntity, double radius)
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
                                  .SelectMany(x => x);

            if (candidatePoints.Any())
            {
                var center = candidatePoints.OrderBy(x =>
                {
                    return (x - firstEntity.SelectionPoint).LengthSquared
                        * (x - secondEntity.SelectionPoint).LengthSquared;
                }).First();

                return new PrimitiveEllipse(center, radius, drawingPlane.Normal);
            }

            return null;
        }

        public static Entity Move(Entity entity, Vector offset)
        {
            return entity.MapEntity<Entity>(
                aggregate => aggregate.Update(location: aggregate.Location + offset),
                arc => arc.Update(center: arc.Center + offset),
                circle => circle.Update(center: circle.Center + offset),
                ellipse => ellipse.Update(center: ellipse.Center + offset),
                image => image.Update(location: image.Location + offset),
                line => line.Update(p1: line.P1 + offset, p2: line.P2 + offset),
                location => location.Update(point: location.Point + offset),
                polyline => polyline.Update(vertices: polyline.Vertices.Select(v => new Vertex(v.Location + offset, v.IncludedAngle, v.Direction))),
                spline => spline.Update(controlPoints: spline.ControlPoints.Select(p => p + offset)),
                text => text.Update(location: text.Location + offset)
            );
        }

        public static Entity Scale(Entity entity, Point basePoint, double scaleFactor)
        {
            return entity.MapEntity<Entity>(
                aggregate => aggregate.Update(
                    location: aggregate.Location.ScaleFrom(basePoint, scaleFactor),
                    children: ReadOnlyList<Entity>.Create(aggregate.Children.Select(c => Scale(c, basePoint, scaleFactor)))),
                arc => arc.Update(
                    center: arc.Center.ScaleFrom(basePoint, scaleFactor),
                    radius: arc.Radius * scaleFactor),
                circle => circle.Update(
                    center: circle.Center.ScaleFrom(basePoint, scaleFactor),
                    radius: circle.Radius * scaleFactor),
                ellipse => ellipse.Update(
                    center: ellipse.Center.ScaleFrom(basePoint, scaleFactor),
                    majorAxis: ellipse.MajorAxis * scaleFactor),
                image => image.Update(
                    location: image.Location.ScaleFrom(basePoint, scaleFactor),
                    width: image.Width * scaleFactor,
                    height: image.Height * scaleFactor),
                line => line.Update(
                    p1: line.P1.ScaleFrom(basePoint, scaleFactor),
                    p2: line.P2.ScaleFrom(basePoint, scaleFactor)),
                location => location.Update(
                    point: location.Point.ScaleFrom(basePoint, scaleFactor)),
                polyline => polyline.Update(
                    vertices: polyline.Vertices.Select(v => new Vertex(v.Location.ScaleFrom(basePoint, scaleFactor), v.IncludedAngle, v.Direction))),
                spline => spline.Update(
                    controlPoints: spline.ControlPoints.Select(p => p.ScaleFrom(basePoint, scaleFactor))),
                text => text.Update(
                    location: text.Location.ScaleFrom(basePoint, scaleFactor),
                    height: text.Height * scaleFactor)
            );
        }

        public static Point ScaleFrom(this Point point, Point basePoint, double scaleFactor)
        {
            var direction = point - basePoint;
            var newDirection = direction * scaleFactor;
            return basePoint + newDirection;
        }

        public static Entity Quantize(Entity entity, QuantizeSettings settings)
        {
            return entity.MapEntity<Entity>(
                aggregate => aggregate.Update(
                    location: settings.Quantize(aggregate.Location),
                    children: ReadOnlyList<Entity>.Create(aggregate.Children.Select(c => Quantize(c, settings)))),
                arc => arc.Update(
                    center: settings.Quantize(arc.Center),
                    radius: settings.QuantizeDistance(arc.Radius),
                    startAngle: settings.QuantizeAngle(arc.StartAngle),
                    endAngle: settings.QuantizeAngle(arc.EndAngle),
                    normal: settings.Quantize(arc.Normal),
                    thickness: settings.QuantizeDistance(arc.Thickness)),
                circle => circle.Update(
                    center: settings.Quantize(circle.Center),
                    radius: settings.QuantizeDistance(circle.Radius),
                    normal: settings.Quantize(circle.Normal),
                    thickness: settings.QuantizeDistance(circle.Thickness)),
                ellipse => ellipse.Update(
                    center: settings.Quantize(ellipse.Center),
                    majorAxis: settings.Quantize(ellipse.MajorAxis),
                    minorAxisRatio: settings.QuantizeDistance(ellipse.MinorAxisRatio),
                    startAngle: settings.QuantizeAngle(ellipse.StartAngle),
                    endAngle: settings.QuantizeAngle(ellipse.EndAngle),
                    normal: settings.Quantize(ellipse.Normal),
                    thickness: settings.QuantizeDistance(ellipse.Thickness)),
                image => image.Update(
                    location: settings.Quantize(image.Location),
                    width: settings.QuantizeDistance(image.Width),
                    height: settings.QuantizeDistance(image.Height),
                    rotation: settings.QuantizeAngle(image.Rotation)),
                line => line.Update(
                    p1: settings.Quantize(line.P1),
                    p2: settings.Quantize(line.P2),
                    thickness: settings.QuantizeDistance(line.Thickness)),
                location => location.Update(
                    point: settings.Quantize(location.Point)),
                polyline => polyline.Update(
                    vertices: polyline.Vertices.Select(
                        v => new Vertex(settings.Quantize(v.Location), settings.QuantizeAngle(v.IncludedAngle), v.Direction))),
                spline => spline.Update(
                    controlPoints: spline.ControlPoints.Select(cp => settings.Quantize(cp))),
                text => text.Update(
                    location: settings.Quantize(text.Location),
                    normal: settings.Quantize(text.Normal),
                    height: settings.QuantizeDistance(text.Height),
                    rotation: settings.QuantizeAngle(text.Rotation))
            );
        }

        private static IEnumerable<IPrimitive> OffsetBothDirections(Plane drawingPlane, IPrimitive primitive, double distance)
        {
            return primitive.MapPrimitive<IEnumerable<IPrimitive>>(
                ellipse => new[] { Offset(drawingPlane, ellipse, ellipse.Center, distance), Offset(drawingPlane, ellipse, ellipse.Center + (ellipse.MajorAxis * 2.0), distance) },
                line =>
                {
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
                },
                point => null,
                text => null,
                bezier => null,
                image => null
            ) ?? Enumerable.Empty<IPrimitive>();
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
            var leftPoints = left.OrderBy(p => (p - pivot).LengthSquared);
            var rightPoints = right.OrderBy(p => (p - pivot).LengthSquared);

            if (leftPoints.Any() || rightPoints.Any())
            {
                // remove the original line
                removedList.Add(lineToTrim);

                // add the new shorted lines where appropriate
                if (leftPoints.Any())
                {
                    addedList.Add(lineToTrim.Update(p1: lineToTrim.P1, p2: leftPoints.First()));
                }
                if (rightPoints.Any())
                {
                    addedList.Add(lineToTrim.Update(p1: rightPoints.First(), p2: lineToTrim.P2));
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
                isClosed = MathHelper.CloseTo(0.0, startAngle) && MathHelper.CloseTo(360.0, endAngle);
            }
            else
            {
                startAngle = 0.0;
                endAngle = 360.0;
                isClosed = true;
            }

            // convert points to angles
            var inverse = entityToTrim.GetUnitCircleProjection().Inverse();

            var angles = intersectionPoints
                .Select(p => inverse.Transform(p))
                .Select(p => (Math.Atan2(p.Y, p.X) * MathHelper.RadiansToDegrees).CorrectAngleDegrees())
                .Where(a => isClosed || (!MathHelper.CloseTo(a, startAngle) && !MathHelper.CloseTo(a, endAngle)))
                .OrderBy(a => a)
                .ToList();
            var unitPivot = inverse.Transform(pivot);
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

            entityToTrim.DoEntity(
                aggregate => throw new ArgumentException(nameof(entityToTrim)),
                arc =>
                {
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
                },
                circle =>
                {
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
                },
                ellipse =>
                {
                    if (ellipse.StartAngle == 0.0 && ellipse.EndAngle == 360.0)
                    {
                        // treat like a circle
                        if (angles.Count >= 2)
                        {
                            removedList.Add(entityToTrim);
                            addedList.Add(ellipse.Update(startAngle: angles[nextIndex], endAngle: angles[previousIndex]));
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
                                addedList.Add(ellipse.Update(endAngle: lesserAngles.Max().CorrectAngleDegrees()));
                            }
                            if (greaterAngles.Any())
                            {
                                addedList.Add(ellipse.Update(startAngle: greaterAngles.Min().CorrectAngleDegrees()));
                            }
                        }
                    }
                },
                image => throw new ArgumentException(nameof(entityToTrim)),
                line => throw new ArgumentException(nameof(entityToTrim)),
                location => throw new ArgumentException(nameof(entityToTrim)),
                polyline => throw new ArgumentException(nameof(entityToTrim)),
                spline => throw new ArgumentException(nameof(entityToTrim)),
                text => throw new ArgumentException(nameof(entityToTrim))
            );

            added = addedList;
            removed = removedList;
        }

        private static void TrimSpline(Spline entityToTrim, Point pivot, IEnumerable<Point> intersectionPoints, out IEnumerable<Entity> removed, out IEnumerable<Entity> added)
        {
            var addedList = new List<Entity>();
            var currentBeziers = new List<PrimitiveBezier>();
            var beziers = entityToTrim.GetPrimitives().Cast<PrimitiveBezier>();
            foreach (var bezier in beziers)
            {
                // split bezier at intersection points and the curve containing the pivot gets deleted, others are kept
                foreach (var intersection in intersectionPoints)
                {
                    // TODO: split at all points at once
                    var t = bezier.GetParameterValueForPoint(intersection);
                    if (t.HasValue)
                    {
                        var parts = bezier.Split(t.GetValueOrDefault());
                        var isOnPart1 = parts.Item1.IsPointOnPrimitive(pivot, MathHelper.BezierEpsilon);
                        var isOnPart2 = parts.Item2.IsPointOnPrimitive(pivot, MathHelper.BezierEpsilon);
                        Debug.Assert(!(isOnPart1 && isOnPart2), "Pivot should not be on both curve parts.");
                        if (isOnPart1)
                        {
                            if (currentBeziers.Count > 0)
                            {
                                // add current collection of curves
                                var spline = Spline.FromBeziers(currentBeziers);
                                addedList.Add(spline);

                                // reset curve generation
                                currentBeziers.Clear();
                            }

                            // start a new curve collection
                            currentBeziers.Add(parts.Item2);
                        }
                        else if (isOnPart2)
                        {
                            // add part 1 and add curve collection
                            currentBeziers.Add(parts.Item1);
                            var spline = Spline.FromBeziers(currentBeziers);
                            addedList.Add(spline);

                            // reset curve generation
                            currentBeziers.Clear();
                        }
                        else
                        {
                            // keep entire curve
                            currentBeziers.Add(bezier);
                        }
                    }
                }
            }

            if (currentBeziers.Count > 0)
            {
                var spline = Spline.FromBeziers(currentBeziers);
                addedList.Add(spline);
            }

            added = addedList;
            removed = addedList.Count == 0
                ? Enumerable.Empty<Entity>()
                : new[] { entityToTrim };
        }

        private static void ExtendLine(Line lineToExtend, Point selectionPoint, IEnumerable<Point> intersectionPoints, out IEnumerable<Entity> removed, out IEnumerable<Entity> added)
        {
            var removedList = new List<Entity>();
            var addedList = new List<Entity>();

            // find closest intersection point to the selection
            var closestRealPoints = intersectionPoints.OrderBy(p => (p - selectionPoint).LengthSquared);
            if (closestRealPoints.Any())
            {
                var closestRealPoint = closestRealPoints.First();

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

        private static void ExtendEllipse(Entity ellipseToExtend, Point selectionPoint, IEnumerable<Point> intersectionPoints, out IEnumerable<Entity> removed, out IEnumerable<Entity> added)
        {
            var removedList = new List<Entity>();
            var addedList = new List<Entity>();

            // get primitive ellipse object
            var primitives = ellipseToExtend.GetPrimitives();
            Debug.Assert(primitives.Count() == 1);
            var primitive = primitives.Single();
            Debug.Assert(primitive.Kind == PrimitiveKind.Ellipse);
            var ellipse = (PrimitiveEllipse)primitive;

            // prepare transformation matrix
            var fromUnitMatrix = ellipse.FromUnitCircle;
            var toUnitMatrix = fromUnitMatrix.Inverse();
            var selectionUnit = toUnitMatrix.Transform(selectionPoint);

            // find closest intersection point to the selection
            var closestRealPoints = intersectionPoints.OrderBy(p => (p - selectionPoint).LengthSquared);
            if (closestRealPoints.Any())
            {
                var closestRealPoint = closestRealPoints.First();
                var closestUnitPoint = toUnitMatrix.Transform(closestRealPoint);
                var newAngle = (Math.Atan2(closestUnitPoint.Y, closestUnitPoint.X) * MathHelper.RadiansToDegrees).CorrectAngleDegrees();

                // find the closest end point to the selection
                var startPoint = toUnitMatrix.Transform(ellipse.StartPoint());
                var endPoint = toUnitMatrix.Transform(ellipse.EndPoint());
                var startAngle = ellipse.StartAngle;
                var endAngle = ellipse.EndAngle;
                if ((startPoint - selectionUnit).LengthSquared < (endPoint - selectionUnit).LengthSquared)
                {
                    // start point should get replaced
                    startAngle = newAngle;
                }
                else
                {
                    // end point should get replaced
                    endAngle = newAngle;
                }

                // remove the old ellipse
                removedList.Add(ellipseToExtend);

                // create the new ellipse
                Entity entityToAdd;
                if (MathHelper.CloseTo(1.0, ellipse.MinorAxisRatio))
                {
                    // circle or arc
                    if (MathHelper.CloseTo(0.0, startAngle) && MathHelper.CloseTo(360.0, endAngle))
                    {
                        // circle
                        entityToAdd = new Circle(ellipse.Center, ellipse.MajorAxis.Length, ellipse.Normal, ellipse.Color);
                    }
                    else
                    {
                        // arc
                        entityToAdd = new Arc(ellipse.Center, ellipse.MajorAxis.Length, startAngle, endAngle, ellipse.Normal, ellipse.Color);
                    }
                }
                else
                {
                    // ellipse
                    entityToAdd = new Ellipse(ellipse.Center, ellipse.MajorAxis, ellipse.MinorAxisRatio, startAngle, endAngle, ellipse.Normal, ellipse.Color);
                }

                addedList.Add(entityToAdd);
            }

            removed = removedList;
            added = addedList;
        }
    }
}
