using System.Collections.Generic;
using BCad.Entities;
using BCad.Extensions;
using BCad.Primitives;

namespace BCad.Commands
{
    [ExportCadCommand("Edit.Intersection", "INTERSECTION", "intersection", "int")]
    public class IntersectionCommand : CombinePolylinesCommandBase
    {
        protected override IEnumerable<IPrimitive> Combine(Polyline a, Polyline b)
        {
            return a.Intersection(b);
        }
    }
}
