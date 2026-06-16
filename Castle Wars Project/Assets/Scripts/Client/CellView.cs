#nullable enable
using CastleWars.Shared.Network;
using UnityEngine;

namespace CastleWars.Client
{
    // One cell cube on the map. Spawned by MapCubeRenderer.
    public class CellView : MonoBehaviour
    {
        public int CellX { get; private set; }
        public int CellY { get; private set; }

        private Renderer? _renderer;
        private static readonly Color BaseColor    = new(0.35f, 0.55f, 0.25f);
        private static readonly Color SelectedColor = new(0.9f, 0.85f, 0.3f);

        private void Awake() => _renderer = GetComponent<Renderer>();

        public void Init(int x, int y)
        {
            CellX = x;
            CellY = y;
            if (_renderer != null) _renderer.material.color = BaseColor;
        }

        public void SetSelected(bool selected)
        {
            if (_renderer == null) return;
            _renderer.material.color = selected ? SelectedColor : BaseColor;
        }
    }
}
