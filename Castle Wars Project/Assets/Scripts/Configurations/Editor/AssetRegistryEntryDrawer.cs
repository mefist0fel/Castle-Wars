#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Configurations.Unity;

namespace Configurations.Editor
{
    [CustomPropertyDrawer(typeof(AssetRegistry.Entry))]
    public class AssetRegistryEntryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var idProp = property.FindPropertyRelative(nameof(AssetRegistry.Entry.compositeId));
            var refProp = property.FindPropertyRelative(nameof(AssetRegistry.Entry.reference));

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();

            var newObj = EditorGUI.ObjectField(position, label, refProp.objectReferenceValue, typeof(Object), false);

            if (EditorGUI.EndChangeCheck())
            {
                refProp.objectReferenceValue = newObj;
                if (newObj != null)
                {
                    string path = AssetDatabase.GetAssetPath(newObj);
                    idProp.stringValue = AssetDatabase.AssetPathToGUID(path);
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