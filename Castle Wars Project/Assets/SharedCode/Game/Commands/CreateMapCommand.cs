#nullable enable
using System.Linq;
using CastleWars.Shared.Core;
using CastleWars.Shared.Game.Entities;

namespace CastleWars.Shared.Game.Commands
{
    public class CreateMapCommand : ILogicCommand
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class CreateMapCommandHandler : CommandHandler<CreateMapCommand>
    {
        public override bool CanExecute(CreateMapCommand cmd, EntityRegistry registry)
        {
            if (cmd.Width <= 0 || cmd.Height <= 0) return false;

            var session = registry.GetAll<SessionEntity>().FirstOrDefault();
            if (session is null) return false;

            // Map can only be created once per session
            return session.MapId == 0;
        }

        public override void Execute(CreateMapCommand cmd, EntityRegistry registry)
        {
            var session = registry.GetAll<SessionEntity>().First();

            var map = new MapEntity { Width = cmd.Width, Height = cmd.Height };
            registry.Register(map);

            for (int x = 0; x < cmd.Width; x++)
            for (int y = 0; y < cmd.Height; y++)
            {
                var region = new RegionEntity { GridX = x, GridY = y };
                registry.Register(region);
                map.RegionIds.Add(region.Id);
            }

            session.MapId = map.Id;
            registry.Mutate(session);
            registry.Mutate(map);
        }
    }
}
