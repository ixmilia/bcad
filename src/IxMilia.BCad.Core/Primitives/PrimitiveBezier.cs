using System;
using System.Collections.Generic;
using System.Linq;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Helpers;

namespace IxMilia.BCad.Primitives
{
    public class PrimitiveBezier : IPrimitive
    {
        // stop at 2^7=128 primitives
        private const int MaximumPrimitiveSplitCount = 7;

        public Point P1 { get; }
        public Point P2 { get; }
        public Point P3 { get; }
        public Point P4 { get; }
        public CadColor? Color { get; private set; }
        public PrimitiveKind Kind { get { return PrimitiveKind.Bezier; } }

        private Lazy<IPrimitive[]> _intersectionPrimitives;

        public IPrimitive[] IntersectionPrimitives => _intersectionPrimitives.Value;

        public PrimitiveBezier(Point p1, Point p2, Point p3, Point p4, CadColor? color = null)
        {
            P1 = p1;
            P2 = p2;
            P3 = p3;
            P4 = p4;
            Color = color;
            _intersectionPrimitives = new Lazy<IPrimitive[]>(() => AsSimplePrimitives(this));
        }

        public Point ComputeParameterizedPoint(double t)
        {
            var tprime = 1.0 - t;
            var point =
                P1 * (tprime * tprime * tprime) +
                P2 * (3.0 * tprime * tprime * t) +
                P3 * (3.0 * tprime * t * t) +
                P4 * (t * t * t);
            return point;
        }

        public double? GetParameterValueForPoint(Point point)
        {
            // translate curve down by `point.Y`, then solve for zeros and see if any X values == `point.X`
            var bezier = (PrimitiveBezier)this.Move(new Vector(0.0, -point.Y, 0.0));
            var roots = bezier.FindYRoots().Where(r => r >= 0.0 && r <= 1.0);
            foreach (var root in roots)
            {
                if (MathHelper.CloseTo(ComputeParameterizedPoint(root).X, point.X, MathHelper.BezierEpsilon))
                {
                    return root;
                }
            }

            return null;
        }

        public IEnumerable<double> FindYRoots()
        {
            // Using Cardano's algorithm from https://pomax.github.io/bezierinfo/#extremities
            var pa = P1.Y;
            var pb = P2.Y;
            var pc = P3.Y;
            var pd = P4.Y;

            if (MathHelper.CloseTo(pa, pd) && MathHelper.CloseTo(pb, pc))
            {
                // symetric values means there is a single root right in the middle
                return new[] { 0.5 };
            }

            var d = -pa + 3.0 * pb - 3.0 * pc + pd;
            var a = (3.0 * pa - 6.0 * pb + 3.0 * pc) / d;
            var b = (-3.0 * pa + 3.0 * pb) / d;
            var c = pa / d;

            var p = (3.0 * b - a * a) / 3.0;
            var p3 = p / 3.0;
            var q = (2.0 * a * a * a - 9.0 * a * b + 27.0 * c) / 27.0;
            var q2 = q / 2.0;
            var discriminant = q2 * q2 + p3 * p3 * p3;

            IEnumerable<double> roots;
            if (discriminant < 0.0)
            {
                // three possible real roots
                var mp3 = -p / 3.0;
                var mp33 = mp3 * mp3 * mp3;
                var r = Math.Sqrt(mp33);
                var t = -q / (2.0 * r);
                var cosphi = t < -1.0 ? -1.0 : t > 1.0 ? 1.0 : t;
                var phi = Math.Acos(cosphi);
                var crtr = MathHelper.CubeRoot(r);
                var t1 = 2.0 * crtr;
                var root1 = t1 * Math.Cos(phi / 3.0) - a / 3.0;
                var root2 = t1 * Math.Cos((phi + 2.0 * Math.PI) / 3.0) - a / 3.0;
                var root3 = t1 * Math.Cos((phi + 4.0 * Math.PI) / 3.0) - a / 3.0;
                roots = new[] { root1, root2, root3 };
            }
            else if (discriminant == 0.0)
            {
                // three real roots, but two are identical
                var u1 = q2 < 0.0 ? MathHelper.CubeRoot(-q2) : MathHelper.CubeRoot(q2);
                var root1 = 2.0 * u1 - a / 3.0;
                var root2 = -u1 - a / 3.0;
                roots = new[] { root1, root2 };
            }
            else
            {
                // one real root, two complex roots
                var sd = Math.Sqrt(discriminant);
                var u1 = MathHelper.CubeRoot(sd - q2);
                var v1 = MathHelper.CubeRoot(sd + q2);
                var root1 = u1 - v1 - a / 3.0;
                roots = new[] { root1 };
            }

            return roots;
        }

        public Tuple<PrimitiveBezier, PrimitiveBezier> Split(double t)
        {
            // Splitting the curve using de Casteljau's algorithm: https://pomax.github.io/bezierinfo/#decasteljau
            if (t < 0.0 || t > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(t), "Curves can only be split on the interval [0.0, 1.0].");
            }

            var midPoint = ComputeParameterizedPoint(t);
            var midControlPoint = Point.ScaledAlongPath(P2, P3, t);

            // compute the first curve part
            var newP2 = Point.ScaledAlongPath(P1, P2, t);
            var newP3 = Point.ScaledAlongPath(newP2, midControlPoint, t);
            var newP4 = midPoint;
            var curve1 = new PrimitiveBezier(P1, newP2, newP3, newP4);

            // compute the second curve part
            var newP1 = midPoint;
            newP3 = Point.ScaledAlongPath(P3, P4, t);
            newP2 = Point.ScaledAlongPath(midControlPoint, newP3, t);
            var curve2 = new PrimitiveBezier(newP1, newP2, newP3, P4);

            return Tuple.Create(curve1, curve2);
        }

        public BoundingBox GetBoundingBox()
        {
            return BoundingBox.FromPoints(P1, P2, P3, P4);
        }

        public override string ToString()
        {
            return $"PrimitiveBezier: p1={P1}, p2={P2}, p3={P3}, p4={P4}";
        }

        public PrimitiveBezier Update(
            Optional<Point> p1 = default,
            Optional<Point> p2 = default,
            Optional<Point> p3 = default,
            Optional<Point> p4 = default,
            Optional<CadColor?> color = default)
        {
            var newP1 = p1.HasValue ? p1.Value : P1;
            var newP2 = p2.HasValue ? p2.Value : P2;
            var newP3 = p3.HasValue ? p3.Value : P3;
            var newP4 = p4.HasValue ? p4.Value : P4;
            var newColor = color.HasValue ? color.Value : Color;

            if (newP1 == P1 &&
                newP2 == P2 &&
                newP3 == P3 &&
                newP4 == P4 &&
                newColor == Color)
            {
                // no change
                return this;
            }

            return new PrimitiveBezier(newP1, newP2, newP3, newP4, newColor);
        }

        public static PrimitiveBezier FromPoints(IList<Point> controlPoints, int startIndex, int pointCount)
        {
            if (pointCount != 4)
            {
                throw new NotImplementedException("Only cubic Bezier curves of 4 points are supported.");
            }

            return new PrimitiveBezier(controlPoints[startIndex], controlPoints[startIndex + 1], controlPoints[startIndex + 2], controlPoints[startIndex + 3]);
        }

        /// <summary>
        /// Approximate the given Bezier curve as lines and arcs.
        /// </summary>
        internal static IPrimitive[] AsSimplePrimitives(PrimitiveBezier bezier)
        {
            var simplePrimitives = new List<IPrimitive>();
            var intervals = new Queue<Tuple<double, double, int>>();
            intervals.Enqueue(Tuple.Create(0.0, 1.0, 0));
            while (intervals.Count > 0)
            {
                var interval = intervals.Dequeue();
                var intervalStart = interval.Item1;
                var intervalEnd = interval.Item2;
                var currentSplitCount = interval.Item3;
                var intervalSpread = intervalEnd - intervalStart;
                var intervalMid = intervalStart + (intervalSpread * 0.5);
                var pStart = bezier.ComputeParameterizedPoint(intervalStart);
                var pMid = bezier.ComputeParameterizedPoint(intervalMid);
                var pEnd = bezier.ComputeParameterizedPoint(intervalEnd);
                var intervalLine = new PrimitiveLine(pStart, pEnd);
                var candidatePrimitive = intervalLine.IsPointOnPrimitive(pMid, MathHelper.BezierEpsilon)
                    ? (IPrimitive)intervalLine
                    : PrimitiveEllipse.ThreePointArc(pStart, pMid, pEnd, idealNormal: Vector.ZAxis);
                candidatePrimitive = candidatePrimitive ?? intervalLine; // ensure it has a value

                var intervalFirstQuarter = intervalStart + (intervalSpread * 0.25);
                var intervalThirdQuarter = intervalStart + (intervalSpread * 0.75);
                var pFirstQuarter = bezier.ComputeParameterizedPoint(intervalFirstQuarter);
                var pThirdQuarter = bezier.ComputeParameterizedPoint(intervalThirdQuarter);
                if (currentSplitCount >= MaximumPrimitiveSplitCount ||
                    (candidatePrimitive.IsPointOnPrimitive(pFirstQuarter, MathHelper.BezierEpsilon) &&
                    candidatePrimitive.IsPointOnPrimitive(pThirdQuarter, MathHelper.BezierEpsilon)))
                {
                    simplePrimitives.Add(candidatePrimitive);
                }
                else
                {
                    intervals.Enqueue(Tuple.Create(intervalStart, intervalMid, currentSplitCount + 1));
                    intervals.Enqueue(Tuple.Create(intervalMid, intervalEnd, currentSplitCount + 1));
                }
            }

            return simplePrimitives.ToArray();
        }
    }
}
