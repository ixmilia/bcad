using BCad.Entities;

namespace BCad
{
    public class SelectedEntity
    {
        public Entity Entity { get; private set; }

        public Point SelectionPoint { get; private set; }

        public SelectedEntity(Entity entity, Point selectionPoint)
        {
            this.Entity = entity;
            this.SelectionPoint = selectionPoint;
        }
    }
}
