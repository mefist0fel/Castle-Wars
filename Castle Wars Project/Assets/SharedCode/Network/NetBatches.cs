// C# 5-compatible (no auto-property initializers) so mpu.exe can parse this file.
using System.Collections.Generic;
using MsgPack.Serialization;

namespace CastleWars.Shared.Network
{
    // ── Commands (client -> server) ──────────────────────────────────────────

    public abstract class NetCommand { }

    public class TeleportArmyNetCommand : NetCommand
    {
        [MessagePackMember(0)] public ulong PlayerId { get; set; }
        [MessagePackMember(1)] public int TargetX { get; set; }
        [MessagePackMember(2)] public int TargetY { get; set; }
    }

    public class AttackArmyNetCommand : NetCommand
    {
        [MessagePackMember(0)] public ulong AttackerPlayerId { get; set; }
        [MessagePackMember(1)] public ulong TargetArmyId { get; set; }
    }

    public class CaptureCityNetCommand : NetCommand
    {
        [MessagePackMember(0)] public ulong PlayerId { get; set; }
        [MessagePackMember(1)] public ulong CityId { get; set; }
    }

    public class HealArmyNetCommand : NetCommand
    {
        [MessagePackMember(0)] public ulong PlayerId { get; set; }
        [MessagePackMember(1)] public int Amount { get; set; }
    }

    public class CreateGameNetCommand : NetCommand
    {
        [MessagePackMember(0)] public int Width { get; set; }
        [MessagePackMember(1)] public int Height { get; set; }
        [MessagePackMember(2)] public int RegionSize { get; set; }
        [MessagePackMember(3)] public int CityCount { get; set; }
        [MessagePackMember(4)] public int BarbarianCount { get; set; }
        [MessagePackMember(5)] public int Seed { get; set; }
    }

    // One logical tick's worth of player input.
    [MessagePackKnownCollectionItemType("teleport",   typeof(TeleportArmyNetCommand))]
    [MessagePackKnownCollectionItemType("attack",     typeof(AttackArmyNetCommand))]
    [MessagePackKnownCollectionItemType("capture",    typeof(CaptureCityNetCommand))]
    [MessagePackKnownCollectionItemType("heal",       typeof(HealArmyNetCommand))]
    [MessagePackKnownCollectionItemType("creategame", typeof(CreateGameNetCommand))]
    public class CommandBatch
    {
        public CommandBatch()
        {
            Commands = new List<NetCommand>();
            SessionToken = string.Empty;
        }

        [MessagePackMember(0)] public long Tick { get; set; }
        [MessagePackMember(1)] public string SessionToken { get; set; }
        [MessagePackMember(2)] public List<NetCommand> Commands { get; set; }
    }

    // ── Entity updates (server -> client) ────────────────────────────────────

    [MessagePackKnownCollectionItemType("session", typeof(SessionSnapshot))]
    [MessagePackKnownCollectionItemType("player",  typeof(PlayerSnapshot))]
    [MessagePackKnownCollectionItemType("map",     typeof(MapSnapshot))]
    [MessagePackKnownCollectionItemType("region",  typeof(RegionSnapshot))]
    [MessagePackKnownCollectionItemType("army",    typeof(ArmySnapshot))]
    [MessagePackKnownCollectionItemType("city",    typeof(CitySnapshot))]
    public class EntityUpdateBatch
    {
        public EntityUpdateBatch()
        {
            Snapshots = new List<EntitySnapshot>();
            AddedSubscriptions = new ulong[0];
            RemovedSubscriptions = new ulong[0];
        }

        [MessagePackMember(0)] public long ServerTick { get; set; }
        [MessagePackMember(1)] public List<EntitySnapshot> Snapshots { get; set; }
        [MessagePackMember(2)] public ulong[] AddedSubscriptions { get; set; }
        [MessagePackMember(3)] public ulong[] RemovedSubscriptions { get; set; }
    }
}
