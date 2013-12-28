using System.Collections.Generic;
using BCad.Primitives;
using BCad.SnapPoints;

namespace BCad.Entities
{
    public class Location : Entity
    {
        private readonly Point location;
        private readonly IndexedColor color;
        private readonly IPrimitive[] primitives;
        private readonly SnapPoint[] snapPoints;
        private readonly BoundingBox boundingBox;

        public Point Point { get { return location; } }

        public IndexedColor Color { get { return color; } }

        public override EntityKind Kind { get { return EntityKind.Location; } }

        public override BoundingBox BoundingBox { get { return this.boundingBox; } }

        public Location(Point location, IndexedColor color)
        {
            this.location = location;
            this.color = color;
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
    }
}
