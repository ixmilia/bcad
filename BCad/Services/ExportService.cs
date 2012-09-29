using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using BCad.Entities;
using BCad.Extensions;

namespace BCad.Services
{
    [Export(typeof(IExportService))]
    internal class ExportService : IExportService
    {
        [Import]
        private IEditService EditService = null;

        public Drawing ProjectTo2D(Drawing drawing, ViewPort viewPort)
        {
            // create transform
            var normal = viewPort.Sight * -1.0;
            var up = viewPort.Up;
            var right = up.Cross(normal).Normalize();
            var transform =
                PrimitiveExtensions.FromUnitCircleProjection(
                normal,
                right,
                up,
                Point.Origin,
                -1.0,
                -1.0,
                0.0);

            // project all entities
            var minx = 0.0;
            var miny = 0.0;
            var layers = new List<Layer>();
            foreach (var layer in from l in drawing.Layers.Values
                                  where l.Entities.Count > 0
                                  orderby l.Name
                                  select l)
            {
                var entities = new List<Entity>();
                foreach (var entity in layer.Entities.OrderBy(x => x.Id))
                {
                    switch (entity.Kind)
                    {
                        case EntityKind.Line:
                            var line = (Line)entity;
                            line = line.Update(p1: new Point(transform.Transform(line.P1.ToPoint3D())), p2: new Point(transform.Transform(line.P2.ToPoint3D())));
                            minx = Math.Min(minx, line.P1.X);
                            minx = Math.Min(minx, line.P2.X);
                            miny = Math.Min(miny, line.P1.Y);
                            miny = Math.Min(miny, line.P2.Y);
                            entities.Add(line);
                            break;
                        default:
                            throw new ArgumentException("entity.Kind");
                    }
                }

                layers.Add(layer.Update(entities: entities));
            }

            // and translate again to make all values positive


            return drawing;
        }
    }
}
