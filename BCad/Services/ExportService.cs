using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media.Media3D;
using System.Linq;
using BCad.Entities;
using BCad.Extensions;
using BCad.Helpers;

namespace BCad.Services
{
    [Export(typeof(IExportService))]
    internal class ExportService : IExportService
    {
        public IEnumerable<ProjectedEntity> ProjectTo2D(Drawing drawing, ViewPort viewPort)
        {
            // create transform
            var normal = viewPort.Sight * -1.0;
            var up = viewPort.Up;
            var right = up.Cross(normal).Normalize();
            var transform = TranslationMatrix(-viewPort.BottomLeft.X, -viewPort.BottomLeft.Y - viewPort.ViewHeight, 0)
                * PrimitiveExtensions.FromUnitCircleProjection(
                    normal,
                    right,
                    up,
                    Point.Origin,
                    -1.0,
                    -1.0,
                    1.0);
            transform.Scale(new Vector3D(1.0, 1.0, 0.0));

            // project all entities
            var entities = new List<ProjectedEntity>();
            foreach (var layer in from l in drawing.Layers.Values
                                  where l.Entities.Count > 0
                                  orderby l.Name
                                  select l)
            {
                foreach (var entity in layer.Entities.OrderBy(x => x.Id))
                {
                    var projected = Project(entity, layer, transform);
                    if (projected != null)
                        entities.Add(projected);
                }
            }

            return entities;
        }

        private ProjectedEntity Project(Entity entity, Layer layer, Matrix3D transform)
        {
            switch (entity.Kind)
            {
                case EntityKind.Line:
                    return Project((Line)entity, layer, transform);
                case EntityKind.Text:
                    return Project((Text)entity, layer, transform);
                case EntityKind.Circle:
                    return Project((Circle)entity, layer, transform);
                default:
                    return null;
            }
        }

        private ProjectedLine Project(Line line, Layer layer, Matrix3D transform)
        {
            var p1 = transform.Transform(line.P1.ToPoint3D());
            var p2 = transform.Transform(line.P2.ToPoint3D());
            return new ProjectedLine(line, layer, new Point(p1), new Point(p2));
        }

        private ProjectedText Project(Text text, Layer layer, Matrix3D transform)
        {
            var loc = transform.Transform(text.Location.ToPoint3D());
            var rad = text.Rotation * MathHelper.DegreesToRadians;
            var right = new Vector(Math.Cos(rad), Math.Sin(rad), 0.0).Normalize();
            var up = text.Normal.Cross(right).Normalize();
            var top = transform.Transform((text.Location + up * text.Height).ToPoint3D());
            var height = (top - loc).Length;
            var rotation = new Vector((top - loc)).ToAngle() * -1.0 - 90.0;
            return new ProjectedText(text, layer, new Point(loc), height, rotation.CorrectAngleDegrees());
        }

        private ProjectedCircle Project(Circle circle, Layer layer, Matrix3D transform)
        {
            var center = transform.Transform(circle.Center.ToPoint3D());
            // find axis endpoints
            var rightVector = Vector.RightVectorFromNormal(circle.Normal);
            var upVector = circle.Normal.Cross(rightVector).Normalize();
            var rightPoint = transform.Transform((circle.Center + (rightVector * circle.Radius)).ToPoint3D());
            var upPoint = transform.Transform((circle.Center + (upVector * circle.Radius)).ToPoint3D());
            var angle = new Vector(rightPoint - center).ToAngle();
            return new ProjectedCircle(circle, layer, new Point(center), (upPoint - center).Length, (rightPoint - center).Length, angle);
        }

        private static Matrix3D TranslationMatrix(double dx, double dy, double dz)
        {
            var matrix = Matrix3D.Identity;
            matrix.OffsetX = dx;
            matrix.OffsetY = dy;
            matrix.OffsetZ = dz;
            return matrix;
        }
    }
}
