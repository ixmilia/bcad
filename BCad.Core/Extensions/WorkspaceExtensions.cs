using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.Entities;

namespace BCad
{
    public static class WorkspaceExtensions
    {
        /// <summary>
        /// Add a layer to the drawing contained by a workspace.
        /// </summary>
        /// <param name="workspace">The workspace containing the drawing.</param>
        /// <param name="layer">The layer to add to the drawing.</param>
        public static void Add(this IWorkspace workspace, Layer layer)
        {
            var updatedDrawing = workspace.Drawing.Add(layer);
            workspace.Drawing = updatedDrawing;
        }

        /// <summary>
        /// Add an entity to the specified layer.
        /// </summary>
        /// <param name="workspace">The workspace containing the drawing.</param>
        /// <param name="layer">The layer to which to add the entity.</param>
        /// <param name="entity">The entity to add.</param>
        public static void Add(this IWorkspace workspace, Layer layer, Entity entity)
        {
            var updatedDrawing = workspace.Drawing.Add(layer, entity);
            workspace.Drawing = updatedDrawing;
        }

        /// <summary>
        /// Add an entity to the current drawing layer.
        /// </summary>
        /// <param name="workspace">The workspace containing the drawing.</param>
        /// <param name="entity">The entity to add.</param>
        public static void AddToCurrentLayer(this IWorkspace workspace, Entity entity)
        {
            workspace.Drawing = workspace.Drawing.AddToCurrentLayer(entity);
        }

        /// <summary>
        /// Replace a layer in the drawing contained by a workspace.
        /// </summary>
        /// <param name="workspace">The workspace containing the drawing.</param>
        /// <param name="oldLayer">The layer to be replaced.</param>
        /// <param name="newLayer">The replacement layer.</param>
        public static void Replace(this IWorkspace workspace, Layer oldLayer, Layer newLayer)
        {
            var updatedDrawing = workspace.Drawing.Replace(oldLayer, newLayer);
            workspace.Drawing = updatedDrawing;
        }

        /// <summary>
        /// Replace the entity in a specified layer.
        /// </summary>
        /// <param name="workspace">The workspace containing the drawing.</param>
        /// <param name="layer">The layer containing the entities.</param>
        /// <param name="oldEntity">The entity to be replaced.</param>
        /// <param name="newEntity">The replacement entity.</param>
        public static void Replace(this IWorkspace workspace, Layer layer, Entity oldEntity, Entity newEntity)
        {
            var updatedDrawing = workspace.Drawing.Replace(layer, oldEntity, newEntity);
            workspace.Drawing = updatedDrawing;
        }

        /// <summary>
        /// Remove a layer from the drawing contained by a workspace.
        /// </summary>
        /// <param name="workspace">The workspace containing the drawing.</param>
        /// <param name="layer">The layer to remove from the drawing.</param>
        public static void Remove(this IWorkspace workspace, Layer layer)
        {
            var updatedDrawing = workspace.Drawing.Remove(layer);
            workspace.Drawing = updatedDrawing;
        }

        /// <summary>
        /// Remove an entity from the specified layer.
        /// </summary>
        /// <param name="workspace">The workspace containing the drawing.</param>
        /// <param name="layer">The layer containing the entity.</param>
        /// <param name="entity">The entity to be removed.</param>
        public static void Remove(this IWorkspace workspace, Layer layer, Entity entity)
        {
            var updatedDrawing = workspace.Drawing.Remove(layer, entity);
            workspace.Drawing = updatedDrawing;
        }

        /// <summary>
        /// Sets the current layer by name.
        /// </summary>
        /// <param name="workspace">The workspace containing the drawing.</param>
        /// <param name="layerName">The name of the desired current layer.</param>
        public static void SetCurrentLayer(this IWorkspace workspace, string layerName)
        {
            workspace.Drawing = workspace.Drawing.Update(currentLayerName: layerName);
        }
    }
}
