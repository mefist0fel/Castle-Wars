#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Configurations.Unity;

namespace Configurations.Editor
{
    [CustomPropertyDrawer(typeof(SpriteRegistry.Entry))]
    public class SpriteRegistryEntryDrawer : PropertyDrawer
    {
        private const float EntryHeight = 64f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EntryHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var idProp = property.FindPropertyRelative(nameof(SpriteRegistry.Entry.guid));
            var refProp = property.FindPropertyRelative(nameof(SpriteRegistry.Entry.spriteRef));

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();

            var newSprite = EditorGUI.ObjectField(position, label, refProp.objectReferenceValue, typeof(Sprite), false) as Sprite;

            if (EditorGUI.EndChangeCheck())
            {
                refProp.objectReferenceValue = newSprite;
                if (newSprite != null)
                {
                    if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(newSprite, out string assetGuid, out long fileId))
                    {
                        idProp.stringValue = $"{assetGuid}:{fileId}";
                    }
                    else
                    {
                        string path = AssetDatabase.GetAssetPath(newSprite);
                        idProp.stringValue = AssetDatabase.AssetPathToGUID(path);
                    }
                }
                else
                {
                    idProp.stringValue = string.Empty;
                }
            }

            EditorGUI.EndProperty();
        }
    }
}
#endif