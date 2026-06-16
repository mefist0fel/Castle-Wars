#nullable enable
using System;
using CastleWars.Shared.Core;
using CastleWars.Shared.Game.Entities;

namespace CastleWars.Shared.Game.Commands
{
    public class HealArmyCommand : ILogicCommand
    {
        public ulong PlayerId { get; set; }
        public int Amount { get; set; } = 20;
    }

    public class HealArmyCommandHandler : CommandHandler<HealArmyCommand>
    {
        public override bool CanExecute(HealArmyCommand cmd, EntityRegistry registry)
        {
            var player = registry.Get<PlayerEntity>(cmd.PlayerId);
            if (player == null || player.ArmyId == 0) return false;

            var army = registry.Get<ArmyEntity>(player.ArmyId);
            return army != null && !army.IsDead && army.Health < 100;
        }

        public override void Execute(HealArmyCommand cmd, EntityRegistry registry)
        {
            var player = registry.Get<PlayerEntity>(cmd.PlayerId)!;
            var army = registry.Get<ArmyEntity>(player.ArmyId)!;
            army.Health = Math.Min(100, army.Health + cmd.Amount);
            registry.Mutate(army);
        }
    }
}
