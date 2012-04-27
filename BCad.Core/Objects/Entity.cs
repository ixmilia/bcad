using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.SnapPoints;

namespace BCad.Objects
{
    public abstract class Entity
    {
        public abstract IEnumerable<IPrimitive> GetPrimitives();
        public abstract IEnumerable<SnapPoint> GetSnapPoints();

        private static uint nextId = 1;

        public uint Id { get; private set; }

        public Entity()
        {
            this.Id = nextId++;
        }

        public abstract EntityKind Kind { get; }

        public override int GetHashCode()
        {
            return (int)this.Id;
        }
    }
}
