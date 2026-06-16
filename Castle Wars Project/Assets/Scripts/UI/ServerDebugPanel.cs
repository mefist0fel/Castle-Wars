#nullable enable
using CastleWars.Shared.Infrastructure;
using TMPro;
using UnityEngine;

namespace CastleWars.UI
{
    // Attach to a Canvas UI element. Assign a TextMeshProUGUI and a reference to GameBootstrap.
    // Displays live server stats in the top-left (or wherever the text element is placed).
    //
    // Prefab setup:
    //   Canvas → Panel → TextMeshProUGUI "ServerDebugText"
    //   Attach ServerDebugPanel.cs to the Panel and assign the text field in Inspector.
    //   The GameServer reference is set via SetServer() from GameBootstrap.
    public class ServerDebugPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI? text;
        [SerializeField] private float            refreshRate = 0.5f;

        private GameServer? _server;
        private float _timer;

        public void SetServer(GameServer server) => _server = server;

        private void Update()
        {
            if (_server == null || text == null) return;
            _timer += Time.deltaTime;
            if (_timer < refreshRate) return;
            _timer = 0f;
            text.text = _server.GetDebugInfo();
        }
    }
}
