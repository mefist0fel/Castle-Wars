using System.Collections.Generic;
using CastleWars.Shared.Core;
using CastleWars.Shared.Game.Entities;
using UnityEngine;

namespace CastleWars.Visualization
{
    public class MapVisualizer : EntityVisualizer
    {
        [SerializeField] private float regionSize = 2f;
        [SerializeField] private float cellSize   = 1f;
        [SerializeField] private Vector3 offset   = Vector3.zero;

        private readonly Dictionary<ulong, RegionView> _views = new();

        protected override void HandleRegistered(BaseEntity entity)
        {
            if (entity is RegionEntity region)
                SpawnTile(region);
        }

        protected override void HandleMutated(BaseEntity entity)
        {
            if (entity is RegionEntity region && _views.TryGetValue(region.Id, out var view))
                view.Refresh(region);
        }

        private void SpawnTile(RegionEntity region)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
            go.name = $"Region_{region.GridX}_{region.GridY}";
            go.transform.SetParent(transform);
            go.transform.localPosition = RegionToWorld(region);
            go.transform.localScale    = Vector3.one * (regionSize / 10f);

            var view = go.AddComponent<RegionView>();
            _views[region.Id] = view;
            view.Refresh(region);
        }

        public Vector3 RegionToWorld(RegionEntity region)
            => new Vector3(region.GridX * regionSize, 0f, region.GridY * regionSize) + offset;

        public Vector3 CellToWorld(int cellX, int cellY)
            => new Vector3(cellX * cellSize, 0f, cellY * cellSize) + offset;
    }
}
