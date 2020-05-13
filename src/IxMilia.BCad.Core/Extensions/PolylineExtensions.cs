using System;
using System.Collections.Generic;
using System.Linq;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Helpers;
using IxMilia.BCad.Primitives;

namespace IxMilia.BCad.Extensions
{
    public static class PolylineExtensions
    {
        public static IEnumerable<Polyline> GetPolylinesFromPrimitives(this IEnumerable<IEnumerable<IPrimitive>> primitiveCollections)
        {
            var result = new List<Polyline>();
            foreach (var primitiveCollection in primitiveCollections)
            {
                var primitiveList = primitiveCollection.ToList();
                var vertices = new List<Vertex>();
                var nextVertex = default(Vertex); // candidate final vertex
                for (int i = 0; i < primitiveList.Count; i++)
                {
                    var primitive = primitiveList[i];
                    switch (primitive)
                    {
                        case PrimitiveLine line:
                            if (nextVertex != null)
                            {
                                // align with previous point
                                if (nextVertex.Location.CloseTo(line.P1))
                                {
                                    // already in order
                                    vertices.Add(new Vertex(line.P1));
                                    nextVertex = new Vertex(line.P2);
                                }
                                else
                                {
                                    // need to process 'backwards'
                                    vertices.Add(new Vertex(line.P2));
                                    nextVertex = new Vertex(line.P1);
                                }
                            }
                            else
                            {
                                // order is arbitrary
                                vertices.Add(new Vertex(line.P1));
                                nextVertex = new Vertex(line.P2);
                            }
                            break;
                        case PrimitiveEllipse el:
                            Point nearPoint, farPoint;
                            var startPoint = el.StartPoint();
                            var endPoint = el.EndPoint();
                            var includedAngle = (el.EndAngle - el.StartAngle).CorrectAngleDegrees();
                            if (nextVertex != null)
                            {
                                // align with previous point
                                if (nextVertex.Location.CloseTo(startPoint))
                                {
                                    // already in order
                                    nearPoint = startPoint;
                                    farPoint = endPoint;
                                }
                                else
                                {
                                    // need to process 'backwards'
                                    nearPoint = endPoint;
                                    farPoint = startPoint;
                                }
                            }
                            else
                            {
                                // align arc with next line primitive
                                var nextLine = primitiveList.Skip(i + 1).OfType<PrimitiveLine>().FirstOrDefault();
                                if (nextLine != null)
                                {
                                    if (endPoint.CloseTo(nextLine.P1) || endPoint.CloseTo(nextLine.P2))
                                    {
                                        nearPoint = startPoint;
                                        farPoint = endPoint;
                                    }
                                    else
                                    {
                                        nearPoint = endPoint;
                                        farPoint = startPoint;
                                    }
                                }
                                else
                                {
                                    // no line, only arcs; just guess
                                    nearPoint = startPoint;
                                    farPoint = endPoint;
                                }
                            }

                            var direction = nearPoint == startPoint
                                ? VertexDirection.CounterClockwise
                                : VertexDirection.Clockwise;
                            vertices.Add(new Vertex(nearPoint, includedAngle, direction));
                            nextVertex = new Vertex(farPoint, includedAngle, direction);
                            break;
                        default:
                            throw new InvalidOperationException("Can only operate on lines and arcs");
                    }
                }

                if (nextVertex != null)
                {
                    vertices.Add(nextVertex);
                }

                var poly = new Polyline(vertices);
                result.Add(poly);
            }

            return result;
        }
    }
}
