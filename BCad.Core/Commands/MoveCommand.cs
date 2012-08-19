using System.Collections.Generic;
using System.ComponentModel.Composition;
using BCad.Entities;
using BCad.Services;

namespace BCad.Commands
{
    [ExportCommand("Edit.Move", "move", "mov", "m")]
    internal class MoveCommand : AbstractCopyMoveCommand
    {
        [Import]
        private IEditService EditService = null;

        protected override string CommandDisplayName
        {
            get { return "MOVE"; }
        }

        protected override Drawing DoEdit(Drawing drawing, IEnumerable<Entity> entities, Vector delta)
        {
            foreach (var ent in entities)
            {
                drawing = drawing.Replace(ent, EditService.Move(ent, delta));
            }

            return drawing;
        }
    }
}
