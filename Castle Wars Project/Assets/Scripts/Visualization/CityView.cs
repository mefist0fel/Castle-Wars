using CastleWars.Shared.Entities;
using UnityEngine;

namespace CastleWars.Visualization
{
    [RequireComponent(typeof(Renderer))]
    public class CityView : MonoBehaviour
    {
        private Renderer _renderer;

        private void Awake() => _renderer = GetComponent<Renderer>();

        public void Bind(CityEntity city, FactionEntity owner)
        {
            _renderer.material.color = owner != null
                ? new Color(owner.ColorR / 255f, owner.ColorG / 255f, owner.ColorB / 255f)
                : Color.white;
        }
    }
}
