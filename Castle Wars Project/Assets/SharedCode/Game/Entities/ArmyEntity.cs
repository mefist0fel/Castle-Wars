#nullable enable
using CastleWars.Shared.Core;

namespace CastleWars.Shared.Game.Entities
{
    // Dynamic. MovementProgress is fixed-point 0–1000 (= 0.0–1.0).
    // TargetRegionId = 0 means stationary.
    public class ArmyEntity : BaseEntity
    {
        public ulong OwnerId { get; set; }
        public ulong CurrentRegionId { get; set; }
        public ulong TargetRegionId { get; set; }
        public int UnitCount { get; set; }
        public int MovementProgress { get; set; }
    }
}
