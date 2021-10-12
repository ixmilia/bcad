using System;
using System.Collections.Generic;
using System.Linq;
using IxMilia.BCad.Primitives;

namespace IxMilia.BCad.Entities
{
    public class SplineBuilder
    {
        private List<Point> _controlPoints;
        private List<double> _knotValues;

        public int Degree { get; }
        public IEnumerable<Point> ControlPoints => _controlPoints;
        public IEnumerable<double> KnotValues => _knotValues;

        public SplineBuilder(int degree, IEnumerable<Point> controlPoints, IEnumerable<double> knotValues)
        {
            if (degree < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(degree), "Degree must be greater than or equal to 2.");
            }

            if (controlPoints == null)
            {
                throw new ArgumentNullException(nameof(controlPoints));
            }

            if (knotValues == null)
            {
                throw new ArgumentNullException(nameof(knotValues));
            }

            Degree = degree;
            _controlPoints = controlPoints.ToList();
            _knotValues = knotValues.ToList();

            ValidateValues();
        }

        private void ValidateValues()
        {
            if (_controlPoints.Count < Degree + 1)
            {
                throw new InvalidOperationException("There must be at least one more control point than the degree of the curve.");
            }

            if (_knotValues.Count < 1)
            {
                throw new InvalidOperationException("Minimum knot value count is 1.");
            }

            if (_knotValues.Count != _controlPoints.Count + Degree + 1)
            {
                throw new InvalidOperationException("Invalid combination of knot value count, control point count, and degree.");
            }

            // knot values must be ascending
            var lastKnotValue = _knotValues[0];
            foreach (var kv in _knotValues.Skip(1))
            {
                if (kv < lastKnotValue)
                {
                    throw new InvalidOperationException($"Knot values must be ascending.  Found values {lastKnotValue} -> {kv}.");
                }

                lastKnotValue = kv;
            }
        }

        public void InsertKnot(double t)
        {
            // find the knot span that contains t
            var knotInsertionIndex = _knotValues.Count(k => k < t);

            // replace points at index [k-p, k]
            // first new point is _controlPoints[index - degree]
            var lowerIndex = knotInsertionIndex - Degree;
            var upperIndex = knotInsertionIndex;
            var pointsToInsert = new List<Point>();
            for (int i = lowerIndex; i < upperIndex; i++)
            {
                var a = (t - _knotValues[i]) / (_knotValues[i + Degree] - _knotValues[i]);
                var q = _controlPoints[i - 1] * (1.0 - a) + _controlPoints[i] * a;
                pointsToInsert.Add(q);
            }

            // insert new values
            _knotValues.Insert(knotInsertionIndex, t);
            var newControlPoints = new List<Point>();
            newControlPoints.AddRange(_controlPoints.Take(lowerIndex));
            newControlPoints.AddRange(pointsToInsert);
            newControlPoints.AddRange(_controlPoints.Skip(lowerIndex + pointsToInsert.Count - 1));
            _controlPoints = newControlPoints;
            ValidateValues();
        }

        public static IEnumerable<PrimitiveBezier> SplineToBezierCurves(Spline spline)
        {
            if (spline == null)
            {
                throw new ArgumentNullException(nameof(spline));
            }

            var expectedIdenticalKnots = spline.Degree + 1;
            var builder = new SplineBuilder(spline.Degree, spline.ControlPoints, spline.KnotValues);

            for (int offset = 0; ; offset++)
            {
                // get next set of values
                var values = builder.KnotValues.Skip(offset * expectedIdenticalKnots).Take(expectedIdenticalKnots).ToList();

                if (values.Count == 0 && builder.KnotValues.Count() % expectedIdenticalKnots == 0)
                {
                    // done
                    break;
                }

                var expectedValue = values[0];
                int missingValueCount;
                if (values.Count < expectedIdenticalKnots)
                {
                    // not enough values
                    missingValueCount = expectedIdenticalKnots - values.Count;
                }
                else if (values.Count < expectedIdenticalKnots || values.Any(v => v != expectedValue))
                {
                    // not all the same
                    missingValueCount = expectedIdenticalKnots - values.Count(v => v == expectedValue);
                }
                else
                {
                    missingValueCount = 0;
                }

                for (int i = 0; i < missingValueCount; i++)
                {
                    builder.InsertKnot(expectedValue);
                }
            }

            var points = builder.ControlPoints.ToList();
            var curves = new List<PrimitiveBezier>();
            for (int startIndex = 0; startIndex < points.Count; startIndex += expectedIdenticalKnots)
            {
                var curve = PrimitiveBezier.FromPoints(points, startIndex, expectedIdenticalKnots);
                curves.Add(curve);
            }

            return curves;
        }
    }
}
