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

        private readonly IndexedColor color;
        public IndexedColor Color { get { return color; } }

        protected const string KindText = "Kind";
        protected const string BoundingBoxText = "BoundingBox";
        protected const string IdText = "Id";
        protected const string ColorText = "Color";
        protected const string CenterText = "Center";
        protected const string NormalText = "Normal";
        protected const string RadiusText = "Radius";
        protected const string StartAngleText = "StartAngle";
        protected const string EndAngleText = "EndAngle";

        protected Entity(IndexedColor color)
        {
            this.color = color;
            this.Id = nextId++;
        }

        public virtual object GetProperty(string propertyName)
        {
            switch (propertyName)
            {
                case KindText:
                    return Kind;
                case BoundingBoxText:
                    return BoundingBox;
                case ColorText:
                    return Color;
                case IdText:
                    return Id;
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
