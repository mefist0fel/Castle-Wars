#nullable enable
using System.Collections.Generic;
using CastleWars.Shared.Core;

namespace CastleWars.Shared.Game.Entities
{
    // Always Id=1. Auto-subscribed by every connecting client.
    public class SessionEntity : BaseEntity
    {
        public ulong MapId { get; set; }
        public int GameTick { get; set; }
        public List<ulong> PlayerIds { get; } = new();
    }
}
