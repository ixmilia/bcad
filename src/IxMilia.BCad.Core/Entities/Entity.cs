using System.Collections.Generic;
using IxMilia.BCad.Primitives;
using IxMilia.BCad.SnapPoints;

namespace IxMilia.BCad.Entities
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

        private static uint nextId = 1;

        public uint Id { get; private set; }

        public override int GetHashCode()
        {
            return (int)this.Id;
        }
    }
}
