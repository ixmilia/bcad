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
                var vertices = new List<Vertex>();
                var nextVertex = default(Vertex); // candidate final vertex
                foreach (var primitive in primitiveCollection)
                {
                    switch (primitive)
                    {
                        case PrimitiveLine line:
                            vertices.Add(new Vertex(line.P1));
                            nextVertex = new Vertex(line.P2);
                            break;
                        case PrimitiveEllipse el:
                            var startPoint = el.StartPoint();
                            var endPoint = el.EndPoint();
                            var includedAngle = (el.EndAngle - el.StartAngle).CorrectAngleDegrees();
                            var direction = VertexDirection.CounterClockwise;
                            vertices.Add(new Vertex(startPoint, includedAngle, direction));
                            nextVertex = new Vertex(endPoint, includedAngle, direction);
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
