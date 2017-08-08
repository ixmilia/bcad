// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace IxMilia.BCad.Entities
{
    public class ProjectedAggregate : ProjectedEntity
    {
        public override EntityKind Kind
        {
            get { return EntityKind.Aggregate; }
        }

        public AggregateEntity Original { get; private set; }

        public Point Location { get; private set; }

        public IEnumerable<ProjectedEntity> Children { get; private set; }

        public ProjectedAggregate(AggregateEntity original, Layer layer, Point location, IEnumerable<ProjectedEntity> children)
            : base(layer)
        {
            Original = original;
            Location = location;
            Children = children;
        }
    }
}
