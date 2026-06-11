using System;
using UnityEngine;

namespace Sound
{
    [Serializable]
    public class SoundEntry
    {
        public string id;
        public SoundType type;
        public LazyLoadReference<AudioClip> clip;
        [Range(0f, 1f)] public float volume = 1f;
    }
}
