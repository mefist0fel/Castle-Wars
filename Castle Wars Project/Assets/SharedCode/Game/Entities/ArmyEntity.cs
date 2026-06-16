#nullable enable
using CastleWars.Shared.Core;

namespace CastleWars.Shared.Game.Entities
{
    // X/Y are cell coordinates. OwnerId = 0 means barbarian.
    public class ArmyEntity : BaseEntity
    {
        public ulong OwnerId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public ulong RegionId { get; set; }
        public int Health { get; set; }
        public bool IsDead { get; set; }
    }
}
