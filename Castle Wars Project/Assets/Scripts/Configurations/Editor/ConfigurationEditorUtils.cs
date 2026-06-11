#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Configurations.Model;
using Configurations.Model.Id;
using Configurations.Model.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace Configurations.Editor
{
    // ---- Shared tree model ----

    internal class ConfigTreeNode
    {
        public string name;
        public GameConfiguration config; // null = folder node
        public readonly List<ConfigTreeNode> children = new();
        public bool IsLeaf => config != null;

        public int LeafCount()
        {
            if (IsLeaf)
                return 1;

            int count = 0;
            foreach (var child in children)
                count += child.LeafCount();

            return count;
        }
    }

    // ---- Shared utilities ----

    /// <summary>
    /// Shared helpers for configuration editor tools (window + SO inspector).
    /// </summary>
    internal static class ConfigurationEditorUtils
    {
        /// <summary>
        /// Builds a slash-separated name tree from a flat list of configs.
        /// Folder nodes have <c>config == null</c>; leaf nodes carry the config.
        /// </summary>
        public static ConfigTreeNode BuildConfigTree(List<GameConfiguration> configs)
        {
            var root = new ConfigTreeNode { name = string.Empty };

            foreach (var cfg in configs)
            {
                var parts = (cfg.name ?? string.Empty).Split('/');
                var node = root;

                for (int i = 0; i < parts.Length - 1; i++)
                {
                    var folder = node.children.Find(c => !c.IsLeaf && c.name == parts[i]);
                    if (folder == null)
                    {
                        folder = new ConfigTreeNode { name = parts[i] };
                        node.children.Add(folder);
                    }

                    node = folder;
                }

                node.children.Add(new ConfigTreeNode { name = parts[^1], config = cfg });
            }

            return root;
        }

        private static readonly FieldInfo NameField = typeof(GameConfiguration)
            .GetField(nameof(GameConfiguration.name), BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo IdField = typeof(GameConfiguration)
            .GetField(nameof(GameConfiguration.id), BindingFlags.Public | BindingFlags.Instance);

        /// <summary>
        /// Creates a deep copy of <paramref name="original"/>, assigns a new unique ID and a
        /// "_copy"-suffixed name, adds it to <paramref name="container"/>, and re-links all
        /// nested config references across the container.
        /// </summary>
        public static GameConfiguration Duplicate(GameConfiguration original, ConfigurationContainer container)
        {
            if (original == null || container == null) return null;

            // Full JSON roundtrip — same serializer that produced the file on disk
            string json;
            GameConfiguration copy;
            try
            {
                json = JsonConvert.SerializeObject(original, typeof(GameConfiguration),
                    Formatting.None, ConfigurationContainer.SerializerSettings);
                copy = JsonConvert.DeserializeObject<GameConfiguration>(json,
                    ConfigurationContainer.SerializerSettings);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Duplicate] Serialization failed: {e.Message}");
                return null;
            }

            if (copy == null) return null;

            // Fresh ID
            ConfigurationId newId;
            do {
                newId = ConfigurationId.NewId();
            }
            while (container.Configuration.Configurations.ContainsKey(newId));
            IdField.SetValue(copy, newId);

            // Unique name
            string baseName = (original.name ?? "copy") + "_copy";
            string newName = baseName;
            int suffix = 1;
            while (container.Configuration.Configurations.Any(kv => kv.Value?.name == newName))
                newName = $"{baseName}_{suffix++}";
            NameField.SetValue(copy, newName);

            // Register and re-link nested config stubs
            container.Configuration.Configurations.Add(newId, copy);
            ConfigurationLinker.LinkConfigurationReferences(container.Configuration.Configurations);

            return copy;
        }
    }
}
#endif
