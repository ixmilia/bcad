using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCad.Primitives
{
    public class PrimitiveText : IPrimitive
    {
        public Color Color { get; private set; }
        public PrimitiveKind Kind { get { return PrimitiveKind.Text; } }

        public Point Location { get; private set; }
        public Vector Normal { get; private set; }
        public double Height { get; private set; }
        public double Rotation { get; private set; }
        public string Value { get; private set; }

        public PrimitiveText(string value, Point location, double height, Vector normal, double rotation, Color color)
        {
            this.Value = value;
            this.Location = location;
            this.Height = height;
            this.Normal = normal;
            this.Rotation = rotation;
            this.Color = color;
        }
    }
}
