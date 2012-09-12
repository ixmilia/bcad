using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using BCad.Entities;
using BCad.EventArguments;

namespace BCad
{
    public class Drawing
    {
        private readonly DrawingSettings settings;
        private readonly Dictionary<string, Layer> layers;

        public Dictionary<string, Layer> Layers { get { return layers; } }

        public DrawingSettings Settings { get { return settings; } }

        public Drawing()
            : this(new DrawingSettings())
        {
        }

        public Drawing(DrawingSettings settings)
            : this(settings, new Dictionary<string, Layer>() { {"0", new Layer("0", Color.Auto) } })
        {
        }

        public Drawing(DrawingSettings settings, IDictionary<string, Layer> layers)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");
            if (layers == null)
                throw new ArgumentNullException("layers");
            if (!layers.Any())
                throw new ArgumentException("At least one layer must be specified.");
            this.settings = settings;
            this.layers = new Dictionary<string, Layer>(layers);
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
            var newLayers = new Dictionary<string, Layer>(this.Layers) { { layer.Name, layer } };
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
            if (this.Layers.Count == 1)
                throw new NotSupportedException("The last layer cannot be removed");
            var newLayers = new Dictionary<string, Layer>(this.Layers);
            newLayers.Remove(layer.Name);
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
            var newLayers = new Dictionary<string, Layer>(this.Layers);
            newLayers.Remove(oldLayer.Name);
            newLayers.Add(newLayer.Name, newLayer);
            return this.Update(layers: newLayers);
        }

        /// <summary>
        /// Updates the drawing with the specified changes.
        /// </summary>
        /// <param name="settings">The drawing settings.</param>
        /// <param name="layers">The layers for the drawing.</param>
        /// <returns>The new drawing with the specified updates.</returns>
        public Drawing Update(DrawingSettings settings = null, IDictionary<string, Layer> layers = null)
        {
            return new Drawing(
                settings ?? this.settings,
                layers ?? this.Layers);
        }
    }
}
