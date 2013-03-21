using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BCad.Entities;

namespace BCad
{
    public static class DrawingExtensions
    {
        /// <summary>
        /// Adds an entity to the specified layer.
        /// </summary>
        /// <param name="layer">The layer to which to add the entity.</param>
        /// <param name="entity">The entity to add.</param>
        /// <returns>The new drawing with the layer added.</returns>
        public static Drawing Add(this Drawing drawing, Layer layer, Entity entity)
        {
            var updatedLayer = layer.Add(entity);
            Debug.Assert(updatedLayer.EntityCount - layer.EntityCount == 1, "Expected to add 1 entity.");
            return drawing.Replace(layer, updatedLayer);
        }

        /// <summary>
        /// Adds an entity to the current drawing layer.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns></returns>
        public static Drawing AddToCurrentLayer(this Drawing drawing, Entity entity)
        {
            var newLayer = drawing.CurrentLayer.Add(entity);
            var newLayerSet = drawing.Layers.Delete(drawing.CurrentLayer.Name).Insert(newLayer.Name, newLayer);
            return drawing.Update(layers: newLayerSet);
        }

        /// <summary>
        /// Replaces the specified entity.
        /// </summary>
        /// <param name="oldEntity">The entity to be replaced.</param>
        /// <param name="newEntity">The replacement entity.</param>
        /// <returns>The new drawing with the entity replaced.</returns>
        public static Drawing Replace(this Drawing drawing, Entity oldEntity, Entity newEntity)
        {
            var layer = drawing.ContainingLayer(oldEntity);
            if (layer == null)
            {
                return drawing;
            }

            return drawing.Replace(layer, oldEntity, newEntity);
        }

        /// <summary>
        /// Replaces the entity in the specified layer.
        /// </summary>
        /// <param name="layer">The layer containing the entity.</param>
        /// <param name="oldEntity">The entity to be replaced.</param>
        /// <param name="newEntity">The replacement entity.</param>
        /// <returns>The new drawing with the entity replaced.</returns>
        public static Drawing Replace(this Drawing drawing, Layer layer, Entity oldEntity, Entity newEntity)
        {
            var updatedLayer = layer.Replace(oldEntity, newEntity);
            Debug.Assert(layer.EntityCount - updatedLayer.EntityCount == 1, "Expected the same number of entities.");
            return drawing.Replace(layer, updatedLayer);
        }


        /// <summary>
        /// Removes the entity from the layer.
        /// </summary>
        /// <param name="layer">The containing layer.</param>
        /// <param name="entity">The entity to remove.</param>
        /// <returns>The new drawing with the entity removed.</returns>
        public static Drawing Remove(this Drawing drawing, Layer layer, Entity entity)
        {
            var updatedLayer = layer.Remove(entity);
            Debug.Assert(layer.EntityCount - updatedLayer.EntityCount == 1, "Expected to remove 1 entity.");
            return drawing.Replace(layer, updatedLayer);
        }

        /// <summary>
        /// Removes the entity from the drawing.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        /// <param name="entity">The entity to remove.</param>
        /// <returns>The new drawing with the entity removed.</returns>
        public static Drawing Remove(this Drawing drawing, Entity entity)
        {
            var layer = drawing.ContainingLayer(entity);
            if (layer != null)
            {
                return drawing.Remove(layer, entity);
            }

            return drawing;
        }

        /// <summary>
        /// Returns the layer that contains the specified entity.
        /// </summary>
        /// <param name="entity">The entity to find.</param>
        /// <returns>The containing layer.</returns>
        public static Layer ContainingLayer(this Drawing drawing, Entity entity)
        {
            foreach (var layer in drawing.GetLayers())
            {
                if (layer.EntityExists(entity))
                    return layer;
            }

            return null;
        }

        /// <summary>
        /// Returns a collection of all entities in the drawing.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        /// <returns>A collection of all entities in the drawing.</returns>
        public static IEnumerable<Entity> GetEntities(this Drawing drawing)
        {
            return drawing.GetLayers().SelectMany(l => l.GetEntities());
        }

        /// <summary>
        /// Returns the entity specified by the ID.
        /// </summary>
        /// <param name="id">The ID of the entity.</param>
        /// <returns>The appropriate entity, or null.</returns>
        public static Entity GetEntityById(this Drawing drawing, uint id)
        {
            return drawing.GetLayers().Select(l => l.GetEntityById(id)).Where(e => e != null).SingleOrDefault();
        }

        /// <summary>
        /// Returns a renamed drawing.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        /// <param name="fileName">The new filename.</param>
        /// <returns></returns>
        public static Drawing Rename(this Drawing drawing, string fileName)
        {
            var newSettings = drawing.Settings.Update(fileName: fileName);
            return drawing.Update(settings: newSettings);
        }
    }
}
