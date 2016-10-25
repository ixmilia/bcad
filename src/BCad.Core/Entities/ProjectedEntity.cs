namespace BCad.Entities
{
    public abstract class ProjectedEntity
    {
        public Layer OriginalLayer { get; private set; }

        public abstract EntityKind Kind { get; }

        public ProjectedEntity(Layer layer)
        {
            OriginalLayer = layer;
        }
    }
}
