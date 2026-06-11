using System;
using System.Collections.Generic;
using UnityEngine;
using Configurations.Model.Types;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Configurations.Unity
{
    [CreateAssetMenu(fileName = "SpriteRegistry", menuName = "Config/Sprite Registry")]
    public sealed class SpriteRegistry : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public string guid;
            public LazyLoadReference<Sprite> spriteRef;
        }

        [SerializeField]
        private List<Entry> _entries = new();

        private readonly Dictionary<string, Sprite> _runtimeCache = new();

        public Sprite GetSprite(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;

            if (_runtimeCache.TryGetValue(guid, out var cachedSprite))
                return cachedSprite;

            var entry = _entries.Find(e => e.guid == guid);
            if (entry.spriteRef.isSet) // Lazy check
            {
                var loadedSprite = entry.spriteRef.asset; // Actual loading happens here
                _runtimeCache[guid] = loadedSprite;
                return loadedSprite;
            }

            return null;
        }

#if UNITY_EDITOR
        public string RegisterSprite(Sprite sprite)
        {
            if (sprite == null) return null;

            string path = AssetDatabase.GetAssetPath(sprite);
            string guid = AssetDatabase.AssetPathToGUID(path);

            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite, out string _, out long fileId))
            {
                string compositeId = $"{guid}:{fileId}";

                int index = _entries.FindIndex(e => e.guid == compositeId);
                if (index == -1)
                {
                    _entries.Add(new Entry { guid = compositeId, spriteRef = new LazyLoadReference<Sprite>(sprite) });
                    EditorUtility.SetDirty(this);
                }
                return compositeId;
            }

            return guid;
        }

        [ContextMenu("Cleanup Dead References")]
        public void CleanupRegistry()
        {
            int removedCount = 0;

            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                var entry = _entries[i];

                string guidOnly = entry.guid.Split(':')[0];
                string path = AssetDatabase.GUIDToAssetPath(guidOnly);

                // If path is empty, the asset file has been deleted.
                // If asset is null, the sprite has been removed from the asset.
                if (string.IsNullOrEmpty(path) || entry.spriteRef.asset == null)
                {
                    _entries.RemoveAt(i);
                    removedCount++;
                }
            }

            if (removedCount > 0)
            {
                Debug.Log($"<color=green>Registry Cleanup:</color> Removed {removedCount} dead references.");
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
            else
            {
                Debug.Log("<color=white>Registry Cleanup:</color> Everything is clean.");
            }
        }
#endif
    }

    public static class SpriteExtensions
    {
        public static Sprite GetSprite(this SpriteId id, SpriteRegistry registry)
        {
            if (!id.IsValid)
                return null;

            return registry != null ? registry.GetSprite(id.id) : null;
        }
    }
}