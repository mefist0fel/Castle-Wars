using UnityEditor;
using UnityEngine;

namespace Sound.Editor
{
    /// <summary>
    /// Property drawer for <see cref="SoundIdAttribute"/>.
    ///
    /// Renders the string field normally plus a small "▼" button.
    /// Clicking the button opens a dropdown built from all IDs registered in the
    /// first SoundLibrary asset found in the project.
    ///
    /// IDs that use '.' as a separator are displayed with '/' so that Unity's
    /// GenericMenu automatically creates nested submenus (e.g. "Effects/Combat/Sword").
    /// </summary>
    [CustomPropertyDrawer(typeof(SoundIdAttribute))]
    public class SoundIdDrawer : PropertyDrawer
    {
        private static SoundLibrary _cachedLibrary;
        private static double _lastSearchTime = -9999.0;

        // Avoid hammering AssetDatabase every repaint — re-search at most once per interval.
        private const double SearchCooldown = 10.0;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "SoundId requires a string field");
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            const float buttonWidth = 22f;
            const float spacing = 2f;

            var fieldRect = new Rect(position.x, position.y, position.width - buttonWidth - spacing, position.height);
            var buttonRect = new Rect(fieldRect.xMax + spacing, position.y, buttonWidth, position.height);

            property.stringValue = EditorGUI.TextField(fieldRect, label, property.stringValue);

            if (GUI.Button(buttonRect, "▼"))
                ShowDropdown(property);

            EditorGUI.EndProperty();
        }

        // ---- Dropdown ----

        private static void ShowDropdown(SerializedProperty property)
        {
            var library = GetOrFindLibrary();
            var menu = new GenericMenu();

            if (library == null || library.Entries.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("No SoundLibrary found in project"));
                menu.ShowAsContext();
                return;
            }

            foreach (var entry in library.Entries)
            {
                if (string.IsNullOrEmpty(entry.id))
                    continue;

                // Capture locals for the closure.
                var capturedId = entry.id;
                var capturedProperty = property;

                // Replace dots with slashes — GenericMenu treats '/' as submenu separator,
                // so both conventions produce a tree automatically.
                var displayPath = capturedId.Replace('.', '/');
                var isSelected = property.stringValue == capturedId;

                menu.AddItem(new GUIContent(displayPath), isSelected, () =>
                {
                    capturedProperty.stringValue = capturedId;
                    capturedProperty.serializedObject.ApplyModifiedProperties();
                });
            }

            menu.ShowAsContext();
        }

        // ---- Library lookup ----

        private static SoundLibrary GetOrFindLibrary()
        {
            if (_cachedLibrary != null)
                return _cachedLibrary;

            double now = EditorApplication.timeSinceStartup;
            if (now - _lastSearchTime < SearchCooldown)
                return null;

            _lastSearchTime = now;

            var guids = AssetDatabase.FindAssets("t:SoundLibrary");
            if (guids.Length == 0)
                return null;

            if (guids.Length > 1)
            {
                Debug.LogWarning("[SoundIdDrawer] Multiple SoundLibrary assets found — using the first one. " +
                                 "Consider keeping a single library asset in the project.");
            }

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _cachedLibrary = AssetDatabase.LoadAssetAtPath<SoundLibrary>(path);

            return _cachedLibrary;
        }
    }
}
