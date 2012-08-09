using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.Primitives;
using BCad.Helpers;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace BCad.Extensions
{
    public static class PrimitiveExtensions
    {
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
            var startAngle = ellipse.StartAngle;
            var endAngle = ellipse.EndAngle;
            return MathHelper.Between(startAngle, endAngle, angle)
                || MathHelper.Between(startAngle - 360.0, endAngle, angle)
                || MathHelper.Between(startAngle, endAngle + 360.0, angle);
        }

        public static PrimitiveLine Transform(this PrimitiveLine line, Matrix3D matrix)
        {
            return new PrimitiveLine(line.P1.Transform(matrix), line.P2.Transform(matrix), line.Color);
        }

        public static IEnumerable<Point> IntersectionPoints(this IPrimitive primitive, IPrimitive other, bool withinBounds = true)
        {
            IEnumerable<Point> result;
            switch (primitive.Kind)
            {
                case PrimitiveKind.Line:
                    result = ((PrimitiveLine)primitive).IntersectionPoints(other, withinBounds);
                    break;
                case PrimitiveKind.Ellipse:
                    result = ((PrimitiveEllipse)primitive).IntersectionPoints(other, withinBounds);
                    break;
                case PrimitiveKind.Text:
                    result = Enumerable.Empty<Point>();
                    break;
                default:
                    Debug.Fail("Unsupported primitive type");
                    result = Enumerable.Empty<Point>();
                    break;
            }

            return result;
        }

        #region Line-primitive intersection

        public static IEnumerable<Point> IntersectionPoints(this PrimitiveLine line, IPrimitive other, bool withinBounds = true)
        {
            IEnumerable<Point> result;
            switch (other.Kind)
            {
                case PrimitiveKind.Line:
                    result = new[] { line.IntersectionPoint((PrimitiveLine)other, withinBounds) };
                    break;
                case PrimitiveKind.Ellipse:
                    result = line.IntersectionPoints((PrimitiveEllipse)other, withinBounds);
                    break;
                case PrimitiveKind.Text:
                    result = Enumerable.Empty<Point>();
                    break;
                default:
                    Debug.Fail("Unsupported primitive type");
                    result = Enumerable.Empty<Point>();
                    break;
            }

            return result;
        }

        #endregion

        #region Line-line intersection

        public static Point IntersectionPoint(this PrimitiveLine first, PrimitiveLine second, bool withinSegment = true)
        {
            var minLength = 0.0000000001;

            //http://local.wasp.uwa.edu.au/~pbourke/geometry/lineline3d/
            // find real 3D intersection within a minimum distance
            var p1 = first.P1;
            var p2 = first.P2;
            var p3 = second.P1;
            var p4 = second.P2;
            var p13 = p1 - p3;
            var p43 = p4 - p3;

            if (p43.LengthSquared < MathHelper.Epsilon)
                return null;

            var p21 = p2 - p1;
            if (p21.LengthSquared < MathHelper.Epsilon)
                return null;

            var d1343 = p13.Dot(p43);
            var d4321 = p43.Dot(p21);
            var d1321 = p13.Dot(p21);
            var d4343 = p43.Dot(p43);
            var d2121 = p21.Dot(p21);

            var denom = d2121 * d4343 - d4321 * d4321;
            if (Math.Abs(denom) < MathHelper.Epsilon)
                return null;

            var num = d1343 * d4321 - d1321 * d4343;
            var mua = num / denom;
            var mub = (d1343 + d4321 * mua) / d4343;

            if (withinSegment)
            {
                if (!MathHelper.Between(0.0, 1.0, mua) ||
                    !MathHelper.Between(0.0, 1.0, mub))
                {
                    return null;
                }
            }

            var connector = new PrimitiveLine((p21 * mua) + p1, (p43 * mub) + p3);
            var cv = connector.P1 - connector.P2;
            if (cv.LengthSquared > minLength)
                return null;

            var point = (Point)((connector.P1 + connector.P2) / 2);
            return point;
        }

        #endregion

        #region Line-circle intersection

        public static IEnumerable<Point> IntersectionPoints(this PrimitiveLine line, PrimitiveEllipse ellipse, bool withinSegment = true)
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
            var transform = ellipse.GenerateUnitCircleProjection();
            var inverse = transform;
            inverse.Invert();

            var denom = l.Dot(n);
            var num = (p0 - l0).Dot(n);

            var flatPoints = new List<Point>();

            if (Math.Abs(denom) < MathHelper.Epsilon)
            {
                // plane either contains the line entirely or is parallel
                if (Math.Abs(num) < MathHelper.Epsilon)
                {
                    // parallel.  normalize the plane and find the intersection
                    var flatLine = line.Transform(inverse);
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

                    return points.Select(p => p.Transform(transform));
                }
            }
            else
            {
                // otherwise line and plane intersect in only 1 point, p
                var d = num / denom;
                var p = l * d + l0;

                // verify within the line segment
                if (withinSegment && !MathHelper.Between(0.0, 1.0, d))
                {
                    return empty;
                }

                // p is the point of intersection.  verify if on transformed unit circle
                var unitVector = (Vector)p.Transform(inverse);
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

        public static IEnumerable<Point> IntersectionPoints(this PrimitiveEllipse ellipse, IPrimitive primitive, bool withinBounds = true)
        {
            IEnumerable<Point> result;
            switch (primitive.Kind)
            {
                case PrimitiveKind.Ellipse:
                    result = ellipse.IntersectionPoints((PrimitiveEllipse)primitive, withinBounds);
                    break;
                case PrimitiveKind.Line:
                    result = ((PrimitiveLine)primitive).IntersectionPoints(ellipse, withinBounds);
                    break;
                case PrimitiveKind.Text:
                    result = Enumerable.Empty<Point>();
                    break;
                default:
                    Debug.Fail("Unsupported primitive type");
                    result = Enumerable.Empty<Point>();
                    break;
            }

            return result;
        }

        #endregion

        #region Circle-circle intersection

        public static IEnumerable<Point> IntersectionPoints(this PrimitiveEllipse first, PrimitiveEllipse second, bool withinBounds = true)
        {
            var empty = Enumerable.Empty<Point>();
            IEnumerable<Point> results;

            // planes either intersect in a line or are parallel
            var lineVector = first.Normal.Cross(second.Normal);
            if (lineVector.IsZeroVector)
            {
                // parallel or the same plane
                if ((first.Center == second.Center) ||
                    Math.Abs((second.Center - first.Center).Dot(first.Normal)) < MathHelper.Epsilon)
                {
                    // if they share a point or second.Center is on the first plane, they are the same plane
                    // project second back to a unit circle and find intersection points
                    var inverse = first.GenerateUnitCircleProjection();
                    var transform = inverse;
                    transform.Invert();

                    // transform second ellipse to be on the unit circle's plane
                    var secondCenter = second.Center.Transform(transform);
                    var secondMajorEnd = (second.Center + second.MajorAxis).Transform(transform);
                    var secondMinorEnd = (second.Center + (second.Normal.Cross(second.MajorAxis).Normalize() * second.MajorAxis.Length * second.MinorAxisRatio))
                        .Transform(transform);
                    var a = (secondMajorEnd - secondCenter).Length;
                    var b = (secondMinorEnd - secondCenter).Length;

                    if (a == b)
                    {
                        // if second ellipse is a circle we can absolutely solve for the intersection points
                        // rotate to place the center of the second circle on the x-axis
                        var angle = ((Vector)secondCenter).ToAngle();
                        var rotation = RotateAboutZ(angle * -1.0);
                        var newSecondCenter = secondCenter.Transform(rotation);
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

                        var returnTransform = inverse * RotateAboutZ(angle);
                        results = results.Distinct().Select(p => p.Transform(returnTransform));
                    }
                    else
                    {
                        // rotate about the origin to make the major axis align with the x-axis
                        var angle = (secondMajorEnd - secondCenter).ToAngle();
                        var rotation = RotateAboutZ(angle * -1.0);
                        var finalCenter = secondCenter.Transform(rotation);

                        // TODO: solve for X- or Y-axis alignment
                        if (finalCenter.Y == 0)
                        {
                            // rotate for y-axis alignment
                            var rotate90 = RotateAboutZ(90);
                            inverse = inverse * rotate90;
                            transform = inverse;
                            transform.Invert();
                            finalCenter = finalCenter.Transform(rotate90);

                            // swap major/minor axis values
                            var temp = a;
                            a = b;
                            b = temp;
                        }

                        if (finalCenter.X == 0)
                        {
                            // absolutely solve
                            var h = finalCenter.X;
                            var k = finalCenter.Y;
                            var k2 = k * k;
                            var a2 = a * a;
                            var b2 = b * b;
                            var a4 = a2 * a2;
                            var b4 = b2 * b2;

                            // find x values
                            var A = a2 - (a2 * b2) + (a2 * k2) - ((2 * a4 * k2) / (a2 - b2));
                            var B = (2 * a2 * k * Math.Sqrt(b2 * (-a2 + a4 + b2 - (a2 * b2) + (a2 * k2)))) / (a2 - b2);
                            var C = Math.Sqrt(a2 - b2);

                            var x1 = Math.Sqrt(A + B) / C;
                            var x2 = Math.Sqrt(A - B) / C;
                            
                            // find y values
                            var D = a2 * k;
                            var E = Math.Sqrt(-a2 * b2 + a4 * b2 - a2 * b4 + a2 * b2 * k2);
                            var F = a2 - b2;

                            var y1 = (D + E) / F;
                            var y2 = (D - E) / F;

                            results = new[] {
                                new Point(x1, y1, 0),
                                new Point(x1, y2, 0),
                                new Point(x2, y1, 0),
                                new Point(x2, y2, 0)
                            };
                        }
                        else
                        {
                            inverse = inverse * RotateAboutZ(angle);
                            transform = inverse;
                            transform.Invert();
                            // use Newtonian convergence to find a close approximation
                            results = new[] {
                                NewtonianConvergence(finalCenter, a, b, true, true),
                                NewtonianConvergence(finalCenter, a, b, true, false),
                                NewtonianConvergence(finalCenter, a, b, false, true),
                                NewtonianConvergence(finalCenter, a, b, false, false)
                            };
                        }

                        results = results
                            .Where(p => !(double.IsNaN(p.X) || double.IsNaN(p.Y) || double.IsNaN(p.Z)))
                            .Where(p => !(double.IsInfinity(p.X) || double.IsInfinity(p.Y) || double.IsInfinity(p.Z)))
                            .Distinct()
                            .Select(p => p.Transform(inverse));
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
                throw new NotImplementedException();
            }

            // verify points are in angle bounds
            if (withinBounds)
            {
                var firstUnit = first.GenerateUnitCircleProjection();
                var secondUnit = second.GenerateUnitCircleProjection();
                firstUnit.Invert();
                secondUnit.Invert();
                results = from res in results
                          // verify point is within first ellipse's angles
                          let trans1 = res.Transform(firstUnit)
                          let ang1 = ((Vector)trans1).ToAngle()
                          where first.IsAngleContained(ang1)
                          // and same for second
                          let trans2 = res.Transform(secondUnit)
                          let ang2 = ((Vector)trans2).ToAngle()
                          where second.IsAngleContained(ang2)
                          select res;
            }

            return results;
        }

        /// <summary>
        /// Performs a Newtonian-approximation convergence to find the intersection of a unit circle and
        /// the specified ellipse.
        /// </summary>
        /// <param name="center">The center of the ellipse.</param>
        /// <param name="a">The major axis length of the ellipse.</param>
        /// <param name="b">The minor axis length of the ellipse.</param>
        /// <param name="xPosRoot">If true, take the positive square root in the x-equation.</param>
        /// <param name="yPosRoot">If true, take the positive square root in the y-equation.</param>
        /// <returns></returns>
        private static Point NewtonianConvergence(Point center, double a, double b, bool xPosRoot, bool yPosRoot)
        {
            var a2 = a * a;
            var b2 = b * b;
            var a4 = a2 * a2;
            var h = center.X;
            var k = center.Y;
            var h2 = h * h;
            Func<double, double> getX = (y) => Math.Sqrt(1.0 - (y * y)) * (xPosRoot ? 1.0 : -1.0);
            Func<double, double, double> yNext = (x, y) =>
                {
                    var x2 = x * x;
                    var sqrOp = a4 * b2 - a2 * b2 * h2 + 2.0 * a2 * b2 * h * x - a2 * b2 * x2;
                    var sqr = Math.Sqrt(sqrOp);
                    var denom = 2.0 * a2 * b2 * (h - x);
                    return y + (2.0 * sqr * ((a2 * k) + sqr * (yPosRoot ? 1.0 : -1.0))) / denom;
                };

            var y0 = 0.0;
            var x0 = getX(y0);

            // TODO: check convergence to a certain number of digits
            for (int i = 0; i < 5; i++)
            {
                y0 = yNext(x0, y0);
                if (double.IsNaN(y0)) break;
                x0 = getX(y0);
                if (double.IsNaN(x0)) break;
            }

            return new Point(x0, y0, 0.0);
        }

        #endregion

        public static Matrix3D GenerateUnitCircleProjection(Vector normal, Vector right, Vector up, Point center, double scaleX, double scaleY, double scaleZ)
        {
            var transformation = Matrix3D.Identity;
            transformation.M11 = right.X;
            transformation.M12 = right.Y;
            transformation.M13 = right.Z;
            transformation.M21 = up.X;
            transformation.M22 = up.Y;
            transformation.M23 = up.Z;
            transformation.M31 = normal.X;
            transformation.M32 = normal.Y;
            transformation.M33 = normal.Z;
            transformation.OffsetX = center.X;
            transformation.OffsetY = center.Y;
            transformation.OffsetZ = center.Z;
            var scale = Matrix3D.Identity;
            scale.M11 = scaleX;
            scale.M22 = scaleY;
            scale.M33 = scaleZ;
            return scale * transformation;
        }

        public static Matrix3D GenerateUnitCircleProjection(this PrimitiveEllipse el)
        {
            var normal = el.Normal.Normalize();
            var right = el.MajorAxis.Normalize();
            var up = normal.Cross(right).Normalize();
            var radiusX = el.MajorAxis.Length;
            return GenerateUnitCircleProjection(normal, right, up, el.Center, radiusX, radiusX * el.MinorAxisRatio, 1.0);
        }

        public static Matrix3D Translation(Vector offset)
        {
            var m = Matrix3D.Identity;
            m.OffsetX = offset.X;
            m.OffsetY = offset.Y;
            m.OffsetZ = offset.Z;
            return m;
        }

        public static Matrix3D RotateAboutZ(double angleInDegrees)
        {
            var theta = angleInDegrees * MathHelper.DegreesToRadians;
            var cos = Math.Cos(theta);
            var sin = Math.Sin(theta);
            var m = Matrix3D.Identity;
            m.M11 = cos;
            m.M12 = -sin;
            m.M21 = sin;
            m.M22 = cos;
            return m;
        }
    }
}
