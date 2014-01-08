using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.Primitives;
using BCad.SnapPoints;

namespace BCad.Entities
{
    public class Polyline : Entity
    {
        private const string PointsText = "Points";
        private readonly IEnumerable<Point> points;
        private readonly IndexedColor color;
        private readonly SnapPoint[] snapPoints;
        private readonly IPrimitive[] primitives;
        private readonly BoundingBox boundingBox;

        public IEnumerable<Point> Points { get { return this.points; } }

        public override IndexedColor Color { get { return this.color; } }

        public Polyline(IEnumerable<Point> points, IndexedColor color)
        {
            this.points = new List<Point>(points); // to prevent backing changes
            this.color = color;

            // add end points
            var parr = points.ToArray();
            var sp = new List<SnapPoint>(points.Select(p => new EndPoint(p)));
            var pr = new List<IPrimitive>();
            // add midpoints
            for (int i = 0; i < parr.Length - 1; i++)
            {
                sp.Add(new MidPoint((parr[i] + parr[i + 1]) / 2.0));
                pr.Add(new PrimitiveLine(parr[i], parr[i + 1], color));
            }
            this.snapPoints = sp.ToArray();
            this.primitives = pr.ToArray();
            this.boundingBox = BoundingBox.FromPoints(parr);
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
                case PointsText:
                    return Points;
                default:
                    return base.GetProperty(propertyName);
            }
        }

        public override EntityKind Kind { get { return EntityKind.Polyline; } }

        public override BoundingBox BoundingBox { get { return this.boundingBox; } }

        public Polyline Update(IEnumerable<Point> points = null, IndexedColor? color = null)
        {
            return new Polyline(
                points ?? this.Points,
                color ?? this.Color);
        }
    }
}
