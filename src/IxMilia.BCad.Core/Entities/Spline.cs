﻿using System;
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

        public Point P1 => ControlPoints[0];
        public Point P2 => ControlPoints[1];
        public Point P3 => ControlPoints[2];
        public Point P4 => ControlPoints[3];

        public override EntityKind Kind => EntityKind.Spline;

        public override BoundingBox BoundingBox { get; }

        public Spline(int degree, IEnumerable<Point> controlPoints, IEnumerable<double> knotValues, CadColor? color = null, LineTypeSpecification lineTypeSpecification = null, object tag = null)
            : base(color, lineTypeSpecification, tag)
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

        public override IEnumerable<IPrimitive> GetPrimitives(DrawingSettings _settings) => GetPrimitives();

        private IEnumerable<IPrimitive> GetPrimitives() => _beziers;

        public override IEnumerable<SnapPoint> GetSnapPoints()
        {
            return _snapPoints;
        }

        public Spline Update(
            Optional<int> degree = default,
            IEnumerable<Point> controlPoints = null,
            IEnumerable<double> knotValues = null,
            Optional<CadColor?> color = default,
            Optional<LineTypeSpecification> lineTypeSpecification = default,
            Optional<object> tag = default)
        {
            var newDegree = degree.HasValue ? degree.Value : Degree;
            var newControlPoints = controlPoints ?? ControlPoints;
            var newKnotValues = knotValues ?? KnotValues;
            var newColor = color.HasValue ? color.Value : Color;
            var newLineTypeSpecification = lineTypeSpecification.HasValue ? lineTypeSpecification.Value : LineTypeSpecification;
            var newTag = tag.HasValue ? tag.Value : Tag;

            if (newDegree == Degree &&
                ReferenceEquals(newControlPoints, ControlPoints) &&
                ReferenceEquals(newKnotValues, KnotValues) &&
                newColor == Color &&
                newLineTypeSpecification == LineTypeSpecification &&
                newTag == Tag)
            {
                return this;
            }

            return new Spline(newDegree, newControlPoints, newKnotValues, newColor, newLineTypeSpecification, newTag);
        }

        public Spline Update(
            Optional<Point> p1 = default,
            Optional<Point> p2 = default,
            Optional<Point> p3 = default,
            Optional<Point> p4 = default)
        {
            var newP1 = p1.HasValue ? p1.Value : P1;
            var newP2 = p2.HasValue ? p2.Value : P2;
            var newP3 = p3.HasValue ? p3.Value : P3;
            var newP4 = p4.HasValue ? p4.Value : P4;

            if (newP1 == P1 &&
                newP2 == P2 &&
                newP3 == P3 &&
                newP4 == P4)
            {
                return this;
            }

            return Update(controlPoints: new[] { newP1, newP2, newP3, newP4 });
        }

        public override string ToString()
        {
            return $"Spline: primitives=[{string.Join(", ", GetPrimitives())}], color={Color}";
        }

        public static Spline FromBezier(PrimitiveBezier bezier, LineTypeSpecification lineTypeSpecification = null)
        {
            return FromBeziers(new[] { bezier }, lineTypeSpecification);
        }

        public static Spline FromBeziers(IEnumerable<PrimitiveBezier> beziers, LineTypeSpecification lineTypeSpecification = null)
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

            return new Spline(degree, controlPoints, knotValues, lineTypeSpecification: lineTypeSpecification);
        }
    }
}
