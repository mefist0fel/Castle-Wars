#nullable enable
using System;
using CastleWars.Shared.Infrastructure;
using CastleWars.Shared.Network;
using MsgPack.Serialization;

namespace CastleWars.Network
{
    // Implements IServerConnection by calling GameServer directly,
    // but puts CommandBatch and EntityUpdateBatch through a MsgPack
    // round-trip to validate the serialization pipeline.
    public class LocalFakeConnection : IServerConnection
    {
        private readonly GameServer _server;
        private readonly MessagePackSerializer<CommandBatch> _cmdSer;
        private readonly MessagePackSerializer<EntityUpdateBatch> _updSer;

        public event Action<EntityUpdateBatch>? OnServerPush;

        public LocalFakeConnection(GameServer server)
        {
            _server = server;
            _cmdSer = MessagePackSerializer.Get<CommandBatch>();
            _updSer = MessagePackSerializer.Get<EntityUpdateBatch>();
        }

        public ConnectResponse Connect(ConnectRequest request)
        {
            // Administrative call — no game data to serialize
            return _server.Connect(request, batch =>
            {
                // Push from server → serialize/deserialize to simulate wire
                var received = RoundTripUpdate(batch);
                OnServerPush?.Invoke(received);
            });
        }

        public EntityUpdateBatch Subscribe(string sessionToken, ulong[] entityIds)
        {
            return RoundTripUpdate(_server.Subscribe(sessionToken, entityIds));
        }

        public void Unsubscribe(string sessionToken, ulong[] entityIds)
        {
            _server.Unsubscribe(sessionToken, entityIds);
        }

        public EntityUpdateBatch ApplyCommands(CommandBatch batch)
        {
            // Commands: serialize → deserialize (simulates client → server wire)
            var bytes = _cmdSer.PackSingleObject(batch);
            var received = _cmdSer.UnpackSingleObject(bytes);

            // Apply on server, then serialize → deserialize the response
            return RoundTripUpdate(_server.ApplyCommands(received));
        }

        public void Disconnect(string sessionToken)
        {
            _server.Disconnect(sessionToken);
        }

        private EntityUpdateBatch RoundTripUpdate(EntityUpdateBatch batch)
        {
            var bytes = _updSer.PackSingleObject(batch);
            return _updSer.UnpackSingleObject(bytes);
        }
    }
}
