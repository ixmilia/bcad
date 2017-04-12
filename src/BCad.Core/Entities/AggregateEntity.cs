// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
        private const string LocationText = "Location";
        private const string ChildrenText = "Children";
        private readonly Point location;
        private readonly ReadOnlyList<Entity> children;
        private readonly IPrimitive[] primitives;
        private readonly SnapPoint[] snapPoints;
        private readonly BoundingBox boundingBox;

        public Point Location { get { return location; } }

        public ReadOnlyList<Entity> Children { get { return children; } }

        public AggregateEntity()
            : this(Point.Origin, ReadOnlyList<Entity>.Empty())
        {
        }

        public AggregateEntity(Point location, ReadOnlyList<Entity> children, CadColor? color = null, object tag = null)
            : base(color, tag)
        {
            if (children == null)
                throw new ArgumentNullException("children");
            this.location = location;
            this.children = children;

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

        public override object GetProperty(string propertyName)
        {
            switch (propertyName)
            {
                case LocationText:
                    return Location;
                case ChildrenText:
                    return Children;
                default:
                    return base.GetProperty(propertyName);
            }
        }

        public override EntityKind Kind { get { return EntityKind.Aggregate; } }

        public override BoundingBox BoundingBox { get { return this.boundingBox; } }

        public override int PrimitiveCount { get { return this.primitives.Count(); } }

        public AggregateEntity Update(
            Optional<Point> location = default(Optional<Point>),
            ReadOnlyList<Entity> children = null,
            Optional<CadColor?> color = default(Optional<CadColor?>),
            Optional<object> tag = default(Optional<object>))
        {
            var newLocation = location.HasValue ? location.Value : this.location;
            var newChildren = children ?? this.children;
            var newColor = color.HasValue ? color.Value : this.Color;
            var newTag = tag.HasValue ? tag.Value : this.Tag;

            if (newLocation == this.location &&
                object.ReferenceEquals(newChildren, this.children) &&
                newColor == this.Color &&
                newTag == this.Tag)
            {
                return this;
            }

            return new AggregateEntity(newLocation, newChildren, newColor, newTag);
        }
    }
}
