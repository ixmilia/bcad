using System.Collections.Generic;
using BCad.Primitives;
using BCad.SnapPoints;

namespace BCad.Entities
{
    public abstract class Entity
    {
        public abstract IEnumerable<IPrimitive> GetPrimitives();
        public abstract IEnumerable<SnapPoint> GetSnapPoints();
        public abstract EntityKind Kind { get; }
        public abstract BoundingBox BoundingBox { get; }
        public virtual int PrimitiveCount { get { return 1; } }

        private readonly CadColor? color;
        private readonly object tag;

        public CadColor? Color { get { return color; } }
        public object Tag { get { return tag; } }

        protected Entity(CadColor? color, object tag)
        {
            this.color = color;
            this.tag = tag;
            this.Id = nextId++;
        }

        public virtual object GetProperty(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(Kind):
                    return Kind;
                case nameof(BoundingBox):
                    return BoundingBox;
                case nameof(Color):
                    return Color;
                case nameof(Id):
                    return Id;
                case nameof(Text):
                    return Tag;
                default:
                    throw new KeyNotFoundException("The property could not be found");
            }
        }

        private static uint nextId = 1;

        public uint Id { get; private set; }

        public override int GetHashCode()
        {
            return (int)this.Id;
        }
    }
}
