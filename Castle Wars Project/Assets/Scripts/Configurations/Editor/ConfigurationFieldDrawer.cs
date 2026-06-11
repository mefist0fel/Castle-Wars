#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Configurations.Model;
using System.Linq;

namespace Configurations.Editor
{
    // Handles ConfigurationField and all subclasses (including ConfigurationField<T> and named
    // concrete subclasses like MapConfigField). useForChildren: true enables subclass matching.
    // Also handles the [ConfigurationType(typeof(X))] attribute on plain ConfigurationField fields.
    [CustomPropertyDrawer(typeof(ConfigurationField), true)]
    [CustomPropertyDrawer(typeof(ConfigurationTypeAttribute))]
    public sealed class ConfigurationFieldDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect labelRect     = new(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            float contentX     = position.x + EditorGUIUtility.labelWidth;
            float contentWidth = position.width - EditorGUIUtility.labelWidth;

            // 30 % container slot, 70 % config popup.
            float containerW = contentWidth * 0.3f;
            float popupW     = contentWidth * 0.7f - 2f;

            Rect containerRect = new(contentX,              position.y, containerW, position.height);
            Rect popupRect     = new(contentX + containerW + 2f, position.y, popupW,     position.height);

            EditorGUI.HandlePrefixLabel(position, labelRect, label);

            var containerProp = property.FindPropertyRelative("container");
            var idProp        = property.FindPropertyRelative("id");

            if (containerProp.objectReferenceValue == null)
            {
                var found = FindContainerInProject();

                if (found != null)
                {
                    containerProp.objectReferenceValue = found;
                    property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            EditorGUI.PropertyField(containerRect, containerProp, GUIContent.none);

            var container = containerProp.objectReferenceValue as ConfigurationContainer;

            if (container != null && container.Configuration != null)
            {
                System.Type filterType = ResolveFilterType();

                var filtered = container.Configuration
                    .Where(c => filterType.IsAssignableFrom(c.GetType()))
                    .ToList();

                if (filtered.Count > 0)
                {
                    string[] names = filtered.Select(c => $"{c.name} [{c.id}]").ToArray();
                    int[]    ids   = filtered.Select(c => (int)c.id).ToArray();

                    int currentIdx = System.Array.IndexOf(ids, idProp.intValue);
                    int newIdx     = EditorGUI.Popup(popupRect, currentIdx, names);

                    if (newIdx != currentIdx && newIdx >= 0)
                        idProp.intValue = ids[newIdx];
                }
                else
                {
                    EditorGUI.HelpBox(popupRect, $"No {filterType.Name} configs", MessageType.None);
                }
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.TextField(popupRect, "← assign Container");
                EditorGUI.EndDisabledGroup();
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUIUtility.singleLineHeight;

        // ── Filter type resolution ────────────────────────────────────────────
        //
        // Priority order:
        //   1. Explicit [ConfigurationType(typeof(X))] attribute on the field.
        //   2. Generic type argument T of ConfigurationField<T> (or a named subclass of it).
        //   3. Fallback: GameConfiguration — show everything.

        private System.Type ResolveFilterType()
        {
            // 1. Explicit attribute override.
            if (attribute is ConfigurationTypeAttribute typeAttr)
                return typeAttr.TargetType;

            // 2. Walk the field's type up the hierarchy looking for ConfigurationField<T>.
            var ft = fieldInfo?.FieldType;
            while (ft != null && ft != typeof(object))
            {
                if (ft.IsGenericType &&
                    ft.GetGenericTypeDefinition() == typeof(ConfigurationField<>))
                {
                    return ft.GetGenericArguments()[0];
                }
                ft = ft.BaseType;
            }

            // 3. Fallback.
            return typeof(GameConfiguration);
        }

        private static ConfigurationContainer _cachedContainer;
        private static bool _containerSearchDone;

        private static ConfigurationContainer FindContainerInProject()
        {
            if (_containerSearchDone)
                return _cachedContainer;

            _containerSearchDone = true;
            var guids = AssetDatabase.FindAssets($"t:{nameof(ConfigurationContainer)}");

            if (guids.Length == 0)
                return null;

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _cachedContainer = AssetDatabase.LoadAssetAtPath<ConfigurationContainer>(path);
            return _cachedContainer;
        }
    }
}
#endif