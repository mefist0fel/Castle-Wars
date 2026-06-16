using CastleWars.Shared.Game.Entities;
using UnityEngine;

namespace CastleWars.Visualization
{
    [RequireComponent(typeof(Renderer))]
    public class ArmyView : MonoBehaviour
    {
        private Renderer _renderer;

        public ArmyEntity? Army { get; private set; }

        private void Awake() => _renderer = GetComponent<Renderer>();

        public void Refresh(ArmyEntity army, PlayerEntity? owner)
        {
            Army = army;
            if (owner != null)
                _renderer.material.color = new Color(owner.ColorR / 255f, owner.ColorG / 255f, owner.ColorB / 255f);
            else
                _renderer.material.color = Color.white;
        }

        public void SetPosition(Vector3 worldPos)
            => transform.position = worldPos + Vector3.up * 0.6f;
    }
}
