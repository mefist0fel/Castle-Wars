using System.Collections.Generic;
using System.Linq;
using CastleWars.Shared.Core;
using CastleWars.Shared.Game.Entities;
using UnityEngine;

namespace CastleWars.Visualization
{
    public class MapVisualizer : EntityVisualizer
    {
        [SerializeField] private float regionSize = 2f;
        [SerializeField] private Vector3 offset = Vector3.zero;

        private readonly Dictionary<ulong, RegionView> _views = new();

        protected override void HandleRegistered(BaseEntity entity)
        {
            if (entity is RegionEntity region)
                SpawnTile(region);
        }

        protected override void HandleMutated(BaseEntity entity)
        {
            if (entity is RegionEntity region && _views.TryGetValue(region.Id, out var view))
                ApplyColor(region, view);
        }

        private void SpawnTile(RegionEntity region)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
            go.name = $"Region_{region.GridX}_{region.GridY}";
            go.transform.SetParent(transform);
            go.transform.localPosition = ToWorld(region);
            go.transform.localScale    = Vector3.one * (regionSize / 10f);

            var view = go.AddComponent<RegionView>();
            _views[region.Id] = view;
            ApplyColor(region, view);
        }

        private void ApplyColor(RegionEntity region, RegionView view)
        {
            var owner = region.OwnerId != 0
                ? Session.Query<FactionEntity>().FirstOrDefault(f => f.Id == region.OwnerId)
                : null;
            view.Refresh(region, owner);
        }

        public Vector3 ToWorld(RegionEntity region)
            => new Vector3(region.GridX * regionSize, 0f, region.GridY * regionSize) + offset;
    }
}
