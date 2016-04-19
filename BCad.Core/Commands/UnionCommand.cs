using System.Collections.Generic;
using BCad.Entities;
using BCad.Extensions;

namespace BCad.Commands
{
    [ExportCadCommand("Edit.Union", "UNION", "union", "un")]
    public class UnionCommand : CombinePolylinesCommandBase
    {
        protected override IEnumerable<Polyline> Combine(IEnumerable<Polyline> polylines)
        {
            return polylines.Union();
        }
    }
}
