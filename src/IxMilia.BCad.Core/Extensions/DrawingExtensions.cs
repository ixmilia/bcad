using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using IxMilia.BCad.Collections;
using IxMilia.BCad.Display;
using IxMilia.BCad.Entities;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.SnapPoints;
using IxMilia.BCad.Utilities;

namespace IxMilia.BCad
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
            Debug.Assert(layer.EntityCount - updatedLayer.EntityCount == 0, "Expected the same number of entities.");
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

        /// <summary>
        /// Formats the specified unit as the appropriate format.
        /// </summary>
        public static string FormatUnits(this Drawing drawing, double value) => DrawingSettings.FormatUnits(value, drawing.Settings.UnitFormat, drawing.Settings.UnitPrecision);

        /// <summary>
        /// Get all transformed snap points for the drawing with the given display transform.
        /// </summary>
        public static QuadTree<TransformedSnapPoint> GetSnapPoints(this Drawing drawing, Matrix4 displayTransform, double width, double height, CancellationToken cancellationToken = default)
        {
            // populate the snap points
            var transformedQuadTree = new QuadTree<TransformedSnapPoint>(new Rect(0, 0, width, height), t => new Rect(t.ControlPoint.X, t.ControlPoint.Y, 0.0, 0.0));
            foreach (var layer in drawing.GetLayers(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (var entity in layer.GetEntities(cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    foreach (var snapPoint in entity.GetSnapPoints())
                    {
                        var projected = displayTransform.Transform(snapPoint.Point);
                        transformedQuadTree.AddItem(new TransformedSnapPoint(snapPoint.Point, projected, snapPoint.Kind));
                    }
                }
            }

            // calculate intersections
            // TODO: can this be done in a meanintful way in a separate task?
            var primitives = drawing.GetEntities().SelectMany(e => e.GetPrimitives()).ToList();
            for (int startIndex = 0; startIndex < primitives.Count; startIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var first = primitives[startIndex];
                for (int i = startIndex + 1; i < primitives.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var current = primitives[i];
                    foreach (var intersection in first.IntersectionPoints(current))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var projected = displayTransform.Transform(intersection);
                        transformedQuadTree.AddItem(new TransformedSnapPoint(intersection, projected, SnapPointKind.Intersection));
                    }
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            return transformedQuadTree;
        }

        /// <summary>
        /// Combine the specified entities into a Polyline entities and add to the drawing.  The original entities will be removed.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        /// <param name="entities">The entities to combine.</param>
        /// <param name="layerName">The name of the layer to which the final Polylines will be added.</param>
        public static Drawing CombineEntitiesIntoPolyline(this Drawing drawing, IEnumerable<Entity> entities, string layerName)
        {
            foreach (var entity in entities)
            {
                drawing = drawing.Remove(entity);
            }

            var primitives = entities.SelectMany(e => e.GetPrimitives());
            var polylines = primitives.GetPolylinesFromSegments();

            var polyLayer = drawing.Layers.GetValue(layerName);
            drawing = drawing.Remove(polyLayer);
            foreach (var poly in polylines)
            {
                polyLayer = polyLayer.Add(poly);
            }

            return drawing.Add(polyLayer);
        }

        public static Rect GetExtents(this Drawing drawing, Vector sight)
        {
            return drawing.GetEntities().SelectMany(e => e.GetPrimitives()).GetExtents(sight);
        }

        public static ViewPort ShowAllViewPort(this Drawing drawing, Vector sight, Vector up, double viewPortWidth, double viewPortHeight, double viewportBuffer = PrimitiveExtensions.DefaultViewportBuffer)
        {
            return drawing.GetExtents(sight).ShowAllViewPort(sight, up, viewPortWidth, viewPortHeight, viewportBuffer);
        }

        public static Layer MapEntities(this Layer layer, Func<Entity, Entity> mapper)
        {
            return layer.Update(entities: ReadOnlyTree<uint, Entity>.FromEnumerable(layer.GetEntities().Select(e => mapper(e)), e => e.Id));
        }

        public static Drawing MapLayers(this Drawing drawing, Func<Layer, Layer> mapper)
        {
            return drawing.Update(layers: ReadOnlyTree<string, Layer>.FromEnumerable(drawing.GetLayers().Select(l => mapper(l)), l => l.Name));
        }

        public static Drawing Map(this Drawing drawing, Func<Layer, Layer> layerMapper, Func<Entity, Entity> entityMapper)
        {
            return drawing.MapLayers(l => layerMapper(l.MapEntities(entityMapper)));
        }

        public static Drawing ScaleEntities(this Drawing drawing, IEnumerable<Entity> entities, Point basePoint, double scaleFactor)
        {
            var result = drawing;
            foreach (var e in entities)
            {
                var layer = result.ContainingLayer(e);
                var scaledEntity = EditUtilities.Scale(e, basePoint, scaleFactor);
                var updatedLayer = layer.Remove(e).Add(scaledEntity);
                result = result.Remove(layer).Add(updatedLayer);
            }

            return result;
        }
    }
}
