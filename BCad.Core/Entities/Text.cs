using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.Primitives;
using BCad.SnapPoints;

namespace BCad.Entities
{
    public class Text : Entity
    {
        private readonly IPrimitive[] primitives;
        private readonly SnapPoint[] snapPoints;

        private readonly string value;
        private readonly Point location;
        private readonly Vector normal;
        private readonly double height;
        private readonly double rotation;
        private readonly Color color;

        public string Value { get { return this.value; } }

        public Point Location { get { return this.location; } }

        public Vector Normal { get { return this.normal; } }

        public double Height { get { return this.height; } }

        public double Rotation { get { return this.rotation; } }

        public Color Color { get { return this.color; } }

        public Text(string value, Point location, Vector normal, double height, double rotation, Color color)
        {
            this.value = value;
            this.location = location;
            this.normal = normal;
            this.height = height;
            this.rotation = rotation;
            this.color = color;

            primitives = new[] { new PrimitiveText(value, location, height, normal, rotation, color) };
            snapPoints = new[] { new EndPoint(location) };
        }

        public override IEnumerable<IPrimitive> GetPrimitives()
        {
            return this.primitives;
        }

        public override IEnumerable<SnapPoint> GetSnapPoints()
        {
            return this.snapPoints;
        }

        public override EntityKind Kind { get { return EntityKind.Text; } }

        public override BoundingBox BoundingBox
        {
            get
            {
                return new BoundingBox();
            }
        }

        public Text Update(string value = null, Point location = null, Vector normal = null, double? height = null, double? rotation = null, Color? color = null)
        {
            return new Text(
                value ?? this.Value,
                location ?? this.Location,
                normal ?? this.Normal,
                height ?? this.Height,
                rotation ?? this.Rotation,
                color ?? this.Color);
        }
    }
}
