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

            RegisterHandler(new CreateMapCommandHandler());
            RegisterHandler(new MoveArmyCommandHandler());
        }

        // Shortcut to the fixed routing entity
        public SessionEntity Session => Registry.Get<SessionEntity>(1)!;

        // Used for seeding static/meta data (factions, players) outside the command system
        public ulong Seed(BaseEntity entity) => Registry.Register(entity);
    }
}
