#nullable enable
using System.Linq;
using CastleWars.Shared.Core;
using CastleWars.Shared.Game.Entities;

namespace CastleWars.Shared.Game.Commands
{
    public class MoveArmyCommand : ILogicCommand
    {
        public ulong ArmyId { get; set; }
        public int TargetX { get; set; }
        public int TargetY { get; set; }
    }

    public class MoveArmyCommandHandler : CommandHandler<MoveArmyCommand>
    {
        public override bool CanExecute(MoveArmyCommand cmd, EntityRegistry registry)
        {
            var army = registry.Get<ArmyEntity>(cmd.ArmyId);
            if (army is null) return false;

            var target = registry.GetAll<RegionEntity>()
                .FirstOrDefault(r => r.GridX == cmd.TargetX && r.GridY == cmd.TargetY);
            if (target is null) return false;

            bool occupied = registry.GetAll<ArmyEntity>().Any(a =>
                a.Id != army.Id &&
                (a.CurrentRegionId == target.Id || a.TargetRegionId == target.Id));

            return !occupied;
        }

        public override void Execute(MoveArmyCommand cmd, EntityRegistry registry)
        {
            var army   = registry.Get<ArmyEntity>(cmd.ArmyId)!;
            var target = registry.GetAll<RegionEntity>()
                .First(r => r.GridX == cmd.TargetX && r.GridY == cmd.TargetY);

            army.TargetRegionId   = target.Id;
            army.MovementProgress = 0;
            registry.Mutate(army);
        }
    }
}
