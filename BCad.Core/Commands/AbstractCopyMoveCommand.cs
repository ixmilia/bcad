using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
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

        protected abstract Drawing DoEdit(Drawing drawing, IEnumerable<Entity> entities, Vector delta);

        public async Task<bool> Execute(object arg)
        {
            var entities = await InputService.GetEntities();
            if (entities.Cancel || !entities.HasValue)
            {
                return false;
            }

            if (!entities.Value.Any())
            {
                return true;
            }

            var origin = await InputService.GetPoint(new UserDirective("Origin point"));
            if (origin.Cancel || !origin.HasValue)
            {
                return false;
            }

            var primitives = entities.Value.SelectMany(e => e.GetPrimitives());
            var destination = await InputService.GetPoint(new UserDirective("Destination point"), p =>
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
            Workspace.Update(drawing: DoEdit(Workspace.Drawing, entities.Value, delta));
            return true;
        }
    }
}
