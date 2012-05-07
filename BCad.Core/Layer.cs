using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using BCad.Entities;

namespace BCad
{
    public class Layer
    {
        private readonly string name;
        private readonly Color color;
        private readonly bool isVisible;
        private readonly ReadOnlyCollection<Entity> entities;

        public string Name { get { return name; } }

        public Color Color { get { return color; } }

        public bool IsVisible { get { return isVisible; } }

        public ReadOnlyCollection<Entity> Entities { get { return entities; } }

        public Layer(string name, Color color)
            : this(name, color, new Entity[0])
        {
        }

        public Layer(string name, Color color, IEnumerable<Entity> entities)
            : this(name, color, true, entities)
        {
        }

        public Layer(string name, Color color, bool isVisible, IEnumerable<Entity> entities)
        {
            this.name = name;
            this.color = color;
            this.isVisible = isVisible;
            this.entities = new ReadOnlyCollection<Entity>(entities.ToList());
        }

        /// <summary>
        /// Returns true if the layer contains the specified object.
        /// </summary>
        /// <param name="entity">The object to find.</param>
        /// <returns>True if the object was found, false otherwise.</returns>
        public bool EntityExists(Entity entity)
        {
            return this.Entities.Any(e => e == entity);
        }

        public Layer Add(Entity entity)
        {
            return this.Update(entities: this.Entities.Concat(new[] { entity }));
        }

        public Layer Remove(Entity entity)
        {
            if (!this.EntityExists(entity))
                throw new ArgumentException("The layer does not contain the specified object.");
            return this.Update(entities: this.Entities.Except(new[] { entity }));
        }

        public Layer Replace(Entity oldEntity, Entity newEntity)
        {
            if (!this.EntityExists(oldEntity))
                throw new ArgumentException("The layer does not contain the specified object.");
            return this.Update(entities: this.Entities.Except(new[] { oldEntity }).Concat(new[] { newEntity }));
        }

        public Layer Update(string name = null, Color? color = null, bool? isVisible = null, IEnumerable<Entity> entities = null)
        {
            return new Layer(
                name ?? this.Name,
                color ?? this.Color,
                isVisible ?? this.isVisible,
                entities ?? this.Entities);
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
