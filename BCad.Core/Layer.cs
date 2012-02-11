using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.Objects;

namespace BCad
{
    public class Layer
    {
        public string Name { get; private set; }

        public Color Color { get; private set; }

        public IEnumerable<IObject> Objects { get { return objects; } }

        public Layer Update(string name = null, Color? color = null, IEnumerable<IObject> objects = null)
        {
            return new Layer(
                name != null ? name : this.Name,
                color != null ? color.Value : this.Color,
                objects != null ? objects : this.Objects);
        }

        private HashSet<IObject> objects = new HashSet<IObject>();

        private int hashCode = 0;

        public Layer(string name, Color color)
            : this(name, color, new IObject[0])
        {
        }

        public Layer(string name, Color color, IEnumerable<IObject> objects)
        {
            Name = name;
            Color = color;
            this.objects = new HashSet<IObject>(objects);
            hashCode = Name.GetHashCode() ^ Color.GetHashCode();
        }

        internal void AddObject(IObject obj)
        {
            if (objects.Contains(obj))
                throw new ArgumentException("Duplicate object specified");
            if (obj.Layer != this)
                throw new ArgumentException("Improper layer specified");
            objects.Add(obj);
        }

        internal void RemoveObject(IObject obj)
        {
            if (!objects.Contains(obj))
                throw new ArgumentException("Layer does not contain the object");
            if (obj.Layer != this)
                throw new ArgumentException("Improper layer specified");
            objects.Remove(obj);
        }

        public override int GetHashCode()
        {
            return hashCode;
        }
    }
}
