using CastleWars.Shared.Entities;
using CastleWars.Shared.World;
using CastleWars.Visualization;
using UnityEngine;

namespace CastleWars.Bootstrap
{
    // Attach to a GameObject in the scene. Assign MapVisualizer in the Inspector.
    // Drives a local tick loop — no networking, pure simulation for prototyping.
    public class LocalSimulation : MonoBehaviour
    {
        [SerializeField] private MapVisualizer mapVisualizer;
        [SerializeField] private float tickInterval = 0.5f;

        private WorldState _world;
        private float _tickTimer;

        private void Start()
        {
            _world = BuildTestWorld();
            mapVisualizer.Initialize(_world);
        }

        private void Update()
        {
            _tickTimer += Time.deltaTime;
            if (_tickTimer < tickInterval) return;
            _tickTimer = 0f;
            Tick();
        }

        private void Tick()
        {
            foreach (var army in _world.GetAll<ArmyEntity>())
            {
                if (army.TargetRegionId == 0) continue;

                army.MovementProgress += 100; // +10% per tick

                if (army.MovementProgress >= 1000)
                {
                    army.CurrentRegionId = army.TargetRegionId;
                    army.TargetRegionId = 0;
                    army.MovementProgress = 0;
                }

                _world.Mutate(army);
            }
        }

        private static WorldState BuildTestWorld()
        {
            var w = new WorldState();

            // --- Factions ---
            var redId  = w.Register(new FactionEntity { Name = "Red Kingdom",  ColorR = 200, ColorG = 50,  ColorB = 50  });
            var blueId = w.Register(new FactionEntity { Name = "Blue Empire",  ColorR = 50,  ColorG = 100, ColorB = 200 });

            // --- Players ---
            var p1 = w.Register(new PlayerEntity { Name = "Alice", FactionId = redId,  Gold = 500 });
            var p2 = w.Register(new PlayerEntity { Name = "Bob",   FactionId = blueId, Gold = 500 });

            // --- 5×5 region grid ---
            // x < 2 → Red, x > 2 → Blue, x == 2 → neutral
            var rid = new ulong[5, 5];
            for (int x = 0; x < 5; x++)
            for (int y = 0; y < 5; y++)
            {
                ulong owner = x < 2 ? redId : x > 2 ? blueId : 0UL;
                rid[x, y] = w.Register(new RegionEntity { GridX = x, GridY = y, OwnerId = owner });
            }

            // --- Cities ---
            w.Register(new CityEntity { Name = "Redfort",   RegionId = rid[0, 2], OwnerId = p1, GarrisonCount = 20 });
            w.Register(new CityEntity { Name = "Ironhold",  RegionId = rid[1, 1], OwnerId = p1, GarrisonCount = 10 });
            w.Register(new CityEntity { Name = "Bluehaven", RegionId = rid[4, 2], OwnerId = p2, GarrisonCount = 20 });
            w.Register(new CityEntity { Name = "Coldmere",  RegionId = rid[3, 3], OwnerId = p2, GarrisonCount = 10 });

            // --- Armies ---
            w.Register(new ArmyEntity
            {
                OwnerId = p1, UnitCount = 15,
                CurrentRegionId = rid[1, 2], TargetRegionId = rid[2, 2],
                MovementProgress = 0
            });
            w.Register(new ArmyEntity
            {
                OwnerId = p2, UnitCount = 10,
                CurrentRegionId = rid[3, 2], TargetRegionId = 0,
                MovementProgress = 0
            });

            return w;
        }
    }
}
