#nullable enable
using System.Collections.Generic;
using CastleWars.Shared.Core;

namespace CastleWars.Shared.Game.Entities
{
    // Fixed routing entity — always the first registered (Id = 1).
    // Holds top-level IDs so clients can navigate the world without scanning all entities.
    public class SessionEntity : BaseEntity
    {
        public ulong MapId { get; set; }
        public List<ulong> ArmyIds { get; } = new();
        public List<ulong> CityIds { get; } = new();
    }
}
