// C# 5-compatible (no auto-property initializers) so mpu.exe can parse this file.
using System.Collections.Generic;
using MsgPack.Serialization;

namespace CastleWars.Shared.Network
{
    // Base for all entity state snapshots flowing server -> client.
    // Concrete types are registered via [MessagePackKnownCollectionItemType] on EntityUpdateBatch.
    public abstract class EntitySnapshot { }

    public class SessionSnapshot : EntitySnapshot
    {
        public SessionSnapshot() { PlayerIds = new List<ulong>(); }

        [MessagePackMember(0)] public ulong EntityId { get; set; }
        [MessagePackMember(1)] public uint Version { get; set; }
        [MessagePackMember(2)] public ulong MapId { get; set; }
        [MessagePackMember(3)] public int GameTick { get; set; }
        [MessagePackMember(4)] public List<ulong> PlayerIds { get; set; }
    }

    public class PlayerSnapshot : EntitySnapshot
    {
        public PlayerSnapshot() { Name = string.Empty; }

        [MessagePackMember(0)] public ulong EntityId { get; set; }
        [MessagePackMember(1)] public uint Version { get; set; }
        [MessagePackMember(2)] public string Name { get; set; }
        [MessagePackMember(3)] public int ColorR { get; set; }
        [MessagePackMember(4)] public int ColorG { get; set; }
        [MessagePackMember(5)] public int ColorB { get; set; }
        [MessagePackMember(6)] public ulong ArmyId { get; set; }
        [MessagePackMember(7)] public bool IsConnected { get; set; }
    }

    public class MapSnapshot : EntitySnapshot
    {
        public MapSnapshot() { RegionIds = new List<ulong>(); }

        [MessagePackMember(0)] public ulong EntityId { get; set; }
        [MessagePackMember(1)] public uint Version { get; set; }
        [MessagePackMember(2)] public int Width { get; set; }
        [MessagePackMember(3)] public int Height { get; set; }
        [MessagePackMember(4)] public int RegionSize { get; set; }
        [MessagePackMember(5)] public List<ulong> RegionIds { get; set; }
    }

    public class RegionSnapshot : EntitySnapshot
    {
        public RegionSnapshot() { ArmyIds = new List<ulong>(); CityIds = new List<ulong>(); }

        [MessagePackMember(0)] public ulong EntityId { get; set; }
        [MessagePackMember(1)] public uint Version { get; set; }
        [MessagePackMember(2)] public int GridX { get; set; }
        [MessagePackMember(3)] public int GridY { get; set; }
        [MessagePackMember(4)] public List<ulong> ArmyIds { get; set; }
        [MessagePackMember(5)] public List<ulong> CityIds { get; set; }
    }

    public class ArmySnapshot : EntitySnapshot
    {
        [MessagePackMember(0)] public ulong EntityId { get; set; }
        [MessagePackMember(1)] public uint Version { get; set; }
        [MessagePackMember(2)] public ulong OwnerId { get; set; }
        [MessagePackMember(3)] public int X { get; set; }
        [MessagePackMember(4)] public int Y { get; set; }
        [MessagePackMember(5)] public ulong RegionId { get; set; }
        [MessagePackMember(6)] public int Health { get; set; }
        [MessagePackMember(7)] public bool IsDead { get; set; }
    }

    public class CitySnapshot : EntitySnapshot
    {
        public CitySnapshot() { Name = string.Empty; }

        [MessagePackMember(0)] public ulong EntityId { get; set; }
        [MessagePackMember(1)] public uint Version { get; set; }
        [MessagePackMember(2)] public string Name { get; set; }
        [MessagePackMember(3)] public int X { get; set; }
        [MessagePackMember(4)] public int Y { get; set; }
        [MessagePackMember(5)] public ulong OwnerId { get; set; }
        [MessagePackMember(6)] public int VisualVariant { get; set; }
    }
}
