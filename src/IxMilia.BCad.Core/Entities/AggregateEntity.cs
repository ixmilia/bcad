using System;
using System.Collections.Generic;
using System.Linq;
using IxMilia.BCad.Collections;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Primitives;
using IxMilia.BCad.SnapPoints;

namespace IxMilia.BCad.Entities
{
    public class AggregateEntity : Entity
    {
        private readonly Point location;
        private readonly ReadOnlyList<Entity> children;
        private readonly SnapPoint[] snapPoints;
        private readonly BoundingBox boundingBox;

        public Point Location { get { return location; } }

        public ReadOnlyList<Entity> Children { get { return children; } }

        public AggregateEntity()
            : this(Point.Origin, ReadOnlyList<Entity>.Empty())
        {
        }

        public AggregateEntity(Point location, ReadOnlyList<Entity> children, CadColor? color = null, LineTypeSpecification lineTypeSpecification = null, object tag = null)
            : base(color, lineTypeSpecification, tag)
        {
            if (children == null)
                throw new ArgumentNullException("children");
            this.location = location;
            this.children = children;

            if (children.Any(c => c.Kind == EntityKind.Aggregate))
                throw new ArgumentOutOfRangeException("children", "Aggregate entities cannot contain other aggregate entities");
            var offset = (Vector)location;
            this.snapPoints = children.SelectMany(c => c.GetSnapPoints().Select(p => p.Move(offset))).ToArray();
            this.boundingBox = BoundingBox.Includes(children.Select(c => c.BoundingBox));
        }

        public override IEnumerable<IPrimitive> GetPrimitives(DrawingSettings settings)
        {
            return GetOrCreatePrimitives(settings, () =>
            {
                var offset = (Vector)Location;
                var primitives = children.SelectMany(c => c.GetPrimitives(settings).Select(p => p.Move(offset))).ToArray();
                return primitives;
            });
        }

        public override IEnumerable<SnapPoint> GetSnapPoints()
        {
            return this.snapPoints;
        }

        public override EntityKind Kind { get { return EntityKind.Aggregate; } }

        public override BoundingBox BoundingBox { get { return this.boundingBox; } }

        public AggregateEntity Update(
            Optional<Point> location = default,
            ReadOnlyList<Entity> children = null,
            Optional<CadColor?> color = default,
            Optional<LineTypeSpecification> lineTypeSpecification = default,
            Optional<object> tag = default)
        {
            var newLocation = location.HasValue ? location.Value : this.location;
            var newChildren = children ?? this.children;
            var newColor = color.HasValue ? color.Value : this.Color;
            var newLineTypeSpecification = lineTypeSpecification.HasValue ? lineTypeSpecification.Value : LineTypeSpecification;
            var newTag = tag.HasValue ? tag.Value : this.Tag;

            if (newLocation == this.location &&
                ReferenceEquals(newChildren, this.children) &&
                newColor == Color &&
                newLineTypeSpecification == LineTypeSpecification &&
                newTag == Tag)
            {
                return this;
            }

            return new AggregateEntity(newLocation, newChildren, newColor, newLineTypeSpecification, newTag);
        }
    }
}
