using System;
using System.Collections.Generic;
using System.Linq;
using BCad.Entities;
using BCad.Primitives;

namespace BCad.Extensions
{
    public static class PolylineExtensions
    {
        public static IEnumerable<IPrimitive> Union(this IEnumerable<Polyline> polylines)
        {
            return CombinePolylines(polylines, doUnion: true);
        }

        public static IEnumerable<IPrimitive> Intersect(this IEnumerable<Polyline> polylines)
        {
            return CombinePolylines(polylines, doUnion: false);
        }

        private static IEnumerable<IPrimitive> CombinePolylines(IEnumerable<Polyline> polylineCollection, bool doUnion)
        {
            if (polylineCollection == null)
            {
                throw new ArgumentNullException(nameof(polylineCollection));
            }

            // TODO: for now this only supports straight line segments
            var polylines = polylineCollection.ToList();
            if (polylines.Count <= 1)
            {
                throw new InvalidOperationException("Must be performed on 2 or more polylines");
            }

            var lines = polylines.Select(p => Tuple.Create(p, p.Points.GetLinesFromPoints().ToList())).ToList();
            var intersections = new Dictionary<IPrimitive, HashSet<Point>>();

            // intersect all polygons
            for (int i = 0; i < polylines.Count; i++)
            {
                for (int j = i + 1; j < polylines.Count; j++)
                {
                    // intersect all lines
                    var lines1 = lines[i].Item2;
                    var lines2 = lines[j].Item2;
                    for (int ii = 0; ii < lines1.Count; ii++)
                    {
                        for (int jj = 0; jj < lines2.Count; jj++)
                        {
                            var points = lines1[ii].IntersectionPoints(lines2[jj]);
                            if (points.Count() > 0)
                            {
                                if (!intersections.ContainsKey(lines1[ii]))
                                {
                                    intersections.Add(lines1[ii], new HashSet<Point>());
                                }

                                if (!intersections.ContainsKey(lines2[jj]))
                                {
                                    intersections.Add(lines2[jj], new HashSet<Point>());
                                }

                                foreach (var point in points)
                                {
                                    intersections[lines1[ii]].Add(point);
                                    intersections[lines2[jj]].Add(point);
                                }
                            }
                        }
                    }
                }
            }

            // split all lines at the intersection points and track back to their original polyline
            var allLines = new Dictionary<PrimitiveLine, Polyline>();
            foreach (var lineGroup in lines)
            {
                var polyline = lineGroup.Item1;
                var primitives = lineGroup.Item2;
                foreach (var line in primitives)
                {
                    if (intersections.ContainsKey(line))
                    {
                        var lineParts = GetLineParts(line, intersections[line]);
                        foreach (var part in lineParts)
                        {
                            allLines.Add(part, polyline);
                        }
                    }
                    else
                    {
                        allLines.Add(line, polyline);
                    }
                }
            }

            // only keep line segments that aren't contained in the other polyline
            var keptLines = new List<PrimitiveLine>();
            foreach (var kvp in allLines)
            {
                var segment = kvp.Key;
                var poly = kvp.Value;
                var contains = !doUnion;
                foreach (var container in polylines.Where(p => !ReferenceEquals(poly, p)))
                {
                    var containsPoint = container.ContainsPoint(segment.MidPoint());
                    if (doUnion)
                    {
                        contains |= containsPoint;
                    }
                    else
                    {
                        contains &= containsPoint;
                    }
                }

                if (contains != doUnion)
                {
                    keptLines.Add(segment);
                }
            }

            return keptLines;
        }

        private static IEnumerable<PrimitiveLine> GetLineParts(PrimitiveLine line, IEnumerable<Point> breakingPoints)
        {
            if (breakingPoints.Count() == 0)
            {
                return new[] { line };
            }
            else
            {
                // order the points by distance from `line.P1` then the resultant line segments are:
                //   (line.P1, orderedPoints[0])
                //   (orderedPoints[0], orderedPointsp[1])
                //   ...
                //   (orderedPoints[N - 1], line.P2)
                var orderedPoints = breakingPoints.OrderBy(p => (line.P1 - p).LengthSquared).ToList();
                var result = new List<PrimitiveLine>();
                result.Add(new PrimitiveLine(line.P1, orderedPoints.First()));

                for (int i = 0; i < orderedPoints.Count - 1; i++)
                {
                    result.Add(new PrimitiveLine(orderedPoints[i], orderedPoints[i + 1]));
                }

                result.Add(new PrimitiveLine(orderedPoints.Last(), line.P2));
                return result;
            }
        }
    }
}
