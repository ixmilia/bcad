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
        /// Add an entity to the current layer.
        /// </summary>
        /// <param name="workspace">The workspace containing the drawing.</param>
        /// <param name="entity">The entity to be added.</param>
        public static void AddToCurrentLayer(this IWorkspace workspace, Entity entity)
        {
            workspace.Add(workspace.CurrentLayer, entity);
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
            var layer = workspace.Drawing.Layers[layerName];
            workspace.CurrentLayer = layer;
        }

        /// <summary>
        /// Returns the normal to the current drawing plane.
        /// </summary>
        /// <param name="workspace">The workspace object.</param>
        /// <returns>The normal to the current drawing plane.</returns>
        public static Vector DrawingPlaneNormal(this IWorkspace workspace)
        {
            Vector normal = null;
            switch (workspace.DrawingPlane)
            {
                case DrawingPlane.XY:
                    normal = Vector.ZAxis;
                    break;
                case DrawingPlane.XZ:
                    normal = Vector.YAxis;
                    break;
                case DrawingPlane.YZ:
                    normal = Vector.XAxis;
                    break;
                default:
                    throw new InvalidOperationException("Unsupported drawing plane " + workspace.DrawingPlane);
            }

            return normal;
        }

        /// <summary>
        /// Returns if the entity is on the current drawing plane.
        /// </summary>
        /// <param name="workspace">The workspace object.</param>
        /// <param name="entity">The entity to verify.</param>
        /// <returns>If the entity is on the current drawing plane.</returns>
        public static bool IsEntityOnDrawingPlane(this IWorkspace workspace, Entity entity)
        {
            var points = new List<Point>();
            Vector entityNormal;
            switch (entity.Kind)
            {
                case EntityKind.Arc:
                    var arc = (Arc)entity;
                    points.Add(arc.Center);
                    entityNormal = arc.Normal;
                    break;
                case EntityKind.Circle:
                    var circle = (Circle)entity;
                    points.Add(circle.Center);
                    entityNormal = circle.Normal;
                    break;
                case EntityKind.Ellipse:
                    var el = (Ellipse)entity;
                    points.Add(el.Center);
                    points.Add(el.Center + el.MajorAxis);
                    entityNormal = el.Normal;
                    break;
                case EntityKind.Line:
                    var line = (Line)entity;
                    points.Add(line.P1);
                    points.Add(line.P2);
                    entityNormal = workspace.DrawingPlaneNormal(); // this is always correct
                    break;
                case EntityKind.Polyline:
                    var poly = (Polyline)entity;
                    points.AddRange(poly.Points);
                    entityNormal = workspace.DrawingPlaneNormal(); // this is always correct
                    break;
                case EntityKind.Text:
                    var text = (Text)entity;
                    points.Add(text.Location);
                    entityNormal = text.Normal;
                    break;
                default:
                    throw new Exception("Unrecognized entity type");
            }

            bool pointsCorrect;
            switch (workspace.DrawingPlane)
            {
                case DrawingPlane.XY:
                    // z-values must be the same
                    pointsCorrect = points.All(p => p.Z == workspace.DrawingPlaneOffset);
                    break;
                case DrawingPlane.XZ:
                    // y-values must be the same
                    pointsCorrect = points.All(p => p.Y == workspace.DrawingPlaneOffset);
                    break;
                case DrawingPlane.YZ:
                    // x-values must be the same
                    pointsCorrect = points.All(p => p.X == workspace.DrawingPlaneOffset);
                    break;
                default:
                    throw new Exception("Unrecognized drawing plane");
            }

            return pointsCorrect && entityNormal == workspace.DrawingPlaneNormal();
        }

        /// <summary>
        /// Returns true if the point is on the current drawing plane.
        /// </summary>
        /// <param name="workspace">The workspace object.</param>
        /// <param name="point">The point to verify.</param>
        /// <returns>True if the point is on the current drawing plane.</returns>
        public static bool IsPointOnDrawingPlane(this IWorkspace workspace, Point point)
        {
            double value;
            switch (workspace.DrawingPlane)
            {
                case DrawingPlane.XY:
                    value = point.Z;
                    break;
                case DrawingPlane.XZ:
                    value = point.Y;
                    break;
                case DrawingPlane.YZ:
                    value = point.X;
                    break;
                default:
                    throw new Exception("Unrecognized drawing plane");
            }

            return value == workspace.DrawingPlaneOffset;
        }

        /// <summary>
        /// Transforms the specified point to the XY drawing plane.
        /// </summary>
        /// <param name="workspace">The workspace object.</param>
        /// <param name="point">The point to transform.</param>
        /// <returns>The point in the XY drawing plane.</returns>
        public static Point ToXYPlane(this IWorkspace workspace, Point point)
        {
            Point result;
            switch (workspace.DrawingPlane)
            {
                case DrawingPlane.XY:
                    result = point;
                    break;
                case DrawingPlane.XZ:
                    result = new Point(point.X, point.Z, point.Y);
                    break;
                case DrawingPlane.YZ:
                    result = new Point(point.Y, point.Z, point.X);
                    break;
                default:
                    throw new Exception("Unrecognized drawing plane");
            }

            return result;
        }

        /// <summary>
        /// Transforms the specified point from the XY drawing plane back to the current drawing plane.
        /// </summary>
        /// <param name="workspace">The workspace object.</param>
        /// <param name="vector">The point to transform.</param>
        /// <returns>The point in the current drawing plane.</returns>
        public static Vector FromXYPlane(this IWorkspace workspace, Vector vector)
        {
            Vector result;
            switch (workspace.DrawingPlane)
            {
                case DrawingPlane.XY:
                    result = vector;
                    break;
                case DrawingPlane.XZ:
                    result = new Vector(vector.X, vector.Z, vector.Y);
                    break;
                case DrawingPlane.YZ:
                    result = new Vector(vector.Z, vector.X, vector.Y);
                    break;
                default:
                    throw new Exception("Unrecognized drawing plane");
            }

            return result;
        }
    }
}
