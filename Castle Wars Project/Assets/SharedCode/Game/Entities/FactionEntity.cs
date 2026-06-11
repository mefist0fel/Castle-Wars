#nullable enable
using CastleWars.Shared.Core;

namespace CastleWars.Shared.Game.Entities
{
    // ColorR/G/B are 0–255 ints (no float, avoids fixed-point concerns on ARM vs x86).
    public class FactionEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int ColorR { get; set; }
        public int ColorG { get; set; }
        public int ColorB { get; set; }
    }
}
