using System;
using System.ComponentModel.Composition;
using System.Linq;
using BCad.Entities;
using BCad.Primitives;

namespace BCad.Commands
{
    [ExportCommand("Edit.Move", "move", "mov", "m")]
    internal class MoveCommand : ICommand
    {
        [Import]
        private IInputService InputService = null;

        [Import]
        private IWorkspace Workspace = null;

        public bool Execute(object arg)
        {
            var entities = InputService.GetEntities();
            if (entities.Cancel || !entities.HasValue)
            {
                return false;
            }

            if (!entities.Value.Any())
            {
                return true;
            }

            var origin = InputService.GetPoint(new UserDirective("Origin point"));
            if (origin.Cancel || !origin.HasValue)
            {
                return false;
            }

            var destination = InputService.GetPoint(new UserDirective("Destination point"), p =>
                {
                    return entities.Value.SelectMany(e => MovedEntity(e, p - origin.Value).GetPrimitives())
                        .Concat(new[] { new PrimitiveLine(origin.Value, p, Color.Default) });
                });

            if (destination.Cancel || !destination.HasValue)
            {
                return false;
            }

            var delta = destination.Value - origin.Value;
            var doc = Workspace.Document;
            foreach (var ent in entities.Value)
            {
                doc = doc.Replace(ent, MovedEntity(ent, delta));
            }

            Workspace.Document = doc;
            return true;
        }

        public string DisplayName
        {
            get { return "MOVE"; }
        }

        private static Entity MovedEntity(Entity ent, Vector delta)
        {
            Entity moved;
            switch (ent.Kind)
            {
                case EntityKind.Arc:
                    var arc = (Arc)ent;
                    moved = arc.Update(center: (arc.Center + delta).ToPoint());
                    break;
                case EntityKind.Circle:
                    var circle = (Circle)ent;
                    moved = circle.Update(center: (circle.Center + delta).ToPoint());
                    break;
                case EntityKind.Ellipse:
                    var el = (Ellipse)ent;
                    moved = el.Update(center: (el.Center + delta).ToPoint());
                    break;
                case EntityKind.Line:
                    var line = (Line)ent;
                    moved = line.Update(p1: (line.P1 + delta).ToPoint(), p2: (line.P2 + delta).ToPoint());
                    break;
                default:
                    throw new ArgumentException("Unsupported entity type", "ent");
            }

            return moved;
        }
    }
}
