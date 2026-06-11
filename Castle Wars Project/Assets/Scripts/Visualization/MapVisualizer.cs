using System.Collections.Generic;
using CastleWars.Shared.Entities;
using CastleWars.Shared.World;
using UnityEngine;

namespace CastleWars.Visualization
{
    public class MapVisualizer : MonoBehaviour
    {
        [SerializeField] private float regionSize = 2f;

        private WorldState _world;
        private readonly Dictionary<ulong, ArmyView> _armyViews = new();

        public void Initialize(WorldState world)
        {
            _world = world;
            SpawnRegions();
            SpawnCities();
            SpawnArmies();
        }

        private void SpawnRegions()
        {
            foreach (var region in _world.GetAll<RegionEntity>())
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
                go.name = $"Region_{region.GridX}_{region.GridY}";
                go.transform.SetParent(transform);
                go.transform.localPosition = RegionToWorld(region);
                go.transform.localScale = Vector3.one * (regionSize / 10f);

                var owner = region.OwnerId != 0 ? _world.Get<FactionEntity>(region.OwnerId) : null;
                go.AddComponent<RegionView>().Bind(region, owner);
            }
        }

        private void SpawnCities()
        {
            foreach (var city in _world.GetAll<CityEntity>())
            {
                var region = _world.Get<RegionEntity>(city.RegionId);
                if (region == null) continue;

                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = $"City_{city.Name}";
                go.transform.SetParent(transform);
                go.transform.localPosition = RegionToWorld(region) + Vector3.up * 0.3f;
                go.transform.localScale = Vector3.one * 0.4f;

                var player = _world.Get<PlayerEntity>(city.OwnerId);
                var faction = player != null ? _world.Get<FactionEntity>(player.FactionId) : null;
                go.AddComponent<CityView>().Bind(city, faction);
            }
        }

        private void SpawnArmies()
        {
            foreach (var army in _world.GetAll<ArmyEntity>())
            {
                var region = _world.Get<RegionEntity>(army.CurrentRegionId);
                if (region == null) continue;

                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.name = $"Army_{army.Id}";
                go.transform.SetParent(transform);
                go.transform.localScale = Vector3.one * 0.35f;

                var player = _world.Get<PlayerEntity>(army.OwnerId);
                var faction = player != null ? _world.Get<FactionEntity>(player.FactionId) : null;

                var view = go.AddComponent<ArmyView>();
                view.Bind(army, faction);
                view.SetPosition(RegionToWorld(region));

                _armyViews[army.Id] = view;
            }
        }

        private void Update()
        {
            if (_world == null) return;

            foreach (var army in _world.GetAll<ArmyEntity>())
            {
                if (!_armyViews.TryGetValue(army.Id, out var view)) continue;

                var from = _world.Get<RegionEntity>(army.CurrentRegionId);
                if (from == null) continue;

                if (army.TargetRegionId != 0)
                {
                    var to = _world.Get<RegionEntity>(army.TargetRegionId);
                    if (to != null) { view.UpdatePosition(RegionToWorld(from), RegionToWorld(to)); continue; }
                }

                view.SetPosition(RegionToWorld(from));
            }
        }

        public Vector3 RegionToWorld(RegionEntity region)
            => new(region.GridX * regionSize, 0f, region.GridY * regionSize);
    }
}
