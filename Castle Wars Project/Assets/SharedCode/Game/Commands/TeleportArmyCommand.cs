#nullable enable
using System.Linq;
using CastleWars.Shared.Core;
using CastleWars.Shared.Game.Entities;

namespace CastleWars.Shared.Game.Commands
{
    public class TeleportArmyCommand : ILogicCommand
    {
        public ulong PlayerId { get; set; }
        public int TargetX { get; set; }
        public int TargetY { get; set; }
    }

    public class TeleportArmyCommandHandler : CommandHandler<TeleportArmyCommand>
    {
        public override bool CanExecute(TeleportArmyCommand cmd, EntityRegistry registry)
        {
            var player = registry.Get<PlayerEntity>(cmd.PlayerId);
            if (player == null || player.ArmyId == 0) return false;

            var army = registry.Get<ArmyEntity>(player.ArmyId);
            if (army == null || army.IsDead) return false;

            var map = registry.GetAll<MapEntity>().FirstOrDefault();
            if (map == null) return false;

            return cmd.TargetX >= 0 && cmd.TargetX < map.Width &&
                   cmd.TargetY >= 0 && cmd.TargetY < map.Height;
        }

        public override void Execute(TeleportArmyCommand cmd, EntityRegistry registry)
        {
            var player = registry.Get<PlayerEntity>(cmd.PlayerId)!;
            var army = registry.Get<ArmyEntity>(player.ArmyId)!;
            var map = registry.GetAll<MapEntity>().First();

            // Remove from old region
            var oldRegion = registry.Get<RegionEntity>(army.RegionId);
            if (oldRegion != null)
            {
                oldRegion.ArmyIds.Remove(army.Id);
                registry.Mutate(oldRegion);
            }

            army.X = cmd.TargetX;
            army.Y = cmd.TargetY;
            army.RegionId = FindRegionId(registry, cmd.TargetX, cmd.TargetY, map.RegionSize);

            // Add to new region
            var newRegion = registry.Get<RegionEntity>(army.RegionId);
            if (newRegion != null)
            {
                if (!newRegion.ArmyIds.Contains(army.Id))
                    newRegion.ArmyIds.Add(army.Id);
                registry.Mutate(newRegion);
            }

            registry.Mutate(army);
        }

        private static ulong FindRegionId(EntityRegistry registry, int x, int y, int regionSize)
        {
            int gx = x / regionSize;
            int gy = y / regionSize;
            return registry.GetAll<RegionEntity>()
                .FirstOrDefault(r => r.GridX == gx && r.GridY == gy)?.Id ?? 0;
        }
    }
}
