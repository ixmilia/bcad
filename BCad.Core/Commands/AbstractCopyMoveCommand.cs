using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BCad.Entities;
using BCad.Extensions;
using BCad.Primitives;
using BCad.Services;

namespace BCad.Commands
{
    internal abstract class AbstractCopyMoveCommand : ICadCommand
    {
        protected abstract Drawing DoEdit(Drawing drawing, IEnumerable<Entity> entities, Vector delta);

        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var inputService = workspace.GetService<IInputService>();
            var entities = await inputService.GetEntities();
            if (entities.Cancel || !entities.HasValue)
            {
                return false;
            }

            if (!entities.Value.Any())
            {
                return true;
            }

            var origin = await inputService.GetPoint(new UserDirective("Origin point"));
            if (origin.Cancel || !origin.HasValue)
            {
                return false;
            }

            var primitives = entities.Value.SelectMany(e => e.GetPrimitives());
            var destination = await inputService.GetPoint(new UserDirective("Destination point"), p =>
            {
                var offset = p - origin.Value;
                return primitives.Select(pr => pr.Move(offset))
                    .Concat(new[] { new PrimitiveLine(origin.Value, p, null) });
            });

            if (destination.Cancel || !destination.HasValue)
            {
                return false;
            }

            var delta = destination.Value - origin.Value;
            workspace.Update(drawing: DoEdit(workspace.Drawing, entities.Value, delta));
            return true;
        }
    }
}
