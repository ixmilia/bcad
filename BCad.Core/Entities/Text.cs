using System;
using System.Collections.Generic;
using BCad.Helpers;
using BCad.Primitives;
using BCad.SnapPoints;

namespace BCad.Entities
{
    public class Text : Entity
    {
        private const string ValueText = "Value";
        private const string LocationText = "Location";
        private const string HeightText = "Height";
        private const string WidthText = "Width";
        private const string RotationText = "Rotation";
        private readonly IPrimitive[] primitives;
        private readonly SnapPoint[] snapPoints;

        private readonly string value;
        private readonly Point location;
        private readonly Vector normal;
        private readonly double height;
        private readonly double width;
        private readonly double rotation;
        private readonly BoundingBox boundingBox;

        public string Value { get { return this.value; } }

        public Point Location { get { return this.location; } }

        public Vector Normal { get { return this.normal; } }

        public double Height { get { return this.height; } }

        public double Width { get { return this.width; } }

        public double Rotation { get { return this.rotation; } }

        public Text(string value, Point location, Vector normal, double height, double rotation, IndexedColor color)
            : base(color)
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

        public override object GetProperty(string propertyName)
        {
            switch (propertyName)
            {
                case ValueText:
                    return Value;
                case LocationText:
                    return Location;
                case NormalText:
                    return Normal;
                case HeightText:
                    return Height;
                case WidthText:
                    return Width;
                case RotationText:
                    return Rotation;
                default:
                    return base.GetProperty(propertyName);
            }
        }

        public override EntityKind Kind { get { return EntityKind.Text; } }

        public override BoundingBox BoundingBox { get { return this.boundingBox; } }

        public Text Update(
            string value = null,
            Optional<Point> location = default(Optional<Point>),
            Optional<Vector> normal = default(Optional<Vector>),
            Optional<double> height = default(Optional<double>),
            Optional<double> rotation = default(Optional<double>),
            Optional<IndexedColor> color = default(Optional<IndexedColor>))
        {
            var newValue = value ?? this.value;
            var newLocation = location.HasValue ? location.Value : this.location;
            var newNormal = normal.HasValue ? normal.Value : this.normal;
            var newHeight = height.HasValue ? height.Value : this.height;
            var newRotation = rotation.HasValue ? rotation.Value : this.rotation;
            var newColor = color.HasValue ? color.Value : this.Color;

            if (newValue == this.value &&
                newLocation == this.location &&
                newNormal == this.normal &&
                newHeight == this.height &&
                newRotation == this.rotation &&
                newColor == this.Color)
            {
                return this;
            }

            return new Text(newValue, newLocation, newNormal, newHeight, newRotation, newColor)
            {
                Tag = this.Tag
            };
        }

        public override string ToString()
        {
            return string.Format("Text: value=\"{0}\", location={1}, normal={2}, height={3}, width={4}, rotation={5}", Value, Location, Normal, Height, Width, Color);
        }
    }
}
