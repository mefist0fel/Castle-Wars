using System.Collections.Generic;
using Sound.Internal;
using UnityEngine;

namespace Sound
{
    /// <summary>
    /// ScriptableObject that acts as both the sound library configuration and the
    /// public API for playing sounds.  Any client can hold a reference to this asset
    /// and call Play / Stop without knowing anything about the runtime player.
    ///
    /// The underlying SoundPlayer MonoBehaviour is created lazily on first use and
    /// lives on a DontDestroyOnLoad GameObject so it survives scene transitions.
    /// </summary>
    [CreateAssetMenu(fileName = "SoundLibrary", menuName = "Sound/Sound Library")]
    public class SoundLibrary : ScriptableObject
    {
        [SerializeField] private List<SoundEntry> _entries = new();
        [SerializeField] private float _musicFadeDuration = 1f;
        [SerializeField] private int _maxSameEffects = 5;

        // ---- Public read-only access for drawers / editor tooling ----

        public IReadOnlyList<SoundEntry> Entries => _entries;
        public float MusicFadeDuration => _musicFadeDuration;
        public int MaxSameEffects => _maxSameEffects;

        // ---- Runtime state (not serialized) ----

        private SoundPlayer _player;

        // ---- Public API ----

        /// <summary>Plays the sound with the given id.  For Effect3D, pass world position.</summary>
        public void Play(string id, Vector3 position = default)
        {
            var entry = FindEntry(id);
            if (entry == null)
            {
                Debug.LogWarning($"[SoundLibrary] Sound id not found: '{id}'");
                return;
            }

            EnsurePlayer().Play(entry, position);
        }

        /// <summary>Fades out the currently playing music track.</summary>
        public void StopMusic()
        {
            _player?.StopMusic();
        }

        /// <summary>Stops all playing instances of the sound with the given id.</summary>
        public void Stop(string id)
        {
            var entry = FindEntry(id);
            if (entry == null)
                return;

            _player?.Stop(entry);
        }

        // ---- Helpers ----

        private SoundEntry FindEntry(string id)
        {
            foreach (var entry in _entries)
            {
                if (entry.id == id)
                    return entry;
            }

            return null;
        }

        private SoundPlayer EnsurePlayer()
        {
            if (_player != null)
                return _player;

            // Reuse an existing player for this library (e.g. after domain reload in editor).
            var goName = $"[SoundPlayer:{name}]";
            var go = GameObject.Find(goName);
            if (go != null)
            {
                _player = go.GetComponent<SoundPlayer>();
                if (_player != null)
                    return _player;
            }

            go = new GameObject(goName);
            DontDestroyOnLoad(go);
            _player = go.AddComponent<SoundPlayer>();
            _player.Initialize(this);

            return _player;
        }
    }
}
