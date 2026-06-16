using System.Collections.Generic;
using MsgPack.Serialization;

namespace CastleWars.Shared.Network
{
    // Client → server. One batch = one logical tick's worth of player input.
    // [MessagePackKnownCollectionItemType] teaches the serializer which concrete
    // NetCommand subtypes can appear in the list, encoding them as ["typeCode", data].
    public class CommandBatch
    {
        public CommandBatch()
        {
            Commands = new List<NetCommand>();
        }

        [MessagePackMember(0)]
        public long Tick { get; set; }

        [MessagePackMember(1)]
        [MessagePackKnownCollectionItemType("move",   typeof(MoveArmyNetCommand))]
        [MessagePackKnownCollectionItemType("create", typeof(CreateMapNetCommand))]
        public List<NetCommand> Commands { get; set; }
    }

    // Server → client. One batch = authoritative world state diff for a tick.
    // Add more [MessagePackKnownCollectionItemType] entries as new entity types appear.
    public class EntityUpdateBatch
    {
        public EntityUpdateBatch()
        {
            Snapshots = new List<EntitySnapshot>();
        }

        [MessagePackMember(0)]
        public long ServerTick { get; set; }

        [MessagePackMember(1)]
        [MessagePackKnownCollectionItemType("army", typeof(ArmySnapshot))]
        [MessagePackKnownCollectionItemType("city", typeof(CitySnapshot))]
        public List<EntitySnapshot> Snapshots { get; set; }
    }
}
