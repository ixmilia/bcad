using System;
using System.ComponentModel.Composition;
using System.Linq;
using BCad.Entities;
using BCad.Extensions;
using BCad.Primitives;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCommand("Edit.Move", "move", "mov", "m")]
    public class MoveCommand : ICommand
    {
        [Import]
        private IInputService InputService = null;

        [Import]
        private IEditService EditService = null;

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

            var primitives = entities.Value.SelectMany(e => e.GetPrimitives());
            var destination = InputService.GetPoint(new UserDirective("Destination point"), p =>
                {
                    var offset = p - origin.Value;
                    return primitives.Select(pr => pr.Move(offset))
                        .Concat(new[] { new PrimitiveLine(origin.Value, p, Color.Default) });
                });

            if (destination.Cancel || !destination.HasValue)
            {
                return false;
            }

            var delta = destination.Value - origin.Value;
            var dwg = Workspace.Drawing;
            foreach (var ent in entities.Value)
            {
                dwg = dwg.Replace(ent, EditService.Move(ent, delta));
            }

            Workspace.Drawing = dwg;
            return true;
        }

        public string DisplayName
        {
            get { return "MOVE"; }
        }
    }
}
