using System;
using System.Collections.Generic;
using IxMilia.BCad.Helpers;
using IxMilia.BCad.Primitives;
using IxMilia.BCad.SnapPoints;

namespace IxMilia.BCad.Entities
{
    public class Text : Entity
    {
        private readonly PrimitiveText _primitive;
        private readonly IPrimitive[] _primitives;
        private readonly SnapPoint[] _snapPoints;

        public string Value => _primitive.Value;

        public Point Location => _primitive.Location;

        public Vector Normal => _primitive.Normal;

        public double Height => _primitive.Height;

        public double Width => _primitive.Width;

        public double Rotation => _primitive.Rotation;

        public override EntityKind Kind => EntityKind.Text;

        public override BoundingBox BoundingBox { get; }

        public Text(string value, Point location, Vector normal, double height, double rotation, CadColor? color = null, LineTypeSpecification lineTypeSpecification = null, object tag = null)
            : this(new PrimitiveText(value, location, height, normal, rotation, color), lineTypeSpecification, tag)
        {
        }

        public Text(PrimitiveText text, LineTypeSpecification lineTypeSpecification = null, object tag = null)
            : base(text.Color, lineTypeSpecification, tag)
        {
            _primitive = text;
            _primitives = new[] { _primitive };
            _snapPoints = new[] { new EndPoint(Location) };

            var rad = Rotation * MathHelper.DegreesToRadians;
            var right = new Vector(Math.Cos(rad), Math.Sin(rad), 0.0).Normalize() * Width;
            var up = Normal.Cross(right).Normalize() * Height;
            BoundingBox = BoundingBox.FromPoints(
                Location,
                Location + right,
                Location + up,
                Location + right + up);
        }

        public override IEnumerable<IPrimitive> GetPrimitives(DrawingSettings _settings)
        {
            return _primitives;
        }

        public override IEnumerable<SnapPoint> GetSnapPoints()
        {
            return _snapPoints;
        }

        public Text Update(
            string value = null,
            Optional<Point> location = default,
            Optional<Vector> normal = default,
            Optional<double> height = default,
            Optional<double> rotation = default,
            Optional<CadColor?> color = default,
            Optional<LineTypeSpecification> lineTypeSpecification = default,
            Optional<object> tag = default)
        {
            var newValue = value ?? Value;
            var newLocation = location.HasValue ? location.Value : Location;
            var newNormal = normal.HasValue ? normal.Value : Normal;
            var newHeight = height.HasValue ? height.Value : Height;
            var newRotation = rotation.HasValue ? rotation.Value : Rotation;
            var newColor = color.HasValue ? color.Value : Color;
            var newLineTypeSpecification = lineTypeSpecification.HasValue ? lineTypeSpecification.Value : LineTypeSpecification;
            var newTag = tag.HasValue ? tag.Value : Tag;

            if (newValue == Value &&
                newLocation == Location &&
                newNormal == Normal &&
                newHeight == Height &&
                newRotation == Rotation &&
                newColor == Color &&
                newLineTypeSpecification == LineTypeSpecification &&
                newTag == Tag)
            {
                return this;
            }

            return new Text(newValue, newLocation, newNormal, newHeight, newRotation, newColor, newLineTypeSpecification, newTag);
        }

        public override string ToString()
        {
            return string.Format("Text: value=\"{0}\", location={1}, normal={2}, height={3}, width={4}, rotation={5}", Value, Location, Normal, Height, Width, Color);
        }
    }
}
