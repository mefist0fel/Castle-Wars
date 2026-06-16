using System.Collections.Generic;
using CastleWars.Shared.Network;
using MsgPack.Serialization;

Console.WriteLine("Castle Wars Server starting...\n");

// ── MessagePack serialization roundtrip test ──────────────────────────────────

Console.WriteLine("[MsgPack] Running serialization test...");

// Client → server: CommandBatch with polymorphic commands
var cmdBatch = new CommandBatch
{
    Tick = 1,
    Commands = new List<NetCommand>
    {
        new MoveArmyNetCommand { ArmyId = 7, TargetX = 3, TargetY = 2 },
        new CreateMapNetCommand { Width = 5, Height = 5 },
    }
};

var cmdSer   = MessagePackSerializer.Get<CommandBatch>();
var cmdBytes = cmdSer.PackSingleObject(cmdBatch);
var cmdBack  = cmdSer.UnpackSingleObject(cmdBytes);

Console.WriteLine($"[MsgPack] CommandBatch OK — {cmdBytes.Length} bytes, {cmdBack.Commands.Count} commands");
Console.WriteLine($"          cmd[0] type : {cmdBack.Commands[0].GetType().Name}");
Console.WriteLine($"          cmd[1] type : {cmdBack.Commands[1].GetType().Name}");

var move = (MoveArmyNetCommand)cmdBack.Commands[0];
Console.WriteLine($"          MoveArmy    : ArmyId={move.ArmyId}  Target=({move.TargetX},{move.TargetY})");

// Server → client: EntityUpdateBatch with polymorphic snapshots
var updBatch = new EntityUpdateBatch
{
    ServerTick = 42,
    Snapshots = new List<EntitySnapshot>
    {
        new ArmySnapshot { EntityId = 1, OwnerId = 2, UnitCount = 15, MovementProgress = 500 },
        new CitySnapshot { EntityId = 3, Name = "Redfort", OwnerId = 2, GarrisonCount = 20 },
    }
};

var updSer   = MessagePackSerializer.Get<EntityUpdateBatch>();
var updBytes = updSer.PackSingleObject(updBatch);
var updBack  = updSer.UnpackSingleObject(updBytes);

Console.WriteLine($"\n[MsgPack] EntityUpdateBatch OK — {updBytes.Length} bytes, {updBack.Snapshots.Count} snapshots");
Console.WriteLine($"          snap[0] type : {updBack.Snapshots[0].GetType().Name}");
Console.WriteLine($"          snap[1] type : {updBack.Snapshots[1].GetType().Name}");

var city = (CitySnapshot)updBack.Snapshots[1];
Console.WriteLine($"          CitySnapshot : Name={city.Name}  Garrison={city.GarrisonCount}");

Console.WriteLine("\n[MsgPack] All tests passed!\n");

// ─────────────────────────────────────────────────────────────────────────────

Console.ReadLine();
