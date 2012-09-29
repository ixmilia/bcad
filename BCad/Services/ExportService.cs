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

            // project all entities
            var entities = new List<ProjectedEntity>();
            foreach (var layer in from l in drawing.Layers.Values
                                  where l.Entities.Count > 0
                                  orderby l.Name
                                  select l)
            {
                foreach (var entity in layer.Entities.OrderBy(x => x.Id))
                {
                    switch (entity.Kind)
                    {
                        case EntityKind.Line:
                            var line = (Line)entity;
                            var p1 = transform.Transform(line.P1.ToPoint3D());
                            var p2 = transform.Transform(line.P2.ToPoint3D());
                            entities.Add(new ProjectedLine(line, layer, new Point(p1), new Point(p2)));
                            break;
                        case EntityKind.Text:
                            var text = (Text)entity;
                            var loc = transform.Transform(text.Location.ToPoint3D());
                            var rad = text.Rotation * MathHelper.DegreesToRadians;
                            var rt = new Vector(Math.Cos(rad), Math.Sin(rad), 0.0).Normalize();
                            var u = text.Normal.Cross(rt).Normalize();
                            var top = transform.Transform((text.Location + u * text.Height).ToPoint3D());
                            var h = (top - loc).Length;
                            var r = new Vector((top - loc)).ToAngle() * -1.0 - 90.0;
                            entities.Add(new ProjectedText(text, layer, new Point(loc), h, r));
                            break;
                        case EntityKind.Circle:
                            var c = (Circle)entity;
                            var center = transform.Transform(c.Center.ToPoint3D());
                            // TODO: proper projection and rotation
                            entities.Add(new ProjectedCircle(c, layer, new Point(center), c.Radius, c.Radius, 0));
                            break;
                        default:
                            //throw new ArgumentException("entity.Kind");
                            break;
                    }
                }
            }

            return entities;
        }

        private static Matrix3D TranslationMatrix(double dx, double dy, double dz)
        {
            var matrix = Matrix3D.Identity;
            matrix.OffsetX = dx;
            matrix.OffsetY = dy;
            matrix.OffsetZ = dz;
            return matrix;
        }

        private static Matrix3D ScalingMatrix(double xs, double ys, double zs)
        {
            var matrix = Matrix3D.Identity;
            matrix.M11 = xs;
            matrix.M22 = ys;
            matrix.M33 = zs;
            return matrix;
        }
    }
}
