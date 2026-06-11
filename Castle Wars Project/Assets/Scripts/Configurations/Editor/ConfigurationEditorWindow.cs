#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Configurations;
using Configurations.Editor;
using Configurations.Editor.Drawers;
using Configurations.Model;
using Configurations.Model.Id;
using UnityEditor;
using UnityEngine;

namespace Assets.Sources.Configurations.Editor
{
    public sealed class ConfigurationEditorWindow : EditorWindow, INestedConfigsProvider
    {
        // ---- State ----

        private ConfigurationContainer configContainer;

        // Left panel
        private Vector2 leftScroll;
        private string filter = string.Empty;
        private readonly Dictionary<string, bool> treeExpanded = new();

        // Right panel
        private Vector2 rightScroll;
        private GameConfiguration selectedConfig;

        // Data
        private List<(Type type, List<GameConfiguration> configs)> typedConfigs;
        private readonly List<ConfigurationId> toRemove = new();

        // Save
        private bool isDirty;
        private double lastAutoSaveTime;
        private const double AutoSaveInterval = 60.0;

        // Styles (created lazily inside OnGUI)
        private GUIStyle selectedItemStyle;
        private GUIStyle normalItemStyle;
        private GUIStyle greyMiniStyle;
        private GUIStyle richFoldoutStyle;

        // Reflection helpers
        private static readonly FieldInfo NameField = typeof(GameConfiguration)
            .GetField(nameof(GameConfiguration.name), BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo IdField = typeof(GameConfiguration)
            .GetField(nameof(GameConfiguration.id), BindingFlags.Public | BindingFlags.Instance);

        private List<Type> configurationTypes;

        // ---- Open ----

        [MenuItem("Tools/Configuration Editor")]
        public static void OpenWindow()
        {
            var window = GetWindow<ConfigurationEditorWindow>("Configuration Editor");
            window.minSize = new Vector2(640, 400);
        }

        // ---- Lifecycle ----

        private void OnEnable()
        {
            NestedConfigurationDrawer.SetConfigsProvider(this);
            EditorApplication.focusChanged += OnAppFocusChanged;
            EditorApplication.update += OnEditorUpdate;
            lastAutoSaveTime = EditorApplication.timeSinceStartup;
            FindAndLoadContainer();
        }

        private void OnDisable()
        {
            NestedConfigurationDrawer.SetConfigsProvider(null);
            EditorApplication.focusChanged -= OnAppFocusChanged;
            EditorApplication.update -= OnEditorUpdate;
            TrySave();
        }

        private void OnLostFocus()
        {
            TrySave();
        }

        private void OnAppFocusChanged(bool isFocused)
        {
            if (!isFocused)
                TrySave();
        }

        private void OnEditorUpdate()
        {
            if (isDirty && EditorApplication.timeSinceStartup - lastAutoSaveTime > AutoSaveInterval)
                TrySave();
        }

        // ---- Save ----

        private void TrySave()
        {
            if (configContainer == null || !isDirty)
                return;

            configContainer.SaveSources();
            EditorUtility.SetDirty(configContainer);
            isDirty = false;
            lastAutoSaveTime = EditorApplication.timeSinceStartup;
        }

        private void ForceSave()
        {
            if (configContainer == null)
                return;

            configContainer.SaveSources();
            EditorUtility.SetDirty(configContainer);
            isDirty = false;
            lastAutoSaveTime = EditorApplication.timeSinceStartup;
        }

        private void MarkDirty()
        {
            isDirty = true;
        }

        // ---- Container discovery ----

        private void FindAndLoadContainer()
        {
            var guids = AssetDatabase.FindAssets("t:ConfigurationContainer");
            if (guids.Length == 0)
                return;

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            configContainer = AssetDatabase.LoadAssetAtPath<ConfigurationContainer>(path);
            configContainer.LoadSources();
            RebuildTypedConfigs();
        }

        private void RebuildTypedConfigs()
        {
            if (configContainer == null)
            {
                typedConfigs = null;
                return;
            }

            var dict = new Dictionary<Type, List<GameConfiguration>>();
            foreach (var item in configContainer.Configuration.Configurations.Values)
            {
                if (item == null)
                    continue;

                var t = item.GetType();
                if (!dict.TryGetValue(t, out var list))
                {
                    list = new List<GameConfiguration>();
                    dict[t] = list;
                }

                list.Add(item);
            }

            typedConfigs = dict.Keys
                .OrderBy(t => t.Name)
                .Select(t => (t, dict[t].OrderBy(c => c.name ?? string.Empty).ToList()))
                .ToList();
        }

        // ===================== GUI =====================

        private void OnGUI()
        {
            EnsureStyles();

            if (configContainer == null)
            {
                EditorGUILayout.HelpBox("No ConfigurationContainer asset found in the project.", MessageType.Warning);
                if (GUILayout.Button("Search Again"))
                    FindAndLoadContainer();

                return;
            }

            DrawToolbar();

            EditorGUILayout.BeginHorizontal();
            DrawLeftPanel();

            // Vertical divider
            var dividerRect = EditorGUILayout.GetControlRect(false, GUILayout.Width(1), GUILayout.ExpandHeight(true));
            EditorGUI.DrawRect(dividerRect, new Color(0.1f, 0.1f, 0.1f, 1f));

            DrawRightPanel();
            EditorGUILayout.EndHorizontal();

            ProcessRemovals();
        }

        // ---- Toolbar ----

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(55)))
                ForceSave();

            if (GUILayout.Button("Reload", EditorStyles.toolbarButton, GUILayout.Width(55)))
            {
                TrySave();
                configContainer.ReloadConfiguration();
                RebuildTypedConfigs();
                selectedConfig = null;
            }

            if (GUILayout.Button("Build JSON", EditorStyles.toolbarButton, GUILayout.Width(75)))
                configContainer.SaveBuildIn();

            GUILayout.FlexibleSpace();

            if (isDirty)
            {
                var dirtyStyle = new GUIStyle(EditorStyles.miniLabel)
                    { normal = { textColor = new Color(1f, 0.65f, 0.15f) } };
                GUILayout.Label("● unsaved", dirtyStyle);
                GUILayout.Space(6);
            }

            EditorGUILayout.EndHorizontal();
        }

        // ---- Left panel ----

        private const float LeftPanelWidth = 260f;

        private void DrawLeftPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(LeftPanelWidth), GUILayout.ExpandHeight(true));

            // Search bar
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            filter = EditorGUILayout.TextField(filter, EditorStyles.toolbarSearchField);
            if (GUILayout.Button("✕", EditorStyles.toolbarButton, GUILayout.Width(18)))
                filter = string.Empty;
            EditorGUILayout.EndHorizontal();

            leftScroll = EditorGUILayout.BeginScrollView(leftScroll, GUILayout.ExpandHeight(true));

            if (typedConfigs != null)
                foreach (var (type, configs) in typedConfigs)
                    DrawTypeSection(type, configs);

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("+ Add Config"))
                ShowAddConfigMenu();

            EditorGUILayout.EndVertical();
        }

        private void DrawTypeSection(Type type, List<GameConfiguration> configs)
        {
            var filtered = string.IsNullOrEmpty(filter)
                ? configs
                : configs.Where(c => c.name != null &&
                                     c.name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            if (filtered.Count == 0)
                return;

            string sectionKey = "T:" + type.FullName;
            if (!treeExpanded.TryGetValue(sectionKey, out bool expanded))
                expanded = false;

            // Draw type header with slightly darker foldoutHeader style
            EditorGUILayout.BeginHorizontal();
            bool newExpanded = EditorGUILayout.Foldout(
                expanded,
                $"{type.Name} [{filtered.Count}]",
                true,
                EditorStyles.foldoutHeader);
            if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(20)))
                AddNewConfigOfType(type);
            EditorGUILayout.EndHorizontal();

            if (newExpanded != expanded)
                treeExpanded[sectionKey] = newExpanded;

            if (!newExpanded)
                return;

            var root = ConfigurationEditorUtils.BuildConfigTree(filtered);
            EditorGUI.indentLevel++;
            foreach (var child in root.children)
                DrawTreeNode(child, sectionKey + "/");
            EditorGUI.indentLevel--;
        }

        private void DrawTreeNode(ConfigTreeNode node, string pathPrefix)
        {
            string fullPath = pathPrefix + node.name;

            if (node.IsLeaf)
            {
                bool isSelected = node.config == selectedConfig;

                var rowRect = GUILayoutUtility.GetRect(
                    new GUIContent(node.name),
                    normalItemStyle,
                    GUILayout.ExpandWidth(true),
                    GUILayout.Height(EditorGUIUtility.singleLineHeight + 2));

                var indented = rowRect;

                // Selection / hover highlight
                if (isSelected)
                    EditorGUI.DrawRect(indented, new Color(0.24f, 0.49f, 0.91f, 0.85f));
                else if (Event.current.type == EventType.Repaint && indented.Contains(Event.current.mousePosition))
                    EditorGUI.DrawRect(indented, new Color(1f, 1f, 1f, 0.05f));

                // Layout: [name [id]]  [✕]
                var btnRect   = new Rect(indented.xMax - 22, indented.y + 1, 20, indented.height - 2);
                var labelRect = new Rect(indented.x + 2, indented.y, indented.width - 26, indented.height);

                EditorGUI.LabelField(
                    labelRect,
                    $"{node.name} <color=#888888>[{node.config.id}]</color>",
                    isSelected ? selectedItemStyle : normalItemStyle);

                if (GUI.Button(btnRect, "✕", EditorStyles.miniButton))
                {
                    toRemove.Add(node.config.id);
                    if (selectedConfig == node.config)
                        selectedConfig = null;
                }
                else if (Event.current.type == EventType.ContextClick && indented.Contains(Event.current.mousePosition))
                {
                    ShowConfigContextMenu(node.config);
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.MouseDown && indented.Contains(Event.current.mousePosition))
                {
                    selectedConfig = node.config;
                    rightScroll = Vector2.zero;
                    GUI.changed = true;
                    Event.current.Use();
                    Repaint();
                }
            }
            else
            {
                // Folder node
                if (!treeExpanded.TryGetValue(fullPath, out bool expanded))
                    expanded = false;

                int leafCount = node.LeafCount();
                bool newExpanded = EditorGUILayout.Foldout(
                    expanded,
                    $"{node.name} <color=#888888>[{leafCount}]</color>",
                    true,
                    richFoldoutStyle);

                if (newExpanded != expanded)
                    treeExpanded[fullPath] = newExpanded;

                if (newExpanded)
                {
                    EditorGUI.indentLevel++;
                    foreach (var child in node.children)
                        DrawTreeNode(child, fullPath + "/");
                    EditorGUI.indentLevel--;
                }
            }
        }

        // ---- Right panel ----

        private void DrawRightPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            if (selectedConfig == null)
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("← Select a config from the tree", EditorStyles.centeredGreyMiniLabel);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
                return;
            }

            rightScroll = EditorGUILayout.BeginScrollView(rightScroll);

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField(selectedConfig.GetType().Name, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"ID: {selectedConfig.id}", EditorStyles.miniLabel);
            EditorGUILayout.Space(6);

            string oldName = selectedConfig.name ?? string.Empty;
            string newName = EditorGUILayout.TextField("Name:", oldName);
            if (newName != oldName)
            {
                NameField.SetValue(selectedConfig, newName);
                MarkDirty();
            }

            EditorGUILayout.Space(6);

            ClassDrawer.DrawComplexTypeEditor(selectedConfig, selectedConfig.GetType(), null, string.Empty);

            if (EditorGUI.EndChangeCheck())
                MarkDirty();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        // ---- Deferred removals ----

        private void ProcessRemovals()
        {
            if (toRemove.Count == 0)
                return;

            foreach (var id in toRemove)
            {
                configContainer.Configuration.Configurations.Remove(id);
                typedConfigs?.ForEach(entry => entry.configs.RemoveAll(c => c.id == id));
            }

            toRemove.Clear();
            MarkDirty();
        }

        // ---- Config creation ----

        private void ShowAddConfigMenu()
        {
            configurationTypes ??= GetSupportedTypes();
            var menu = new GenericMenu();
            foreach (var type in configurationTypes)
            {
                var t = type;
                menu.AddItem(new GUIContent(type.Name), false, () => AddNewConfigOfType(t));
            }
            menu.ShowAsContext();
        }

        private void AddNewConfigOfType(Type type)
        {
            var newCfg = (GameConfiguration)Activator.CreateInstance(type);
            var id = GenerateNewId();
            IdField.SetValue(newCfg, id);
            NameField.SetValue(newCfg, GenerateUniqueName(type));
            configContainer.Configuration.Configurations.Add(id, newCfg);

            var idx = typedConfigs?.FindIndex(e => e.type == type) ?? -1;
            if (idx >= 0)
                typedConfigs[idx].configs.Add(newCfg);
            else
                RebuildTypedConfigs();

            selectedConfig = newCfg;
            MarkDirty();
        }

        private string GenerateUniqueName(Type type)
        {
            string baseName = type.Name;
            if (!IsNameExists(baseName))
                return baseName;

            int i = 1;
            string name;
            do
                name = $"{baseName}_{i++}";
            while (IsNameExists(name));

            return name;
        }

        private bool IsNameExists(string name) =>
            configContainer.Configuration.Configurations.Any(kv => kv.Value?.name == name);

        private ConfigurationId GenerateNewId()
        {
            ConfigurationId id;
            do
                id = ConfigurationId.NewId();
            while (configContainer.Configuration.Configurations.ContainsKey(id));

            return id;
        }

        // ---- INestedConfigsProvider ----

        public GameConfiguration[] GetAvailableConfigurations(Type baseType) =>
            typedConfigs?
                .Where(c => baseType.IsAssignableFrom(c.type))
                .SelectMany(c => c.configs)
                .ToArray() ?? Array.Empty<GameConfiguration>();

        // ---- Context menu ----

        private void ShowConfigContextMenu(GameConfiguration config)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Duplicate"), false, () =>
            {
                var copy = ConfigurationEditorUtils.Duplicate(config, configContainer);
                if (copy != null)
                {
                    var idx = typedConfigs?.FindIndex(e => e.type == copy.GetType()) ?? -1;
                    if (idx >= 0)
                        typedConfigs[idx].configs.Add(copy);
                    else
                        RebuildTypedConfigs();

                    selectedConfig = copy;
                    MarkDirty();
                }
            });
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent($"Copy ID [{config.id}]"), false, () =>
                GUIUtility.systemCopyBuffer = config.id.ToString());
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Delete"), false, () =>
            {
                toRemove.Add(config.id);
                if (selectedConfig == config)
                    selectedConfig = null;
            });
            menu.ShowAsContext();
        }

        // ---- Utility ----

        private static readonly string[] ExcludedAssemblies =
            { "UnityEngine", "UnityEditor", "System", "mscorlib", "Mono", "netstandard" };

        private List<Type> GetSupportedTypes() =>
            AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !ExcludedAssemblies.Any(e => a.FullName.StartsWith(e)))
                .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(GameConfiguration)))
                .ToList();

        // ---- Styles ----

        private void EnsureStyles()
        {
            // Each style is initialised independently — if one fails, others still get created.
            // Using ??= also handles domain-reload scenarios where fields are individually reset.
            selectedItemStyle ??= new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold,
                richText = true,
                normal = { textColor = Color.white },
                padding = new RectOffset(2, 2, 1, 1)
            };

            normalItemStyle ??= new GUIStyle(EditorStyles.label)
            {
                richText = true,
                padding = new RectOffset(2, 2, 1, 1)
            };

            greyMiniStyle ??= new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = new Color(0.5f, 0.5f, 0.5f) },
                alignment = TextAnchor.MiddleLeft
            };

            richFoldoutStyle ??= new GUIStyle(EditorStyles.foldout)
            {
                richText = true
            };
        }
    }
}
#endif