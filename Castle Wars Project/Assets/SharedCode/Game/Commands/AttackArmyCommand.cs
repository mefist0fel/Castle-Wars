#nullable enable
using System;
using System.Linq;
using CastleWars.Shared.Core;
using CastleWars.Shared.Game.Entities;

namespace CastleWars.Shared.Game.Commands
{
    public class AttackArmyCommand : ILogicCommand
    {
        public ulong AttackerPlayerId { get; set; }
        public ulong TargetArmyId { get; set; }
    }

    public class AttackArmyCommandHandler : CommandHandler<AttackArmyCommand>
    {
        private readonly Random _rng;

        public AttackArmyCommandHandler(int seed = 0) => _rng = new Random(seed);

        public override bool CanExecute(AttackArmyCommand cmd, EntityRegistry registry)
        {
            var player = registry.Get<PlayerEntity>(cmd.AttackerPlayerId);
            if (player == null || player.ArmyId == 0) return false;

            var attacker = registry.Get<ArmyEntity>(player.ArmyId);
            if (attacker == null || attacker.IsDead) return false;

            var target = registry.Get<ArmyEntity>(cmd.TargetArmyId);
            if (target == null || target.IsDead) return false;

            // Cannot attack own army
            if (target.OwnerId == cmd.AttackerPlayerId) return false;

            // Must be adjacent (Chebyshev distance == 1)
            int dx = Math.Abs(attacker.X - target.X);
            int dy = Math.Abs(attacker.Y - target.Y);
            return dx <= 1 && dy <= 1 && (dx + dy) > 0;
        }

        public override void Execute(AttackArmyCommand cmd, EntityRegistry registry)
        {
            var player = registry.Get<PlayerEntity>(cmd.AttackerPlayerId)!;
            var attacker = registry.Get<ArmyEntity>(player.ArmyId)!;
            var target = registry.Get<ArmyEntity>(cmd.TargetArmyId)!;

            int damage = _rng.Next(1, 7);
            bool attackerTakesHit = _rng.Next(2) == 0;

            if (attackerTakesHit)
            {
                attacker.Health = Math.Max(0, attacker.Health - damage);
                if (attacker.Health == 0) attacker.IsDead = true;
                registry.Mutate(attacker);
            }
            else
            {
                target.Health = Math.Max(0, target.Health - damage);
                if (target.Health == 0) target.IsDead = true;
                registry.Mutate(target);
            }
        }
    }
}
