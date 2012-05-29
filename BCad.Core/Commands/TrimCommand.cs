using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using BCad.Entities;
using BCad.Extensions;
using BCad.Helpers;
using BCad.Primitives;

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

            var dwg = Workspace.Drawing;
            var directive = new UserDirective("Entity to trim");
            var selected = InputService.GetEntity(directive);
            while (!selected.Cancel && selected.HasValue)
            {
                Debug.Assert(selected.Value.Entity.Kind == EntityKind.Line, "only line trimming is supported for now");
                var sel = selected.Value.Entity.GetPrimitives().OfType<PrimitiveLine>().Single();

                // find all intersection points
                var intersectionPoints = boundaryPrimitives
                    .Select(b => b.IntersectionPoints(sel))
                    .Where(p => p != null)
                    .SelectMany(b => b)
                    .Where(p => p != null);

                if (intersectionPoints.Any())
                {
                    // perform the trim operation
                    switch (selected.Value.Entity.Kind)
                    {
                        case EntityKind.Line:
                            dwg = TrimLine(dwg, (Line)selected.Value.Entity, selected.Value.SelectionPoint, sel, intersectionPoints);
                            break;
                        default:
                            Debug.Fail("only lines are supported");
                            break;
                    }

                    // commit the change
                    Workspace.Drawing = dwg;
                }

                // get next entity to trim
                selected = InputService.GetEntity(directive);
            }

            Workspace.SelectedEntities.Clear();
            return true;
        }

        private static Drawing TrimLine(Drawing dwg, Line line, Point pivot, PrimitiveLine sel, IEnumerable<Point> intersectionPoints)
        {
            // split intersection points based on which side of the selection point they are
            var left = new List<Point>();
            var right = new List<Point>();
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
            if (leftPoint != null || rightPoint != null)
            {
                var layer = dwg.ContainingLayer(line).Name;
                dwg = dwg.Remove(line);
                if (leftPoint != null)
                {
                    dwg = dwg.Add(dwg.Layers[layer], line.Update(p1: line.P1, p2: leftPoint));
                }
                if (rightPoint != null)
                {
                    dwg = dwg.Add(dwg.Layers[layer], line.Update(p1: rightPoint, p2: line.P2));
                }
            }

            return dwg;
        }

        public string DisplayName
        {
            get { return "TRIM"; }
        }
    }
}
