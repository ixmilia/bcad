namespace BCad.Primitives
{
    public class PrimitiveText : IPrimitive
    {
        public IndexedColor Color { get; private set; }
        public PrimitiveKind Kind { get { return PrimitiveKind.Text; } }

        public Point Location { get; private set; }
        public Vector Normal { get; private set; }
        public double Height { get; private set; }
        public double Width { get; private set; }
        public double Rotation { get; private set; }
        public string Value { get; private set; }

        public PrimitiveText(string value, Point location, double height, Vector normal, double rotation, IndexedColor color)
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
    }
}
