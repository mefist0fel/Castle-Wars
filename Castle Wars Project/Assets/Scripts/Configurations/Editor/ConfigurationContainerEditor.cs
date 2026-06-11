#if UNITY_EDITOR
using Configurations;
using Configurations.Editor;
using System.Reflection;
using System;
using UnityEditor;
using UnityEngine;
using Configurations.Model;
using Configurations.Model.Id;
using System.Collections.Generic;
using System.Linq;
using Configurations.Editor.Drawers;

namespace Assets.Sources.Configurations
{
    [CustomEditor(typeof(ConfigurationContainer))]
    public class ConfigurationContainerEditor : UnityEditor.Editor, INestedConfigsProvider
    {
        private ConfigurationContainer configurationTarget;

        private Dictionary<int, bool> configurationsFoldout = new();

        private readonly List<ConfigurationId> itemsToRemove = new();
        private readonly List<Type> itemsToAdd = new();
        private List<(Type type, List<GameConfiguration> configs)> typedConfigs;

        private bool settingsFoldout = false;
        private GUIStyle labelStyle;
        private GUIStyle folderLabelStyle;
        private GUIStyle typeHeaderStyle;
        private GUIStyle panelStyle;
        private string filter = string.Empty;

        private void OnEnable()
        {
            NestedConfigurationDrawer.SetConfigsProvider(this);
            configurationTarget = (ConfigurationContainer)base.target;
            configurationTarget.LoadSources();
            TryLoadConfigs();
        }

        private void OnDisable()
        {
            configurationTarget.SaveSources();
            NestedConfigurationDrawer.SetConfigsProvider(null);
        }

        private void TryLoadConfigs()
        {
            if (typedConfigs != null)
                return;

            var dict = new Dictionary<Type, List<GameConfiguration>>();
            foreach (var item in configurationTarget.Configuration.Configurations.Values)
            {
                if (item == null)
                    continue;

                var type = item.GetType();
                if (!dict.TryGetValue(type, out var list))
                {
                    list = new List<GameConfiguration>();
                    dict.Add(type, list);
                }

                list.Add(item);
            }

            typedConfigs = new List<(Type type, List<GameConfiguration> configs)>(dict.Count);
            foreach (var typeKey in dict.Keys.OrderBy(type => type.Name))
                typedConfigs.Add((typeKey, dict[typeKey].OrderBy(item => item.name).ToList()));
        }

        public override void OnInspectorGUI()
        {
            TryCreateStyles();
            TryLoadConfigs();

            serializedObject.Update();
            settingsFoldout = EditorGUILayout.Foldout(settingsFoldout, "Settings", true);
            if (settingsFoldout)
            {
                base.OnInspectorGUI();

                if (GUILayout.Button("Reload Configs"))
                    configurationTarget.ReloadConfiguration();

                if (GUILayout.Button("Save sources JSON"))
                    configurationTarget.SaveSources();

                if (GUILayout.Button("Rewrite Build-In JSON"))
                    configurationTarget.SaveBuildIn();
            }

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            filter = EditorGUILayout.TextField("Filter:", filter);

            foreach (var (type, configs) in typedConfigs)
            {
                EditorGUILayout.Space();

                // Type section header — uses foldoutHeader style to visually differ from folders
                EditorGUILayout.BeginHorizontal();
                var showType = GetFoldout(type);
                var newShowType = EditorGUILayout.Foldout(
                    showType,
                    $"{type.Name} [{configs.Count}]",
                    true,
                    typeHeaderStyle);
                SetFoldout(type, newShowType);

                if (GUILayout.Button("s", GUILayout.Width(20)))
                    configs.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));

                EditorGUILayout.EndHorizontal();

                if (!newShowType)
                    continue;

                // Build tree from filtered configs
                var filtered = string.IsNullOrEmpty(filter)
                    ? configs
                    : configs.Where(c => c.name != null &&
                                         c.name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

                if (filtered.Count == 0)
                    continue;

                var root = ConfigurationEditorUtils.BuildConfigTree(filtered);
                EditorGUI.indentLevel++;
                foreach (var child in root.children)
                    DrawSOTreeNode(child, type, configs);
                EditorGUI.indentLevel--;
            }

            // Add config button lives outside settings, after all types
            EditorGUILayout.Space();
            if (GUILayout.Button("+ Add new config"))
                ShowAddConfigurationMenu();

            RemoveMarkedItems();
            CreateMarkedItems();
        }

        // ---- Tree rendering for SO editor ----

        private void DrawSOTreeNode(ConfigTreeNode node, Type type, List<GameConfiguration> allConfigs)
        {
            if (node.IsLeaf)
            {
                DrawSOConfigItem(node.config, type, allConfigs);
            }
            else
            {
                // Folder node with grey leaf count
                int leafCount = node.LeafCount();
                string folderKey = $"folder:{type.FullName}/{node.name}";

                EditorGUILayout.BeginHorizontal();
                var folderExpanded = GetFoldout(folderKey);
                var newFolderExpanded = EditorGUILayout.Foldout(
                    folderExpanded,
                    $"{node.name} <color=#888888>[{leafCount}]</color>",
                    true,
                    folderLabelStyle);
                EditorGUILayout.EndHorizontal();

                SetFoldout(folderKey, newFolderExpanded);

                if (newFolderExpanded)
                {
                    EditorGUI.indentLevel++;
                    foreach (var child in node.children)
                        DrawSOTreeNode(child, type, allConfigs);
                    EditorGUI.indentLevel--;
                }
            }
        }

        private void DrawSOConfigItem(GameConfiguration item, Type type, List<GameConfiguration> allConfigs)
        {
            var key = item.id;
            var itemName = item.name ?? string.Empty;

            EditorGUILayout.BeginVertical(panelStyle);
            EditorGUILayout.BeginHorizontal();

            var showComponent = GetFoldout(key);
            var newShowComponent = EditorGUILayout.Foldout(
                showComponent,
                $"{GetConfigDisplayName(item)} <color=grey>[{key}]</color>",
                true,
                labelStyle);
            SetFoldout(key, newShowComponent);

            if (GUILayout.Button("✕", GUILayout.Width(22)))
                itemsToRemove.Add(key);

            EditorGUILayout.EndHorizontal();

            // Right-click on config header → context menu
            var headerRect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.ContextClick
                && headerRect.Contains(Event.current.mousePosition))
            {
                var capturedItem = item;
                var capturedConfigs = allConfigs;
                var menu = new GenericMenu();

                menu.AddItem(new GUIContent("Duplicate"), false, () =>
                {
                    var copy = ConfigurationEditorUtils.Duplicate(capturedItem, configurationTarget);
                    if (copy != null)
                    {
                        capturedConfigs.Add(copy);
                        SetFoldout(copy.id, true);
                        typedConfigs = null;
                    }
                });
                menu.AddSeparator(string.Empty);
                menu.AddItem(new GUIContent($"Copy ID [{capturedItem.id}]"), false, () =>
                    GUIUtility.systemCopyBuffer = capturedItem.id.ToString());
                menu.AddSeparator(string.Empty);
                menu.AddItem(new GUIContent("Delete"), false, () =>
                    itemsToRemove.Add(capturedItem.id));

                menu.ShowAsContext();
                Event.current.Use();
            }

            if (newShowComponent)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUI.indentLevel++;

                var newName = EditorGUILayout.TextField("Name:", itemName);
                ClassDrawer.DrawComplexTypeEditor(item, item.GetType(), null, $"Element {key}");

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();

                if (newName != itemName)
                    nameField.SetValue(item, newName);
            }

            EditorGUILayout.EndVertical();
        }

        // ---- Helpers ----

        private bool FitToFilter(string itemName, string filter)
        {
            if (string.IsNullOrEmpty(filter))
                return true;

            return itemName.Contains(filter);
        }

        private void RemoveMarkedItems()
        {
            if (itemsToRemove.Count == 0)
                return;

            foreach (var key in itemsToRemove)
            {
                if (configurationTarget.Configuration.Configurations.ContainsKey(key))
                    configurationTarget.Configuration.Configurations.Remove(key);

                foreach (var (type, configs) in typedConfigs)
                    configs.RemoveAll(item => item.id == key);
            }

            itemsToRemove.Clear();
        }

        private void CreateMarkedItems()
        {
            if (itemsToAdd.Count == 0)
                return;

            foreach (var type in itemsToAdd)
                AddNewConfigurationOfType(type);

            itemsToAdd.Clear();
        }

        private string GetConfigDisplayName(GameConfiguration item)
        {
            if (item == null)
                return string.Empty;

            var name = item.name ?? string.Empty;
            var slashIdx = name.LastIndexOf('/');

            return slashIdx >= 0 ? name.Substring(slashIdx + 1) : name;
        }

        private bool GetFoldout(object item)
        {
            if (configurationsFoldout.TryGetValue(item.GetHashCode(), out var foldout))
                return foldout;

            return false;
        }

        private void SetFoldout(object item, bool value)
        {
            if (!configurationsFoldout.TryGetValue(item.GetHashCode(), out var foldout))
                foldout = false;

            if (foldout != value)
                configurationsFoldout[item.GetHashCode()] = value;
        }

        private List<Type> configurationTypes;

        private readonly FieldInfo nameField = typeof(GameConfiguration)
            .GetField(nameof(GameConfiguration.name), BindingFlags.Public | BindingFlags.Instance);
        private readonly FieldInfo idField = typeof(GameConfiguration)
            .GetField(nameof(GameConfiguration.id), BindingFlags.Public | BindingFlags.Instance);

        private void ShowAddConfigurationMenu()
        {
            configurationTypes ??= GetSupportedTypes();

            var menu = new GenericMenu();
            foreach (var type in configurationTypes)
            {
                menu.AddItem(new GUIContent(type.Name), false, () =>
                    AddNewConfigurationOfType(type));
            }

            menu.ShowAsContext();
        }

        private void AddNewConfigurationOfType(Type type)
        {
            var newItem = Activator.CreateInstance(type);
            var id = GenerateNewId();
            idField.SetValue(newItem, id);
            nameField.SetValue(newItem, GetDefaultName(type));
            configurationTarget.Configuration.Configurations.Add(id, (GameConfiguration)newItem);
            SetFoldout(id, false);
            SetFoldout(type, true);

            AddToTypedList(type, (GameConfiguration)newItem);
        }

        private void AddToTypedList(Type type, GameConfiguration item)
        {
            var entry = typedConfigs.FirstOrDefault(e => e.type == type);
            var configs = entry.configs;
            if (configs == null)
            {
                configs = new List<GameConfiguration>();
                typedConfigs.Add((type, configs));
                typedConfigs.Sort((a, b) => string.Compare(a.type.Name, b.type.Name, StringComparison.Ordinal));
            }

            configs.Add(item);
        }

        private string GetDefaultName(Type type)
        {
            string name = type.Name;
            if (!IsNameExist(name))
                return name;

            int i = 1;
            do
            {
                name = type.Name + "_" + i;
                i += 1;
            }
            while (IsNameExist(name));

            return name;
        }

        private bool IsNameExist(string name) =>
            configurationTarget.Configuration.Configurations.Any(item => item.Value.name == name);

        private ConfigurationId GenerateNewId()
        {
            ConfigurationId id;
            do
                id = ConfigurationId.NewId();
            while (configurationTarget.Configuration.Configurations.ContainsKey(id));

            return id;
        }

        private static readonly string[] ExcludedAssemblies =
            { "UnityEngine", "UnityEditor", "System", "mscorlib", "Mono", "netstandard" };

        private List<Type> GetSupportedTypes()
        {
            var types = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()
                     .Where(a => !ExcludedAssemblies.Any(excluded => a.FullName.StartsWith(excluded))))
            {
                try
                {
                    types.AddRange(assembly.GetTypes().Where(type => IsSubclass(type, typeof(GameConfiguration))));
                }
                catch { }
            }

            return types;
        }

        private bool IsSubclass(Type type, Type baseType) =>
            type.IsClass && !type.IsAbstract && type.IsSubclassOf(baseType);

        private void TryCreateStyles()
        {
            panelStyle ??= CreatePanelStyle();

            labelStyle ??= new GUIStyle(EditorStyles.foldout)
            {
                richText = true,
                normal = { textColor = Color.white }
            };

            folderLabelStyle ??= new GUIStyle(EditorStyles.foldout)
            {
                richText = true
            };

            typeHeaderStyle ??= new GUIStyle(EditorStyles.foldoutHeader)
            {
                richText = true
            };
        }

        private static GUIStyle CreatePanelStyle()
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(15, 5, 3, 3),
                margin = new RectOffset(5, 5, 0, 0),
                border = new RectOffset(2, 2, 2, 2)
            };
            style.normal.background = MakeTex(2, 2, new Color(0.2f, 0.3f, 0.4f, 1f));

            return style;
        }

        private static Texture2D MakeTex(int width, int height, Color32 color)
        {
            var pixels = new Color32[width * height];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;

            var result = new Texture2D(width, height);
            result.SetPixels32(pixels);
            result.Apply();

            return result;
        }

        public GameConfiguration[] GetAvailableConfigurations(Type baseType) =>
            typedConfigs
                .Where(c => baseType == c.type || baseType.IsSubclassOf(c.type))
                .SelectMany(c => c.configs)
                .ToArray();
    }
}
#endif