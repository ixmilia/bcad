using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IxMilia.BCad.Collections;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Helpers;
using IxMilia.BCad.Primitives;

namespace IxMilia.BCad.Extensions
{
    public static partial class PrimitiveExtensions
    {
        private static double[] SIN;
        private static double[] COS;
        public const double DefaultViewportBuffer = 20.0;

        static PrimitiveExtensions()
        {
            SIN = new double[360];
            COS = new double[360];
            double rad;
            for (int i = 0; i < 360; i++)
            {
                rad = (double)i * MathHelper.DegreesToRadians;
                SIN[i] = Math.Sin(rad);
                COS[i] = Math.Cos(rad);
            }
        }

        public static double GetThickness(this IPrimitive primitive)
        {
            return primitive.MapPrimitive<double>(
                ellipse => ellipse.Thickness,
                line => line.Thickness,
                point => 0.0,
                text => 0.0,
                bezier => 0.0,
                image => 0.0
            );
        }

        public static Point ClosestPoint(this PrimitiveLine line, Point point, bool withinBounds = true)
        {
            var v = line.P1;
            var w = line.P2;
            var p = point;
            var wv = w - v;
            var l2 = wv.LengthSquared;
            if (Math.Abs(l2) < MathHelper.Epsilon)
                return v;
            var t = (p - v).Dot(wv) / l2;
            if (withinBounds)
            {
                t = Math.Max(t, 0.0 - MathHelper.Epsilon);
                t = Math.Min(t, 1.0 + MathHelper.Epsilon);
            }

            var result = v + (wv) * t;
            return result;
        }

        public static double Slope(this PrimitiveLine line)
        {
            var denom = line.P2.X - line.P1.X;
            return denom == 0.0 ? double.NaN : (line.P2.Y - line.P1.Y) / denom;
        }

        public static double PerpendicularSlope(this PrimitiveLine line)
        {
            var slope = line.Slope();
            if (double.IsNaN(slope))
                return 0.0;
            else if (slope == 0.0)
                return double.NaN;
            else
                return -1.0 / slope;
        }

        public static bool IsAngleContained(this PrimitiveEllipse ellipse, double angle)
        {
            angle = angle.CorrectAngleDegrees();
            var startAngle = ellipse.StartAngle;
            var endAngle = ellipse.EndAngle;
            if (endAngle < startAngle)
            {
                // we pass zero.  angle should either be [startAngle, 360.0] or [0.0, endAngle]
                return MathHelper.Between(startAngle, 360.0, angle)
                    || MathHelper.Between(0.0, endAngle, angle);
            }
            else
            {
                // we're within normal bounds
                return MathHelper.Between(startAngle, endAngle, angle)
                    || MathHelper.Between(startAngle, endAngle, angle + 360.0);
            }
        }

        public static IEnumerable<Point> IntersectionPoints(this IPrimitive primitive, IPrimitive other, bool withinBounds = true)
        {
            return primitive.MapPrimitive<IEnumerable<Point>>(
                ellipse => ellipse.IntersectionPointsBetweenEllipseAndPrimitive(other, withinBounds),
                line => line.IntersectionPointsBetweenLineAndPrimitive(other, withinBounds),
                point => point.IntersectionPointsBetweenPointAndPrimitive(other, withinBounds),
                text => Enumerable.Empty<Point>(),
                bezier => bezier.IntersectionPointsBetweenBezierAndPrimitive(other),
                image => image.GetBoundaryLines().SelectMany(l => l.IntersectionPoints(other, withinBounds))
            );
        }

        #region Point-primitive intersection

        private static IEnumerable<Point> IntersectionPointsBetweenPointAndPrimitive(this PrimitivePoint point, IPrimitive other, bool withinBounds = true)
        {
            return other.MapPrimitive<IEnumerable<Point>>(
                ellipse => ellipse.IntersectionPointsBetweenEllipseAndPoint(point, withinBounds),
                line => line.IntersectionPointsBetweenLineAndPoint(point, withinBounds),
                otherPoint => point.IntersectionPointsBetweenPointAndPoint(otherPoint),
                text => Enumerable.Empty<Point>(),
                bezier => bezier.IntersectionPointsBetweenBezierAndPrimitive(point),
                image => image.IntersectionPointsBetweenImageAndPoint(point)
            );
        }

        private static IEnumerable<Point> IntersectionPointsBetweenPointAndPoint(this PrimitivePoint first, PrimitivePoint second)
        {
            if (first.Location.CloseTo(second.Location))
                return new Point[] { first.Location };
            else
                return Enumerable.Empty<Point>();
        }

        #endregion

        #region Line-primitive intersection

        private static IEnumerable<Point> IntersectionPointsBetweenLineAndPrimitive(this PrimitiveLine line, IPrimitive other, bool withinBounds = true)
        {
            if (line.IsPoint())
            {
                var point = new PrimitivePoint(line.P1);
                return point.IntersectionPointsBetweenPointAndPrimitive(other, withinBounds);
            }

            return other.MapPrimitive<IEnumerable<Point>>(
                ellipse => line.IntersectionPointsBetweenLineAndEllipse(ellipse, withinBounds),
                otherLine => line.IntersectionPointsBetweenLineAndLine(otherLine, withinBounds),
                point => line.IntersectionPointsBetweenLineAndPoint(point, withinBounds),
                text => Enumerable.Empty<Point>(),
                bezier => bezier.IntersectionPointsBetweenBezierAndPrimitive(line),
                image => image.GetBoundaryLines().SelectMany(l => l.IntersectionPoints(line, withinBounds))
            );
        }

        #endregion

        private static IEnumerable<Point> IntersectionPointsBetweenLineAndPoint(this PrimitiveLine line, PrimitivePoint point, bool withinBounds = true)
        {
            if (line.IsPointOnPrimitiveLine(point.Location, withinSegment: withinBounds))
            {
                return new Point[] { point.Location };
            }
            else
            {
                return new Point[0];
            }
        }

        #region Line-line intersection

        private static IEnumerable<Point> IntersectionPointsBetweenLineAndLine(this PrimitiveLine line, PrimitiveLine other, bool withinBounds = true)
        {
            var intersection = line.IntersectionPoint(other, withinSegment: withinBounds);
            if (intersection.HasValue)
            {
                return new Point[] { intersection.GetValueOrDefault() };
            }
            else
            {
                return Enumerable.Empty<Point>();
            }
        }

        #endregion

        #region Line-circle intersection

        private static IEnumerable<Point> IntersectionPointsBetweenLineAndEllipse(this PrimitiveLine line, PrimitiveEllipse ellipse, bool withinSegment = true)
        {
            var empty = Enumerable.Empty<Point>();

            var l0 = line.P1;
            var l = line.P2 - line.P1;
            var p0 = ellipse.Center;
            var n = ellipse.Normal;

            var right = ellipse.MajorAxis.Normalize();
            var up = ellipse.Normal.Cross(right).Normalize();
            var radiusX = ellipse.MajorAxis.Length;
            var radiusY = radiusX * ellipse.MinorAxisRatio;
            var transform = ellipse.FromUnitCircle;
            var inverse = transform.Inverse();

            var denom = l.Dot(n);
            var num = (p0 - l0).Dot(n);

            var flatPoints = new List<Point>();

            if (Math.Abs(denom) < MathHelper.Epsilon)
            {
                // plane either contains the line entirely or is parallel
                if (Math.Abs(num) < MathHelper.Epsilon)
                {
                    // parallel.  normalize the plane and find the intersection
                    var flatLine = inverse.Transform(line);
                    // the ellipse is now centered at the origin with a radius of 1.
                    // find the intersection points then reproject
                    var dv = flatLine.P2 - flatLine.P1;
                    var dx = dv.X;
                    var dy = dv.Y;
                    var dr2 = dx * dx + dy * dy;
                    var D = flatLine.P1.X * flatLine.P2.Y - flatLine.P2.X * flatLine.P1.Y;
                    var det = dr2 - D * D;
                    var points = new List<Point>();
                    if (det < 0.0 || Math.Abs(dr2) < MathHelper.Epsilon)
                    {
                        // no intersection or line is too short
                    }
                    else if (Math.Abs(det) < MathHelper.Epsilon)
                    {
                        // 1 point
                        var x = (D * dy) / dr2;
                        var y = (-D * dx) / dr2;
                        points.Add(new Point(x, y, 0.0));
                    }
                    else
                    {
                        // 2 points
                        var sgn = dy < 0.0 ? -1.0 : 1.0;
                        var det2 = Math.Sqrt(det);
                        points.Add(
                            new Point(
                                (D * dy + sgn * dx * det2) / dr2,
                                (-D * dx + Math.Abs(dy) * det2) / dr2,
                                0.0));
                        points.Add(
                            new Point(
                                (D * dy - sgn * dx * det2) / dr2,
                                (-D * dx - Math.Abs(dy) * det2) / dr2,
                                0.0));
                    }

                    // ensure the points are within appropriate bounds
                    if (withinSegment)
                    {
                        // line test
                        points = points.Where(p =>
                            MathHelper.Between(flatLine.P1.X, flatLine.P2.X, p.X) &&
                            MathHelper.Between(flatLine.P1.Y, flatLine.P2.Y, p.Y)).ToList();

                        // circle test
                        points = points.Where(p => ellipse.IsAngleContained(((Vector)p).ToAngle())).ToList();
                    }

                    return points.Select(p => (Point)transform.Transform(p));
                }
            }
            else
            {
                // otherwise line and plane intersect in only 1 point, p
                var d = num / denom;
                var p = (Point)(l * d + l0);

                // verify within the line segment
                if (withinSegment && !MathHelper.Between(0.0, 1.0, d))
                {
                    return empty;
                }

                // p is the point of intersection.  verify if on transformed unit circle
                var unitVector = (Vector)inverse.Transform(p);
                if (Math.Abs(unitVector.Length - 1.0) < MathHelper.Epsilon)
                {
                    // point is on the unit circle
                    if (withinSegment)
                    {
                        // verify within the angles specified
                        var angle = Math.Atan2(unitVector.Y, unitVector.X) * MathHelper.RadiansToDegrees;
                        if (MathHelper.Between(ellipse.StartAngle, ellipse.EndAngle, angle.CorrectAngleDegrees()))
                        {
                            return new[] { p };
                        }
                    }
                    else
                    {
                        return new[] { p };
                    }
                }
            }

            // point is not on unit circle
            return empty;
        }

        #endregion

        #region Circle-primitive intersection

        private static IEnumerable<Point> IntersectionPointsBetweenEllipseAndPrimitive(this PrimitiveEllipse ellipse, IPrimitive primitive, bool withinBounds = true)
        {
            return primitive.MapPrimitive<IEnumerable<Point>>(
                otherEllipse => ellipse.IntersectionPointsBetweenEllipseAndEllipse(otherEllipse, withinBounds),
                line => line.IntersectionPointsBetweenLineAndEllipse(ellipse, withinBounds),
                point => ellipse.IntersectionPointsBetweenEllipseAndPoint(point, withinBounds),
                text => Enumerable.Empty<Point>(),
                bezier => bezier.IntersectionPointsBetweenBezierAndPrimitive(ellipse),
                image => image.GetBoundaryLines().SelectMany(l => l.IntersectionPointsBetweenLineAndEllipse(ellipse))
            );
        }

        #endregion

        #region Circle-point intersection

        private static IEnumerable<Point> IntersectionPointsBetweenEllipseAndPoint(this PrimitiveEllipse ellipse, PrimitivePoint point, bool withinBounds = true)
        {
            var fromUnit = ellipse.FromUnitCircle;
            var toUnit = fromUnit.Inverse();
            var pointOnUnit = (Vector)toUnit.Transform(point.Location);
            if (MathHelper.CloseTo(pointOnUnit.LengthSquared, 1.0))
            {
                if (!withinBounds)
                    return new Point[] { point.Location }; // always a match
                else
                {
                    var pointAngle = pointOnUnit.ToAngle();
                    if (ellipse.IsAngleContained(pointAngle))
                        return new Point[] { point.Location };
                    else
                        return new Point[0];
                }
            }
            else
            {
                // wrong distance
                return new Point[0];
            }
        }

        #endregion

        #region Circle-circle intersection

        private static IEnumerable<Point> IntersectionPointsBetweenEllipseAndEllipse(this PrimitiveEllipse first, PrimitiveEllipse second, bool withinBounds = true)
        {
            var empty = Enumerable.Empty<Point>();
            IEnumerable<Point> results;

            // planes either intersect in a line or are parallel
            var lineVector = first.Normal.Cross(second.Normal);
            if (lineVector.IsCloseToZeroVector())
            {
                // parallel or the same plane
                if ((first.Center == second.Center) ||
                    Math.Abs((second.Center - first.Center).Dot(first.Normal)) < MathHelper.Epsilon)
                {
                    // if they share a point or second.Center is on the first plane, they are the same plane
                    // project second back to a unit circle and find intersection points
                    var fromUnit = first.FromUnitCircle;
                    var toUnit = fromUnit.Inverse();

                    // transform second ellipse to be on the unit circle's plane
                    var secondCenter = toUnit.Transform(second.Center);
                    var secondMajorEnd = toUnit.Transform(second.Center + second.MajorAxis);
                    var secondMinorEnd = toUnit.Transform(second.Center + (second.Normal.Cross(second.MajorAxis).Normalize() * second.MajorAxis.Length * second.MinorAxisRatio));
                    RoundedDouble a = (secondMajorEnd - secondCenter).Length;
                    RoundedDouble b = (secondMinorEnd - secondCenter).Length;

                    if (a == b)
                    {
                        // if second ellipse is a circle we can absolutely solve for the intersection points
                        // rotate to place the center of the second circle on the x-axis
                        var angle = ((Vector)secondCenter).ToAngle();
                        var rotation = Matrix4.RotateAboutZ(angle);
                        var returnTransform = fromUnit * Matrix4.RotateAboutZ(-angle);
                        var newSecondCenter = rotation.Transform(secondCenter);
                        var secondRadius = a;

                        if (Math.Abs(newSecondCenter.X) > secondRadius + 1.0)
                        {
                            // no points of intersection
                            results = empty;
                        }
                        else
                        {
                            // 2 points of intersection
                            var x = (secondRadius * secondRadius - newSecondCenter.X * newSecondCenter.X - 1.0)
                                / (-2.0 * newSecondCenter.X);
                            var y = Math.Sqrt(1.0 - x * x);
                            results = new[]
                                {
                                    new Point(x, y, 0),
                                    new Point(x, -y, 0)
                                };
                        }

                        results = results.Distinct().Select(p => returnTransform.Transform(p));
                    }
                    else
                    {
                        // rotate about the origin to make the major axis align with the x-axis
                        var angle = (secondMajorEnd - secondCenter).ToAngle();
                        var rotation = Matrix4.RotateAboutZ(angle);
                        var finalCenter = rotation.Transform(secondCenter);
                        fromUnit = fromUnit * Matrix4.RotateAboutZ(-angle);
                        toUnit = fromUnit.Inverse();

                        if (a < b)
                        {
                            // rotate to ensure a > b
                            fromUnit = fromUnit * Matrix4.RotateAboutZ(90);
                            toUnit = fromUnit.Inverse();
                            finalCenter = Matrix4.RotateAboutZ(-90).Transform(finalCenter);

                            // and swap a and b
                            var temp = a;
                            a = b;
                            b = temp;
                        }

                        RoundedDouble h = finalCenter.X;
                        RoundedDouble k = finalCenter.Y;
                        var h2 = h * h;
                        var k2 = k * k;
                        var a2 = a * a;
                        var b2 = b * b;
                        var a4 = a2 * a2;
                        var b4 = b2 * b2;

                        // RoundedDouble is used to ensure all operations are rounded to a certain precision
                        if (Math.Abs(h) < MathHelper.Epsilon)
                        {
                            // ellipse x = 0; wide
                            // find x values
                            var A = a2 - a2 * b2 + a2 * k2 - ((2 * a4 * k2) / (a2 - b2));
                            var B = (2 * a2 * k * Math.Sqrt(b2 * (-a2 + a4 + b2 - a2 * b2 + a2 * k2))) / (a2 - b2);
                            var C = (RoundedDouble)Math.Sqrt(a2 - b2);

                            var x1 = -Math.Sqrt(A + B) / C;
                            var x2 = (RoundedDouble)(-x1);
                            var x3 = -Math.Sqrt(A - B) / C;
                            var x4 = (RoundedDouble)(-x3);

                            // find y values
                            var D = a2 * k;
                            var E = Math.Sqrt(-a2 * b2 + a4 * b2 + b4 - a2 * b4 + a2 * b2 * k2);
                            var F = a2 - b2;

                            var y1 = (D - E) / F;
                            var y2 = (D + E) / F;

                            results = new[] {
                                new Point(x1, y1, 0),
                                new Point(x2, y1, 0),
                                new Point(x3, y2, 0),
                                new Point(x4, y2, 0)
                            };
                        }
                        else if (Math.Abs(k) < MathHelper.Epsilon)
                        {
                            // ellipse y = 0; wide
                            // find x values
                            var A = -b2 * h;
                            var B = Math.Sqrt(a4 - a2 * b2 - a4 * b2 + a2 * b4 + a2 * b2 * h2);
                            var C = a2 - b2;

                            var x1 = (A - B) / C;
                            var x2 = (A + B) / C;

                            // find y values
                            var D = -b2 + a2 * b2 - b2 * h2 - ((2 * b4 * h2) / (a2 - b2));
                            var E = (2 * b2 * h * Math.Sqrt(a2 * (a2 - b2 - a2 * b2 + b4 + b2 * h2))) / (a2 - b2);
                            var F = (RoundedDouble)Math.Sqrt(a2 - b2);

                            var y1 = -Math.Sqrt(D - E) / F;
                            var y2 = (RoundedDouble)(-y1);
                            var y3 = -Math.Sqrt(D + E) / F;
                            var y4 = (RoundedDouble)(-y3);

                            results = new[] {
                                new Point(x1, y1, 0),
                                new Point(x1, y2, 0),
                                new Point(x2, y3, 0),
                                new Point(x2, y4, 0)
                            };
                        }
                        else
                        {
                            // brute-force approximate intersections
                            results = BruteForceEllipseWithUnitCircle(finalCenter, a, b);
                        }

                        results = results
                            .Where(p => !(double.IsNaN(p.X) || double.IsNaN(p.Y) || double.IsNaN(p.Z)))
                            .Where(p => !(double.IsInfinity(p.X) || double.IsInfinity(p.Y) || double.IsInfinity(p.Z)))
                            .Distinct()
                            .Select(p => fromUnit.Transform(p))
                            .Select(p => new Point((RoundedDouble)p.X, (RoundedDouble)p.Y, (RoundedDouble)p.Z));
                    }
                }
                else
                {
                    // parallel with no intersections
                    results = empty;
                }
            }
            else
            {
                // intersection was a line
                // find a common point to complete the line then intersect that line with the circles
                double x = 0, y = 0, z = 0;
                var n = first.Normal;
                var m = second.Normal;
                var q = first.Center;
                var r = second.Center;
                if (Math.Abs(lineVector.X) >= MathHelper.Epsilon)
                {
                    x = 0.0;
                    y = (-m.Z * n.X * q.X - m.Z * n.Y * q.Y - m.Z * n.Z * q.Z + m.X * n.Z * r.X + m.Y * n.Z * r.Y + m.Z * n.Z * r.Z)
                        / (-m.Z * n.Y + m.Y * n.Z);
                    z = (-m.Y * n.X * q.X - m.Y * n.Y * q.Y - m.Y * n.Z * q.Z + m.X * n.Y * r.X + m.Y * n.Y * r.Y + m.Z * n.Y * r.Z)
                        / (-m.Z * n.Y + m.Y * n.Z);
                }
                else if (Math.Abs(lineVector.Y) >= MathHelper.Epsilon)
                {
                    x = (-m.Z * n.X * q.X - m.Z * n.Y * q.Y - m.Z * n.Z * q.Z + m.X * n.Z * r.X + m.Y * n.Z * r.Y + m.Z * n.Z * r.Z)
                        / (-m.Z * n.X + m.X * n.Z);
                    y = 0.0;
                    z = (-m.X * n.X * q.X - m.X * n.Y * q.Y - m.X * n.Z * q.Z + m.X * n.X * r.X + m.Y * n.X * r.Y + m.Z * n.X * r.Z)
                        / (-m.Z * n.X + m.X * n.Z);
                }
                else if (Math.Abs(lineVector.Z) >= MathHelper.Epsilon)
                {
                    x = (-m.Y * n.X * q.X - m.Y * n.Y * q.Y - m.Y * n.Z * q.Z + m.X * n.Y * r.X + m.Y * n.Y * r.Y + m.Z * n.Y * r.Z)
                        / (m.Y * n.X - m.X * n.Y);
                    y = (-m.X * n.X * q.X - m.X * n.Y * q.Y - m.X * n.Z * q.Z + m.X * n.X * r.X + m.Y * n.X * r.Y + m.Z * n.X * r.Z)
                        / (m.Y * n.X - m.X * n.Y);
                    z = 0.0;
                }
                else
                {
                    //Debug.Assert(false, "zero-vector shouldn't get here");
                }

                var point = new Point(x, y, z);
                var other = point + lineVector;
                var intersectionLine = new PrimitiveLine(point, other);
                var firstIntersections = intersectionLine.IntersectionPoints(first, false)
                    .Select(p => new Point((RoundedDouble)p.X, (RoundedDouble)p.Y, (RoundedDouble)p.Z));
                var secondIntersections = intersectionLine.IntersectionPoints(second, false)
                    .Select(p => new Point((RoundedDouble)p.X, (RoundedDouble)p.Y, (RoundedDouble)p.Z));
                results = firstIntersections
                    .Union(secondIntersections)
                    .Distinct();
            }

            // verify points are in angle bounds
            if (withinBounds)
            {
                var toFirstUnit = first.FromUnitCircle.Inverse();
                var toSecondUnit = second.FromUnitCircle.Inverse();
                results = from res in results
                              // verify point is within first ellipse's angles
                          let trans1 = toFirstUnit.Transform(res)
                          let ang1 = ((Vector)trans1).ToAngle()
                          where first.IsAngleContained(ang1)
                          // and same for second
                          let trans2 = toSecondUnit.Transform(res)
                          let ang2 = ((Vector)trans2).ToAngle()
                          where second.IsAngleContained(ang2)
                          select res;
            }

            return results;
        }

        private static IEnumerable<Point> BruteForceEllipseWithUnitCircle(Point center, double a, double b)
        {
            var results = new List<Point>();

            Func<int, Point> ellipsePoint = (an) => new Point(COS[an] * a + center.X, SIN[an] * b + center.Y, 0);
            Func<Point, bool> isInside = (p) => ((p.X * p.X) + (p.Y * p.Y)) <= 1;
            var current = ellipsePoint(0);
            var inside = isInside(current);
            for (int angle = 1; angle < 360; angle++)
            {
                var nextPoint = ellipsePoint(angle);
                var next = isInside(nextPoint);
                if (next != inside)
                {
                    results.Add(nextPoint);
                }

                inside = next;
                current = nextPoint;
            }

            return results;
        }

        #endregion

        #region Bezier-primitive intersection

        private static IEnumerable<Point> IntersectionPointsBetweenBezierAndPrimitive(this PrimitiveBezier bezier, IPrimitive other)
        {
            return other.MapPrimitive<IEnumerable<Point>>(
                ellipse => bezier.IntersectionPrimitives.SelectMany(p => p.IntersectionPoints(other)),
                line => bezier.IntersectionPrimitives.SelectMany(p => p.IntersectionPoints(other)),
                point => bezier.IntersectionPrimitives.SelectMany(p => p.IntersectionPoints(other)),
                text => bezier.IntersectionPrimitives.SelectMany(p => p.IntersectionPoints(other)),
                otherBezier => bezier.IntersectionPrimitives.SelectMany(p1 => otherBezier.IntersectionPrimitives.SelectMany(p2 => p1.IntersectionPoints(p2))),
                image => bezier.IntersectionPrimitives.SelectMany(p => p.IntersectionPoints(image))
            );
        }

        #endregion

        #region Image-primitive intersection

        private static IEnumerable<Point> IntersectionPointsBetweenImageAndPoint(this PrimitiveImage image, PrimitivePoint point)
        {
            if (image.GetBoundingBox().Contains(point.Location))
            {
                return new[] { point.Location };
            }
            else
            {
                return Enumerable.Empty<Point>();
            }
        }

        #endregion

        public static Entity ToEntity(this IPrimitive primitive, LineTypeSpecification lineTypeSpecification = null)
        {
            return primitive.MapPrimitive<Entity>(
                ellipse => ellipse.MinorAxisRatio == 1.0
                    ? ellipse.IsClosed
                        ? new Circle(ellipse, lineTypeSpecification: lineTypeSpecification)
                        : new Arc(ellipse, lineTypeSpecification: lineTypeSpecification)
                    : new Ellipse(ellipse, lineTypeSpecification: lineTypeSpecification),
                line => new Line(line, lineTypeSpecification: lineTypeSpecification),
                point => new Location(point, lineTypeSpecification: lineTypeSpecification),
                text => new Text(text, lineTypeSpecification: lineTypeSpecification),
                bezier => Spline.FromBezier(bezier, lineTypeSpecification: lineTypeSpecification),
                image => new Image(image, lineTypeSpecification: lineTypeSpecification)
            );
        }

        public static IPrimitive Move(this IPrimitive primitive, Vector offset)
        {
            return primitive.MapPrimitive<IPrimitive>(
                ellipse => new PrimitiveEllipse(
                    ellipse.Center + offset,
                    ellipse.MajorAxis,
                    ellipse.Normal,
                    ellipse.MinorAxisRatio,
                    ellipse.StartAngle,
                    ellipse.EndAngle,
                    ellipse.Color),
                line => new PrimitiveLine(
                    line.P1 + offset,
                    line.P2 + offset,
                    line.Color),
                point => new PrimitivePoint(
                    point.Location + offset,
                    point.Color),
                text => new PrimitiveText(
                    text.Value,
                    text.Location + offset,
                    text.Height,
                    text.Normal,
                    text.Rotation,
                    text.Color),
                bezier => new PrimitiveBezier(
                    bezier.P1 + offset,
                    bezier.P2 + offset,
                    bezier.P3 + offset,
                    bezier.P4 + offset,
                    bezier.Color),
                image => new PrimitiveImage(
                    image.Location + offset,
                    image.ImageData,
                    image.Path,
                    image.Width,
                    image.Height,
                    image.Rotation,
                    image.Color)
            );
        }

        public static bool IsPointOnPrimitive(this IPrimitive primitive, Point point, double epsilon = MathHelper.Epsilon)
        {
            return primitive.MapPrimitive<bool>(
                ellipse => IsPointOnPrimitiveEllipse(ellipse, point, epsilon),
                line => IsPointOnPrimitiveLine(line, point, epsilon),
                otherPoint => otherPoint.Location.CloseTo(point, epsilon),
                text => IsPointOnPrimitiveText(text, point),
                bezier => IsPointOnPrimitiveBezier(bezier, point),
                image => IsPointOnPrimitiveImage(image, point)
            );
        }

        private static bool IsPointOnPrimitiveLine(this PrimitiveLine line, Point point, double epsilon = MathHelper.Epsilon, bool withinSegment = true)
        {
            if (point == line.P1)
                return true;
            var lineVector = line.P2 - line.P1;
            var pointVector = point - line.P1;
            return (lineVector.Normalize().CloseTo(pointVector.Normalize(), epsilon))
                && (!withinSegment
                    || MathHelper.Between(0.0, lineVector.LengthSquared, pointVector.LengthSquared));
        }

        private static bool IsPointOnPrimitiveEllipse(this PrimitiveEllipse el, Point point, double epsilon = MathHelper.Epsilon)
        {
            var unitPoint = el.FromUnitCircle.Inverse().Transform(point);
            return MathHelper.CloseTo(0.0, unitPoint.Z) // on the XY plane
                && MathHelper.CloseTo(1.0, ((Vector)unitPoint).LengthSquared, epsilon) // on the unit circle
                && el.IsAngleContained(Math.Atan2(unitPoint.Y, unitPoint.X) * MathHelper.RadiansToDegrees); // within angle bounds
        }

        private static bool IsPointOnPrimitiveText(this PrimitiveText text, Point point)
        {
            // check for plane containment
            var plane = new Plane(text.Location, text.Normal);
            if (plane.Contains(point))
            {
                // check for horizontal/vertical containment
                var right = Vector.RightVectorFromNormal(text.Normal);
                var up = text.Normal.Cross(right).Normalize();
                var projection = Matrix4.FromUnitCircleProjection(text.Normal, right, up, text.Location, 1.0, 1.0, 1.0).Inverse();

                var projected = projection.Transform(point);
                if (MathHelper.Between(0.0, text.Width, projected.X) &&
                    MathHelper.Between(0.0, text.Height, projected.Y))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsPointOnPrimitiveBezier(this PrimitiveBezier bezier, Point point)
        {
            var t = bezier.GetParameterValueForPoint(point);
            return t.HasValue;
        }

        private static bool IsPointOnPrimitiveImage(this PrimitiveImage image, Point point)
        {
            return image.GetBoundingBox().Contains(point);
        }

        public static double GetAngle(this PrimitiveEllipse ellipse, Point point)
        {
            var transform = ellipse.FromUnitCircle.Inverse();
            var pointFromUnitCircle = transform.Transform(point);
            var angle = Math.Atan2(pointFromUnitCircle.Y, pointFromUnitCircle.X) * MathHelper.RadiansToDegrees;
            return angle.CorrectAngleDegrees();
        }

        public static Point GetPoint(this PrimitiveEllipse ellipse, double angle)
        {
            var pointUnit = new Point(Math.Cos(angle * MathHelper.DegreesToRadians), Math.Sin(angle * MathHelper.DegreesToRadians), 0.0);
            var pointTransformed = ellipse.FromUnitCircle.Transform(pointUnit);
            return pointTransformed;
        }

        public static Point[] GetInterestingPoints(this IPrimitive primitive)
        {
            return primitive.MapPrimitive<Point[]>(
                ellipse => ellipse.GetInterestingPointsEllipse(360),
                line => new[] { line.P1, line.P2 },
                point => new[] { point.Location },
                text =>
                {
                    var rad = text.Rotation * MathHelper.DegreesToRadians;
                    var right = new Vector(Math.Cos(rad), Math.Sin(rad), 0.0).Normalize() * text.Width;
                    var up = text.Normal.Cross(right).Normalize() * text.Height;
                    return new[]
                    {
                        text.Location,
                        text.Location + right,
                        text.Location + right + up,
                        text.Location + up,
                        text.Location
                    };
                },
                bezier => new[] { bezier.P1, bezier.P2, bezier.P3, bezier.P4 },
                image =>
                {
                    var rad = image.Rotation * MathHelper.DegreesToRadians;
                    var right = new Vector(Math.Cos(rad), Math.Sin(rad), 0.0).Normalize() * image.Width;
                    var up = Vector.ZAxis.Cross(right).Normalize() * image.Height;
                    return new[]
                    {
                        image.Location,
                        image.Location + right,
                        image.Location + right + up,
                        image.Location + up,
                        image.Location
                    };
                }
            );
        }

        private static Point[] GetInterestingPointsEllipse(this PrimitiveEllipse ellipse, int maxSeg)
        {
            var startAngleDeg = ellipse.StartAngle;
            var endAngleDeg = ellipse.EndAngle;
            if (endAngleDeg < startAngleDeg)
                endAngleDeg += MathHelper.ThreeSixty;
            var startAngleRad = startAngleDeg * MathHelper.DegreesToRadians;
            var endAngleRad = endAngleDeg * MathHelper.DegreesToRadians;
            if (endAngleRad < startAngleRad)
                endAngleRad += MathHelper.TwoPI;
            var vertexCount = (int)Math.Ceiling((endAngleDeg - startAngleDeg) / MathHelper.ThreeSixty * maxSeg);
            var points = new Point[vertexCount + 1];
            var angleDelta = MathHelper.ThreeSixty / maxSeg * MathHelper.DegreesToRadians;
            var trans = ellipse.FromUnitCircle;
            double angle;
            int i;
            for (angle = startAngleRad, i = 0; i < vertexCount; angle += angleDelta, i++)
            {
                points[i] = trans.Transform(new Point(Math.Cos(angle), Math.Sin(angle), 0.0));
            }

            points[i] = trans.Transform(new Point(Math.Cos(angle), Math.Sin(angle), 0.0));
            return points;
        }

        public static Rect GetExtents(this IEnumerable<IPrimitive> primitives, Vector sight)
        {
            var drawingPlane = new Plane(Point.Origin, sight);
            var planeProjection = drawingPlane.ToXYPlaneProjection();
            var allPoints = primitives
                .SelectMany(p => p.GetInterestingPoints())
                .Select(p => planeProjection.Transform(p));

            if (allPoints.Count() < 2)
            {
                // TODO: better handling for 0 or 1 primitives
                return new Rect(0, 0, 1, 1);
            }

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

            var width = maxx - minx;
            var height = maxy - miny;
            return new Rect(minx, miny, width, height);
        }

        public static ViewPort ShowAllViewPort(this IEnumerable<IPrimitive> primitives, Vector sight, Vector up, double viewPortWidth, double viewPortHeight, double viewportBuffer = DefaultViewportBuffer)
        {
            return primitives.GetExtents(sight).ShowAllViewPort(sight, up, viewPortWidth, viewPortHeight, viewportBuffer);
        }

        public static ViewPort ShowAllViewPort(this Rect extents, Vector sight, Vector up, double viewPortWidth, double viewPortHeight, double viewportBuffer = DefaultViewportBuffer)
        {
            var minx = extents.Left;
            var miny = extents.Top;
            var maxx = extents.BottomRight.X;
            var maxy = extents.BottomRight.Y;
            var deltaX = maxx - minx;
            var deltaY = maxy - miny;

            // translate back out of XY plane
            var drawingPlane = new Plane(Point.Origin, sight);
            var planeProjection = drawingPlane.ToXYPlaneProjection();
            var unproj = planeProjection.Inverse();
            var bottomLeft = unproj.Transform(new Point(minx, miny, 0));
            var topRight = unproj.Transform(new Point(minx + deltaX, miny + deltaY, 0));
            var drawingHeight = deltaY;
            var drawingWidth = deltaX;

            double viewHeight, drawingToViewScale;
            var viewRatio = viewPortWidth / viewPortHeight;
            var drawingRatio = drawingWidth / drawingHeight;
            if (MathHelper.CloseTo(0.0, drawingHeight) || drawingRatio > viewRatio)
            {
                // fit to width
                var viewWidth = drawingWidth;
                viewHeight = viewPortHeight * viewWidth / viewPortWidth;
                drawingToViewScale = viewPortWidth / viewWidth;

                // add a buffer of some amount of pixels
                var pixelWidth = drawingWidth * drawingToViewScale;
                var newPixelWidth = pixelWidth + (viewportBuffer * 2.0);
                viewHeight *= newPixelWidth / pixelWidth;
            }
            else
            {
                // fit to height
                viewHeight = drawingHeight;
                drawingToViewScale = viewPortHeight / viewHeight;

                // add a buffer of some amount of pixels
                var pixelHeight = drawingHeight * drawingToViewScale;
                var newPixelHeight = pixelHeight + (viewportBuffer * 2.0);
                viewHeight *= newPixelHeight / pixelHeight;
            }

            // center viewport
            var tempViewport = new ViewPort(bottomLeft, sight, up, viewHeight);
            var pixelMatrix = tempViewport.GetTransformationMatrixWindowsStyle(viewPortWidth, viewPortHeight);
            var bottomLeftScreen = pixelMatrix.Transform(bottomLeft);
            var topRightScreen = pixelMatrix.Transform(topRight);

            // center horizontally
            var leftXGap = bottomLeftScreen.X;
            var rightXGap = viewPortWidth - topRightScreen.X;
            var xAdjust = Math.Abs((rightXGap - leftXGap) / 2.0) / drawingToViewScale;

            // center vertically
            var topYGap = topRightScreen.Y;
            var bottomYGap = viewPortHeight - bottomLeftScreen.Y;
            var yAdjust = Math.Abs((topYGap - bottomYGap) / 2.0) / drawingToViewScale;

            var newBottomLeft = new Point(bottomLeft.X - xAdjust, bottomLeft.Y - yAdjust, bottomLeft.Z);

            var newVp = tempViewport.Update(bottomLeft: newBottomLeft, viewHeight: viewHeight);
            return newVp;
        }

        public static Point StartPoint(this IPrimitive primitive)
        {
            return primitive.MapPrimitive<Point>(
                ellipse => ellipse.GetPoint(ellipse.StartAngle),
                line => line.P1,
                point => point.Location,
                text => text.Location,
                bezier => bezier.P1,
                image => image.Location
            );
        }

        public static Point EndPoint(this IPrimitive primitive)
        {
            return primitive.MapPrimitive<Point>(
                ellipse => ellipse.GetPoint(ellipse.EndAngle),
                line => line.P2,
                point => point.Location,
                text => throw new ArgumentException($"{nameof(primitive)}.{nameof(primitive.Kind)}"),
                bezier => bezier.P4,
                image => throw new ArgumentException($"{nameof(primitive)}.{nameof(primitive.Kind)}")
            );
        }

        public static Point MidPoint(this IPrimitive primitive)
        {
            return primitive.MapPrimitive<Point>(
                ellipse =>
                {
                    var endAngle = ellipse.EndAngle;
                    if (endAngle < ellipse.StartAngle)
                    {
                        endAngle += MathHelper.ThreeSixty;
                    }

                    return ellipse.GetPoint((ellipse.StartAngle + endAngle) * 0.5);
                },
                line => (line.P1 + line.P2) / 2.0,
                point => point.Location,
                text => throw new ArgumentException($"{nameof(primitive)}.{nameof(primitive.Kind)}"),
                bezier => bezier.ComputeParameterizedPoint(0.5), // find midpoint by length?
                image => throw new ArgumentException($"{nameof(primitive)}.{nameof(primitive.Kind)}")
            );
        }

        public static IEnumerable<Point> GetProjectedVerticies(this IPrimitive primitive, Matrix4 transformationMatrix)
        {
            return primitive.MapPrimitive<IEnumerable<Point>>(
                ellipse => ellipse.GetProjectedVerticies(transformationMatrix, 360),
                line => line.GetInterestingPoints().Select(p => transformationMatrix.Transform(p)),
                point => point.GetInterestingPoints().Select(p => transformationMatrix.Transform(p)),
                text => text.GetInterestingPoints().Select(p => transformationMatrix.Transform(p)),
                bezier => bezier.GetInterestingPoints().Select(p => transformationMatrix.Transform(p)),
                image => image.GetInterestingPoints().Select(p => transformationMatrix.Transform(p))
            );
        }

        public static IEnumerable<Point> GetProjectedVerticies(this PrimitiveEllipse ellipse, Matrix4 transformationMatrix, int maxSeg)
        {
            return ellipse.GetInterestingPointsEllipse(maxSeg)
                .Select(p => transformationMatrix.Transform(p));
        }

        public static IEnumerable<IEnumerable<IPrimitive>> GetLineStripsFromPrimitives(this IEnumerable<IPrimitive> primitives)
        {
            var remainingPrimitives = new HashSet<IPrimitive>(primitives);
            var result = new List<List<IPrimitive>>();
            while (remainingPrimitives.Count > 0)
            {
                var shapePrimitives = new List<IPrimitive>();
                var first = remainingPrimitives.First();
                remainingPrimitives.Remove(first);
                shapePrimitives.Add(first);

                while (remainingPrimitives.Count > 0)
                {
                    var lastPrimitive = shapePrimitives.Last();
                    var lastStartPoint = lastPrimitive.StartPoint();
                    var lastEndPoint = lastPrimitive.EndPoint();
                    var nextPrimitive = remainingPrimitives.FirstOrDefault(l =>
                        l.StartPoint().CloseTo(lastStartPoint) ||
                        l.EndPoint().CloseTo(lastStartPoint) ||
                        l.StartPoint().CloseTo(lastEndPoint) ||
                        l.EndPoint().CloseTo(lastEndPoint));
                    if (nextPrimitive == null)
                    {
                        // no more segments
                        break;
                    }

                    remainingPrimitives.Remove(nextPrimitive);
                    shapePrimitives.Add(nextPrimitive);
                }

                result.Add(shapePrimitives);
            }

            return result;
        }

        public static IEnumerable<Polyline> GetPolylinesFromSegments(this IEnumerable<IPrimitive> primitives)
        {
            return primitives
                .GetLineStripsFromPrimitives()
                .GetPolylinesFromPrimitives();
        }

        public static void DoPrimitive(
            this IPrimitive primitive,
            Action<PrimitiveEllipse> ellipseAction,
            Action<PrimitiveLine> lineAction,
            Action<PrimitivePoint> pointAction,
            Action<PrimitiveText> textAction,
            Action<PrimitiveBezier> bezierAction,
            Action<PrimitiveImage> imageAction)
        {
            switch (primitive)
            {
                case PrimitiveEllipse ellipse:
                    ellipseAction(ellipse);
                    break;
                case PrimitiveLine line:
                    lineAction(line);
                    break;
                case PrimitivePoint point:
                    pointAction(point);
                    break;
                case PrimitiveText text:
                    textAction(text);
                    break;
                case PrimitiveBezier bezier:
                    bezierAction(bezier);
                    break;
                case PrimitiveImage image:
                    imageAction(image);
                    break;
                default:
                    throw new ArgumentException($"Unexpected primitive: {primitive.Kind}");
            }
        }

        public static TResult MapPrimitive<TResult>(
            this IPrimitive primitive,
            Func<PrimitiveEllipse, TResult> ellipseMapper,
            Func<PrimitiveLine, TResult> lineMapper,
            Func<PrimitivePoint, TResult> pointMapper,
            Func<PrimitiveText, TResult> textMapper,
            Func<PrimitiveBezier, TResult> bezierMapper,
            Func<PrimitiveImage, TResult> imageMapper)
        {
            TResult result = default;
            primitive.DoPrimitive(
                ellipse => result = ellipseMapper(ellipse),
                line => result = lineMapper(line),
                point => result = pointMapper(point),
                text => result = textMapper(text),
                bezier => result = bezierMapper(bezier),
                image => result = imageMapper(image)
            );

            return result;
        }
    }
}
