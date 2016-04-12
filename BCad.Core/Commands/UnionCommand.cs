using System.Collections.Generic;
using BCad.Entities;
using BCad.Extensions;
using BCad.Primitives;

namespace BCad.Commands
{
    [ExportCadCommand("Edit.Union", "UNION", "union", "un")]
    public class UnionCommand : CombinePolylinesCommandBase
    {
        protected override IEnumerable<IPrimitive> Combine(IEnumerable<Polyline> polylines)
        {
            return PolylineExtensions.UnionPolylines(polylines);
        }
    }
}
