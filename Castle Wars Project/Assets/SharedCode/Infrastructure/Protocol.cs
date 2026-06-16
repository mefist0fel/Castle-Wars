#nullable enable
using CastleWars.Shared.Network;

namespace CastleWars.Shared.Infrastructure
{
    public class ConnectRequest
    {
        public string PlayerName { get; set; } = string.Empty;
        public int ColorR { get; set; }
        public int ColorG { get; set; }
        public int ColorB { get; set; }
    }

    public class ConnectResponse
    {
        public bool Success { get; set; }
        public ulong PlayerId { get; set; }
        public string SessionToken { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        // Initial snapshot: session entity + newly created player entity
        public EntityUpdateBatch InitialState { get; set; } = new EntityUpdateBatch();
    }
}
