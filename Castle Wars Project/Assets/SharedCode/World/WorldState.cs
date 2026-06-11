using System.Collections.Generic;
using System.Linq;
using CastleWars.Shared.Entities;

namespace CastleWars.Shared.World
{
    public class WorldState
    {
        private readonly Dictionary<ulong, BaseEntity> _entities = new();
        private ulong _nextId = 1;

        public ulong Register(BaseEntity entity)
        {
            entity.Id = _nextId++;
            entity.Version = 1;
            _entities[entity.Id] = entity;
            return entity.Id;
        }

        public T? Get<T>(ulong id) where T : BaseEntity
            => _entities.TryGetValue(id, out var e) ? e as T : null;

        public IEnumerable<T> GetAll<T>() where T : BaseEntity
            => _entities.Values.OfType<T>();

        // Increments version and marks entity dirty for replication.
        public void Mutate(BaseEntity entity)
        {
            entity.Version++;
        }
    }
}
