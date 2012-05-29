using System.ComponentModel.Composition;
using System.Diagnostics;
using BCad.Entities;
using BCad.Extensions;
using BCad.Primitives;

namespace BCad.Commands
{
    [ExportCommand("Edit.Offset", "offset", "off", "of")]
    public class OffsetCommand : ICommand
    {
        [Import]
        private IInputService InputService = null;

        [Import]
        private IWorkspace Workspace = null;

        private static double lastOffsetDistance = 0.0;

        public bool Execute(object arg)
        {
            var distance = InputService.GetDistance(lastOffsetDistance);
            if (distance.Cancel)
            {
                return false;
            }

            double dist;
            if (distance.HasValue)
            {
                dist = distance.Value;
            }
            else
            {
                dist = lastOffsetDistance;
            }

            InputService.WriteLine("Using offset distance of {0}", dist);
            var selection = InputService.GetEntity(new UserDirective("Select entity"));
            while (!selection.Cancel && selection.HasValue)
            {
                var ent = selection.Value.Entity;
                if (!Workspace.IsEntityOnDrawingPlane(ent))
                {
                    InputService.WriteLine("Entity must be entirely on the drawing plane to offset");
                    selection = InputService.GetEntity(new UserDirective("Select entity"));
                    continue;
                }

                Workspace.SelectedEntities.Clear();
                Workspace.SelectedEntities.Add(ent);
                var point = InputService.GetPoint(new UserDirective("Side to offset"));
                if (point.Cancel || !point.HasValue)
                {
                    break;
                }

                if (!Workspace.IsPointOnDrawingPlane(point.Value))
                {
                    InputService.WriteLine("Point must be on the drawing plane to offset");
                    selection = InputService.GetEntity(new UserDirective("Select entity"));
                    continue;
                }

                // do the actual offset
                Entity updated = null;
                bool isInside;
                switch (selection.Value.Entity.Kind)
                {
                    // for ellipse-like shapes, the radius changes
                    case EntityKind.Arc:
                        var arc = (Arc)ent;
                        isInside = (point.Value - arc.Center).Length < arc.Radius;
                        if (isInside && dist > arc.Radius)
                        {
                            InputService.WriteLine("Resultant radius would be negative");
                            break;
                        }
                        updated = arc.Update(radius: isInside
                            ? arc.Radius - dist
                            : arc.Radius + dist);
                        break;
                    case EntityKind.Circle:
                        var circle = (Circle)ent;
                        // TODO: project to determine this
                        isInside = (point.Value - circle.Center).Length < circle.Radius;
                        if (isInside && dist > circle.Radius)
                        {
                            InputService.WriteLine("Resultant radius would be negative");
                            break;
                        }
                        updated = circle.Update(radius: isInside
                            ? circle.Radius - dist
                            : circle.Radius + dist);
                        break;
                    case EntityKind.Ellipse:
                        var el = (Ellipse)ent;
                        var majorRadius = el.MajorAxis.Length;
                        var minorRadius = majorRadius * el.MinorAxisRatio;
                        isInside = (point.Value - el.Center).Length < el.MajorAxis.Length;
                        if (isInside && (dist > majorRadius || dist > minorRadius))
                        {
                            InputService.WriteLine("Resultant radius would be negative");
                            break;
                        }
                        var newMajorRadius = isInside ? majorRadius - dist : majorRadius + dist;
                        updated = el.Update(majorAxis: (el.MajorAxis.Normalize() * newMajorRadius));
                        break;
                    case EntityKind.Line:
                        // find what side the offset occured on and move both end points
                        var line = (Line)ent;
                        // normalize to XY plane
                        var picked = Workspace.ToXYPlane(point.Value);
                        var p1 = Workspace.ToXYPlane(line.P1);
                        var p2 = Workspace.ToXYPlane(line.P2);
                        var pline = new PrimitiveLine(p1, p2);
                        var perpendicular = new PrimitiveLine(picked, pline.PerpendicularSlope());
                        var intersection = pline.IntersectionXY(perpendicular, false);
                        if (intersection != null)
                        {
                            var offsetVector = (picked - intersection).Normalize() * dist;
                            offsetVector = Workspace.FromXYPlane(offsetVector);
                            updated = line.Update(p1: line.P1 + offsetVector, p2: line.P2 + offsetVector);
                        }
                        break;
                    default:
                        Debug.Fail("Unsupported entity type");
                        InputService.WriteLine("Unable to offset {0}", selection.Value.GetType().Name);
                        break;
                }

                Workspace.SelectedEntities.Clear();

                if (updated != null)
                {
                    Workspace.AddToCurrentLayer(updated);
                }

                selection = InputService.GetEntity(new UserDirective("Select entity"));
            }

            return true;
        }

        public string DisplayName
        {
            get { return "OFFSET"; }
        }
    }
}
