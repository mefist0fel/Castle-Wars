#nullable enable
using CastleWars.Shared.Core;

namespace CastleWars.Shared.Game.Entities
{
    // Dynamic. OwnerId = PlayerEntity.Id.
    public class CityEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public ulong RegionId { get; set; }
        public ulong OwnerId { get; set; }
        public int GarrisonCount { get; set; }
    }
}
