using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using BCad.Collections;
using BCad.Entities;
using BCad.EventArguments;

namespace BCad
{
    public class Drawing
    {
        private readonly DrawingSettings settings;
        private readonly ReadOnlyDictionary<string, Layer> layers;
        private readonly Layer currentLayer;

        public ReadOnlyDictionary<string, Layer> Layers { get { return layers; } }

        public DrawingSettings Settings { get { return settings; } }

        public Layer CurrentLayer { get { return currentLayer; } }

        public Drawing()
            : this(new DrawingSettings())
        {
        }

        public Drawing(DrawingSettings settings)
            : this(settings, new ReadOnlyDictionary<string, Layer>().Add("0", new Layer("0", Color.Auto)))
        {
        }

        public Drawing(DrawingSettings settings, ReadOnlyDictionary<string, Layer> layers)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");
            if (layers == null)
                throw new ArgumentNullException("layers");
            if (!layers.Any())
                throw new ArgumentException("At least one layer must be specified.");
            this.settings = settings;
            this.layers = layers;
            this.currentLayer = layers.First().Value;
        }

        public Drawing(DrawingSettings settings, ReadOnlyDictionary<string, Layer> layers, Layer currentLayer)
            : this(settings, layers)
        {
            if (!layers.Values.Contains(currentLayer))
                throw new ArgumentException("The current layer is not part of the layer set.");
            this.currentLayer = currentLayer;
        }

        /// <summary>
        /// Determines if the specified layer is part of the drawing.
        /// </summary>
        /// <param name="layer">The layer to find.</param>
        /// <returns>If the layer was found on the drawing.</returns>
        public bool LayerExists(Layer layer)
        {
            return this.Layers.ContainsKey(layer.Name) && this.Layers[layer.Name] == layer;
        }

        /// <summary>
        /// Adds the layer to the drawing.
        /// </summary>
        /// <param name="layer">The layer to add.</param>
        /// <returns>The updated drawing.</returns>
        public Drawing Add(Layer layer)
        {
            var newLayers = this.Layers.Add(layer.Name, layer);
            return this.Update(layers: newLayers);
        }

        /// <summary>
        /// Removes the layer from the drawing.
        /// </summary>
        /// <param name="layer">The layer to remove.</param>
        /// <returns>The new drawing with the layer removed.</returns>
        public Drawing Remove(Layer layer)
        {
            if (!LayerExists(layer))
                throw new ArgumentException("The drawing does not contain the specified layer");
            var newLayers = this.Layers.Remove(layer.Name);
            return this.Update(layers: newLayers);
        }

        /// <summary>
        /// Replaces a layer in the drawing.
        /// </summary>
        /// <param name="oldLayer">The layer to be replaced.</param>
        /// <param name="newLayer">The replacement layer.</param>
        /// <returns>The new drawing with the layer replaced.</returns>
        public Drawing Replace(Layer oldLayer, Layer newLayer)
        {
            if (!LayerExists(oldLayer))
                throw new ArgumentException("The drawing does not contain the specified layer");
            var newLayers = this.Layers.Remove(oldLayer.Name).Add(newLayer.Name, newLayer);
            return this.Update(layers: newLayers);
        }

        /// <summary>
        /// Updates the drawing with the specified changes.
        /// </summary>
        /// <param name="settings">The drawing settings.</param>
        /// <param name="layers">The layers for the drawing.</param>
        /// <param name="currentLayerName">The name of the current layer.</param>
        /// <returns>The new drawing with the specified updates.</returns>
        public Drawing Update(DrawingSettings settings = null, ReadOnlyDictionary<string, Layer> layers = null, string currentLayerName = null)
        {
            var newLayers = layers ?? this.Layers;
            if (newLayers.Count == 0)
            {
                // ensure there is always at least one layer
                newLayers = newLayers.Add("0", new Layer("0", Color.Auto));
            }

            var newCurrentName = currentLayerName ?? this.currentLayer.Name;
            if (!newLayers.ContainsKey(newCurrentName))
            {
                // ensure the current layer is available
                newCurrentName = newLayers.Keys.OrderBy(x => x).First();
            }

            return new Drawing(
                settings ?? this.settings,
                newLayers,
                newLayers[newCurrentName]);
        }
    }
}
