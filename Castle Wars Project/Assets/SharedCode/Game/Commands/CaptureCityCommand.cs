#nullable enable
using CastleWars.Shared.Core;
using CastleWars.Shared.Game.Entities;

namespace CastleWars.Shared.Game.Commands
{
    public class CaptureCityCommand : ILogicCommand
    {
        public ulong PlayerId { get; set; }
        public ulong CityId { get; set; }
    }

    public class CaptureCityCommandHandler : CommandHandler<CaptureCityCommand>
    {
        public override bool CanExecute(CaptureCityCommand cmd, EntityRegistry registry)
        {
            var player = registry.Get<PlayerEntity>(cmd.PlayerId);
            if (player == null || player.ArmyId == 0) return false;

            var army = registry.Get<ArmyEntity>(player.ArmyId);
            if (army == null || army.IsDead) return false;

            var city = registry.Get<CityEntity>(cmd.CityId);
            if (city == null) return false;

            // Army must stand on the city cell
            return army.X == city.X && army.Y == city.Y;
        }

        public override void Execute(CaptureCityCommand cmd, EntityRegistry registry)
        {
            var city = registry.Get<CityEntity>(cmd.CityId)!;
            city.OwnerId = cmd.PlayerId;
            registry.Mutate(city);
        }
    }
}
