using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using IxMilia.BCad.Collections;

namespace IxMilia.BCad
{
    public class Drawing
    {
        private readonly DrawingSettings settings;
        private readonly ReadOnlyTree<string, Layer> layers;
        private readonly string currentLayerName;
        private readonly string author;

        public DrawingSettings Settings { get { return settings; } }

        public string CurrentLayerName { get { return currentLayerName; } }

        public Layer CurrentLayer { get { return this.layers.GetValue(this.currentLayerName); } }

        public ReadOnlyTree<string, Layer> Layers { get { return this.layers; } }

        public string Author { get { return author; } }

        public object Tag { get; set; }

        public Drawing()
            : this(new DrawingSettings(), new ReadOnlyTree<string, Layer>().Insert("0", new Layer("0")), "0", null)
        {
        }

        public Drawing(DrawingSettings settings)
            : this(settings, new ReadOnlyTree<string, Layer>().Insert("0", new Layer("0")))
        {
        }

        public Drawing(DrawingSettings settings, ReadOnlyTree<string, Layer> layers)
            : this(settings, layers, layers.GetKeys().OrderBy(x => x).First(), null)
        {
        }

        public Drawing(DrawingSettings settings, ReadOnlyTree<string, Layer> layers, string author)
            : this(settings, layers, layers.GetKeys().OrderBy(x => x).First(), author)
        {
        }

        public Drawing(DrawingSettings settings, ReadOnlyTree<string, Layer> layers, string currentLayerName, string author)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");
            if (layers == null)
                throw new ArgumentNullException("layers");
            if (layers.Count == 0)
                throw new ArgumentException("At least one layer must be specified.");
            if (currentLayerName == null)
                throw new ArgumentNullException("currentLayerName");
            if (!layers.KeyExists(currentLayerName))
                throw new InvalidOperationException("The current layer is not part of the layers collection.");
            this.settings = settings;
            this.layers = layers;
            this.currentLayerName = currentLayerName;
            this.author = author;
        }

        public IEnumerable<Layer> GetLayers(CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.layers.GetValues(cancellationToken);
        }

        /// <summary>
        /// Determines if the specified layer is part of the drawing.
        /// </summary>
        /// <param name="layer">The layer to find.</param>
        /// <returns>If the layer was found on the drawing.</returns>
        public bool LayerExists(Layer layer)
        {
            Layer found;
            if (this.layers.TryFind(layer.Name, out found))
                return found == layer;
            return false;
        }

        /// <summary>
        /// Adds the layer to the drawing.
        /// </summary>
        /// <param name="layer">The layer to add.</param>
        /// <returns>The updated drawing.</returns>
        public Drawing Add(Layer layer)
        {
            return this.Update(layers: this.layers.Insert(layer.Name, layer));
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
            return this.Update(layers: this.layers.Delete(layer.Name));
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
            return this.Update(layers: this.layers.Delete(oldLayer.Name).Insert(newLayer.Name, newLayer));
        }

        /// <summary>
        /// Updates the drawing with the specified changes.
        /// </summary>
        /// <param name="settings">The drawing settings.</param>
        /// <param name="layers">The layers for the drawing.</param>
        /// <param name="currentLayerName">The name of the current layer.</param>
        /// <returns>The new drawing with the specified updates.</returns>
        public Drawing Update(DrawingSettings settings = null, ReadOnlyTree<string, Layer> layers = null, string currentLayerName = null, string author = null)
        {
            var newLayers = layers ?? this.layers;
            if (newLayers.Count == 0)
            {
                // ensure there is always at least one layer
                newLayers = newLayers.Insert("0", new Layer("0"));
            }

            var newCurrentName = currentLayerName ?? this.currentLayerName;
            if (!newLayers.KeyExists(newCurrentName))
            {
                // ensure the current layer is available
                newCurrentName = newLayers.GetKeys().OrderBy(x => x).First();
            }

            return new Drawing(
                settings ?? this.settings,
                newLayers,
                newCurrentName,
                author ?? this.author)
            {
                Tag = this.Tag
            };
        }
    }
}
