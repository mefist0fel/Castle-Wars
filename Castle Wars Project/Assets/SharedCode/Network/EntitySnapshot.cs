using MsgPack.Serialization;

namespace CastleWars.Shared.Network
{
    // Base type for all entity state snapshots flowing server → client.
    // Concrete types are registered via [MessagePackKnownCollectionItemType] on EntityUpdateBatch.Snapshots.
    public abstract class EntitySnapshot { }

    public class ArmySnapshot : EntitySnapshot
    {
        [MessagePackMember(0)] public ulong EntityId { get; set; }
        [MessagePackMember(1)] public ulong OwnerId { get; set; }
        [MessagePackMember(2)] public ulong CurrentRegionId { get; set; }
        [MessagePackMember(3)] public ulong TargetRegionId { get; set; }
        [MessagePackMember(4)] public int UnitCount { get; set; }
        // Fixed-point 0–1000 (= 0.0–1.0), same convention as ArmyEntity.MovementProgress
        [MessagePackMember(5)] public int MovementProgress { get; set; }
    }

    public class CitySnapshot : EntitySnapshot
    {
        public CitySnapshot() { Name = string.Empty; }

        [MessagePackMember(0)] public ulong EntityId { get; set; }
        [MessagePackMember(1)] public string Name { get; set; }
        [MessagePackMember(2)] public ulong RegionId { get; set; }
        [MessagePackMember(3)] public ulong OwnerId { get; set; }
        [MessagePackMember(4)] public int GarrisonCount { get; set; }
    }
}
