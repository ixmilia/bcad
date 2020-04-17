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
