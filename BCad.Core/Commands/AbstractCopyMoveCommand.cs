using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using BCad.Entities;
using BCad.Extensions;
using BCad.Primitives;
using BCad.Services;

namespace BCad.Commands
{
    internal abstract class AbstractCopyMoveCommand : ICommand
    {
        [Import]
        private IInputService InputService = null;

        [Import]
        private IWorkspace Workspace = null;

        protected abstract string CommandDisplayName { get; }

        protected abstract Drawing DoEdit(Drawing drawing, IEnumerable<Entity> entities, Vector delta);

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
            Workspace.Drawing = DoEdit(Workspace.Drawing, entities.Value, delta);
            return true;
        }

        public string DisplayName
        {
            get { return CommandDisplayName; }
        }
    }
}
