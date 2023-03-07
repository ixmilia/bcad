using System;
using System.Linq;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Helpers;
using IxMilia.BCad.Primitives;

namespace IxMilia.BCad.Utilities
{
    public class FilletOptions
    {
        public Plane DrawingPlane { get; }
        public PrimitiveLine Line1 { get; }
        public Point Line1SelectionPoint { get; }
        public PrimitiveLine Line2 { get; }
        public Point Line2SelectionPoint { get; }
        public double Radius { get; }

        public FilletOptions(Plane drawingPlane, PrimitiveLine line1, Point line1SelectionPoint, PrimitiveLine line2, Point line2SelectionPoint, double radius)
        {
            DrawingPlane = drawingPlane ?? throw new ArgumentNullException(nameof(drawingPlane));
            Line1 = line1 ?? throw new ArgumentNullException(nameof(line1));
            Line1SelectionPoint = line1SelectionPoint;
            Line2 = line2 ?? throw new ArgumentNullException(nameof(line2));
            Line2SelectionPoint = line2SelectionPoint;
            Radius = radius;
            if (radius < 0.0)
            {
                throw new ArgumentException(nameof(radius), "Radius cannot be negative");
            }
        }
    }
    
    public class FilletResult
    {
        public PrimitiveLine UpdatedLine1 { get; }
        public PrimitiveLine UpdatedLine2 { get; }
        public PrimitiveEllipse Fillet { get; }

        public FilletResult(PrimitiveLine updatedLine1, PrimitiveLine updatedLine2, PrimitiveEllipse fillet)
        {
            UpdatedLine1 = updatedLine1 ?? throw new ArgumentNullException(nameof(updatedLine1));
            UpdatedLine2 = updatedLine2 ?? throw new ArgumentNullException(nameof(updatedLine2));
            Fillet = fillet;
        }
    }

    public static class FilletUtility
    {
        public static bool TryFillet(FilletOptions options, out FilletResult result)
        {
            result = default;

            // first make sure the lines would even intersect
            var intersectionCandidate = options.Line1.IntersectionPoint(options.Line2, withinSegment: false);
            if (!intersectionCandidate.HasValue)
            {
                return false;
            }

            var normalizedLine1SelectionPoint = options.Line1.ClosestPoint(options.Line1SelectionPoint);
            var normalizedLine2SelectionPoint = options.Line2.ClosestPoint(options.Line2SelectionPoint);

            if (options.Radius == 0.0)
            {
                // simple intersection and trim
                var intersectionPoint = intersectionCandidate.Value;
                var l1ReplaceP1 = IsPointBetween(intersectionPoint, options.Line1.P2, normalizedLine1SelectionPoint);
                var l2ReplaceP1 = IsPointBetween(intersectionPoint, options.Line2.P2, normalizedLine2SelectionPoint);

                var updatedL1 = l1ReplaceP1
                    ? options.Line1.Update(p1: intersectionPoint)
                    : options.Line1.Update(p2: intersectionPoint);
                var updatedL2 = l2ReplaceP1
                    ? options.Line2.Update(p1: intersectionPoint)
                    : options.Line2.Update(p2: intersectionPoint);
                result = new FilletResult(updatedL1, updatedL2, null);
            }
            else
            {
                // we'll have to insert an arc
                var line1Offset = EditUtilities.Offset(options.DrawingPlane, options.Line1, normalizedLine2SelectionPoint, options.Radius);
                var line2Offset = EditUtilities.Offset(options.DrawingPlane, options.Line2, normalizedLine1SelectionPoint, options.Radius);
                var centerCandidates = line1Offset.IntersectionPoints(line2Offset, withinBounds: false).ToList();
                if (centerCandidates.Count == 0)
                {
                    return false;
                }

                var center = centerCandidates.First();
                var l1PerpendicularVector = (options.Line1.P2 - options.Line1.P1).Cross(options.DrawingPlane.Normal);
                var l2PerpendicularVector = (options.Line2.P2 - options.Line2.P1).Cross(options.DrawingPlane.Normal);
                var l1Perpendicular = new PrimitiveLine(center, center + l1PerpendicularVector);
                var l2Perpendicular = new PrimitiveLine(center, center + l2PerpendicularVector);
                var l1IntersectionCandidate = options.Line1.IntersectionPoint(l1Perpendicular, withinSegment: false);
                var l2IntersectionCandidate = options.Line2.IntersectionPoint(l2Perpendicular, withinSegment: false);
                if (!l1IntersectionCandidate.HasValue || !l2IntersectionCandidate.HasValue)
                {
                    return false;
                }

                var l1Intersection = l1IntersectionCandidate.Value;
                var l2Intersection = l2IntersectionCandidate.Value;
                var l1ReplaceP1 = IsPointBetween(l1Intersection, options.Line1.P2, normalizedLine1SelectionPoint);
                var l2ReplaceP1 = IsPointBetween(l2Intersection, options.Line2.P2, normalizedLine2SelectionPoint);

                var updatedL1 = l1ReplaceP1
                    ? options.Line1.Update(p1: l1Intersection)
                    : options.Line1.Update(p2: l1Intersection);
                var updatedL2 = l2ReplaceP1
                    ? options.Line2.Update(p1: l2Intersection)
                    : options.Line2.Update(p2: l2Intersection);
                var angle1Vector = l1Intersection - center;
                var angle2Vector = l2Intersection - center;
                var angle1 = angle1Vector.ToAngle();
                var angle2 = angle2Vector.ToAngle();

                // angle can never be > 180
                var correctedAngles = MathHelper.EnsureMinorAngleDegrees(angle1, angle2);
                var startAngle = correctedAngles.Item1;
                var endAngle = correctedAngles.Item2;
                var arc = new PrimitiveEllipse(center, options.Radius, startAngle: startAngle, endAngle: endAngle, normal: options.DrawingPlane.Normal);
                result = new FilletResult(updatedL1, updatedL2, arc);
            }

            return result != null;
        }

        private static bool IsPointBetween(Point boundary1, Point boundary2, Point candidate)
        {
            var betweenX = MathHelper.Between(boundary1.X, boundary2.X, candidate.X);
            var betweenY = MathHelper.Between(boundary1.Y, boundary2.Y, candidate.Y);
            var betweenZ = MathHelper.Between(boundary1.Z, boundary2.Z, candidate.Z);
            return betweenX && betweenY && betweenZ;
        }
    }
}
