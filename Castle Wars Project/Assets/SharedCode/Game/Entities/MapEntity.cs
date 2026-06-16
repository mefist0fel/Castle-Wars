#nullable enable
using System.Collections.Generic;
using CastleWars.Shared.Core;

namespace CastleWars.Shared.Game.Entities
{
    // Mostly static. RegionSize = cells per region side.
    public class MapEntity : BaseEntity
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int RegionSize { get; set; }
        public List<ulong> RegionIds { get; } = new();
    }
}
