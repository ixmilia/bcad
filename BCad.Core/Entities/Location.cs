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

        public Location(Point location, IndexedColor color)
            : base(color)
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

        public Location Update(Point point = null, IndexedColor? color = null)
        {
            return new Location(
                point ?? this.Point,
                color ?? this.Color)
            {
                Tag = this.Tag
            };
        }
    }
}
