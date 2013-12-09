using System;
using System.Collections.Generic;
using System.Linq;
using BCad.Primitives;

namespace BCad.Extensions
{
    public static class PrimitiveExtensions
    {
        public static IEnumerable<Point> GetProjectedVerticies(this IPrimitive primitive, Matrix4 transformationMatrix)
        {
            switch (primitive.Kind)
            {
                case PrimitiveKind.Line:
                case PrimitiveKind.Text:
                    return primitive.GetInterestingPoints().Select(p => transformationMatrix.Transform(p));
                case PrimitiveKind.Ellipse:
                    return ((PrimitiveEllipse)primitive).GetProjectedVerticies(transformationMatrix, 360);
                default:
                    throw new InvalidOperationException();
            }
        }

        public static IEnumerable<Point> GetProjectedVerticies(this PrimitiveEllipse ellipse, Matrix4 transformationMatrix, int maxSeg)
        {
            return ellipse.GetInterestingPoints(maxSeg)
                .Select(p => transformationMatrix.Transform(p));
        }
    }
}
