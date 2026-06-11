using CastleWars.Shared.Game;
using CastleWars.Shared.Game.Commands;
using CastleWars.Shared.Game.Entities;
using CastleWars.Visualization;
using System.Linq;
using UnityEngine;

namespace CastleWars.Bootstrap
{
    // Attach to one GameObject. Assign the three visualizer references in the Inspector.
    // Drives a local tick loop — no networking, pure in-process simulation.
    public class LocalSimulation : MonoBehaviour
    {
        [SerializeField] private MapVisualizer  mapVisualizer;
        [SerializeField] private ArmyVisualizer armyVisualizer;
        [SerializeField] private CityVisualizer cityVisualizer;
        [SerializeField] private float tickInterval = 0.5f;

        private CastleWarsSession _session;
        private float _tickTimer;

        private void Start()
        {
            _session = new CastleWarsSession();

            // Bind BEFORE applying commands so visualizers catch every entity registration
            mapVisualizer.Bind(_session);
            armyVisualizer.Bind(_session);
            cityVisualizer.Bind(_session);

            // Generate the map — MapVisualizer will receive OnEntityRegistered for every region
            _session.Apply(new CreateMapCommand { Width = 5, Height = 5 });

            SeedTestData();
        }

        private void SeedTestData()
        {
            var redId  = _session.Seed(new FactionEntity { Name = "Red Kingdom",  ColorR = 200, ColorG = 50,  ColorB = 50  });
            var blueId = _session.Seed(new FactionEntity { Name = "Blue Empire",  ColorR = 50,  ColorG = 100, ColorB = 200 });

            var p1 = _session.Seed(new PlayerEntity { Name = "Alice", FactionId = redId,  Gold = 500 });
            var p2 = _session.Seed(new PlayerEntity { Name = "Bob",   FactionId = blueId, Gold = 500 });

            _session.Seed(new CityEntity { Name = "Redfort",   RegionId = RegionId(0, 2), OwnerId = p1, GarrisonCount = 20 });
            _session.Seed(new CityEntity { Name = "Ironhold",  RegionId = RegionId(1, 1), OwnerId = p1, GarrisonCount = 10 });
            _session.Seed(new CityEntity { Name = "Bluehaven", RegionId = RegionId(4, 2), OwnerId = p2, GarrisonCount = 20 });
            _session.Seed(new CityEntity { Name = "Coldmere",  RegionId = RegionId(3, 3), OwnerId = p2, GarrisonCount = 10 });

            var army1 = _session.Seed(new ArmyEntity { OwnerId = p1, UnitCount = 15, CurrentRegionId = RegionId(1, 2) });
            _session.Seed(new ArmyEntity                             { OwnerId = p2, UnitCount = 10, CurrentRegionId = RegionId(3, 2) });

            // Kick off first movement through the command system
            _session.Apply(new MoveArmyCommand { ArmyId = army1, TargetX = 2, TargetY = 2 });
        }

        private ulong RegionId(int x, int y)
            => _session.Query<RegionEntity>().FirstOrDefault(r => r.GridX == x && r.GridY == y)?.Id ?? 0;

        private void Update()
        {
            if (_session == null) return;
            _tickTimer += Time.deltaTime;
            if (_tickTimer < tickInterval) return;
            _tickTimer = 0f;
            Tick();
        }

        private void Tick()
        {
            foreach (var army in _session.Query<ArmyEntity>())
            {
                if (army.TargetRegionId == 0) continue;

                army.MovementProgress += 100;
                if (army.MovementProgress >= 1000)
                {
                    army.CurrentRegionId  = army.TargetRegionId;
                    army.TargetRegionId   = 0;
                    army.MovementProgress = 0;
                }

                _session.Mutate(army); // fires OnEntityMutated → ArmyVisualizer updates position
            }
        }
    }
}
