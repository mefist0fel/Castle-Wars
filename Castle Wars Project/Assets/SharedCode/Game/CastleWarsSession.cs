#nullable enable
using CastleWars.Shared.Core;
using CastleWars.Shared.Game.Commands;
using CastleWars.Shared.Game.Entities;

namespace CastleWars.Shared.Game
{
    public class CastleWarsSession : GameSession
    {
        public CastleWarsSession()
        {
            // SessionEntity is always registered first → always gets Id = 1
            Registry.Register(new SessionEntity());

            RegisterHandler(new CreateGameCommandHandler());
            RegisterHandler(new TeleportArmyCommandHandler());
            RegisterHandler(new AttackArmyCommandHandler());
            RegisterHandler(new CaptureCityCommandHandler());
            RegisterHandler(new HealArmyCommandHandler());
        }

        public SessionEntity Session => Registry.Get<SessionEntity>(1)!;

        // Register pre-built entities (players, etc.) outside the command system
        public ulong Seed(BaseEntity entity) => Registry.Register(entity);
    }
}
