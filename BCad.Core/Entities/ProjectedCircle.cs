using BCad.Helpers;

namespace BCad.Entities
{
    public class ProjectedCircle : ProjectedEntity
    {
        public override EntityKind Kind
        {
            get { return EntityKind.Ellipse; }
        }

        public Circle OriginalCircle { get; private set; }

        public Point Center { get; private set; }

        public double RadiusX { get; private set; }

        public double RadiusY { get; private set; }

        public double Rotation { get; private set; }

        public ProjectedCircle(Circle circle, Layer layer, Point center, double radiusX, double radiusY, double rotation)
            : base(layer)
        {
            OriginalCircle = circle;
            Center = center;
            RadiusX = radiusX;
            RadiusY = radiusY;
            Rotation = rotation;
        }

        /// <summary>
        /// Using algorithm as described at http://www.ceometric.com/support/examples/v-ellipse-projection-using-rytz-construction.html
        /// </summary>
        public static ProjectedCircle FromConjugateDiameters(Circle originalCircle, Layer originalLayer, Point center, Point majorAxisConjugate, Point minorAxisConjugate)
        {
            // re-map to shorter variables
            var m = center;
            var p = majorAxisConjugate;
            var q = minorAxisConjugate;

            // PM must be longer than QM.  Swap if necessary.
            var pm = p - m;
            var qm = q - m;
            if (pm.LengthSquared > qm.LengthSquared)
            {
                var temp = p;
                p = q;
                q = temp;
                pm = p - m;
                qm = q - m;
            }

            // if axes are already orthoganal, no transform is needed
            if (MathHelper.CloseTo(0.0, pm.Dot(qm)))
            {
                return new ProjectedCircle(originalCircle, originalLayer, m, qm.Length, pm.Length, 0.0);
            }

            // find the plane containing the projected ellipse
            Plane

            return null;
        }
    }
}
