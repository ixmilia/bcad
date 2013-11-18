using System;
using System.Collections.Generic;
using System.Linq;
using BCad.Collections;
using BCad.Extensions;
using BCad.Primitives;
using BCad.SnapPoints;

namespace BCad.Entities
{
    public class AggregateEntity : Entity
    {
        private readonly Point location;
        private readonly ReadOnlyList<Entity> children;
        private readonly IndexedColor color;
        private readonly IPrimitive[] primitives;
        private readonly SnapPoint[] snapPoints;
        private readonly BoundingBox boundingBox;

        public Point Location { get { return location; } }

        public ReadOnlyList<Entity> Children { get { return children; } }

        public IndexedColor Color { get { return color; } }

        public AggregateEntity()
            : this(Point.Origin, ReadOnlyList<Entity>.Empty(), IndexedColor.Auto)
        {
        }

        public AggregateEntity(Point location, ReadOnlyList<Entity> children, IndexedColor color)
        {
            if (location == null)
                throw new ArgumentNullException("location");
            if (children == null)
                throw new ArgumentNullException("children");
            this.location = location;
            this.children = children;
            this.color = color;

            if (children.Any(c => c.Kind == EntityKind.Aggregate))
                throw new ArgumentOutOfRangeException("children", "Aggregate entities cannot contain other aggregate entities");
            var offset = (Vector)location;
            this.primitives = children.SelectMany(c => c.GetPrimitives().Select(p => p.Move(offset))).ToArray();
            this.snapPoints = children.SelectMany(c => c.GetSnapPoints().Select(p => p.Move(offset))).ToArray();
            this.boundingBox = BoundingBox.Includes(children.Select(c => c.BoundingBox));
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

        public AggregateEntity Update(Point location = null, ReadOnlyList<Entity> children = null, IndexedColor? color = null)
        {
            return new AggregateEntity(
                location ?? this.location,
                children ?? this.children,
                color ?? this.color);
        }
    }
}
