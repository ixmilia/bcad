using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Primitives;

namespace IxMilia.BCad.Commands
{
    internal abstract class AbstractCopyMoveCommand : ICadCommand
    {
        protected abstract bool AllowMultiplePlacements { get; }
        protected abstract Drawing DoEdit(Drawing drawing, IEnumerable<Entity> entities, Vector delta);

        public async Task<bool> Execute(IWorkspace workspace, object arg)
        {
            var entities = await workspace.InputService.GetEntities();
            if (entities.Cancel || !entities.HasValue)
            {
                return false;
            }

            if (!entities.Value.Any())
            {
                return true;
            }

            var origin = await workspace.InputService.GetPoint(new UserDirective("Origin point"));
            if (origin.Cancel || !origin.HasValue)
            {
                return false;
            }

            var primitives = entities.Value.SelectMany(e => e.GetPrimitives(workspace.Drawing.Settings));

            do
            {
                var destination = await workspace.InputService.GetPoint(new UserDirective("Destination point"), p =>
                {
                    var offset = p - origin.Value;
                    return primitives.Select(pr => pr.Move(offset))
                        .Concat(new[] { new PrimitiveLine(origin.Value, p) });
                });

                if (destination.Cancel || !destination.HasValue)
                {
                    if (!AllowMultiplePlacements)
                    {
                        return false;
                    }
                    else
                    {
                        break;
                    }
                }

                var delta = destination.Value - origin.Value;
                workspace.Update(drawing: DoEdit(workspace.Drawing, entities.Value, delta));
            } while (AllowMultiplePlacements);

            return true;
        }
    }
}
