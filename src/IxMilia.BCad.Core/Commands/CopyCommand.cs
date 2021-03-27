using System.Collections.Generic;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Utilities;

namespace IxMilia.BCad.Commands
{
    internal class CopyCommand : AbstractCopyMoveCommand
    {
        protected override Drawing DoEdit(Drawing drawing, IEnumerable<Entity> entities, Vector delta)
        {
            foreach (var ent in entities)
            {
                var layer = drawing.ContainingLayer(ent);
                drawing = drawing.Add(layer, EditUtilities.Move(ent, delta));
            }

            return drawing;
        }
    }
}
