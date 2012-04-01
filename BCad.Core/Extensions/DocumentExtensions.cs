using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.Objects;

namespace BCad
{
    public static class DocumentExtensions
    {
        /// <summary>
        /// Adds an object to the specified layer.
        /// </summary>
        /// <param name="layer">The layer to which to add the object.</param>
        /// <param name="obj">The object to add.</param>
        /// <returns>The new document with the layer added.</returns>
        public static Document Add(this Document document, Layer layer, IObject obj)
        {
            var updatedLayer = layer.Add(obj);
            return document.Replace(layer, updatedLayer);
        }

        /// <summary>
        /// Replaces the object in the specified layer.
        /// </summary>
        /// <param name="layer">The layer containing the object.</param>
        /// <param name="oldObject">The object to be replaced.</param>
        /// <param name="newObject">The replacement object.</param>
        /// <returns>The new document with the object replacedl</returns>
        public static Document Replace(this Document document, Layer layer, IObject oldObject, IObject newObject)
        {
            var updatedLayer = layer.Replace(oldObject, newObject);
            return document.Replace(layer, updatedLayer);
        }


        /// <summary>
        /// Removes the object from the layer.
        /// </summary>
        /// <param name="layer">The containing layer.</param>
        /// <param name="obj">The object to remove.</param>
        /// <returns>The new document with the object removed.</returns>
        public static Document Remove(this Document document, Layer layer, IObject obj)
        {
            var updatedLayer = layer.Remove(obj);
            return document.Replace(layer, updatedLayer);
        }
    }
}
