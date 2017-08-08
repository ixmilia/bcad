// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Helpers;
using IxMilia.BCad.Primitives;

namespace IxMilia.BCad.Entities
{
    public class ProjectedCircle : ProjectedEntity
    {
        public override EntityKind Kind
        {
            get { return EntityKind.Circle; }
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
            var plane = Plane.From3Points(m, p, q);

            // rotate P by 90 degrees around the normal
            var rotationMatrix = Matrix4.Identity;
            rotationMatrix.RotateAt(new Quaternion(plane.Normal, 90.0), m);
            var rotp = rotationMatrix.Transform(p);

            // the angle between (rotp-M) and (Q-M) should be less than 90 degrees.  mirror if not
            if (Vector.AngleBetween(rotp - m, qm) > 90.0)
                rotp = ((rotp - m) * -1.0) + m;

            // construct the rytz circle
            // the center is the midpoint of the edge from rotp to Q
            // the radius is the distance from M to the center
            // the normal is the normal of the plane
            var rc = (rotp + q) / 2.0;
            var rytz = new PrimitiveEllipse(rc, (m - rc).Length, plane.Normal);

            // intersect the rytz circle with a line passing through the center and Q
            var intersectingLine = new PrimitiveLine(rc, q);
            var intersectingPoints = intersectingLine.IntersectionPoints(rytz, false).ToList();
            if (intersectingPoints.Count != 2)
                return null;
            var da = (q - intersectingPoints[0]).Length;
            var db = (q - intersectingPoints[1]).Length;
            if (da < db)
            {
                // da must be large than db
                var temp = da;
                da = db;
                db = temp;
            }

            // get main axes
            var a = intersectingPoints[0] - m;
            var b = intersectingPoints[1] - m;
            if (b.LengthSquared > a.LengthSquared)
            {
                var temp = a;
                a = b;
                b = temp;
            }

            // return the new ellipse
            return new ProjectedCircle(originalCircle, originalLayer, m, da, db, a.ToAngle() * -1.0);
        }
    }
}
