using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using BCad.SnapPoints;

namespace BCad.Objects
{
    public class Line : IObject, IPrimitive
    {
        public Point P1 { get; private set; }

        public Point P2 { get; private set; }

        public Color Color { get; private set; }

        public Layer Layer { get; private set; }

        private int hashCode;

        public Line(Point p1, Point p2, Color color)
            : this(p1, p2, color, null)
        {
        }

        public Line(Point p1, Point p2, Color color, Layer layer)
        {
            P1 = p1;
            P2 = p2;
            Color = color;
            Layer = layer;
            hashCode = P1.GetHashCode() ^ P2.GetHashCode() ^ Color.GetHashCode() ^ Layer.GetHashCode();
        }

        public IEnumerable<IPrimitive> GetPrimitives()
        {
            yield return this;
        }

        public IEnumerable<SnapPoint> GetSnapPoints()
        {
            yield return new EndPoint(P1);
            yield return new EndPoint(P2);
            yield return new MidPoint(((P1 + P2) / 2.0).ToPoint());
        }

        public override int GetHashCode()
        {
            return hashCode;
        }
    }
}
