#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using CastleWars.Shared.Core;
using CastleWars.Shared.Game.Entities;

namespace CastleWars.Shared.Game.Commands
{
    public class CreateGameCommand : ILogicCommand
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int RegionSize { get; set; }
        public int CityCount { get; set; }
        public int BarbarianCount { get; set; }
        public int Seed { get; set; }
    }

    public class CreateGameCommandHandler : CommandHandler<CreateGameCommand>
    {
        private static readonly string[] CityNames =
        {
            "Ironhold", "Ashveil", "Coldmere", "Duskport", "Embervast",
            "Frostwall", "Grimhaven", "Hollowgate", "Icemoor", "Jadekeep",
            "Koldrun", "Lostmark", "Mordfeld", "Nightspur", "Oldfort"
        };

        public override bool CanExecute(CreateGameCommand cmd, EntityRegistry registry)
        {
            if (cmd.Width <= 0 || cmd.Height <= 0 || cmd.RegionSize <= 0) return false;
            var session = registry.GetAll<SessionEntity>().FirstOrDefault();
            return session != null && session.MapId == 0;
        }

        public override void Execute(CreateGameCommand cmd, EntityRegistry registry)
        {
            var session = registry.GetAll<SessionEntity>().First();
            var rng = new Random(cmd.Seed);

            var map = new MapEntity
            {
                Width = cmd.Width,
                Height = cmd.Height,
                RegionSize = cmd.RegionSize
            };
            registry.Register(map);

            int regCols = (cmd.Width + cmd.RegionSize - 1) / cmd.RegionSize;
            int regRows = (cmd.Height + cmd.RegionSize - 1) / cmd.RegionSize;
            var regionGrid = new Dictionary<(int, int), RegionEntity>();

            for (int gx = 0; gx < regCols; gx++)
            for (int gy = 0; gy < regRows; gy++)
            {
                var region = new RegionEntity { GridX = gx, GridY = gy };
                registry.Register(region);
                map.RegionIds.Add(region.Id);
                regionGrid[(gx, gy)] = region;
            }

            var occupied = new HashSet<(int, int)>();
            int cityCount = Math.Min(cmd.CityCount, CityNames.Length);
            var shuffledNames = CityNames.OrderBy(_ => rng.Next()).Take(cityCount).ToArray();

            for (int i = 0; i < cityCount; i++)
            {
                var pos = RandomFreeCell(rng, cmd.Width, cmd.Height, occupied);
                if (pos == null) break;
                occupied.Add(pos.Value);

                var city = new CityEntity
                {
                    Name = shuffledNames[i],
                    X = pos.Value.Item1,
                    Y = pos.Value.Item2,
                    VisualVariant = rng.Next(3)
                };
                registry.Register(city);

                var region = RegionAt(regionGrid, pos.Value.Item1, pos.Value.Item2, cmd.RegionSize);
                if (region != null)
                {
                    region.CityIds.Add(city.Id);
                    registry.Mutate(region);
                }
            }

            for (int i = 0; i < cmd.BarbarianCount; i++)
            {
                var pos = RandomFreeCell(rng, cmd.Width, cmd.Height, occupied);
                if (pos == null) break;
                occupied.Add(pos.Value);

                var army = new ArmyEntity
                {
                    OwnerId = 0,
                    X = pos.Value.Item1,
                    Y = pos.Value.Item2,
                    Health = 50
                };
                registry.Register(army);

                var region = RegionAt(regionGrid, pos.Value.Item1, pos.Value.Item2, cmd.RegionSize);
                if (region != null)
                {
                    army.RegionId = region.Id;
                    region.ArmyIds.Add(army.Id);
                    registry.Mutate(army);
                    registry.Mutate(region);
                }
            }

            session.MapId = map.Id;
            registry.Mutate(session);
            registry.Mutate(map);
        }

        private static (int, int)? RandomFreeCell(Random rng, int w, int h, HashSet<(int, int)> occupied)
        {
            int attempts = w * h;
            for (int i = 0; i < attempts; i++)
            {
                var pos = (rng.Next(w), rng.Next(h));
                if (!occupied.Contains(pos)) return pos;
            }
            return null;
        }

        private static RegionEntity? RegionAt(Dictionary<(int, int), RegionEntity> grid, int x, int y, int size)
        {
            grid.TryGetValue((x / size, y / size), out var r);
            return r;
        }
    }
}
