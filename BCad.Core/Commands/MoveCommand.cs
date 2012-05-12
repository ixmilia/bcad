using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using BCad.Entities;

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

            var origin = InputService.GetPoint(new UserDirective("Origin point"));
            if (origin.Cancel || !origin.HasValue)
            {
                return false;
            }

            var destination = InputService.GetPoint(new UserDirective("Destination point"), p =>
                {
                    return entities.Value.SelectMany(e => MovedEntity(e, p - origin.Value).GetPrimitives())
                        .Concat(new[] { new Line(origin.Value, p, Color.Default) });
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
            if (ent is Arc)
            {
                var arc = (Arc)ent;
                moved = arc.Update(center: (arc.Center + delta).ToPoint());
            }
            else if (ent is Circle)
            {
                var circle = (Circle)ent;
                moved = circle.Update(center: (circle.Center + delta).ToPoint());
            }
            else if (ent is Ellipse)
            {
                var el = (Ellipse)ent;
                moved = el.Update(center: (el.Center + delta).ToPoint());
            }
            else if (ent is Line)
            {
                var line = (Line)ent;
                moved = line.Update(p1: (line.P1 + delta).ToPoint(), p2: (line.P2 + delta).ToPoint());
            }
            else
            {
                throw new Exception("Unsupported entity type");
            }

            return moved;
        }
    }
}
