using System;
using System.Collections.Generic;
using System.Linq;
using BCad.Extensions;
using BCad.Helpers;
using BCad.Primitives;
using BCad.SnapPoints;

namespace BCad.Entities
{
    public class Polyline : Entity
    {
        private readonly List<SnapPoint> _snapPoints;
        private readonly List<IPrimitive> _primitives;

        public IEnumerable<Vertex> Vertices { get; }

        public override EntityKind Kind => EntityKind.Polyline;

        public override int PrimitiveCount => _primitives.Count;

        public override BoundingBox BoundingBox { get; }

        public Polyline(IEnumerable<Vertex> vertices, CadColor? color, object tag = null)
            : base(color, tag)
        {
            var vertexList = new List<Vertex>(vertices); // to prevent backing changes
            Vertices = vertexList;

            if (vertexList.Count < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(vertices));
            }

            // walk all vertices and compute primitives and snap points
            _primitives = new List<IPrimitive>();
            _snapPoints = new List<SnapPoint>();

            var last = vertexList[0];
            _snapPoints.Add(new EndPoint(last.Location));
            for (int i = 1; i < vertexList.Count; i++)
            {
                var current = vertexList[i];
                IPrimitive primitive;
                if (current.IsLine)
                {
                    primitive = new PrimitiveLine(last.Location, current.Location);
                }
                else
                {
                    primitive = PrimitiveEllipse.ArcFromPointsAndIncludedAngle(last.Location, current.Location, current.IncludedAngle, current.Direction);
                }

                _primitives.Add(primitive);
                _snapPoints.Add(new MidPoint(primitive.MidPoint()));
                _snapPoints.Add(new EndPoint(current.Location));

                last = current;
            }

            BoundingBox = BoundingBox.FromPoints(_snapPoints.Select(sp => sp.Point).ToArray());
        }

        public override IEnumerable<IPrimitive> GetPrimitives()
        {
            return _primitives;
        }

        public override IEnumerable<SnapPoint> GetSnapPoints()
        {
            return _snapPoints;
        }

        public override object GetProperty(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(Vertices):
                    return Vertices;
                default:
                    return base.GetProperty(propertyName);
            }
        }

        public Polyline Update(
            IEnumerable<Vertex> vertices = null,
            Optional<CadColor?> color = default(Optional<CadColor?>),
            Optional<object> tag = default(Optional<object>))
        {
            var newVectices = vertices ?? Vertices;
            var newColor = color.HasValue ? color.Value : Color;
            var newTag = tag.HasValue ? tag.Value : Tag;

            if (object.ReferenceEquals(newVectices, Vertices) &&
                newColor == Color &&
                newTag == Tag)
            {
                return this;
            }

            return new Polyline(newVectices, newColor, newTag);
        }
    }
}
