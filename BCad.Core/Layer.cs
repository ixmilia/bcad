using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using BCad.Objects;

namespace BCad
{
    public class Layer
    {
        private readonly string name;
        private readonly Color color;
        private readonly bool isVisible;
        private readonly ReadOnlyCollection<IObject> objects;

        public string Name { get { return name; } }

        public Color Color { get { return color; } }

        public bool IsVisible { get { return isVisible; } }

        public ReadOnlyCollection<IObject> Objects { get { return objects; } }

        public Layer(string name, Color color)
            : this(name, color, new IObject[0])
        {
        }

        public Layer(string name, Color color, IEnumerable<IObject> objects)
            : this(name, color, true, objects)
        {
        }

        public Layer(string name, Color color, bool isVisible, IEnumerable<IObject> objects)
        {
            this.name = name;
            this.color = color;
            this.isVisible = isVisible;
            this.objects = new ReadOnlyCollection<IObject>(objects.ToList());
        }

        /// <summary>
        /// Returns true if the layer contains the specified object.
        /// </summary>
        /// <param name="obj">The object to find.</param>
        /// <returns>True if the object was found, false otherwise.</returns>
        public bool ObjectExists(IObject obj)
        {
            return this.Objects.Any(o => o == obj);
        }

        public Layer Add(IObject obj)
        {
            return this.Update(objects: this.Objects.Concat(new[] { obj }));
        }

        public Layer Remove(IObject obj)
        {
            if (!this.ObjectExists(obj))
                throw new ArgumentException("The layer does not contain the specified object.");
            return this.Update(objects: this.Objects.Except(new[] { obj }));
        }

        public Layer Replace(IObject oldObject, IObject newObject)
        {
            if (!this.ObjectExists(oldObject))
                throw new ArgumentException("The layer does not contain the specified object.");
            return this.Update(objects: this.Objects.Except(new[] { oldObject }).Concat(new[] { newObject }));
        }

        public Layer Update(string name = null, Color? color = null, bool? isVisible = null, IEnumerable<IObject> objects = null)
        {
            return new Layer(
                name ?? this.Name,
                color ?? this.Color,
                isVisible ?? this.isVisible,
                objects ?? this.Objects);
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
