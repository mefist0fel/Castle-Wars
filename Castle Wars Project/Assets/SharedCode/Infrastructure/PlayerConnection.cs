#nullable enable
using System;
using System.Collections.Generic;
using CastleWars.Shared.Network;

namespace CastleWars.Shared.Infrastructure
{
    public class PlayerConnection
    {
        public ulong PlayerId { get; }
        public string SessionToken { get; }
        public HashSet<ulong> SubscribedEntityIds { get; } = new HashSet<ulong>();
        public Action<EntityUpdateBatch> OnPush { get; }
        public DateTime LastHeartbeat { get; set; }
        public bool IsConnected { get; set; } = true;

        public PlayerConnection(ulong playerId, string token, Action<EntityUpdateBatch> onPush)
        {
            PlayerId = playerId;
            SessionToken = token;
            OnPush = onPush;
            LastHeartbeat = DateTime.UtcNow;
        }
    }
}
