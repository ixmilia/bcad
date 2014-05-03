using System.Collections.Generic;
using BCad.Primitives;
using BCad.SnapPoints;

namespace BCad.Entities
{
    public class Location : Entity
    {
        private const string PointText = "Point";
        private readonly Point location;
        private readonly IPrimitive[] primitives;
        private readonly SnapPoint[] snapPoints;
        private readonly BoundingBox boundingBox;

        public Point Point { get { return location; } }

        public override EntityKind Kind { get { return EntityKind.Location; } }

        public override BoundingBox BoundingBox { get { return this.boundingBox; } }

        public Location(Point location, IndexedColor color, object tag = null)
            : base(color, tag)
        {
            this.location = location;
            this.snapPoints = new[]
            {
                new EndPoint(location)
            };
            this.primitives = new[]
            {
                new PrimitivePoint(location, color)
            };
            this.boundingBox = new BoundingBox(location, Vector.Zero);
        }

        public override IEnumerable<IPrimitive> GetPrimitives()
        {
            return this.primitives;
        }

        public override IEnumerable<SnapPoint> GetSnapPoints()
        {
            return this.snapPoints;
        }

        public override object GetProperty(string propertyName)
        {
            switch (propertyName)
            {
                case PointText:
                    return Point;
                default:
                    return base.GetProperty(propertyName);
            }
        }

        public Location Update(
            Optional<Point> point = default(Optional<Point>),
            Optional<IndexedColor> color = default(Optional<IndexedColor>),
            Optional<object> tag = default(Optional<object>))
        {
            var newPoint = point.HasValue ? point.Value : this.Point;
            var newColor = color.HasValue ? color.Value : this.Color;
            var newTag = tag.HasValue ? tag.Value : this.Tag;

            if (newPoint == this.Point &&
                newColor == this.Color &&
                newTag == this.Tag)
            {
                return this;
            }

            return new Location(newPoint, newColor, newTag);
        }

        public override string ToString()
        {
            return string.Format("Location: point={0}, color={1}", Point, Color);
        }
    }
}
