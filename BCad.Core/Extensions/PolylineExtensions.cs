using System.Collections.Generic;
using System.Linq;
using BCad.Entities;
using BCad.Primitives;

namespace BCad.Extensions
{
    public static class PolylineExtensions
    {
        public static IEnumerable<IPrimitive> Union(this Polyline polyline, Polyline other)
        {
            // TODO: for now this only supports straight line segments
            var intersections = new Dictionary<IPrimitive, HashSet<Point>>();
            var lines1 = polyline.Points.GetLinesFromPoints().ToList();
            var lines2 = other.Points.GetLinesFromPoints().ToList();

            // find all intersection points and group by the lines that contain them
            for (int i = 0; i < lines1.Count; i++)
            {
                for (int j = 0; j < lines2.Count; j++)
                {
                    var intersectionPoint = lines1[i].IntersectionPoint(lines2[j]);
                    if (intersectionPoint.HasValue)
                    {
                        if (!intersections.ContainsKey(lines1[i]))
                        {
                            intersections.Add(lines1[i], new HashSet<Point>());
                        }

                        if (!intersections.ContainsKey(lines2[j]))
                        {
                            intersections.Add(lines2[j], new HashSet<Point>());
                        }

                        intersections[lines1[i]].Add(intersectionPoint.GetValueOrDefault());
                        intersections[lines2[j]].Add(intersectionPoint.GetValueOrDefault());
                    }
                }
            }

            // split all lines at the intersection points and track back to their original polyline
            var allLines = new Dictionary<PrimitiveLine, Polyline>();
            foreach (var line in lines1)
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

            foreach (var line in lines2)
            {
                if (intersections.ContainsKey(line))
                {
                    var lineParts = GetLineParts(line, intersections[line]);
                    foreach (var part in lineParts)
                    {
                        allLines.Add(part, other);
                    }
                }
                else
                {
                    allLines.Add(line, other);
                }
            }

            // only keep line segments that aren't contained in the other polyline
            var keptLines = new List<PrimitiveLine>();
            foreach (var kvp in allLines)
            {
                var segment = kvp.Key;
                var poly = kvp.Value;
                var container = ReferenceEquals(poly, polyline) ? other : polyline;
                if (!container.ContainsPoint(segment.MidPoint()))
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
