#nullable enable
using System.Collections.Generic;
using CastleWars.Shared.Core;

namespace CastleWars.Shared.Game.Entities
{
    // GridX/GridY are region-grid coords (cell / RegionSize).
    public class RegionEntity : BaseEntity
    {
        public int GridX { get; set; }
        public int GridY { get; set; }
        public List<ulong> ArmyIds { get; } = new();
        public List<ulong> CityIds { get; } = new();
    }
}
