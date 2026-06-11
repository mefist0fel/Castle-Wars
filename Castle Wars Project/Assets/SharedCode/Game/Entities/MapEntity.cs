#nullable enable
using System.Collections.Generic;
using CastleWars.Shared.Core;

namespace CastleWars.Shared.Game.Entities
{
    public class MapEntity : BaseEntity
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public List<ulong> RegionIds { get; } = new();
    }
}
