// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
                vertices.Add(new Vertex(primitiveCollection.First().StartPoint()));
                foreach (var primitive in primitiveCollection)
                {
                    switch (primitive.Kind)
                    {
                        case PrimitiveKind.Line:
                            var line = (PrimitiveLine)primitive;
                            vertices.Add(new Vertex(line.P2));
                            break;
                        case PrimitiveKind.Ellipse:
                            var el = (PrimitiveEllipse)primitive;
                            var startPoint = el.StartPoint().CloseTo(vertices.Last().Location)
                                ? el.StartPoint()
                                : el.EndPoint();
                            var otherPoint = el.StartPoint().CloseTo(vertices.Last().Location)
                                ? el.EndPoint()
                                : el.StartPoint();
                            var includedAngle = (el.EndAngle - el.StartAngle).CorrectAngleDegrees();
                            var direction = startPoint.CloseTo(vertices.Last().Location)
                                ? VertexDirection.CounterClockwise
                                : VertexDirection.Clockwise;
                            vertices.Add(new Vertex(otherPoint, includedAngle, direction));
                            break;
                        default:
                            throw new InvalidOperationException("Can only operate on lines and arcs");
                    }
                }

                var poly = new Polyline(vertices);
                result.Add(poly);
            }

            return result;
        }
    }
}
