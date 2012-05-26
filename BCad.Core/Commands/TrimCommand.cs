using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using BCad.Entities;
using System.Diagnostics;
using BCad.Primitives;
using BCad.Helpers;

namespace BCad.Commands
{
    [ExportCommand("Edit.Trim", "trim", "tr", "t")]
    public class TrimCommand : ICommand
    {
        [Import]
        private IInputService InputService = null;

        [Import]
        private IWorkspace Workspace = null;

        public bool Execute(object arg)
        {
            var boundaries = InputService.GetEntities("Select cutting edges");
            if (boundaries.Cancel || !boundaries.HasValue || !boundaries.Value.Any())
            {
                return false;
            }

            var boundaryPrimitives = boundaries.Value.SelectMany(b => b.GetPrimitives());
            Workspace.SelectedEntities.Set(boundaries.Value);

            var doc = Workspace.Document;
            var directive = new UserDirective("Entity to trim");
            var selected = InputService.GetEntity(directive);
            while (!selected.Cancel && selected.HasValue)
            {
                Debug.Assert(selected.Value.Entity.Kind == EntityKind.Line, "only line trimming is supported for now");
                var sel = selected.Value.Entity.GetPrimitives().OfType<PrimitiveLine>().Single();

                // find all intersection points
                var intersectionPoints = boundaryPrimitives.OfType<PrimitiveLine>()
                    // TODO: real intersection, not just XY
                    .Select(b => b.IntersectionXY(sel))
                    .Where(p => p != null);

                if (intersectionPoints.Any())
                {
                    // split intersection points based on which side of the selection point they are
                    var left = new List<Point>();
                    var right = new List<Point>();
                    var pivot = selected.Value.SelectionPoint;
                    foreach (var point in intersectionPoints)
                    {
                        if (MathHelper.Between(Math.Min(sel.P1.X, pivot.X), Math.Max(sel.P1.X, pivot.X), point.X) &&
                            MathHelper.Between(Math.Min(sel.P1.Y, pivot.Y), Math.Max(sel.P1.Y, pivot.Y), point.Y) &&
                            MathHelper.Between(Math.Min(sel.P1.Z, pivot.Z), Math.Max(sel.P1.Z, pivot.Z), point.Z))
                        {
                            left.Add(point);
                        }
                        else
                        {
                            right.Add(point);
                        }
                    }

                    // find the closest points on each side.  these are the new endpoints
                    var leftPoint = left.OrderBy(p => (p - pivot).LengthSquared).FirstOrDefault();
                    var rightPoint = right.OrderBy(p => (p - pivot).LengthSquared).FirstOrDefault();

                    // remove the original line
                    var layer = doc.ContainingLayer(selected.Value.Entity).Name;
                    doc = doc.Remove(selected.Value.Entity);

                    // re-add left and right lines
                    switch (selected.Value.Entity.Kind)
                    {
                        case EntityKind.Line:
                            var line = (Line)selected.Value.Entity;
                            if (leftPoint != null)
                            {
                                doc = doc.Add(doc.Layers[layer], line.Update(p1: line.P1, p2: leftPoint));
                            }
                            if (rightPoint != null)
                            {
                                doc = doc.Add(doc.Layers[layer], line.Update(p1: rightPoint, p2: line.P2));
                            }
                            break;
                        default:
                            Debug.Fail("only lines are supported");
                            break;
                    }

                    // commit the change
                    Workspace.Document = doc;
                }

                // get next entity to trim
                selected = InputService.GetEntity(directive);
            }

            Workspace.SelectedEntities.Clear();
            return true;
        }

        public string DisplayName
        {
            get { return "TRIM"; }
        }
    }
}
