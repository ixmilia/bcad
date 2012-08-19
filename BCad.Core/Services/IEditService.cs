using System.Collections.Generic;
using BCad.Entities;
using BCad.Primitives;

namespace BCad.Services
{
    public interface IEditService
    {
        void Trim(SelectedEntity entityToTrim, IEnumerable<IPrimitive> boundaryPrimitives, out IEnumerable<Entity> removed, out IEnumerable<Entity> added);
        bool CanOffsetEntity(Entity entityToOffset);
        Entity Offset(IWorkspace workspace, Entity entityToOffset, Point offsetDirection, double offsetDistance);
        PrimitiveEllipse Ttr(Plane drawingPlane, SelectedEntity firstEntity, SelectedEntity secondEntity, double radius);
        Entity Move(Entity entity, Vector offset);
    }
}
