using System.Collections.Generic;
using BCad.Entities;
using BCad.Primitives;

namespace BCad.Services
{
    public interface IEditService
    {
        void Trim(Drawing drawing, SelectedEntity entityToTrim, IEnumerable<IPrimitive> boundaryPrimitives, out IEnumerable<Entity> removed, out IEnumerable<Entity> added);
    }
}
