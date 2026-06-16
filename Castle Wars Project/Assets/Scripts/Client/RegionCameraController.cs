#nullable enable
using CastleWars.Shared.Infrastructure;
using CastleWars.Shared.Network;
using UnityEngine;

namespace CastleWars.Client
{
    // Attach to the Main Camera. Pans within the current region.
    // WASD / arrow keys to pan. Home key to jump to player's army.
    public class RegionCameraController : MonoBehaviour
    {
        [SerializeField] private float panSpeed    = 5f;
        [SerializeField] private float height      = 8f;
        [SerializeField] private float tiltAngle   = 55f;
        [SerializeField] private MapCubeRenderer? mapRenderer;

        private GameClient? _client;

        public void Bind(GameClient client)
        {
            _client = client;
        }

        private void Start()
        {
            var angles = transform.eulerAngles;
            transform.eulerAngles = new Vector3(tiltAngle, angles.y, angles.z);
        }

        private void Update()
        {
            if (mapRenderer == null) return;

            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
            {
                var move = new Vector3(h, 0, v) * (panSpeed * Time.deltaTime);
                transform.position += move;
            }

            if (Input.GetKeyDown(KeyCode.Home))
                JumpToPlayerArmy();
        }

        // Centers camera on the player's army cell.
        public void JumpToPlayerArmy()
        {
            if (_client == null || mapRenderer == null) return;
            var army = _client.GetMyArmy();
            if (army == null) return;
            var worldPos = mapRenderer.CellToWorld(army.X, army.Y);
            transform.position = new Vector3(worldPos.x, height, worldPos.z - height * 0.6f);
        }

        // Jump camera to the center of a given region.
        public void JumpToRegion(RegionSnapshot region, MapSnapshot map)
        {
            if (mapRenderer == null) return;
            float cx = (region.GridX + 0.5f) * map.RegionSize;
            float cy = (region.GridY + 0.5f) * map.RegionSize;
            var worldPos = mapRenderer.CellToWorld(Mathf.RoundToInt(cx), Mathf.RoundToInt(cy));
            transform.position = new Vector3(worldPos.x, height, worldPos.z - height * 0.6f);
        }
    }
}
