using CastleWars.Shared.Entities;
using UnityEngine;

namespace CastleWars.Visualization
{
    [RequireComponent(typeof(Renderer))]
    public class ArmyView : MonoBehaviour
    {
        private Renderer _renderer;

        public ArmyEntity Army { get; private set; }

        private void Awake() => _renderer = GetComponent<Renderer>();

        public void Bind(ArmyEntity army, FactionEntity faction)
        {
            Army = army;
            _renderer.material.color = faction != null
                ? new Color(faction.ColorR / 255f, faction.ColorG / 255f, faction.ColorB / 255f)
                : Color.white;
        }

        public void UpdatePosition(Vector3 from, Vector3 to)
        {
            float t = Army.MovementProgress / 1000f;
            transform.position = Vector3.Lerp(from, to, t) + Vector3.up * 0.6f;
        }

        public void SetPosition(Vector3 regionWorld)
        {
            transform.position = regionWorld + Vector3.up * 0.6f;
        }
    }
}
