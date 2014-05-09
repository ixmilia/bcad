using System.Collections.Generic;
using BCad.Entities;
using BCad.Utilities;

namespace BCad.Commands
{
    [ExportCommand("Edit.Copy", "COPY", ModifierKeys.Control, Key.C, "copy", "co")]
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
