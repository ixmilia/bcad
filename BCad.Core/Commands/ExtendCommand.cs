using System.Collections.Generic;
using BCad.Entities;

namespace BCad.Commands
{
    [ExportCommand("Edit.Extend", "EXTEND", "extend", "ex")]
    public class ExtendCommand : AbstractTrimExtendCommand
    {
        protected override string GetBoundsText()
        {
            return "Select bounding edges";
        }

        protected override string GetTrimExtendText()
        {
            return "Entity to extend";
        }

        protected override void DoTrimExtend(SelectedEntity selectedEntity, IEnumerable<Primitives.IPrimitive> boundaryPrimitives, out IEnumerable<Entity> removed, out IEnumerable<Entity> added)
        {
            EditService.Extend(selectedEntity, boundaryPrimitives, out removed, out added);
        }
    }
}
