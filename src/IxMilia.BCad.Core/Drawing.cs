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
        private readonly ReadOnlyTree<string, LineType> lineTypes;
        private readonly string author;

        public DrawingSettings Settings => settings;

        public ReadOnlyTree<string, Layer> Layers => layers;

        public ReadOnlyTree<string, LineType> LineTypes => lineTypes;

        public string Author => author;

        public object Tag { get; set; }

        public Drawing()
            : this(new DrawingSettings(), new ReadOnlyTree<string, Layer>().Insert("0", new Layer("0")), new ReadOnlyTree<string, LineType>())
        {
        }

        public Drawing(DrawingSettings settings)
            : this(settings, new ReadOnlyTree<string, Layer>().Insert("0", new Layer("0")), new ReadOnlyTree<string, LineType>())
        {
        }

        public Drawing(DrawingSettings settings, ReadOnlyTree<string, Layer> layers, ReadOnlyTree<string, LineType> lineTypes)
            : this(settings, layers, lineTypes, null)
        {
        }

        public Drawing(DrawingSettings settings, ReadOnlyTree<string, Layer> layers, ReadOnlyTree<string, LineType> lineTypes, string author)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.layers = layers ?? throw new ArgumentNullException(nameof(layers));
            this.lineTypes = lineTypes ?? throw new ArgumentNullException(nameof(lineTypes));
            this.author = author;

            if (layers.Count == 0)
            {
                throw new ArgumentException("At least one layer must be specified.");
            }
            
            if (!layers.KeyExists(settings.CurrentLayerName))
            {
                throw new InvalidOperationException("The current layer is not part of the layers collection.");
            }

            if (settings.CurrentLineTypeSpecification is not null &&
                !lineTypes.KeyExists(settings.CurrentLineTypeSpecification.Name))
            {
                throw new InvalidOperationException("The current line type is not part of the line types collection.");
            }
        }

        public IEnumerable<Layer> GetLayers(CancellationToken cancellationToken = default)
        {
            return this.layers.GetValues(cancellationToken);
        }

        public IEnumerable<LineType> GetLineTypes(CancellationToken cancellationToken = default)
        {
            return this.lineTypes.GetValues(cancellationToken);
        }

        /// <summary>
        /// Determines if the specified layer is part of the drawing.
        /// </summary>
        /// <param name="layer">The layer to find.</param>
        /// <returns>If the layer was found on the drawing.</returns>
        public bool LayerExists(Layer layer)
        {
            if (Layers.TryFind(layer.Name, out var found))
            {
                return found == layer;
            }

            return false;
        }

        public bool LineTypeExists(LineType lineType)
        {
            if (LineTypes.TryFind(lineType.Name, out var found))
            {
                return found == lineType;
            }

            return false;
        }

        /// <summary>
        /// Adds the layer to the drawing.
        /// </summary>
        /// <param name="layer">The layer to add.</param>
        /// <returns>The updated drawing.</returns>
        public Drawing Add(Layer layer)
        {
            return this.Update(layers: layers.Insert(layer.Name, layer));
        }

        public Drawing Add(LineType lineType)
        {
            return this.Update(lineTypes: LineTypes.Insert(lineType.Name, lineType));
        }

        /// <summary>
        /// Removes the layer from the drawing.
        /// </summary>
        /// <param name="layer">The layer to remove.</param>
        /// <returns>The new drawing with the layer removed.</returns>
        public Drawing Remove(Layer layer)
        {
            if (!LayerExists(layer))
            {
                throw new ArgumentException("The drawing does not contain the specified layer");
            }

            return Update(layers: Layers.Delete(layer.Name));
        }

        public Drawing Remove(LineType lineType)
        {
            if (!LineTypeExists(lineType))
            {
                throw new ArgumentException("The drawing does not contain the specified line type");
            }

            return Update(lineTypes: LineTypes.Delete(lineType.Name));
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
            {
                throw new ArgumentException("The drawing does not contain the specified layer");
            }

            return Update(layers: Layers.Delete(oldLayer.Name).Insert(newLayer.Name, newLayer));
        }
        
        public Drawing Replace(LineType oldLineType, LineType newLineType)
        {
            if (!LineTypeExists(oldLineType))
            {
                throw new ArgumentException("The drawing does not contain the specified line type");
            }

            return Update(lineTypes: LineTypes.Delete(oldLineType.Name).Insert(newLineType.Name, newLineType));
        }

        /// <summary>
        /// Updates the drawing with the specified changes.
        /// </summary>
        /// <param name="settings">The drawing settings.</param>
        /// <param name="layers">The layers for the drawing.</param>
        /// <param name="currentLayerName">The name of the current layer.</param>
        /// <returns>The new drawing with the specified updates.</returns>
        public Drawing Update(
            DrawingSettings settings = null,
            ReadOnlyTree<string, Layer> layers = null,
            ReadOnlyTree<string, LineType> lineTypes = null,
            string author = null)
        {
            var newLayers = layers ?? this.layers;
            if (newLayers.Count == 0)
            {
                // ensure there is always at least one layer
                newLayers = newLayers.Insert("0", new Layer("0"));
            }

            var newLineTypes = lineTypes ?? LineTypes;
            var newSettings = settings ?? Settings;

            // ensure current layer is still part of the layers collection
            if (!newLayers.KeyExists(newSettings.CurrentLayerName))
            {
                newSettings = newSettings.Update(currentLayerName: newLayers.GetKeys().OrderBy(x => x).First());
            }

            return new Drawing(
                newSettings,
                newLayers,
                newLineTypes,
                author ?? this.author)
            {
                Tag = this.Tag
            };
        }
    }
}
