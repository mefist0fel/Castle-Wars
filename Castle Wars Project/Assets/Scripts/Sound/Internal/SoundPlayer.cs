using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sound.Internal
{
    /// <summary>
    /// Runtime playback engine created lazily by SoundLibrary.
    /// Not intended to be added manually; hidden from the Add Component menu.
    ///
    /// Music channel: two AudioSources (A/B) used for seamless crossfade.
    /// Effects channel: a grow-only pool of AudioSources; free slots are reused.
    ///   - 3D effects: pool source is moved to world position with spatialBlend = 1.
    ///   - Simultaneous-instance limit enforced per entry id.
    /// </summary>
    [AddComponentMenu("")]
    internal sealed class SoundPlayer : MonoBehaviour
    {
        private SoundLibrary _library;

        // ---- Music ----

        private AudioSource _musicA;
        private AudioSource _musicB;
        private bool _activeMusicIsA = true;
        private Coroutine _musicFade;

        private AudioSource ActiveMusic => _activeMusicIsA ? _musicA : _musicB;
        private AudioSource InactiveMusic => _activeMusicIsA ? _musicB : _musicA;

        // ---- Effects ----

        // Effect sources are named after their entry id so we can count active instances
        // and stop them by id without any extra data structures.
        private readonly List<AudioSource> _effectPool = new();

        // ---- Init ----

        public void Initialize(SoundLibrary library)
        {
            _library = library;

            _musicA = CreateAudioSource("Music_A");
            _musicB = CreateAudioSource("Music_B");
            _musicA.loop = true;
            _musicB.loop = true;
        }

        // ---- Public API (called by SoundLibrary) ----

        public void Play(SoundEntry entry, Vector3 position)
        {
            switch (entry.type)
            {
                case SoundType.Music:
                    PlayMusic(entry);
                    break;
                case SoundType.Effect:
                    PlayEffect(entry);
                    break;
                case SoundType.Effect3D:
                    PlayEffect3D(entry, position);
                    break;
            }
        }

        public void StopMusic()
        {
            if (_musicFade != null)
                StopCoroutine(_musicFade);

            _musicFade = StartCoroutine(FadeOut(ActiveMusic, _library.MusicFadeDuration));

            InactiveMusic.Stop();
            InactiveMusic.clip = null;
        }

        public void Stop(SoundEntry entry)
        {
            if (entry.type == SoundType.Music)
            {
                StopMusic();
                return;
            }

            foreach (var source in _effectPool)
            {
                if (source.isPlaying && source.gameObject.name == entry.id)
                    source.Stop();
            }
        }

        // ---- Music playback ----

        private void PlayMusic(SoundEntry entry)
        {
            var clip = entry.clip.asset;
            if (clip == null)
                return;

            if (ActiveMusic.isPlaying && ActiveMusic.clip == clip)
                return;

            if (_musicFade != null)
                StopCoroutine(_musicFade);

            _musicFade = StartCoroutine(CrossfadeMusic(clip, entry.volume));
        }

        private IEnumerator CrossfadeMusic(AudioClip clip, float targetVolume)
        {
            float duration = _library.MusicFadeDuration;
            var outgoing = ActiveMusic;
            var incoming = InactiveMusic;

            incoming.clip = clip;
            incoming.volume = 0f;
            incoming.Play();

            float elapsed = 0f;
            float startVolume = outgoing.volume;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                outgoing.volume = Mathf.Lerp(startVolume, 0f, t);
                incoming.volume = Mathf.Lerp(0f, targetVolume, t);
                yield return null;
            }

            outgoing.Stop();
            outgoing.clip = null;
            outgoing.volume = 0f;

            _activeMusicIsA = !_activeMusicIsA;
            _musicFade = null;
        }

        private IEnumerator FadeOut(AudioSource source, float duration)
        {
            float elapsed = 0f;
            float startVolume = source.volume;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            source.Stop();
            source.clip = null;
            source.volume = 0f;
        }

        // ---- Effect playback ----

        private void PlayEffect(SoundEntry entry)
        {
            if (CountActiveEffects(entry.id) >= _library.MaxSameEffects)
                return;

            var clip = entry.clip.asset;
            if (clip == null)
                return;

            var source = GetFreeEffectSource(entry.id);
            source.transform.localPosition = Vector3.zero;
            source.spatialBlend = 0f;
            source.volume = entry.volume;
            source.clip = clip;
            source.Play();
        }

        private void PlayEffect3D(SoundEntry entry, Vector3 position)
        {
            if (CountActiveEffects(entry.id) >= _library.MaxSameEffects)
                return;

            var clip = entry.clip.asset;
            if (clip == null)
                return;

            var source = GetFreeEffectSource(entry.id);
            source.transform.position = position;
            source.spatialBlend = 1f;
            source.volume = entry.volume;
            source.clip = clip;
            source.Play();
        }

        private int CountActiveEffects(string id)
        {
            int count = 0;
            foreach (var source in _effectPool)
            {
                if (source.isPlaying && source.gameObject.name == id)
                    count++;
            }

            return count;
        }

        private AudioSource GetFreeEffectSource(string entryId)
        {
            foreach (var source in _effectPool)
            {
                if (!source.isPlaying)
                {
                    source.gameObject.name = entryId;
                    return source;
                }
            }

            var newSource = CreateAudioSource(entryId);
            _effectPool.Add(newSource);

            return newSource;
        }

        // ---- Helpers ----

        private AudioSource CreateAudioSource(string sourceName)
        {
            var go = new GameObject(sourceName);
            go.transform.SetParent(transform);

            return go.AddComponent<AudioSource>();
        }
    }
}
