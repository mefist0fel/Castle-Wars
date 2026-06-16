#nullable enable
using CastleWars.Shared.Network;
using UnityEngine;

namespace CastleWars.Client
{
    // Sphere sitting on a cell. Created by MapCubeRenderer for each ArmySnapshot in the region.
    public class ArmyMarker : MonoBehaviour
    {
        public ArmySnapshot? Snapshot { get; private set; }

        private Renderer? _renderer;

        private void Awake() => _renderer = GetComponent<Renderer>();

        public void Refresh(ArmySnapshot army, PlayerSnapshot? owner)
        {
            Snapshot = army;
            if (_renderer == null) return;
            _renderer.material.color = owner != null
                ? new Color(owner.ColorR / 255f, owner.ColorG / 255f, owner.ColorB / 255f)
                : Color.white;
            // Visually indicate dead armies (dim)
            if (army.IsDead)
            {
                var c = _renderer.material.color;
                _renderer.material.color = new Color(c.r * 0.4f, c.g * 0.4f, c.b * 0.4f);
            }
        }
    }
}
