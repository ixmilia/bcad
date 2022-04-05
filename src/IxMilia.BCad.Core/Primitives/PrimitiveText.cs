namespace IxMilia.BCad.Primitives
{
    public class PrimitiveText : IPrimitive
    {
        public CadColor? Color { get; private set; }
        public PrimitiveKind Kind { get { return PrimitiveKind.Text; } }

        public Point Location { get; private set; }
        public Vector Normal { get; private set; }
        public double Height { get; private set; }
        public double Width { get; private set; }
        public double Rotation { get; private set; }
        public string Value { get; private set; }

        public PrimitiveText(string value, Point location, double height, Vector normal, double rotation, CadColor? color = null)
        {
            this.Value = value;
            this.Location = location;
            this.Height = height;
            this.Normal = normal;
            this.Rotation = rotation;
            this.Color = color;

            // currently, not a good way to measure text, but assume a character's width is ~77% of its height
            this.Width = value.Length * this.Height * 0.77;
        }

        public PrimitiveText Update(
            Optional<string> value = default,
            Optional<Point> location = default,
            Optional<double> height = default,
            Optional<Vector> normal = default,
            Optional<double> rotation = default,
            Optional<CadColor?> color = default)
        {
            var newValue = value.HasValue ? value.Value : Value;
            var newLocation = location.HasValue ? location.Value : Location;
            var newHeight = height.HasValue ? height.Value : Height;
            var newNormal = normal.HasValue ? normal.Value : Normal;
            var newRotation = rotation.HasValue ? rotation.Value : Rotation;
            var newColor = color.HasValue ? color.Value : Color;

            if (newValue == Value &&
                newLocation == Location &&
                newHeight == Height &&
                newNormal == Normal &&
                newRotation == Rotation &&
                newColor == Color)
            {
                // no change
                return this;
            }

            return new PrimitiveText(newValue, newLocation, newHeight, newNormal, newRotation, newColor);
        }
    }
}
