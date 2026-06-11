using System.Collections.Generic;
using System.Linq;
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
            var player  = Session.Get<PlayerEntity>(army.OwnerId);
            var faction = player != null ? Session.Get<FactionEntity>(player.FactionId) : null;
            view.Refresh(army, faction);

            var fromRegion = Session.Get<RegionEntity>(army.CurrentRegionId);
            if (fromRegion == null) return;

            var fromPos = mapVisualizer.ToWorld(fromRegion);

            if (army.TargetRegionId != 0)
            {
                var toRegion = Session.Get<RegionEntity>(army.TargetRegionId);
                if (toRegion != null)
                {
                    view.UpdatePosition(fromPos, mapVisualizer.ToWorld(toRegion));
                    return;
                }
            }

            view.SetPosition(fromPos);
        }
    }
}
