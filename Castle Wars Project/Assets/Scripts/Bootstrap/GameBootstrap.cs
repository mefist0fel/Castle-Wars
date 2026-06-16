#nullable enable
using System.Collections.Generic;
using CastleWars.Client;
using CastleWars.Network;
using CastleWars.Shared.Infrastructure;
using CastleWars.Shared.Network;
using UnityEngine;

namespace CastleWars.Bootstrap
{
    // Main entry point for local play. Attach to one GameObject in the scene.
    // Wires: GameServer → LocalFakeConnection → GameClient → MapCubeRenderer.
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Map")]
        [SerializeField] private int mapWidth       = 10;
        [SerializeField] private int mapHeight      = 10;
        [SerializeField] private int regionSize     = 4;
        [SerializeField] private int cityCount      = 6;
        [SerializeField] private int barbarianCount = 4;
        [SerializeField] private int seed           = 42;

        [Header("Player 1")]
        [SerializeField] private string player1Name  = "Player1";
        [SerializeField] private Color  player1Color = Color.red;
        [SerializeField] private int    p1StartX     = 1;
        [SerializeField] private int    p1StartY     = 1;

        [Header("Player 2 (optional)")]
        [SerializeField] private bool   spawnPlayer2  = true;
        [SerializeField] private string player2Name   = "Player2";
        [SerializeField] private Color  player2Color  = Color.blue;
        [SerializeField] private int    p2StartX      = 8;
        [SerializeField] private int    p2StartY      = 8;

        [Header("Visualization")]
        [SerializeField] private MapCubeRenderer? mapRenderer;
        [SerializeField] private RegionCameraController? cameraController;

        // ── Public references for other scripts ───────────────────────────────
        public GameServer  Server  { get; private set; } = null!;
        public GameClient  Client1 { get; private set; } = null!;
        public GameClient? Client2 { get; private set; }

        private void Start()
        {
            Server = new GameServer();

            // ── Player 1 ──────────────────────────────────────────────────────
            var conn1 = new LocalFakeConnection(Server);
            Client1   = new GameClient(conn1);

            var r1 = ConnectClient(Client1, player1Name, player1Color);
            if (!r1.Success) { Debug.LogError("[Bootstrap] Player1 connect failed: " + r1.ErrorMessage); return; }

            // Create game world (done once by the first player)
            Client1.SendCommands(new CreateGameNetCommand
            {
                Width = mapWidth, Height = mapHeight, RegionSize = regionSize,
                CityCount = cityCount, BarbarianCount = barbarianCount, Seed = seed
            });

            // Pull full world state into Client1
            SubscribeToWorld(Client1);

            // Spawn player 1 army — server pushes update synchronously so client is up-to-date
            Server.SpawnPlayerArmy(Client1.PlayerId, p1StartX, p1StartY);
            SubscribeRegionContents(Client1);

            // ── Player 2 (optional) ───────────────────────────────────────────
            if (spawnPlayer2)
            {
                var conn2 = new LocalFakeConnection(Server);
                Client2   = new GameClient(conn2);

                var r2 = ConnectClient(Client2, player2Name, player2Color);
                if (r2.Success)
                {
                    SubscribeToWorld(Client2);
                    Server.SpawnPlayerArmy(Client2.PlayerId, p2StartX, p2StartY);
                    SubscribeRegionContents(Client2);
                }
            }

            // ── Wire visualization ─────────────────────────────────────────────
            if (mapRenderer != null)
            {
                mapRenderer.Bind(Client1);
                mapRenderer.ShowPlayerRegion();
            }
            if (cameraController != null)
            {
                cameraController.Bind(Client1);
                cameraController.JumpToPlayerArmy();
            }

            Debug.Log("[Bootstrap] Game ready.\n" + Server.GetDebugInfo());
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private ConnectResponse ConnectClient(GameClient client, string name, Color color)
            => client.Connect(name,
                Mathf.RoundToInt(color.r * 255),
                Mathf.RoundToInt(color.g * 255),
                Mathf.RoundToInt(color.b * 255));

        // Subscribe client to map entity + all region entities.
        private void SubscribeToWorld(GameClient client)
        {
            var session = client.GetSession();
            if (session == null || session.MapId == 0) return;

            client.Subscribe(session.MapId);

            var map = client.GetSnapshot<MapSnapshot>(session.MapId);
            if (map == null || map.RegionIds.Count == 0) return;

            client.Subscribe(map.RegionIds.ToArray());
        }

        // After army is spawned, subscribe to cities/armies in the player's starting region.
        private void SubscribeRegionContents(GameClient client)
        {
            var army = client.GetMyArmy();
            if (army == null) return;

            var region = client.GetSnapshot<RegionSnapshot>(army.RegionId);
            if (region == null) return;

            var ids = new List<ulong>(region.ArmyIds);
            ids.AddRange(region.CityIds);
            if (ids.Count > 0)
                client.Subscribe(ids.ToArray());
        }
    }
}
