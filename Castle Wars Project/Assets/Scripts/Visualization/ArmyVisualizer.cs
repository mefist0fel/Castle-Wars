using System.Collections.Generic;
using CastleWars.Shared.Core;
using CastleWars.Shared.Game.Entities;
using UnityEngine;

namespace CastleWars.Visualization
{
    public class ArmyVisualizer : EntityVisualizer
    {
        [SerializeField] private MapVisualizer mapVisualizer;

        private readonly Dictionary<ulong, ArmyView> _views = new();

        protected override void HandleRegistered(BaseEntity entity)
        {
            if (entity is ArmyEntity army)
                SpawnArmy(army);
        }

        protected override void HandleMutated(BaseEntity entity)
        {
            if (entity is ArmyEntity army && _views.TryGetValue(army.Id, out var view))
                RefreshArmy(army, view);
        }

        private void SpawnArmy(ArmyEntity army)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = $"Army_{army.Id}";
            go.transform.SetParent(transform);
            go.transform.localScale = Vector3.one * 0.35f;

            var view = go.AddComponent<ArmyView>();
            _views[army.Id] = view;
            RefreshArmy(army, view);
        }

        private void RefreshArmy(ArmyEntity army, ArmyView view)
        {
            var owner = army.OwnerId != 0 ? Session.Get<PlayerEntity>(army.OwnerId) : null;
            view.Refresh(army, owner);
            view.SetPosition(mapVisualizer.CellToWorld(army.X, army.Y));
        }
    }
}
