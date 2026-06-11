using CastleWars.Shared.Game.Entities;
using UnityEngine;

namespace CastleWars.Visualization
{
    [RequireComponent(typeof(Renderer))]
    public class RegionView : MonoBehaviour
    {
        private Renderer _renderer;

        private void Awake() => _renderer = GetComponent<Renderer>();

        public void Refresh(RegionEntity region, FactionEntity owner)
        {
            _renderer.material.color = owner != null
                ? new Color(owner.ColorR / 255f, owner.ColorG / 255f, owner.ColorB / 255f, 0.6f)
                : new Color(0.45f, 0.45f, 0.45f);
        }
    }
}
