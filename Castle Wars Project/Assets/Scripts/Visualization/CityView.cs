using CastleWars.Shared.Game.Entities;
using UnityEngine;

namespace CastleWars.Visualization
{
    [RequireComponent(typeof(Renderer))]
    public class CityView : MonoBehaviour
    {
        private Renderer _renderer;

        private void Awake() => _renderer = GetComponent<Renderer>();

        public void Refresh(CityEntity city, FactionEntity faction)
        {
            _renderer.material.color = faction != null
                ? new Color(faction.ColorR / 255f, faction.ColorG / 255f, faction.ColorB / 255f)
                : Color.white;
        }
    }
}
