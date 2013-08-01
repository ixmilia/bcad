using System;
using System.Collections.Generic;
using System.Linq;
using BCad.Collections;
using BCad.Primitives;
using BCad.SnapPoints;

namespace BCad.Entities
{
    public class AggregateEntity : Entity
    {
        private readonly Point location;
        private readonly ReadOnlyTree<uint, Entity> children;
        private readonly Color color;
        private readonly IPrimitive[] primitives;
        private readonly SnapPoint[] snapPoints;
        private readonly BoundingBox boundingBox;

        public Point Location { get { return location; } }

        public ReadOnlyTree<uint, Entity> Children { get { return children; } }

        public Color Color { get { return color; } }

        public AggregateEntity()
            : this(Point.Origin, new ReadOnlyTree<uint, Entity>(), Color.Auto)
        {
        }

        public AggregateEntity(Point location, ReadOnlyTree<uint, Entity> children, Color color)
        {
            if (location == null)
                throw new ArgumentNullException("location");
            if (children == null)
                throw new ArgumentNullException("children");
            this.location = location;
            this.children = children;
            this.color = color;

            var childList = children.GetValues();
            if (childList.Any(c => c.Kind == EntityKind.Aggregate))
                throw new ArgumentOutOfRangeException("children", "Aggregate entities cannot contain other aggregate entities");
            this.primitives = childList.SelectMany(c => c.GetPrimitives()).ToArray();
            this.snapPoints = childList.SelectMany(c => c.GetSnapPoints()).ToArray();
            this.boundingBox = BoundingBox.Includes(childList.Select(c => c.BoundingBox));
        }

        public override IEnumerable<IPrimitive> GetPrimitives()
        {
            return this.primitives;
        }

        public override IEnumerable<SnapPoint> GetSnapPoints()
        {
            return this.snapPoints;
        }

        public override EntityKind Kind { get { return EntityKind.Aggregate; } }

        public override BoundingBox BoundingBox { get { return this.boundingBox; } }

        public AggregateEntity Update(Point location = null, ReadOnlyTree<uint, Entity> children = null, Color? color = null)
        {
            return new AggregateEntity(
                location ?? this.location,
                children ?? this.children,
                color ?? this.color);
        }
    }
}
