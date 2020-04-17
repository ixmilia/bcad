using System;
using System.Collections.Generic;
using System.Threading;
using IxMilia.BCad.Collections;
using IxMilia.BCad.Entities;

namespace IxMilia.BCad
{
    public class Layer
    {
        private readonly string name;
        private readonly CadColor? color;
        private readonly bool isVisible;
        private readonly ReadOnlyTree<uint, Entity> entities;

        public string Name { get { return name; } }

        public CadColor? Color { get { return color; } }

        public bool IsVisible { get { return isVisible; } }

        public int EntityCount
        {
            get { return this.entities.Count; }
        }

        public Layer(string name, IEnumerable<Entity> entities, CadColor? color = null, bool isVisible = true)
            : this(name, ReadOnlyTree<uint, Entity>.FromEnumerable(entities, (ent) => ent.Id), color, isVisible)
        {
        }

        public Layer(string name, ReadOnlyTree<uint, Entity> entities = null, CadColor? color = null, bool isVisible = true)
        {
            this.name = name;
            this.color = color;
            this.isVisible = isVisible;
            this.entities = entities ?? new ReadOnlyTree<uint, Entity>();
        }

        public IEnumerable<Entity> GetEntities(CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.entities.GetValues(cancellationToken);
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

        public Layer Update(
            string name = null,
            Optional<CadColor?> color = default(Optional<CadColor?>),
            Optional<bool> isVisible = default(Optional<bool>),
            ReadOnlyTree<uint, Entity> entities = null)
        {
            var newName = name ?? this.name;
            var newColor = color.HasValue ? color.Value : this.color;
            var newIsVisible = isVisible.HasValue ? isVisible.Value : this.isVisible;
            var newEntities = entities ?? this.entities;

            if (newName == this.name &&
                newColor == this.color &&
                newIsVisible == this.isVisible &&
                object.ReferenceEquals(newEntities, this.entities))
            {
                return this;
            }

            return new Layer(newName, newEntities, newColor, newIsVisible);
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
