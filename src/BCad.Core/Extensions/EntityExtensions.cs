// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using BCad.Entities;
using BCad.Helpers;
using BCad.Primitives;

namespace BCad.Extensions
{
    public static class EntityExtensions
    {
        public static bool EquivalentTo(this Arc arc, Entity entity)
        {
            if (entity is Arc other)
            {
                return arc.Center.CloseTo(other.Center)
                    && arc.Color == other.Color
                    && MathHelper.CloseTo(arc.EndAngle, other.EndAngle)
                    && arc.Normal.CloseTo(other.Normal)
                    && MathHelper.CloseTo(arc.Radius, other.Radius)
                    && MathHelper.CloseTo(arc.StartAngle, other.StartAngle)
                    && MathHelper.CloseTo(arc.Thickness, other.Thickness);
            }

            return false;
        }

        public static bool EquivalentTo(this Circle circle, Entity entity)
        {
            if (entity is Circle other)
            {
                return circle.Center.CloseTo(other.Center)
                    && circle.Color == other.Color
                    && circle.Normal.CloseTo(other.Normal)
                    && MathHelper.CloseTo(circle.Radius, other.Radius)
                    && MathHelper.CloseTo(circle.Thickness, other.Thickness);
            }

            return false;
        }

        public static bool EquivalentTo(this Ellipse el, Entity entity)
        {
            if (entity is Ellipse other)
            {
                return el.Center.CloseTo(other.Center)
                    && el.Color == other.Color
                    && MathHelper.CloseTo(el.EndAngle, other.EndAngle)
                    && el.MajorAxis.CloseTo(other.MajorAxis)
                    && MathHelper.CloseTo(el.MinorAxisRatio, other.MinorAxisRatio)
                    && el.Normal.CloseTo(other.Normal)
                    && MathHelper.CloseTo(el.StartAngle, other.StartAngle)
                    && MathHelper.CloseTo(el.Thickness, other.Thickness);
            }

            return false;
        }

        public static bool EquivalentTo(this Line line, Entity entity)
        {
            if (entity is Line other)
            {
                return line.Color == other.Color
                    && line.P1.CloseTo(other.P1)
                    && line.P2.CloseTo(other.P2)
                    && MathHelper.CloseTo(line.Thickness, other.Thickness);
            }

            return false;
        }

        public static bool EquivalentTo(this Polyline poly, Entity entity)
        {
            if (entity is Polyline other)
            {
                return poly.Color == other.Color
                    && poly.Vertices.EquivalentTo(other.Vertices);
            }

            return false;
        }

        public static bool EquivalentTo(this IEnumerable<Vertex> a, IEnumerable<Vertex> b)
        {
            var expected = a.ToList();
            var actual = b.ToList();
            if (expected.Count != actual.Count)
            {
                return false;
            }

            for (int i = 0; i < expected.Count; i++)
            {
                if (!expected[i].EquivalentTo(actual[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool EquivalentTo(this Vertex a, Vertex b)
        {
            return a.Location == b.Location
                && MathHelper.CloseTo(a.IncludedAngle, b.IncludedAngle)
                && a.Direction == b.Direction;
        }

        public static bool EquivalentTo(this Entity a, Entity b)
        {
            switch (a)
            {
                case Arc arc:
                    return arc.EquivalentTo(b);
                case Circle circle:
                    return circle.EquivalentTo(b);
                case Ellipse el:
                    return el.EquivalentTo(b);
                case Line l:
                    return l.EquivalentTo(b);
                case Polyline p:
                    return p.EquivalentTo(b);
                default:
                    throw new NotSupportedException("Unsupported entity type");
            }
        }

        public static Matrix4 GetUnitCircleProjection(this Entity entity)
        {
            Matrix4 matrix;
            switch (entity.Kind)
            {
                case EntityKind.Arc:
                    var arc = (Arc)entity;
                    matrix = arc.FromUnitCircle;
                    break;
                case EntityKind.Circle:
                    var circle = (Circle)entity;
                    matrix = circle.FromUnitCircle;
                    break;
                case EntityKind.Ellipse:
                    var el = (Ellipse)entity;
                    matrix = el.FromUnitCircle;
                    break;
                default:
                    throw new ArgumentException("entity");
            }

            return matrix;
        }

        public static bool EnclosesPoint(this Entity entity, Point point)
        {
            switch (entity.Kind)
            {
                case EntityKind.Circle:
                case EntityKind.Polyline:
                    return entity.GetPrimitives().PolygonContains(point);
                default:
                    return false;
            }
        }

        public static Point MidPoint(this Line line)
        {
            return line.GetPrimitives().Single().MidPoint();
        }

        public static Entity WithThickness(this Entity entity, double thickness)
        {
            switch (entity)
            {
                case Arc arc:
                    return arc.Update(thickness: thickness);
                case Circle circle:
                    return circle.Update(thickness: thickness);
                case Ellipse el:
                    return el.Update(thickness: thickness);
                case Line line:
                    return line.Update(thickness: thickness);
                default:
                    return entity;
            }
        }

        public static IEnumerable<Entity> Union(this IEnumerable<Entity> entities)
        {
            return CombineEntities(entities, doUnion: true);
        }

        public static IEnumerable<Entity> Intersect(this IEnumerable<Entity> entities)
        {
            return CombineEntities(entities, doUnion: false);
        }

        public static IEnumerable<Entity> Subtract(this Entity original, IEnumerable<Entity> others)
        {
            if (original.Kind != EntityKind.Circle && original.Kind != EntityKind.Polyline)
            {
                throw new ArgumentException("Original entity must be a circle or polyline");
            }

            if (!others.All(o => o.Kind == EntityKind.Circle || o.Kind == EntityKind.Polyline))
            {
                throw new ArgumentException("Other entities must be circles or polylines");
            }

            var all = new[] { original }.Concat(others);
            var allLines = all.PerformAllIntersections();

            var keptLines = new List<IPrimitive>();
            foreach (var kvp in allLines)
            {
                var segment = kvp.Key;
                var container = kvp.Value;
                var keep = true;
                if (ReferenceEquals(container, original))
                {
                    // if we're testing a line from the parent polyline, keep if not in any of the others
                    keep = others.All(o => !o.EnclosesPoint(segment.MidPoint()));
                }
                else
                {
                    // if we're testing a line from a subsequent polyline, keep if in the original
                    keep = original.EnclosesPoint(segment.MidPoint());
                }

                if (keep)
                {
                    keptLines.Add(segment);
                }
            }

            return keptLines.GetPolylinesFromSegments();
        }

        private static IEnumerable<Entity> CombineEntities(IEnumerable<Entity> entityCollection, bool doUnion)
        {
            var allSegments = PerformAllIntersections(entityCollection);

            // only keep segments that aren't contained in the other entity
            var keptSegments = new List<IPrimitive>();
            foreach (var kvp in allSegments)
            {
                var segment = kvp.Key;
                var poly = kvp.Value;
                var contains = !doUnion;
                foreach (var container in entityCollection.Where(p => !ReferenceEquals(poly, p)))
                {
                    var containsPoint = container.EnclosesPoint(segment.MidPoint());
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
                    keptSegments.Add(segment);
                }
            }

            return keptSegments.GetPolylinesFromSegments();
        }

        internal static Dictionary<IPrimitive, Entity> PerformAllIntersections(this IEnumerable<Entity> entityCollection)
        {
            if (entityCollection == null)
            {
                throw new ArgumentNullException(nameof(entityCollection));
            }

            var entities = entityCollection.ToList();
            if (entities.Count <= 1)
            {
                throw new InvalidOperationException("Must be performed on 2 or more entities");
            }

            var segments = entities.Select(e => Tuple.Create(e, e.GetPrimitives().ToList())).ToList();
            var intersections = new Dictionary<IPrimitive, HashSet<Point>>();

            // intersect all polygons
            for (int i = 0; i < entities.Count; i++)
            {
                for (int j = i + 1; j < entities.Count; j++)
                {
                    // intersect all segments
                    var segments1 = segments[i].Item2;
                    var segments2 = segments[j].Item2;
                    for (int ii = 0; ii < segments1.Count; ii++)
                    {
                        for (int jj = 0; jj < segments2.Count; jj++)
                        {
                            var points = segments1[ii].IntersectionPoints(segments2[jj]);
                            if (points.Count() > 0)
                            {
                                if (!intersections.ContainsKey(segments1[ii]))
                                {
                                    intersections.Add(segments1[ii], new HashSet<Point>());
                                }

                                if (!intersections.ContainsKey(segments2[jj]))
                                {
                                    intersections.Add(segments2[jj], new HashSet<Point>());
                                }

                                foreach (var point in points)
                                {
                                    intersections[segments1[ii]].Add(point);
                                    intersections[segments2[jj]].Add(point);
                                }
                            }
                        }
                    }
                }
            }

            // split all segments at the intersection points and track back to their original entity
            var allSegments = new Dictionary<IPrimitive, Entity>();
            foreach (var segmentGroup in segments)
            {
                var entity = segmentGroup.Item1;
                var primitives = segmentGroup.Item2;
                foreach (var primitive in primitives)
                {
                    if (intersections.ContainsKey(primitive))
                    {
                        var segmentParts = GetSegmentParts(primitive, intersections[primitive]);
                        foreach (var part in segmentParts)
                        {
                            allSegments.Add(part, entity);
                        }
                    }
                    else
                    {
                        allSegments.Add(primitive, entity);
                    }
                }
            }

            return allSegments;
        }

        private static IEnumerable<IPrimitive> GetSegmentParts(IPrimitive primitive, IEnumerable<Point> breakingPoints)
        {
            if (breakingPoints.Count() == 0)
            {
                return new[] { primitive };
            }
            else
            {
                var result = new List<IPrimitive>();
                switch (primitive.Kind)
                {
                    case PrimitiveKind.Line:
                        {
                            // order the points by distance from `line.P1` then the resultant line segments are:
                            //   (line.P1, orderedPoints[0])
                            //   (orderedPoints[0], orderedPoints[1])
                            //   ...
                            //   (orderedPoints[N - 1], line.P2)
                            var line = (PrimitiveLine)primitive;
                            var orderedPoints = breakingPoints.OrderBy(p => (line.P1 - p).LengthSquared).ToList();
                            result.Add(new PrimitiveLine(line.P1, orderedPoints.First()));

                            for (int i = 0; i < orderedPoints.Count - 1; i++)
                            {
                                result.Add(new PrimitiveLine(orderedPoints[i], orderedPoints[i + 1]));
                            }

                            result.Add(new PrimitiveLine(orderedPoints.Last(), line.P2));
                        }
                        break;
                    case PrimitiveKind.Ellipse:
                        {
                            // order the points by angle from `el.StartAngle` then the resultant arc segments are:
                            //   (arc.StartAngle, orderedAngles[0])
                            //   (orderedAngles[0], orderedAngles[1])
                            //   ...
                            //   (orderedAngles[N - 1], arc.EndAngle)
                            // but if it's closed, don't use the start/end angles, instead do:
                            //   (orderedAngles[0], orderedAngles[1])
                            //   (orderedAngles[1], orderedAngles[2])
                            //   ...
                            //   (orderedAngles[N - 1], orderedAngles[0])

                            var el = (PrimitiveEllipse)primitive;
                            var orderedAngles = breakingPoints.Select(p => el.GetAngle(p)).OrderBy(a => a).ToList();

                            if (!el.IsClosed)
                            {
                                result.Add(new PrimitiveEllipse(el.Center, el.MajorAxis, el.Normal, el.MinorAxisRatio, el.StartAngle, orderedAngles.First(), el.Color));
                            }

                            for (int i = 0; i < orderedAngles.Count - 1; i++)
                            {
                                result.Add(new PrimitiveEllipse(el.Center, el.MajorAxis, el.Normal, el.MinorAxisRatio, orderedAngles[i], orderedAngles[i + 1], el.Color));
                            }

                            if (el.IsClosed)
                            {
                                result.Add(new PrimitiveEllipse(el.Center, el.MajorAxis, el.Normal, el.MinorAxisRatio, orderedAngles.Last(), orderedAngles.First(), el.Color));
                            }
                            else
                            {
                                result.Add(new PrimitiveEllipse(el.Center, el.MajorAxis, el.Normal, el.MinorAxisRatio, orderedAngles.Last(), el.EndAngle, el.Color));
                            }
                        }
                        break;
                }

                return result;
            }
        }
    }
}
