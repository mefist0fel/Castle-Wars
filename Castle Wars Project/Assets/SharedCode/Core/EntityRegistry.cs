#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace CastleWars.Shared.Core
{
    // Flat entity store. All entities live here; cross-references are by Id only.
    public class EntityRegistry
    {
        private readonly Dictionary<ulong, BaseEntity> _entities = new();
        private ulong _nextId = 1;

        public event Action<BaseEntity>? OnEntityRegistered;
        public event Action<BaseEntity>? OnEntityMutated;

        public ulong Register(BaseEntity entity)
        {
            entity.Id = _nextId++;
            entity.Version = 1;
            _entities[entity.Id] = entity;
            OnEntityRegistered?.Invoke(entity);
            return entity.Id;
        }

        public T? Get<T>(ulong id) where T : BaseEntity
            => _entities.TryGetValue(id, out var e) ? e as T : null;

        public IEnumerable<T> GetAll<T>() where T : BaseEntity
            => _entities.Values.OfType<T>();

        // Increments version and notifies subscribers.
        public void Mutate(BaseEntity entity)
        {
            entity.Version++;
            OnEntityMutated?.Invoke(entity);
        }
    }
}
