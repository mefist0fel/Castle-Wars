#if UNITY_EDITOR
using Configurations.Model.Types;
using Configurations.Unity;
using UnityEditor;
using UnityEngine;

namespace Configurations.Editor.Drawers
{
    internal class SpriteIdDrawer : AbstractDrawer<SpriteId>
    {
        private SpriteRegistry _registry;

        private void EnsureRegistry()
        {
            if (_registry != null) return;
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(SpriteRegistry)}");
            if (guids.Length > 0)
            {
                _registry = AssetDatabase.LoadAssetAtPath<SpriteRegistry>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
        }

        public override void ShowDrawerGUI(SpriteId value, string name, IValueSetter setter, int _)
        {
            EnsureRegistry();

            Sprite currentSprite = null;
            if (_registry != null && value.IsValid)
            {
                currentSprite = _registry.GetSprite(value.id);
            }

            EditorGUILayout.BeginHorizontal();

            // Draw visual slot
            var newSprite = (Sprite)EditorGUILayout.ObjectField(new GUIContent(name), currentSprite, typeof(Sprite), false);

            if (newSprite != currentSprite)
            {
                if (newSprite == null)
                {
                    setter.SetValue(new SpriteId(null));
                }
                else if (_registry != null)
                {
                    string newGuid = _registry.RegisterSprite(newSprite);
                    setter.SetValue(new SpriteId(newGuid));
                }
            }

            if (currentSprite != null && GUILayout.Button("*", GUILayout.Width(20)))
            {
                EditorGUIUtility.PingObject(currentSprite);
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif