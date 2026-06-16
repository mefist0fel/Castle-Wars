#nullable enable
using System.Collections.Generic;
using CastleWars.Shared.Infrastructure;
using CastleWars.Shared.Network;
using UnityEngine;

namespace CastleWars.Client
{
    // Renders one region as a grid of cubes. Responds to GameClient snapshot updates.
    // Bind this to a GameClient and call ShowRegion() to display a region.
    //
    // Prefab setup:
    //   CellPrefab   — Cube with a CellView component
    //   ArmyPrefab   — Sphere with an ArmyMarker component
    //   CityPrefab   — Cube (smaller, different color) with a CityMarker component (and optional TMP label)
    public class MapCubeRenderer : MonoBehaviour
    {
        [SerializeField] private float cellWorldSize = 1f;
        [SerializeField] private GameObject? cellPrefab;
        [SerializeField] private GameObject? armyPrefab;
        [SerializeField] private GameObject? cityPrefab;

        private GameClient? _client;
        private ulong _currentRegionId;

        private readonly Dictionary<(int, int), CellView>    _cells   = new();
        private readonly Dictionary<ulong, ArmyMarker>       _armies  = new();
        private readonly Dictionary<ulong, CityMarker>       _cities  = new();

        private CellView? _selectedCell;

        public float CellWorldSize => cellWorldSize;

        public void Bind(GameClient client)
        {
            _client = client;
            _client.OnSnapshotUpdated     += HandleSnapshot;
            _client.OnSubscriptionsChanged += HandleSubscriptionsChanged;
        }

        private void OnDestroy()
        {
            if (_client == null) return;
            _client.OnSnapshotUpdated     -= HandleSnapshot;
            _client.OnSubscriptionsChanged -= HandleSubscriptionsChanged;
        }

        // Show the region that contains the player's army (call after Connect + SpawnArmy).
        public void ShowPlayerRegion()
        {
            var army = _client?.GetMyArmy();
            if (army == null) return;
            ShowRegion(army.RegionId);
        }

        public void ShowRegion(ulong regionId)
        {
            if (regionId == _currentRegionId) return;
            _currentRegionId = regionId;
            RebuildRegion();
        }

        public Vector3 CellToWorld(int x, int y)
            => new Vector3(x * cellWorldSize, 0f, y * cellWorldSize);

        public CellView? GetCell(int x, int y)
        {
            _cells.TryGetValue((x, y), out var c);
            return c;
        }

        public void SelectCell(int x, int y)
        {
            if (_selectedCell != null) _selectedCell.SetSelected(false);
            if (_cells.TryGetValue((x, y), out var cell))
            {
                cell.SetSelected(true);
                _selectedCell = cell;
            }
        }

        // ── Internal ──────────────────────────────────────────────────────────

        private void HandleSnapshot(EntitySnapshot snap)
        {
            if (_client == null) return;

            switch (snap)
            {
                case RegionSnapshot r when r.EntityId == _currentRegionId:
                    RefreshRegionContents(r);
                    break;

                case ArmySnapshot a when _armies.TryGetValue(a.EntityId, out var am):
                    am.Refresh(a, _client.GetSnapshot<PlayerSnapshot>(a.OwnerId));
                    break;

                case CitySnapshot c when _cities.TryGetValue(c.EntityId, out var cm):
                    cm.Refresh(c, _client.GetSnapshot<PlayerSnapshot>(c.OwnerId));
                    break;

                case PlayerSnapshot:
                    // Refresh any markers owned by this player
                    foreach (var am in _armies.Values)
                    {
                        if (am.Snapshot != null)
                            am.Refresh(am.Snapshot, _client.GetSnapshot<PlayerSnapshot>(am.Snapshot.OwnerId));
                    }
                    foreach (var cm in _cities.Values)
                    {
                        if (cm.Snapshot != null)
                            cm.Refresh(cm.Snapshot, _client.GetSnapshot<PlayerSnapshot>(cm.Snapshot.OwnerId));
                    }
                    break;
            }
        }

        private void HandleSubscriptionsChanged(ulong[] added, ulong[] removed)
        {
            // If our region subscription changed, rebuild
            foreach (var id in added)
                if (id == _currentRegionId) RebuildRegion();
        }

        private void RebuildRegion()
        {
            ClearAll();
            if (_client == null || _currentRegionId == 0) return;

            var region = _client.GetSnapshot<RegionSnapshot>(_currentRegionId);
            if (region == null) return;

            var map = GetMap();
            if (map == null) return;

            int size = map.RegionSize;
            int startX = region.GridX * size;
            int startY = region.GridY * size;

            for (int dx = 0; dx < size; dx++)
            for (int dy = 0; dy < size; dy++)
            {
                int cx = startX + dx, cy = startY + dy;
                if (cx >= map.Width || cy >= map.Height) continue;
                SpawnCell(cx, cy);
            }

            RefreshRegionContents(region);
        }

        private void RefreshRegionContents(RegionSnapshot region)
        {
            if (_client == null) return;

            // Armies
            var toRemoveArmies = new System.Collections.Generic.HashSet<ulong>(_armies.Keys);
            foreach (var id in region.ArmyIds)
            {
                toRemoveArmies.Remove(id);
                var snap = _client.GetSnapshot<ArmySnapshot>(id);
                if (snap == null) continue;
                if (!_armies.TryGetValue(id, out var marker))
                {
                    marker = SpawnArmy(snap.X, snap.Y);
                    _armies[id] = marker;
                }
                marker.Refresh(snap, _client.GetSnapshot<PlayerSnapshot>(snap.OwnerId));
                marker.transform.position = CellToWorld(snap.X, snap.Y) + Vector3.up * 0.7f;
            }
            foreach (var id in toRemoveArmies) DestroyMarker(_armies, id);

            // Cities
            var toRemoveCities = new System.Collections.Generic.HashSet<ulong>(_cities.Keys);
            foreach (var id in region.CityIds)
            {
                toRemoveCities.Remove(id);
                var snap = _client.GetSnapshot<CitySnapshot>(id);
                if (snap == null) continue;
                if (!_cities.TryGetValue(id, out var marker))
                {
                    marker = SpawnCity(snap.X, snap.Y);
                    _cities[id] = marker;
                }
                marker.Refresh(snap, _client.GetSnapshot<PlayerSnapshot>(snap.OwnerId));
            }
            foreach (var id in toRemoveCities) DestroyMarker(_cities, id);
        }

        private void SpawnCell(int x, int y)
        {
            var go = cellPrefab != null
                ? Instantiate(cellPrefab, CellToWorld(x, y), Quaternion.identity, transform)
                : CreatePrimitive(PrimitiveType.Cube, x, y, Vector3.zero, new Vector3(cellWorldSize * 0.98f, 0.1f, cellWorldSize * 0.98f));
            var view = go.GetComponent<CellView>() ?? go.AddComponent<CellView>();
            view.Init(x, y);
            _cells[(x, y)] = view;
        }

        private ArmyMarker SpawnArmy(int x, int y)
        {
            var go = armyPrefab != null
                ? Instantiate(armyPrefab, CellToWorld(x, y) + Vector3.up * 0.7f, Quaternion.identity, transform)
                : CreatePrimitive(PrimitiveType.Sphere, x, y, Vector3.up * 0.7f, Vector3.one * (cellWorldSize * 0.4f));
            return go.GetComponent<ArmyMarker>() ?? go.AddComponent<ArmyMarker>();
        }

        private CityMarker SpawnCity(int x, int y)
        {
            var go = cityPrefab != null
                ? Instantiate(cityPrefab, CellToWorld(x, y) + Vector3.up * 0.3f, Quaternion.identity, transform)
                : CreatePrimitive(PrimitiveType.Cube, x, y, Vector3.up * 0.3f, Vector3.one * (cellWorldSize * 0.5f));
            return go.GetComponent<CityMarker>() ?? go.AddComponent<CityMarker>();
        }

        private GameObject CreatePrimitive(PrimitiveType type, int x, int y, Vector3 yOffset, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(type);
            go.transform.SetParent(transform);
            go.transform.position   = CellToWorld(x, y) + yOffset;
            go.transform.localScale = scale;
            return go;
        }

        private static void DestroyMarker<T>(Dictionary<ulong, T> dict, ulong id) where T : MonoBehaviour
        {
            if (dict.TryGetValue(id, out var m)) { Destroy(m.gameObject); dict.Remove(id); }
        }

        private void ClearAll()
        {
            foreach (var c in _cells.Values)   if (c) Destroy(c.gameObject);
            foreach (var a in _armies.Values)  if (a) Destroy(a.gameObject);
            foreach (var c in _cities.Values)  if (c) Destroy(c.gameObject);
            _cells.Clear(); _armies.Clear(); _cities.Clear();
            _selectedCell = null;
        }

        private MapSnapshot? GetMap()
        {
            if (_client == null) return null;
            var session = _client.GetSession();
            return session != null ? _client.GetSnapshot<MapSnapshot>(session.MapId) : null;
        }
    }
}
