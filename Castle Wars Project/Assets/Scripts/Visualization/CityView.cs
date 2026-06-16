using CastleWars.Shared.Game.Entities;
using UnityEngine;

namespace CastleWars.Visualization
{
    [RequireComponent(typeof(Renderer))]
    public class CityView : MonoBehaviour
    {
        private Renderer _renderer;

        private void Awake() => _renderer = GetComponent<Renderer>();

        public void Refresh(CityEntity city, PlayerEntity? owner)
        {
            if (owner != null)
                _renderer.material.color = new Color(owner.ColorR / 255f, owner.ColorG / 255f, owner.ColorB / 255f);
            else
                _renderer.material.color = new Color(0.8f, 0.7f, 0.2f);
        }
    }
}
