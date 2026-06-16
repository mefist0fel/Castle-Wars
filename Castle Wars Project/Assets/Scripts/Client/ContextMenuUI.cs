#nullable enable
using System;
using CastleWars.Shared.Infrastructure;
using CastleWars.Shared.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CastleWars.Client
{
    // Context menu that appears when a cell is clicked.
    //
    // Prefab setup (create under a Canvas):
    //   GameObject "ContextMenu" with this script
    //     └─ Panel (background image)
    //          ├─ Button "TeleportBtn"  → assign to teleportButton
    //          ├─ Button "AttackBtn"    → assign to attackButton
    //          ├─ Button "CaptureBtn"   → assign to captureButton
    //          ├─ Button "HealBtn"      → assign to healButton
    //          └─ TextMeshProUGUI "InfoLabel" → assign to infoLabel
    public class ContextMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject?      panel;
        [SerializeField] private Button?          teleportButton;
        [SerializeField] private Button?          attackButton;
        [SerializeField] private Button?          captureButton;
        [SerializeField] private Button?          healButton;
        [SerializeField] private TextMeshProUGUI? infoLabel;

        private GameClient? _client;
        private int _cellX, _cellY;

        private void Awake()
        {
            teleportButton?.onClick.AddListener(OnTeleport);
            attackButton?.onClick.AddListener(OnAttack);
            captureButton?.onClick.AddListener(OnCapture);
            healButton?.onClick.AddListener(OnHeal);
            Hide();
        }

        public void ShowForCell(int x, int y, GameClient? client)
        {
            _client = client;
            _cellX = x; _cellY = y;

            if (client == null || panel == null) return;

            var myArmy   = client.GetMyArmy();
            var myPlayer = client.GetMyPlayer();

            bool armyAlive  = myArmy != null && !myArmy.IsDead;
            bool onSameCell = myArmy != null && myArmy.X == x && myArmy.Y == y;

            // Find what's on the target cell
            ArmySnapshot? targetArmy = FindArmyAt(client, x, y);
            CitySnapshot? targetCity = FindCityAt(client, x, y);

            bool targetIsEnemy = targetArmy != null && targetArmy.OwnerId != client.PlayerId;
            bool targetIsOwn   = targetArmy != null && targetArmy.OwnerId == client.PlayerId;
            bool isAdjacent    = myArmy != null && Math.Max(Math.Abs(myArmy.X - x), Math.Abs(myArmy.Y - y)) == 1;

            // Teleport: to any empty or own-army cell, not to enemy army cell
            bool canTeleport = armyAlive && !onSameCell && targetArmy?.OwnerId != client.PlayerId || (targetArmy == null);
            // Actually simplify: teleport to any cell that isn't occupied by an enemy alive army
            canTeleport = armyAlive && !(targetIsEnemy && !targetArmy!.IsDead);

            bool canAttack  = armyAlive && isAdjacent && targetIsEnemy && !targetArmy!.IsDead;
            bool canCapture = armyAlive && onSameCell && targetCity != null && targetCity.OwnerId != client.PlayerId;
            bool canHeal    = armyAlive && onSameCell && (myArmy?.Health ?? 100) < 100;

            SetBtn(teleportButton, canTeleport);
            SetBtn(attackButton,   canAttack);
            SetBtn(captureButton,  canCapture);
            SetBtn(healButton,     canHeal);

            if (infoLabel != null)
            {
                string info = $"Cell ({x},{y})";
                if (targetArmy != null) info += $"\nArmy hp={targetArmy.Health}{(targetArmy.IsDead?" [dead]":"")}";
                if (targetCity != null) info += $"\nCity: {targetCity.Name}";
                infoLabel.text = info;
            }

            panel.SetActive(true);
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }

        // ── Actions ───────────────────────────────────────────────────────────

        private void OnTeleport()
        {
            if (_client == null) return;
            _client.SendCommands(new TeleportArmyNetCommand
            {
                PlayerId = _client.PlayerId, TargetX = _cellX, TargetY = _cellY
            });
            Hide();
        }

        private void OnAttack()
        {
            if (_client == null) return;
            var target = FindArmyAt(_client, _cellX, _cellY);
            if (target == null) return;
            _client.SendCommands(new AttackArmyNetCommand
            {
                AttackerPlayerId = _client.PlayerId, TargetArmyId = target.EntityId
            });
            Hide();
        }

        private void OnCapture()
        {
            if (_client == null) return;
            var city = FindCityAt(_client, _cellX, _cellY);
            if (city == null) return;
            _client.SendCommands(new CaptureCityNetCommand
            {
                PlayerId = _client.PlayerId, CityId = city.EntityId
            });
            Hide();
        }

        private void OnHeal()
        {
            if (_client == null) return;
            _client.SendCommands(new HealArmyNetCommand
            {
                PlayerId = _client.PlayerId, Amount = 20
            });
            Hide();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static ArmySnapshot? FindArmyAt(GameClient client, int x, int y)
        {
            foreach (var a in client.QuerySnapshots<ArmySnapshot>())
                if (a.X == x && a.Y == y) return a;
            return null;
        }

        private static CitySnapshot? FindCityAt(GameClient client, int x, int y)
        {
            foreach (var c in client.QuerySnapshots<CitySnapshot>())
                if (c.X == x && c.Y == y) return c;
            return null;
        }

        private static void SetBtn(Button? btn, bool active)
        {
            if (btn != null) btn.gameObject.SetActive(active);
        }
    }
}
