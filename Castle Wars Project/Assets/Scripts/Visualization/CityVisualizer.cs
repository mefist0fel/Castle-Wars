using System.Collections.Generic;
using CastleWars.Shared.Core;
using CastleWars.Shared.Game.Entities;
using UnityEngine;

namespace CastleWars.Visualization
{
    public class CityVisualizer : EntityVisualizer
    {
        [SerializeField] private MapVisualizer mapVisualizer;

        private readonly Dictionary<ulong, CityView> _views = new();

        protected override void HandleRegistered(BaseEntity entity)
        {
            if (entity is CityEntity city)
                SpawnCity(city);
        }

        protected override void HandleMutated(BaseEntity entity)
        {
            if (entity is CityEntity city && _views.TryGetValue(city.Id, out var view))
                RefreshCity(city, view);
        }

        private void SpawnCity(CityEntity city)
        {
            var region = Session.Get<RegionEntity>(city.RegionId);
            if (region == null) return;

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"City_{city.Name}";
            go.transform.SetParent(transform);
            go.transform.localPosition = mapVisualizer.ToWorld(region) + Vector3.up * 0.3f;
            go.transform.localScale    = Vector3.one * 0.4f;

            var view = go.AddComponent<CityView>();
            _views[city.Id] = view;
            RefreshCity(city, view);
        }

        private void RefreshCity(CityEntity city, CityView view)
        {
            var player  = Session.Get<PlayerEntity>(city.OwnerId);
            var faction = player != null ? Session.Get<FactionEntity>(player.FactionId) : null;
            view.Refresh(city, faction);
        }
    }
}
