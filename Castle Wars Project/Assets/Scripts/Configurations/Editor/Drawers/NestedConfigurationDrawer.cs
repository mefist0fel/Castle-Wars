#if UNITY_EDITOR
using Configurations.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Configurations.Editor.Drawers
{
    internal sealed class NestedConfigurationDrawer : AbstractDrawer
    {
        public override bool IsTypeSupported(Type type) =>
            type.IsSubclassOf(typeof(GameConfiguration));

        public override void ShowDrawerGUI(object value, Type type, string name, IValueSetter setter, int instanceId)
        {
            GameConfiguration config = (GameConfiguration)value;
            EditorGUILayout.BeginHorizontal();

            var configName = config?.name ?? $"null [{type.Name}]";
            var configId = config?.id.ToString() ?? $"null [{type.Name}]";
            GUI.enabled = false;
            EditorGUILayout.TextField(name, $"{configName} [{configId}]");
            GUI.enabled = true;
            // TODO try
            // int newIndex = EditorGUI.Popup(popupRect, currentIndex, names);
            if (GUILayout.Button("*", GUILayout.Width(20)))
            {
                ShowMyAdvancedPopup(type, setter);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void ShowMyAdvancedPopup(Type configType, IValueSetter setter)
        {
            var availableObjects = provider?.GetAvailableConfigurations(configType) ?? Array.Empty<GameConfiguration>();
            Array.Sort(availableObjects, CompareNames);
            var objectList = new List<GameConfiguration>(availableObjects.Length + 1)
            {
                null
            };
            objectList.AddRange(availableObjects);
            var options = objectList.Select(config => config?.name ?? config?.id.ToString() ?? "null").ToList();
            var cachedSetter = setter.Clone();

            var dropdown = new NestedConfigSelectionDropdown(new AdvancedDropdownState(), options, (selectedId) =>
            {
                if (selectedId >= 0 && selectedId < options.Count)
                {
                    cachedSetter.SetValue(objectList[selectedId]);
                }
            });

            // Show the dropdown at the position of the last UI element (the button)
            Rect rect = GUILayoutUtility.GetLastRect();
            dropdown.Show(rect);
        }

        private static int CompareNames(GameConfiguration a, GameConfiguration b)
        {
            // 1. Check objects null
            if (a == null && b == null) return 0;
            if (a == null) return 1; // null in the end
            if (b == null) return -1;

            // 2. name comparison
            string nameA = a.name ?? string.Empty;
            string nameB = b.name ?? string.Empty;

            return string.Compare(nameA, nameB, System.StringComparison.OrdinalIgnoreCase);
        }

        private static INestedConfigsProvider provider;

        public static void SetConfigsProvider(INestedConfigsProvider newProvider = null)
        {
            provider = newProvider;
        }
    }

    internal interface INestedConfigsProvider
    {
        GameConfiguration[] GetAvailableConfigurations(Type baseType);
    }
}
#endif