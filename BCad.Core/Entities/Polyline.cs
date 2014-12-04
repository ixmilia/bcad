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
        private readonly SnapPoint[] snapPoints;
        private readonly IPrimitive[] primitives;
        private readonly BoundingBox boundingBox;

        public IEnumerable<Point> Points { get { return this.points; } }

        public Polyline(IEnumerable<Point> points, IndexedColor color, object tag = null)
            : base(color, tag)
        {
            this.points = new List<Point>(points); // to prevent backing changes

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

        public override int PrimitiveCount { get { return this.primitives.Count(); } }

        public Polyline Update(
            IEnumerable<Point> points = null,
            Optional<IndexedColor> color = default(Optional<IndexedColor>),
            Optional<object> tag = default(Optional<object>))
        {
            var newPoints = points ?? this.points;
            var newColor = color.HasValue ? color.Value : this.Color;
            var newTag = tag.HasValue ? tag.Value : this.Tag;

            if (object.ReferenceEquals(newPoints, this.points) &&
                newColor == this.Color &&
                newTag == this.Tag)
            {
                return this;
            }

            return new Polyline(newPoints, newColor, newTag);
        }
    }
}
