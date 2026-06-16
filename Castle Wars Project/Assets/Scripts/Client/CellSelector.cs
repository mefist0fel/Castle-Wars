#nullable enable
using CastleWars.Shared.Infrastructure;
using UnityEngine;

namespace CastleWars.Client
{
    // Attach to the Main Camera. On click: raycasts for a CellView,
    // selects it, and tells ContextMenuUI what to display.
    public class CellSelector : MonoBehaviour
    {
        [SerializeField] private MapCubeRenderer? mapRenderer;
        [SerializeField] private ContextMenuUI?   contextMenu;
        [SerializeField] private LayerMask        cellLayer = Physics.DefaultRaycastLayers;

        private GameClient? _client;

        public void Bind(GameClient client) => _client = client;

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0)) return;

            var ray = Camera.main != null
                ? Camera.main.ScreenPointToRay(Input.mousePosition)
                : new Ray(transform.position, transform.forward);

            if (!Physics.Raycast(ray, out var hit, 100f, cellLayer)) return;

            var cell = hit.collider.GetComponentInParent<CellView>();
            if (cell == null) return;

            mapRenderer?.SelectCell(cell.CellX, cell.CellY);
            contextMenu?.ShowForCell(cell.CellX, cell.CellY, _client);
        }
    }
}
