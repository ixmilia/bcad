using System.Collections.Generic;
using BCad.Primitives;

namespace BCad.Services
{
    public interface ITrimExtendService
    {
        Drawing Trim(Drawing drawing, SelectedEntity entityToTrim, IEnumerable<IPrimitive> boundaryPrimitives);
    }
}
