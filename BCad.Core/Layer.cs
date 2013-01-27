using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using BCad.Collections;
using BCad.Entities;

namespace BCad
{
    public class Layer
    {
        private readonly string name;
        private readonly Color color;
        private readonly bool isVisible;
        private readonly ReadOnlyTree<uint, Entity> entities;

        public string Name { get { return name; } }

        public Color Color { get { return color; } }

        public bool IsVisible { get { return isVisible; } }

        public int EntityCount
        {
            get { return this.entities.Count; }
        }

        public Layer(string name, Color color)
            : this(name, color, new ReadOnlyTree<uint, Entity>())
        {
        }

        public Layer(string name, Color color, ReadOnlyTree<uint, Entity> entities)
            : this(name, color, true, entities)
        {
        }

        public Layer(string name, Color color, bool isVisible, ReadOnlyTree<uint, Entity> entities)
        {
            this.name = name;
            this.color = color;
            this.isVisible = isVisible;
            this.entities = entities;
        }

        public IEnumerable<Entity> GetEntities()
        {
            return this.entities.GetValues();
        }

        public Entity GetEntityById(uint id)
        {
            Entity result;
            if (this.entities.TryFind(id, out result))
                return result;
            return null;
        }

        /// <summary>
        /// Returns true if the layer contains the specified entity.
        /// </summary>
        /// <param name="entity">The entity to find.</param>
        /// <returns>True if the entity was found, false otherwise.</returns>
        public bool EntityExists(Entity entity)
        {
            return this.entities.KeyExists(entity.Id);
        }

        public Layer Add(Entity entity)
        {
            return this.Update(entities: this.entities.Insert(entity.Id, entity));
        }

        public Layer Remove(Entity entity)
        {
            if (!this.EntityExists(entity))
                throw new ArgumentException("The layer does not contain the specified entity.");
            return this.Update(entities: this.entities.Delete(entity.Id));
        }

        public Layer Replace(Entity oldEntity, Entity newEntity)
        {
            if (!this.EntityExists(oldEntity))
                throw new ArgumentException("The layer does not contain the specified entity.");
            return this.Update(entities: this.entities.Delete(oldEntity.Id).Insert(newEntity.Id, newEntity));
        }

        public Layer Update(string name = null, Color? color = null, bool? isVisible = null, ReadOnlyTree<uint, Entity> entities = null)
        {
            return new Layer(
                name ?? this.Name,
                color ?? this.Color,
                isVisible ?? this.isVisible,
                entities ?? this.entities);
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
