#nullable enable
using System;
using CastleWars.Shared.Network;

namespace CastleWars.Shared.Infrastructure
{
    // Client-side interface. LocalFakeConnection implements this for local play;
    // a future NetworkConnection will implement it for real multiplayer.
    public interface IServerConnection
    {
        ConnectResponse Connect(ConnectRequest request);
        EntityUpdateBatch Subscribe(string sessionToken, ulong[] entityIds);
        void Unsubscribe(string sessionToken, ulong[] entityIds);
        EntityUpdateBatch ApplyCommands(CommandBatch batch);
        void Disconnect(string sessionToken);

        // Server pushes unsolicited updates (other players' actions, timeouts, etc.)
        event Action<EntityUpdateBatch> OnServerPush;
    }
}
