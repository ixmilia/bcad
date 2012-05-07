using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.Entities;

namespace BCad
{
    public static class DocumentExtensions
    {
        /// <summary>
        /// Adds an object to the specified layer.
        /// </summary>
        /// <param name="layer">The layer to which to add the object.</param>
        /// <param name="entity">The object to add.</param>
        /// <returns>The new document with the layer added.</returns>
        public static Document Add(this Document document, Layer layer, Entity entity)
        {
            var updatedLayer = layer.Add(entity);
            return document.Replace(layer, updatedLayer);
        }

        /// <summary>
        /// Replaces the object in the specified layer.
        /// </summary>
        /// <param name="layer">The layer containing the object.</param>
        /// <param name="oldEntity">The object to be replaced.</param>
        /// <param name="newEntity">The replacement object.</param>
        /// <returns>The new document with the object replacedl</returns>
        public static Document Replace(this Document document, Layer layer, Entity oldEntity, Entity newEntity)
        {
            var updatedLayer = layer.Replace(oldEntity, newEntity);
            return document.Replace(layer, updatedLayer);
        }


        /// <summary>
        /// Removes the object from the layer.
        /// </summary>
        /// <param name="layer">The containing layer.</param>
        /// <param name="entity">The object to remove.</param>
        /// <returns>The new document with the object removed.</returns>
        public static Document Remove(this Document document, Layer layer, Entity entity)
        {
            var updatedLayer = layer.Remove(entity);
            return document.Replace(layer, updatedLayer);
        }

        /// <summary>
        /// Removes the object from the document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="entity">The object to remove.</param>
        /// <returns>The new document with the object removed.</returns>
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
        /// Returns the layer that contains the specified object.
        /// </summary>
        /// <param name="entity">The object to find.</param>
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
