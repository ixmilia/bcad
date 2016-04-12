using System.Collections.Generic;
using BCad.Entities;
using BCad.Extensions;
using BCad.Primitives;

namespace BCad.Commands
{
    [ExportCadCommand("Edit.Intersection", "INTERSECTION", "intersection", "int")]
    public class IntersectionCommand : CombinePolylinesCommandBase
    {
        protected override IEnumerable<IPrimitive> Combine(IEnumerable<Polyline> polylines)
        {
            return PolylineExtensions.IntersectPolylines(polylines);
        }
    }
}
