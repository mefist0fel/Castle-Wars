#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CastleWars.Shared.Core;
using CastleWars.Shared.Game;
using CastleWars.Shared.Game.Commands;
using CastleWars.Shared.Game.Entities;
using CastleWars.Shared.Network;

namespace CastleWars.Shared.Infrastructure
{
    public class GameServer
    {
        private readonly CastleWarsSession _session;
        private readonly Dictionary<string, PlayerConnection> _byToken = new();
        private readonly Dictionary<ulong, PlayerConnection> _byPlayerId = new();
        private readonly HashSet<ulong> _changedThisBatch = new();
        private readonly List<string> _commandHistory = new();
        private long _serverTick;

        public CastleWarsSession Session => _session;

        public GameServer()
        {
            _session = new CastleWarsSession();
            _session.OnEntityRegistered += e => _changedThisBatch.Add(e.Id);
            _session.OnEntityMutated    += e => _changedThisBatch.Add(e.Id);
        }

        // ── Connection ────────────────────────────────────────────────────────

        public ConnectResponse Connect(ConnectRequest request, Action<EntityUpdateBatch> onPush)
        {
            _changedThisBatch.Clear();

            var player = new PlayerEntity
            {
                Name = request.PlayerName,
                ColorR = request.ColorR, ColorG = request.ColorG, ColorB = request.ColorB,
                IsConnected = true
            };
            var playerId = _session.Seed(player);

            var session = _session.Session;
            if (!session.PlayerIds.Contains(playerId)) session.PlayerIds.Add(playerId);
            _session.Mutate(session);

            var token = Guid.NewGuid().ToString("N");
            var conn = new PlayerConnection(playerId, token, onPush);
            conn.SubscribedEntityIds.Add(1);         // session entity always
            conn.SubscribedEntityIds.Add(playerId);  // own player entity

            _byToken[token] = conn;
            _byPlayerId[playerId] = conn;

            var changed = TakeChanged();
            var initialState = BuildDelta(changed, conn);
            initialState.AddedSubscriptions = new[] { 1UL, playerId };

            _commandHistory.Add($"[CONNECT] {request.PlayerName} id={playerId}");

            return new ConnectResponse
            {
                Success = true,
                PlayerId = playerId,
                SessionToken = token,
                InitialState = initialState
            };
        }

        public void Disconnect(string token)
        {
            if (!_byToken.TryGetValue(token, out var conn)) return;
            conn.IsConnected = false;

            var player = _session.Get<PlayerEntity>(conn.PlayerId);
            if (player != null) { player.IsConnected = false; _session.Mutate(player); }

            var session = _session.Session;
            session.PlayerIds.Remove(conn.PlayerId);
            _session.Mutate(session);

            var changed = TakeChanged();
            PushToOthers(changed, conn);
            _commandHistory.Add($"[DISCONNECT] player {conn.PlayerId}");
        }

        // ── Subscriptions ─────────────────────────────────────────────────────

        public EntityUpdateBatch Subscribe(string token, ulong[] entityIds)
        {
            var conn = GetConnection(token);
            if (conn == null) return new EntityUpdateBatch();

            var added = new List<ulong>();
            foreach (var id in entityIds)
                if (conn.SubscribedEntityIds.Add(id)) added.Add(id);

            var batch = new EntityUpdateBatch { ServerTick = ++_serverTick };
            foreach (var id in entityIds)
            {
                var snap = SnapshotFactory.Create(_session.GetAny(id)!);
                if (snap != null) batch.Snapshots.Add(snap);
            }
            batch.AddedSubscriptions = added.ToArray();
            return batch;
        }

        public void Unsubscribe(string token, ulong[] entityIds)
        {
            var conn = GetConnection(token);
            if (conn == null) return;
            foreach (var id in entityIds) conn.SubscribedEntityIds.Remove(id);
        }

        // ── Commands ──────────────────────────────────────────────────────────

        public EntityUpdateBatch ApplyCommands(CommandBatch batch)
        {
            var conn = GetConnection(batch.SessionToken);
            if (conn == null) return new EntityUpdateBatch();

            conn.LastHeartbeat = DateTime.UtcNow;
            _changedThisBatch.Clear();

            ulong oldArmyRegion = GetPlayerArmyRegion(conn.PlayerId);

            // Commands execute in order; the chain breaks on first rejection
            foreach (var netCmd in batch.Commands)
            {
                var logicCmd = ToLogicCommand(netCmd);
                if (logicCmd == null) continue;
                bool ok = _session.Apply(logicCmd);
                _commandHistory.Add($"  [{netCmd.GetType().Name.Replace("NetCommand","")}] player={conn.PlayerId} {(ok?"OK":"REJECTED")}");
                if (!ok) break;
            }

            ulong newArmyRegion = GetPlayerArmyRegion(conn.PlayerId);
            var addedSubs = new HashSet<ulong>();
            var removedSubs = new HashSet<ulong>();

            if (newArmyRegion != oldArmyRegion)
            {
                if (oldArmyRegion != 0) removedSubs = RemoveRegionSubs(conn, oldArmyRegion);
                if (newArmyRegion != 0)
                {
                    addedSubs = AddRegionSubs(conn, newArmyRegion);
                    foreach (var id in addedSubs) _changedThisBatch.Add(id);
                }
            }

            var changed = TakeChanged();
            var response = BuildDelta(changed, conn);
            response.AddedSubscriptions = addedSubs.ToArray();
            response.RemovedSubscriptions = removedSubs.ToArray();

            PushToOthers(changed, conn);
            return response;
        }

        // ── Game helpers (called by bootstrap) ────────────────────────────────

        // Spawns a player's starting army at the given cell and wires up subscriptions.
        // Pushes the changes to the owning client synchronously so the client's
        // local player snapshot is up-to-date immediately after this call returns.
        public ulong SpawnPlayerArmy(ulong playerId, int x, int y)
        {
            _changedThisBatch.Clear();

            var player = _session.Get<PlayerEntity>(playerId);
            if (player == null) return 0;

            var map = _session.Query<MapEntity>().FirstOrDefault();
            if (map == null) return 0;

            ulong regionId = FindRegionId(x, y, map.RegionSize);
            var army = new ArmyEntity { OwnerId = playerId, X = x, Y = y, RegionId = regionId, Health = 100 };
            var armyId = _session.Seed(army);

            var region = _session.Get<RegionEntity>(regionId);
            if (region != null) { region.ArmyIds.Add(armyId); _session.Mutate(region); }

            player.ArmyId = armyId;
            _session.Mutate(player);

            if (_byPlayerId.TryGetValue(playerId, out var conn))
            {
                conn.SubscribedEntityIds.Add(armyId);
                AddRegionSubs(conn, regionId);

                // Push army + updated player + region to the client immediately
                var changed = TakeChanged();
                var delta = BuildDelta(changed, conn);
                if (delta.Snapshots.Count > 0) conn.OnPush(delta);
            }
            else
            {
                TakeChanged();
            }

            return armyId;
        }

        // ── Debug ─────────────────────────────────────────────────────────────

        public string GetDebugInfo()
        {
            var sb = new StringBuilder();
            int connected = _byToken.Values.Count(c => c.IsConnected);
            sb.AppendLine($"Players: {_byToken.Count} ({connected} connected)");
            sb.AppendLine($"Entities: {_session.EntityCount} total");
            sb.AppendLine($"  Session:  {_session.Query<SessionEntity>().Count()}");
            sb.AppendLine($"  Players:  {_session.Query<PlayerEntity>().Count()}");
            sb.AppendLine($"  Maps:     {_session.Query<MapEntity>().Count()}");
            sb.AppendLine($"  Regions:  {_session.Query<RegionEntity>().Count()}");
            sb.AppendLine($"  Cities:   {_session.Query<CityEntity>().Count()}");
            var armies = _session.Query<ArmyEntity>().ToList();
            sb.AppendLine($"  Armies:   {armies.Count} ({armies.Count(a => a.IsDead)} dead)");

            if (_commandHistory.Count > 0)
            {
                sb.AppendLine("Recent commands:");
                int from = Math.Max(0, _commandHistory.Count - 10);
                for (int i = from; i < _commandHistory.Count; i++)
                    sb.AppendLine(_commandHistory[i]);
            }
            return sb.ToString();
        }

        // ── Private ───────────────────────────────────────────────────────────

        private PlayerConnection? GetConnection(string token)
        {
            _byToken.TryGetValue(token, out var c);
            return c?.IsConnected == true ? c : null;
        }

        private ulong GetPlayerArmyRegion(ulong playerId)
        {
            var player = _session.Get<PlayerEntity>(playerId);
            if (player == null || player.ArmyId == 0) return 0;
            return _session.Get<ArmyEntity>(player.ArmyId)?.RegionId ?? 0;
        }

        private HashSet<ulong> AddRegionSubs(PlayerConnection conn, ulong regionId)
        {
            var added = new HashSet<ulong>();
            if (conn.SubscribedEntityIds.Add(regionId)) added.Add(regionId);
            var r = _session.Get<RegionEntity>(regionId);
            if (r == null) return added;
            foreach (var id in r.ArmyIds)  if (conn.SubscribedEntityIds.Add(id)) added.Add(id);
            foreach (var id in r.CityIds)  if (conn.SubscribedEntityIds.Add(id)) added.Add(id);
            return added;
        }

        private HashSet<ulong> RemoveRegionSubs(PlayerConnection conn, ulong regionId)
        {
            var removed = new HashSet<ulong>();
            if (conn.SubscribedEntityIds.Remove(regionId)) removed.Add(regionId);
            var r = _session.Get<RegionEntity>(regionId);
            if (r == null) return removed;
            foreach (var id in r.ArmyIds) if (conn.SubscribedEntityIds.Remove(id)) removed.Add(id);
            foreach (var id in r.CityIds) if (conn.SubscribedEntityIds.Remove(id)) removed.Add(id);
            return removed;
        }

        private EntityUpdateBatch BuildDelta(HashSet<ulong> changedIds, PlayerConnection conn)
        {
            var batch = new EntityUpdateBatch { ServerTick = ++_serverTick };
            foreach (var id in changedIds)
            {
                if (!conn.SubscribedEntityIds.Contains(id)) continue;
                var entity = _session.GetAny(id);
                if (entity == null) continue;
                var snap = SnapshotFactory.Create(entity);
                if (snap != null) batch.Snapshots.Add(snap);
            }
            return batch;
        }

        private void PushToOthers(HashSet<ulong> changed, PlayerConnection sender)
        {
            foreach (var other in _byToken.Values)
            {
                if (other == sender || !other.IsConnected) continue;
                var delta = BuildDelta(changed, other);
                if (delta.Snapshots.Count > 0) other.OnPush(delta);
            }
        }

        private HashSet<ulong> TakeChanged()
        {
            var result = new HashSet<ulong>(_changedThisBatch);
            _changedThisBatch.Clear();
            return result;
        }

        private ulong FindRegionId(int x, int y, int regionSize)
        {
            if (regionSize <= 0) return 0;
            int gx = x / regionSize, gy = y / regionSize;
            foreach (var r in _session.Query<RegionEntity>())
                if (r.GridX == gx && r.GridY == gy) return r.Id;
            return 0;
        }

        private static ILogicCommand? ToLogicCommand(NetCommand net)
        {
            switch (net)
            {
                case TeleportArmyNetCommand t:
                    return new TeleportArmyCommand { PlayerId = t.PlayerId, TargetX = t.TargetX, TargetY = t.TargetY };
                case AttackArmyNetCommand a:
                    return new AttackArmyCommand { AttackerPlayerId = a.AttackerPlayerId, TargetArmyId = a.TargetArmyId };
                case CaptureCityNetCommand c:
                    return new CaptureCityCommand { PlayerId = c.PlayerId, CityId = c.CityId };
                case HealArmyNetCommand h:
                    return new HealArmyCommand { PlayerId = h.PlayerId, Amount = h.Amount };
                case CreateGameNetCommand cg:
                    return new CreateGameCommand
                    {
                        Width = cg.Width, Height = cg.Height, RegionSize = cg.RegionSize,
                        CityCount = cg.CityCount, BarbarianCount = cg.BarbarianCount, Seed = cg.Seed
                    };
                default:
                    return null;
            }
        }
    }
}
