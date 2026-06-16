#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using CastleWars.Shared.Network;

namespace CastleWars.Shared.Infrastructure
{
    // Holds the client's partial view of world state (subscribed entities only).
    // Works the same whether backed by LocalFakeConnection or a real network connection.
    public class GameClient
    {
        private readonly IServerConnection _conn;
        private string _sessionToken = string.Empty;

        public ulong PlayerId { get; private set; }
        public bool IsConnected => PlayerId != 0 && _sessionToken.Length > 0;

        // Snapshots for all entities this client is subscribed to
        private readonly Dictionary<ulong, EntitySnapshot> _snapshots = new();

        public event Action<EntitySnapshot>? OnSnapshotUpdated;
        // (addedIds, removedIds)
        public event Action<ulong[], ulong[]>? OnSubscriptionsChanged;

        public GameClient(IServerConnection connection)
        {
            _conn = connection;
            _conn.OnServerPush += ApplyBatch;
        }

        // ── Lifecycle ─────────────────────────────────────────────────────────

        public ConnectResponse Connect(string playerName, int r, int g, int b)
        {
            var response = _conn.Connect(new ConnectRequest
            {
                PlayerName = playerName, ColorR = r, ColorG = g, ColorB = b
            });

            if (response.Success)
            {
                PlayerId = response.PlayerId;
                _sessionToken = response.SessionToken;
                ApplyBatch(response.InitialState);
            }
            return response;
        }

        public void Disconnect()
        {
            if (!IsConnected) return;
            _conn.Disconnect(_sessionToken);
            PlayerId = 0;
            _sessionToken = string.Empty;
            _snapshots.Clear();
        }

        // ── Subscriptions ─────────────────────────────────────────────────────

        public void Subscribe(params ulong[] entityIds)
        {
            if (!IsConnected) return;
            var batch = _conn.Subscribe(_sessionToken, entityIds);
            ApplyBatch(batch);
        }

        public void Unsubscribe(params ulong[] entityIds)
        {
            if (!IsConnected) return;
            _conn.Unsubscribe(_sessionToken, entityIds);
            foreach (var id in entityIds)
                _snapshots.Remove(id);
        }

        // ── Commands ──────────────────────────────────────────────────────────

        public void SendCommands(params NetCommand[] commands)
        {
            if (!IsConnected) return;
            var batch = new CommandBatch
            {
                Tick = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                SessionToken = _sessionToken,
                Commands = new System.Collections.Generic.List<NetCommand>(commands)
            };
            var response = _conn.ApplyCommands(batch);
            ApplyBatch(response);
        }

        // ── Local state queries ───────────────────────────────────────────────

        public T? GetSnapshot<T>(ulong id) where T : EntitySnapshot
        {
            _snapshots.TryGetValue(id, out var s);
            return s as T;
        }

        public IEnumerable<T> QuerySnapshots<T>() where T : EntitySnapshot
            => _snapshots.Values.OfType<T>();

        public SessionSnapshot? GetSession()
            => GetSnapshot<SessionSnapshot>(1);

        public PlayerSnapshot? GetMyPlayer()
            => GetSnapshot<PlayerSnapshot>(PlayerId);

        public ArmySnapshot? GetMyArmy()
        {
            var me = GetMyPlayer();
            if (me == null || me.ArmyId == 0) return null;
            return GetSnapshot<ArmySnapshot>(me.ArmyId);
        }

        // ── Internal ──────────────────────────────────────────────────────────

        private void ApplyBatch(EntityUpdateBatch batch)
        {
            if (batch == null) return;

            foreach (var snap in batch.Snapshots)
            {
                ulong id = SnapshotFactory.GetEntityId(snap);
                if (id == 0) continue;
                _snapshots[id] = snap;
                OnSnapshotUpdated?.Invoke(snap);
            }

            if ((batch.AddedSubscriptions?.Length > 0) || (batch.RemovedSubscriptions?.Length > 0))
            {
                OnSubscriptionsChanged?.Invoke(
                    batch.AddedSubscriptions ?? Array.Empty<ulong>(),
                    batch.RemovedSubscriptions ?? Array.Empty<ulong>());
            }
        }
    }
}
