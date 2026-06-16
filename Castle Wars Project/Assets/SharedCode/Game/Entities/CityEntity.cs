#nullable enable
using CastleWars.Shared.Core;

namespace CastleWars.Shared.Game.Entities
{
    // X/Y are cell coordinates. OwnerId = 0 means neutral.
    public class CityEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int X { get; set; }
        public int Y { get; set; }
        public ulong OwnerId { get; set; }
        public int VisualVariant { get; set; }
    }
}
