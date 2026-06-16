#nullable enable
using MsgPack.Serialization;

namespace CastleWars.Shared.Network
{
    // Base type for all commands flowing client → server.
    // No attribute needed: MsgPack.Cli serializes by reflection unless members carry [MessagePackMember].
    // Concrete types are registered via [MessagePackKnownCollectionItemType] on CommandBatch.Commands.
    public abstract class NetCommand { }

    // Wraps game-logic MoveArmyCommand data for the wire.
    public class MoveArmyNetCommand : NetCommand
    {
        [MessagePackMember(0)] public ulong ArmyId { get; set; }
        [MessagePackMember(1)] public int TargetX { get; set; }
        [MessagePackMember(2)] public int TargetY { get; set; }
    }

    // Wraps game-logic CreateMapCommand data for the wire.
    public class CreateMapNetCommand : NetCommand
    {
        [MessagePackMember(0)] public int Width { get; set; }
        [MessagePackMember(1)] public int Height { get; set; }
    }
}
