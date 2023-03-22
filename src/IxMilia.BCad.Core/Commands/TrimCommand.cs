using System.Collections.Generic;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Utilities;

namespace IxMilia.BCad.Commands
{
    public class TrimCommand : AbstractTrimExtendCommand
    {
        protected override string GetBoundsText()
        {
            return "Select cutting edges";
        }

        protected override string GetTrimExtendText()
        {
            return "Entity to trim";
        }

        protected override void DoTrimExtend(SelectedEntity selectedEntity, IEnumerable<Primitives.IPrimitive> boundaryPrimitives, DrawingSettings settings, out IEnumerable<Entity> removed, out IEnumerable<Entity> added)
        {
            EditUtilities.Trim(selectedEntity, boundaryPrimitives, settings, out removed, out added);
        }
    }
}
