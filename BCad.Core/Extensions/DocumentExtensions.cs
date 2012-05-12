using BCad.Entities;

namespace BCad
{
    public static class DocumentExtensions
    {
        /// <summary>
        /// Adds an entity to the specified layer.
        /// </summary>
        /// <param name="layer">The layer to which to add the entity.</param>
        /// <param name="entity">The entity to add.</param>
        /// <returns>The new document with the layer added.</returns>
        public static Document Add(this Document document, Layer layer, Entity entity)
        {
            var updatedLayer = layer.Add(entity);
            return document.Replace(layer, updatedLayer);
        }

        /// <summary>
        /// Replaces the specified entity.
        /// </summary>
        /// <param name="oldEntity">The entity to be replaced.</param>
        /// <param name="newEntity">The replacement entity.</param>
        /// <returns>The new document with the entity replaced.</returns>
        public static Document Replace(this Document document, Entity oldEntity, Entity newEntity)
        {
            var layer = document.ContainingLayer(oldEntity);
            if (layer == null)
            {
                return document;
            }

            return document.Replace(layer, oldEntity, newEntity);
        }

        /// <summary>
        /// Replaces the entity in the specified layer.
        /// </summary>
        /// <param name="layer">The layer containing the entity.</param>
        /// <param name="oldEntity">The entity to be replaced.</param>
        /// <param name="newEntity">The replacement entity.</param>
        /// <returns>The new document with the entity replaced.</returns>
        public static Document Replace(this Document document, Layer layer, Entity oldEntity, Entity newEntity)
        {
            var updatedLayer = layer.Replace(oldEntity, newEntity);
            return document.Replace(layer, updatedLayer);
        }


        /// <summary>
        /// Removes the entity from the layer.
        /// </summary>
        /// <param name="layer">The containing layer.</param>
        /// <param name="entity">The entity to remove.</param>
        /// <returns>The new document with the entity removed.</returns>
        public static Document Remove(this Document document, Layer layer, Entity entity)
        {
            var updatedLayer = layer.Remove(entity);
            return document.Replace(layer, updatedLayer);
        }

        /// <summary>
        /// Removes the entity from the document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="entity">The entity to remove.</param>
        /// <returns>The new document with the entity removed.</returns>
        public static Document Remove(this Document document, Entity entity)
        {
            var layer = document.ContainingLayer(entity);
            if (layer != null)
            {
                return document.Remove(layer, entity);
            }

            return document;
        }

        /// <summary>
        /// Returns the layer that contains the specified entity.
        /// </summary>
        /// <param name="entity">The entity to find.</param>
        /// <returns>The containing layer.</returns>
        public static Layer ContainingLayer(this Document document, Entity entity)
        {
            foreach (var layer in document.Layers.Values)
            {
                if (layer.EntityExists(entity))
                    return layer;
            }

            return null;
        }
    }
}
