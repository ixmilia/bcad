using System;
using System.Collections.Generic;
using System.Linq;
using IxMilia.BCad.Primitives;
using IxMilia.BCad.SnapPoints;

namespace IxMilia.BCad.Entities
{
    public class Spline : Entity
    {
        private readonly PrimitiveBezier[] _beziers;
        private readonly SnapPoint[] _snapPoints;

        public int Degree { get; }
        public Point[] ControlPoints { get; }
        public double[] KnotValues { get; }

        public override EntityKind Kind => EntityKind.Spline;

        public override BoundingBox BoundingBox { get; }

        public Spline(int degree, IEnumerable<Point> controlPoints, IEnumerable<double> knotValues, CadColor? color = null, object tag = null)
            : base(color, tag)
        {
            if (controlPoints == null)
            {
                throw new ArgumentNullException(nameof(controlPoints));
            }

            if (knotValues == null)
            {
                throw new ArgumentNullException(nameof(knotValues));
            }

            if (degree != 3)
            {
                throw new NotImplementedException("Only cubic splines are currently supported.");
            }

            Degree = degree;
            ControlPoints = controlPoints.ToArray();
            KnotValues = knotValues.ToArray();

            if (KnotValues.Length != ControlPoints.Length + Degree + 1)
            {
                throw new InvalidOperationException("The number of knot values must be one greater than the sum of the number of control points and the degree of the curve.");
            }

            _snapPoints = new[]
            {
                new EndPoint(ControlPoints[0]),
                new EndPoint(ControlPoints[ControlPoints.Length - 1])
            };

            _beziers = SplineBuilder.SplineToBezierCurves(this).ToArray();
            BoundingBox = BoundingBox.Includes(_beziers.Select(b => b.GetBoundingBox()));
        }

        public override IEnumerable<IPrimitive> GetPrimitives()
        {
            return _beziers;
        }

        public override IEnumerable<SnapPoint> GetSnapPoints()
        {
            return _snapPoints;
        }

        public Spline Update(
            Optional<int> degree = default(Optional<int>),
            IEnumerable<Point> controlPoints = null,
            IEnumerable<double> knotValues = null,
            Optional<CadColor?> color = default(Optional<CadColor?>),
            Optional<object> tag = default(Optional<object>))
        {
            var newDegree = degree.HasValue ? degree.Value : Degree;
            var newControlPoints = controlPoints ?? ControlPoints;
            var newKnotValues = knotValues ?? KnotValues;
            var newColor = color.HasValue ? color.Value : Color;
            var newTag = tag.HasValue ? tag.Value : Tag;

            if (newDegree == Degree &&
                ReferenceEquals(newControlPoints, ControlPoints) &&
                ReferenceEquals(newKnotValues, KnotValues) &&
                newColor == Color &&
                newTag == Tag)
            {
                return this;
            }

            return new Spline(newDegree, newControlPoints, newKnotValues, newColor, newTag);
        }

        public override string ToString()
        {
            return $"Spline: primitives=[{string.Join(", ", GetPrimitives())}], color={Color}";
        }

        public static Spline FromBezier(PrimitiveBezier bezier)
        {
            return FromBeziers(new[] { bezier });
        }

        public static Spline FromBeziers(IEnumerable<PrimitiveBezier> beziers)
        {
            if (beziers == null)
            {
                throw new ArgumentNullException(nameof(beziers));
            }

            var degree = 3;
            var bezierList = beziers.ToList();

            if (bezierList.Count == 0)
            {
                throw new InvalidOperationException("At least one Bezier curve must be specified.");
            }

            var controlPoints = new List<Point>();
            var knotValues = new List<double>();
            var knotStep = 1.0 / bezierList.Count;
            var knotValue = 0.0;
            foreach (var bezier in bezierList)
            {
                if (bezier == null)
                {
                    throw new InvalidOperationException("Curve cannot be null.");
                }

                controlPoints.AddRange(new[] { bezier.P1, bezier.P2, bezier.P3, bezier.P4 });
                knotValues.AddRange(Enumerable.Repeat(knotValue, 4));

                knotValue += knotStep;
            }

            knotValues.AddRange(Enumerable.Repeat(1.0, 4));

            return new Spline(degree, controlPoints, knotValues);
        }
    }
}
