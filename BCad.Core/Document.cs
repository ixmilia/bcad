using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using BCad.EventArguments;
using BCad.Objects;

namespace BCad
{
    public class Document
    {
        private HashSet<Layer> layers = new HashSet<Layer>();
        private Layer currentLayer = null;

        public IEnumerable<Layer> Layers { get { return layers; } }

        public Layer CurrentLayer
        {
            get { return currentLayer; }
            set
            {
                if (currentLayer == value)
                    return;
                if (!layers.Contains(value))
                    throw new ArgumentException("The layer must be added first");
                currentLayer = value;
                OnCurrentLayerChanged(new CurrentLayerChangedEventArgs(currentLayer));
            }
        }

        public Layer DefaultLayer { get { return layers.First(); } }

        public Layer GetLayerByName(string name)
        {
            return layers.SingleOrDefault(x => x.Name == name);
        }

        public Document()
        {
            layers.Add(new Layer("Default", Color.Auto));
            currentLayer = DefaultLayer;
        }

        public void AddObject(IObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (obj.Layer == null)
                throw new ArgumentException("Layer cannot be null");
            if (!layers.Contains(obj.Layer))
                throw new ArgumentException("Invalid layer");
            obj.Layer.AddObject(obj);
            Dirty = true;
            OnObjectAdded(new ObjectAddedEventArgs(obj));
        }

        public void RemoveObject(IObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (obj.Layer == null)
                throw new ArgumentException("Layer cannot be null");
            if (!layers.Contains(obj.Layer))
                throw new ArgumentException("Invalid layer");
            obj.Layer.RemoveObject(obj);
            Dirty = true;
            OnObjectRemoved(new ObjectRemovedEventArgs(obj));
        }

        public void AddLayer(Layer layer)
        {
            if (layer == null)
                throw new ArgumentNullException("layer");
            if (layers.Contains(layer))
                throw new ArgumentException("Duplicate layer");
            if (layers.Count(x => x.Name == layer.Name) > 0)
                throw new ArgumentException("Duplicate layer name");
            layers.Add(layer);
            Dirty = true;
            OnLayerAdded(new LayerAddedEventArgs(layer));
        }

        public void RemoveLayer(Layer layer)
        {
            if (layer == null)
                throw new ArgumentNullException("layer");
            if (!layers.Contains(layer))
                throw new ArgumentException("Invalid layer");
            if (layer == DefaultLayer)
                throw new ArgumentException("Cannot remove the default layer");
            layers.Remove(layer);
            Dirty = true;
            OnLayerRemoved(new LayerRemovedEventArgs(layer));
            if (layer == currentLayer)
                CurrentLayer = DefaultLayer;
        }

        public void UpdateLayer(Layer oldLayer, Layer newLayer)
        {
            if (oldLayer == null)
                throw new ArgumentNullException("oldLayer");
            if (newLayer == null)
                throw new ArgumentNullException("newLayer");
            if (!layers.Contains(oldLayer))
                throw new ArgumentException("Invalid layer");
            if (layers.Contains(newLayer))
                throw new ArgumentException("Invalid layer");
            layers.Remove(oldLayer);
            layers.Add(newLayer);
            Dirty = true;
            OnLayerUpdated(new LayerUpdatedEventArgs(oldLayer, newLayer));
            if (currentLayer == oldLayer)
                CurrentLayer = newLayer;
        }

        private string filename = null;

        public string FileName
        {
            get { return filename; }
            set
            {
                if (filename == value)
                    return;
                filename = value;
                OnDocumentDetailsChanged(new DocumentDetailsChangedEventArgs(this));
            }
        }

        public event CurrentLayerChangedEventHandler CurrentLayerChanged;

        protected virtual void OnCurrentLayerChanged(CurrentLayerChangedEventArgs e)
        {
            if (CurrentLayerChanged != null)
                CurrentLayerChanged(this, e);
        }

        public event ObjectAddedEventHandler ObjectAdded;

        protected virtual void OnObjectAdded(ObjectAddedEventArgs e)
        {
            if (ObjectAdded != null)
                ObjectAdded(this, e);
        }

        public event ObjectRemovedEventHandler ObjectRemoved;

        protected virtual void OnObjectRemoved(ObjectRemovedEventArgs e)
        {
            if (ObjectRemoved != null)
                ObjectRemoved(this, e);
        }

        public event LayerAddedEventHandler LayerAdded;

        protected virtual void OnLayerAdded(LayerAddedEventArgs e)
        {
            if (LayerAdded != null)
                LayerAdded(this, e);
        }

        public event LayerUpdatedEventHandler LayerUpdated;

        protected virtual void OnLayerUpdated(LayerUpdatedEventArgs e)
        {
            if (LayerUpdated != null)
                LayerUpdated(this, e);
        }

        public event LayerRemovedEventHandler LayerRemoved;

        protected virtual void OnLayerRemoved(LayerRemovedEventArgs e)
        {
            if (LayerRemoved != null)
                LayerRemoved(this, e);
        }

        public event DocumentDetailsChangedEventHandler DocumentDetailsChanged;

        protected virtual void OnDocumentDetailsChanged(DocumentDetailsChangedEventArgs e)
        {
            if (DocumentDetailsChanged != null)
                DocumentDetailsChanged(this, e);
        }

        private bool dirty = false;

        public bool Dirty
        {
            get { return dirty; }
            set
            {
                if (dirty == value)
                    return;
                dirty = value;
                OnDocumentDetailsChanged(new DocumentDetailsChangedEventArgs(this));
            }
        }
    }
}
