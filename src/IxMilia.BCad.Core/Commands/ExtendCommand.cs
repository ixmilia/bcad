using System.Collections.Generic;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Utilities;

namespace IxMilia.BCad.Commands
{
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
            EditUtilities.Extend(selectedEntity, boundaryPrimitives, out removed, out added);
        }
    }
}
