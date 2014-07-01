using System;
using System.Collections.Generic;
using System.Linq;
using BCad.Entities;
using BCad.Extensions;
using BCad.Helpers;
using BCad.Primitives;

namespace BCad.Helpers
{
    public static class ProjectionHelper
    {
        public static IEnumerable<ProjectedEntity> ProjectTo2D(Drawing drawing, ViewPort viewPort, int width, int height)
        {
            // create transform
            var transform = viewPort.GetTransformationMatrixWindowsStyle(width, height);
            
            // project all entities
            var entities = new List<ProjectedEntity>();
            foreach (var layer in from l in drawing.GetLayers()
                                  where l.EntityCount > 0
                                  orderby l.Name
                                  select l)
            {
                foreach (var entity in layer.GetEntities().OrderBy(x => x.Id))
                {
                    var projected = Project(entity, layer, transform);
                    if (projected != null)
                        entities.Add(projected);
                }
            }

            return entities;
        }

        public static ProjectedEntity Project(Entity entity, Layer layer, Matrix4 transform)
        {
            switch (entity.Kind)
            {
                case EntityKind.Line:
                    return Project((Line)entity, layer, transform);
                case EntityKind.Text:
                    return Project((Text)entity, layer, transform);
                case EntityKind.Circle:
                    return Project((Circle)entity, layer, transform);
                case EntityKind.Arc:
                    return Project((Arc)entity, layer, transform);
                case EntityKind.Aggregate:
                    return Project((AggregateEntity)entity, layer, transform);
                default:
                    return null;
            }
        }

        public static ProjectedLine Project(Line line, Layer layer, Matrix4 transform)
        {
            var p1 = transform.Transform(line.P1);
            var p2 = transform.Transform(line.P2);
            return new ProjectedLine(line, layer, p1, p2);
        }

        public static ProjectedText Project(Text text, Layer layer, Matrix4 transform)
        {
            var loc = transform.Transform(text.Location);
            var rad = text.Rotation * MathHelper.DegreesToRadians;
            var right = new Vector(Math.Cos(rad), Math.Sin(rad), 0.0).Normalize();
            var up = text.Normal.Cross(right).Normalize();
            var top = transform.Transform(text.Location + up * text.Height);
            var height = ((Point)top - loc).Length;
            var rotation = (top - loc).ToAngle() * -1.0 - 90.0;
            return new ProjectedText(text, layer, loc, height, rotation.CorrectAngleDegrees());
        }

        public static ProjectedCircle Project(Circle circle, Layer layer, Matrix4 transform)
        {
            // find axis endpoints
            var rightVector = Vector.RightVectorFromNormal(circle.Normal);
            var upVector = circle.Normal.Cross(rightVector).Normalize();
            var pt = transform.Transform(circle.Center + (rightVector * circle.Radius));
            var qt = transform.Transform(circle.Center + (upVector * circle.Radius));
            var m = transform.Transform(circle.Center);
            return ProjectedCircle.FromConjugateDiameters(circle, layer, m, pt, qt);
        }

        public static PrimitiveEllipse Project(PrimitiveEllipse ellipse, Matrix4 transform)
        {
            // brute-force the endpoints
            var fromUnit = Matrix4.CreateScale(1.0, 1.0, 0.0) * transform * ellipse.FromUnitCircle;
            var center = transform.Transform(ellipse.Center);
            var minPoint = fromUnit.Transform(new Point(MathHelper.COS[0], MathHelper.SIN[0], 0));
            var maxPoint = minPoint;
            var minDistance = (minPoint - center).LengthSquared;
            var maxDistance = minDistance;
            for (int i = 1; i < 360; i++)
            {
                var next = fromUnit.Transform(new Point(MathHelper.COS[i], MathHelper.SIN[i], 0));
                var dist = (next - center).LengthSquared;
                if (dist < minDistance)
                {
                    minPoint = next;
                    minDistance = dist;
                }

                if (dist > maxDistance)
                {
                    maxPoint = next;
                    maxDistance = dist;
                }
            }

            var majorAxis = maxPoint - center;
            var minorAxisRatio = Math.Sqrt(minDistance) / Math.Sqrt(maxDistance);
            var startAngle = ellipse.StartAngle;
            var endAngle = ellipse.EndAngle;

            // projection looks like a circle
            if (Math.Abs(maxDistance - minDistance) < MathHelper.Epsilon * 1000) // TODO: need a better way to do this
            {
                majorAxis = new Vector(majorAxis.Length, 0.0, 0.0); // right vector
                minorAxisRatio = 1.0;
            }

            // correct start/end angles
            if (!MathHelper.CloseTo(0.0, ellipse.StartAngle) || !MathHelper.CloseTo(MathHelper.ThreeSixty, ellipse.EndAngle))
            {
                var tempEllipse = new PrimitiveEllipse(center, majorAxis, Vector.ZAxis, minorAxisRatio, 0.0, MathHelper.ThreeSixty, ellipse.Color);
                var majorAngle = majorAxis.ToAngle();
                var startPoint = transform.Transform(ellipse.GetStartPoint());
                var endPoint = transform.Transform(ellipse.GetEndPoint());
                var toUnit = tempEllipse.FromUnitCircle;
                toUnit.Invert();
                var startUnit = (Vector)toUnit.Transform(startPoint);
                var endUnit = (Vector)toUnit.Transform(endPoint);
                startAngle = (startUnit.ToAngle() - majorAngle).CorrectAngleDegrees();
                endAngle = (endUnit.ToAngle() - majorAngle).CorrectAngleDegrees();
            }

            return new PrimitiveEllipse(center, majorAxis, Vector.ZAxis, minorAxisRatio, startAngle, endAngle, ellipse.Color);
        }

        public static ProjectedArc Project(Arc arc, Layer layer, Matrix4 transform)
        {
            // find the containing circle
            var rightVector = Vector.RightVectorFromNormal(arc.Normal);
            var upVector = arc.Normal.Cross(rightVector).Normalize();
            var pt = transform.Transform(arc.Center + (rightVector * arc.Radius));
            var qt = transform.Transform(arc.Center + (upVector * arc.Radius));
            var m = transform.Transform(arc.Center);
            var circle = ProjectedCircle.FromConjugateDiameters(null, layer, m, pt, qt);

            // find the new start and end angles
            var startPoint = transform.Transform(arc.EndPoint1);
            var endPoint = transform.Transform(arc.EndPoint2);
            var startAngle = (startPoint - circle.Center).ToAngle();
            var endAngle = (endPoint - circle.Center).ToAngle();

            return new ProjectedArc(arc, layer, circle.Center, circle.RadiusX, circle.RadiusY, circle.Rotation, startAngle, endAngle, startPoint, endPoint);
        }

        public static ProjectedAggregate Project(AggregateEntity aggregate, Layer layer, Matrix4 transform)
        {
            var loc = transform.Transform(aggregate.Location);
            var newOrigin = transform.Transform(Point.Origin);
            var offset = (Point)loc - (Point)newOrigin;
            return new ProjectedAggregate(aggregate, layer, offset, aggregate.Children.Select(c => Project(c, layer, transform)));
        }

        private static Matrix4 TranslationMatrix(double dx, double dy, double dz)
        {
            var matrix = Matrix4.Identity;
            matrix.M41 = dx;
            matrix.M42 = dy;
            matrix.M43 = dz;
            return matrix;
        }
    }
}
