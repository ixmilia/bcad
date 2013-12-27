using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BCad.Entities;
using BCad.Extensions;
using BCad.Helpers;

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

        public static ViewPort ShowAllViewPort(this Drawing drawing, Vector sight, Vector up, int viewPortWidth, int viewPortHeight)
        {
            int zoomPixelBuffer = 20;
            var drawingPlane = new Plane(Point.Origin, sight);
            var planeProjection = drawingPlane.ToXYPlaneProjection();
            var allPoints = drawing.GetEntities()
                .SelectMany(e => e.GetPrimitives())
                .SelectMany(p => p.GetInterestingPoints())
                .Select(p => planeProjection.Transform(p));

            if (allPoints.Count() < 2)
                return null;

            var first = allPoints.First();
            var minx = first.X;
            var miny = first.Y;
            var maxx = first.X;
            var maxy = first.Y;
            foreach (var point in allPoints.Skip(1))
            {
                if (point.X < minx)
                    minx = point.X;
                if (point.X > maxx)
                    maxx = point.X;
                if (point.Y < miny)
                    miny = point.Y;
                if (point.Y > maxy)
                    maxy = point.Y;
            }

            var deltaX = maxx - minx;
            var deltaY = maxy - miny;

            // translate back out of XY plane
            var unproj = planeProjection;
            unproj.Invert();
            var bottomLeft = unproj.Transform(new Point(minx, miny, 0));
            var topRight = unproj.Transform(new Point(minx + deltaX, miny + deltaY, 0));
            var drawingHeight = deltaY;
            var drawingWidth = deltaX;

            double viewHeight, drawingToViewScale;
            var viewRatio = (double)viewPortWidth / viewPortHeight;
            var drawingRatio = drawingWidth / drawingHeight;
            if (MathHelper.CloseTo(0.0, drawingHeight) || drawingRatio > viewRatio)
            {
                // fit to width
                var viewWidth = drawingWidth;
                viewHeight = viewPortHeight * viewWidth / viewPortWidth;
                drawingToViewScale = viewPortWidth / viewWidth;

                // add a buffer of some amount of pixels
                var pixelWidth = drawingWidth * drawingToViewScale;
                var newPixelWidth = pixelWidth + (zoomPixelBuffer * 2);
                viewHeight *= newPixelWidth / pixelWidth;
            }
            else
            {
                // fit to height
                viewHeight = drawingHeight;
                drawingToViewScale = viewPortHeight / viewHeight;

                // add a buffer of some amount of pixels
                var pixelHeight = drawingHeight * drawingToViewScale;
                var newPixelHeight = pixelHeight + (zoomPixelBuffer * 2);
                viewHeight *= newPixelHeight / pixelHeight;
            }

            // center viewport
            var tempViewport = new ViewPort(bottomLeft, sight, up, viewHeight);
            var pixelMatrix = tempViewport.GetTransformationMatrixWindowsStyle(viewPortWidth, viewPortHeight);
            var bottomLeftScreen = pixelMatrix.Transform(bottomLeft);
            var topRightScreen = pixelMatrix.Transform(topRight);

            // center horizontally
            var leftXGap = bottomLeftScreen.X;
            var rightXGap = viewPortWidth - topRightScreen.X;
            var xAdjust = Math.Abs((rightXGap - leftXGap) / 2.0) / drawingToViewScale;

            // center vertically
            var topYGap = topRightScreen.Y;
            var bottomYGap = viewPortHeight - bottomLeftScreen.Y;
            var yAdjust = Math.Abs((topYGap - bottomYGap) / 2.0) / drawingToViewScale;

            var newBottomLeft = new Point(bottomLeft.X - xAdjust, bottomLeft.Y - yAdjust, bottomLeft.Z);

            var newVp = tempViewport.Update(bottomLeft: newBottomLeft, viewHeight: viewHeight);
            return newVp;
        }
    }
}
