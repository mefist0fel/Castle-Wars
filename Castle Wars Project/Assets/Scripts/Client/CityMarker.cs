#nullable enable
using CastleWars.Shared.Network;
using TMPro;
using UnityEngine;

namespace CastleWars.Client
{
    // Cube sitting on a cell. Created by MapCubeRenderer for each CitySnapshot in the region.
    public class CityMarker : MonoBehaviour
    {
        public CitySnapshot? Snapshot { get; private set; }

        [SerializeField] private TextMeshPro? label;
        private Renderer? _renderer;

        private void Awake() => _renderer = GetComponent<Renderer>();

        public void Refresh(CitySnapshot city, PlayerSnapshot? owner)
        {
            Snapshot = city;
            if (label != null) label.text = city.Name;
            if (_renderer == null) return;
            _renderer.material.color = owner != null
                ? new Color(owner.ColorR / 255f, owner.ColorG / 255f, owner.ColorB / 255f)
                : new Color(0.8f, 0.7f, 0.2f);
        }
    }
}
