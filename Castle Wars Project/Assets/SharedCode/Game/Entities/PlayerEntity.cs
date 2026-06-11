#nullable enable
using CastleWars.Shared.Core;

namespace CastleWars.Shared.Game.Entities
{
    public class PlayerEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public ulong FactionId { get; set; }
        public int Gold { get; set; }
    }
}
