#nullable enable
using CastleWars.Shared.Core;

namespace CastleWars.Shared.Game.Entities
{
    public class PlayerEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        // 0–255 ints, no float
        public int ColorR { get; set; }
        public int ColorG { get; set; }
        public int ColorB { get; set; }
        public ulong ArmyId { get; set; }
        public bool IsConnected { get; set; }
    }
}
