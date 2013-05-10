using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BCad.Helpers;
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
        private readonly double width;
        private readonly double rotation;
        private readonly Color color;
        private readonly BoundingBox boundingBox;

        public string Value { get { return this.value; } }

        public Point Location { get { return this.location; } }

        public Vector Normal { get { return this.normal; } }

        public double Height { get { return this.height; } }

        public double Width { get { return this.width; } }

        public double Rotation { get { return this.rotation; } }

        public Color Color { get { return this.color; } }

        public Text(string value, Point location, Vector normal, double height, double rotation, Color color)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            this.value = value;
            this.location = location;
            this.normal = normal;
            this.height = height;
            this.rotation = rotation;
            this.color = color;

            var textPrimitive = new PrimitiveText(value, location, height, normal, rotation, color);

            primitives = new[] { textPrimitive };
            snapPoints = new[] { new EndPoint(location) };

            this.width = textPrimitive.Width;
            var rad = this.rotation * MathHelper.DegreesToRadians;
            var right = new Vector(Math.Cos(rad), Math.Sin(rad), 0.0).Normalize() * width;
            var up = normal.Cross(right).Normalize() * this.height;
            boundingBox = BoundingBox.FromPoints(
                this.location,
                this.location + right,
                this.location + up,
                this.location + right + up);
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

        public override BoundingBox BoundingBox { get { return this.boundingBox; } }

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
