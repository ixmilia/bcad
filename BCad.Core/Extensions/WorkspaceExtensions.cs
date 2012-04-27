using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.Objects;

namespace BCad
{
    public static class WorkspaceExtensions
    {
        /// <summary>
        /// Add a layer to the document contained by a workspace.
        /// </summary>
        /// <param name="workspace">The workspace containing the document.</param>
        /// <param name="layer">The layer to add to the document.</param>
        public static void Add(this IWorkspace workspace, Layer layer)
        {
            var updatedDocument = workspace.Document.Add(layer);
            workspace.Document = updatedDocument;
        }

        /// <summary>
        /// Add an object to the specified layer.
        /// </summary>
        /// <param name="workspace">The workspace containing the document.</param>
        /// <param name="layer">The layer to which to add the object.</param>
        /// <param name="obj">The object to add.</param>
        public static void Add(this IWorkspace workspace, Layer layer, Entity obj)
        {
            var updatedDocument = workspace.Document.Add(layer, obj);
            workspace.Document = updatedDocument;
        }

        /// <summary>
        /// Add an object to the current layer.
        /// </summary>
        /// <param name="workspace">The workspace containing the document.</param>
        /// <param name="obj">The object to be added.</param>
        public static void AddToCurrentLayer(this IWorkspace workspace, Entity obj)
        {
            workspace.Add(workspace.CurrentLayer, obj);
        }

        /// <summary>
        /// Replace a layer in the document contained by a workspace.
        /// </summary>
        /// <param name="workspace">The workspace containing the document.</param>
        /// <param name="oldLayer">The layer to be replaced.</param>
        /// <param name="newLayer">The replacement layer.</param>
        public static void Replace(this IWorkspace workspace, Layer oldLayer, Layer newLayer)
        {
            var updatedDocument = workspace.Document.Replace(oldLayer, newLayer);
            workspace.Document = updatedDocument;
        }

        /// <summary>
        /// Replace the object in a specified layer.
        /// </summary>
        /// <param name="workspace">The workspace containing the document.</param>
        /// <param name="layer">The layer containing the objects.</param>
        /// <param name="oldObject">The object to be replaced.</param>
        /// <param name="newObject">The replacement object.</param>
        public static void Replace(this IWorkspace workspace, Layer layer, Entity oldObject, Entity newObject)
        {
            var updatedDocument = workspace.Document.Replace(layer, oldObject, newObject);
            workspace.Document = updatedDocument;
        }

        /// <summary>
        /// Remove a layer from the document contained by a workspace.
        /// </summary>
        /// <param name="workspace">The workspace containing the document.</param>
        /// <param name="layer">The layer to remove from the document.</param>
        public static void Remove(this IWorkspace workspace, Layer layer)
        {
            var updatedDocument = workspace.Document.Remove(layer);
            workspace.Document = updatedDocument;
        }

        /// <summary>
        /// Remove an object from the specified layer.
        /// </summary>
        /// <param name="workspace">The workspace containing the document.</param>
        /// <param name="layer">The layer containing the object.</param>
        /// <param name="obj">The object to be removed.</param>
        public static void Remove(this IWorkspace workspace, Layer layer, Entity obj)
        {
            var updatedDocument = workspace.Document.Remove(layer, obj);
            workspace.Document = updatedDocument;
        }

        /// <summary>
        /// Sets the current layer by name.
        /// </summary>
        /// <param name="workspace">The workspace containing the document.</param>
        /// <param name="layerName">The name of the desired current layer.</param>
        public static void SetCurrentLayer(this IWorkspace workspace, string layerName)
        {
            var layer = workspace.Document.Layers[layerName];
            workspace.CurrentLayer = layer;
        }
    }
}
