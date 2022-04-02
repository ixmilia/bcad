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

                void AddVertex(Vertex v)
                {
                    if (vertices.Count > 0 && vertices[vertices.Count - 1].Location == v.Location)
                    {
                        // if trying to add a duplicate vertex, it means there was a rogue line segment that doesn't
                        // connect on both sides
                    }
                    else
                    {
                        vertices.Add(v);
                    }
                }

                IPrimitive NextPrimitive(int currentIndex) =>
                    currentIndex < primitiveList.Count - 1
                        ? primitiveList[currentIndex + 1]
                        : null;

                var nextVertex = default(Vertex); // candidate final vertex
                for (int i = 0; i < primitiveList.Count; i++)
                {
                    var primitive = primitiveList[i];
                    primitive.DoPrimitive(
                        ellipse =>
                        {
                            Point nearPoint, farPoint;
                            var startPoint = ellipse.StartPoint();
                            var endPoint = ellipse.EndPoint();
                            var includedAngle = (ellipse.EndAngle - ellipse.StartAngle).CorrectAngleDegrees();
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
                                // align arc with next primitive
                                var nextPrimitive = NextPrimitive(i);
                                if (nextPrimitive != null)
                                {
                                    var nextStart = nextPrimitive.StartPoint();
                                    var nextEnd = nextPrimitive.EndPoint();
                                    if (endPoint.CloseTo(nextStart) || endPoint.CloseTo(nextEnd))
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
                            AddVertex(new Vertex(nearPoint, includedAngle, direction));
                            nextVertex = new Vertex(farPoint, includedAngle, direction);
                        },
                        line =>
                        {
                            if (nextVertex != null)
                            {
                                // align with previous point
                                if (nextVertex.Location.CloseTo(line.P1))
                                {
                                    // already in order
                                    AddVertex(new Vertex(line.P1));
                                    nextVertex = new Vertex(line.P2);
                                }
                                else
                                {
                                    // need to process 'backwards'
                                    AddVertex(new Vertex(line.P2));
                                    nextVertex = new Vertex(line.P1);
                                }
                            }
                            else
                            {
                                // align line with next primitive
                                var nextPrimitive = NextPrimitive(i);
                                if (nextPrimitive != null)
                                {
                                    var nextStart = nextPrimitive.StartPoint();
                                    var nextEnd = nextPrimitive.EndPoint();
                                    if (line.P2.CloseTo(nextStart) || line.P2.CloseTo(nextEnd))
                                    {
                                        AddVertex(new Vertex(line.P1));
                                        nextVertex = new Vertex(line.P2);
                                    }
                                    else
                                    {
                                        AddVertex(new Vertex(line.P2));
                                        nextVertex = new Vertex(line.P1);
                                    }
                                }
                                else
                                {
                                    // order is arbitrary
                                    AddVertex(new Vertex(line.P1));
                                    nextVertex = new Vertex(line.P2);
                                }
                            }
                        },
                        point => throw new InvalidOperationException("Can only operate on lines and arcs"),
                        text => throw new InvalidOperationException("Can only operate on lines and arcs"),
                        bezier => throw new InvalidOperationException("Can only operate on lines and arcs"),
                        image => throw new InvalidOperationException("Can only operate on lines and arcs")
                    );
                }

                if (nextVertex != null)
                {
                    AddVertex(nextVertex);
                }

                var poly = new Polyline(vertices);
                result.Add(poly);
            }

            return result;
        }
    }
}
