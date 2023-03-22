using System;
using System.Collections.Generic;
using IxMilia.BCad.Primitives;
using IxMilia.BCad.SnapPoints;

namespace IxMilia.BCad.Entities
{
    public abstract class Entity
    {
        public abstract IEnumerable<IPrimitive> GetPrimitives(DrawingSettings settings);
        public abstract IEnumerable<SnapPoint> GetSnapPoints();
        public abstract EntityKind Kind { get; }
        public abstract BoundingBox BoundingBox { get; }

        private readonly CadColor? color;
        private readonly LineTypeSpecification lineTypeSpecification;
        private readonly object tag;

        public CadColor? Color => color;
        public LineTypeSpecification LineTypeSpecification => lineTypeSpecification;
        public object Tag => tag;

        protected Entity(CadColor? color, LineTypeSpecification lineTypeSpecification, object tag)
        {
            this.color = color;
            this.lineTypeSpecification = lineTypeSpecification;
            this.tag = tag;
            this.Id = nextId++;
        }

        private static uint nextId = 1;

        public uint Id { get; private set; }

        public override int GetHashCode()
        {
            return (int)this.Id;
        }

        private object _cachedPrimitivesGate = new object();
        private DrawingSettings _cachedSettingsKey;
        private IEnumerable<IPrimitive> _cachedPrimitives;
        protected IEnumerable<IPrimitive> GetOrCreatePrimitives(DrawingSettings settings, Func<IEnumerable<IPrimitive>> creator)
        {
            lock (_cachedPrimitivesGate)
            {
                if (_cachedPrimitives is null || !ReferenceEquals(settings, _cachedSettingsKey))
                {
                    _cachedPrimitives = creator();
                    _cachedSettingsKey = settings;
                }
            }

            return _cachedPrimitives;
        }
    }
}
