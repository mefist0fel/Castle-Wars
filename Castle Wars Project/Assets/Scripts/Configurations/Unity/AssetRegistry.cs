using Configurations.Model;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Configurations.Unity
{
    [CreateAssetMenu(fileName = "AssetRegistry", menuName = "Config/Asset Registry")]
    public sealed class AssetRegistry : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public string compositeId; // guid
            public LazyLoadReference<UnityEngine.Object> reference;
        }

        [SerializeField] private List<Entry> _entries = new();

        public UnityEngine.Object GetAsset(AssetId id)
        {
            var entry = _entries.Find(e => e.compositeId == id.guid);
            return entry.reference.isSet ? entry.reference.asset : null;
        }

        public T GetAsset<T>(AssetId id) where T : UnityEngine.Object
        {
            var entry = _entries.Find(e => e.compositeId == id.guid);
            return entry.reference.isSet ? entry.reference.asset as T : null;
        }

#if UNITY_EDITOR
        public string Register(UnityEngine.Object obj)
        {
            if (obj == null) return null;
            string path = UnityEditor.AssetDatabase.GetAssetPath(obj);
            string guid = UnityEditor.AssetDatabase.AssetPathToGUID(path);

            string id = $"{guid}";
            if (!_entries.Exists(e => e.compositeId == id))
            {
                _entries.Add(new Entry { compositeId = id, reference = new LazyLoadReference<UnityEngine.Object>(obj) });
                UnityEditor.EditorUtility.SetDirty(this);
            }
            return id;
        }

        [ContextMenu("Cleanup Dead References")]
        public void CleanupRegistry()
        {
            int removedCount = 0;

            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                var entry = _entries[i];

                string guidOnly = entry.compositeId.Split(':')[0];
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guidOnly);

                // If path is empty, the asset file has been deleted.
                // If asset is null, the sprite has been removed from the asset.
                if (string.IsNullOrEmpty(path) || entry.reference.asset == null)
                {
                    _entries.RemoveAt(i);
                    removedCount++;
                }
            }

            if (removedCount > 0)
            {
                Debug.Log($"<color=green>Registry Cleanup:</color> Removed {removedCount} dead references.");
                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.AssetDatabase.SaveAssets();
            }
            else
            {
                Debug.Log("<color=white>Registry Cleanup:</color> Everything is clean.");
            }
        }
#endif
    }

    public static class AssetReferenceExtensions
    {
        public static T Get<T>(this AssetId id, AssetRegistry registry) where T : class
        {
            if (id == null || !id.IsValid)
                return null;

            if (registry == null)
                return null;

            UnityEngine.Object asset = registry.GetAsset(id);
            if (asset == null)
                return null;

            if (asset is T result)
                return result;

            if (asset is GameObject go)
            {
                return go.GetComponent<T>();
            }

            return null;
        }
    }
}