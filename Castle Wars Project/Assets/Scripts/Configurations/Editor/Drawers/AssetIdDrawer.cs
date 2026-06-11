#if UNITY_EDITOR
using Configurations.Model;
using Configurations.Unity;
using System;
using UnityEditor;
using UnityEngine;

namespace Configurations.Editor.Drawers
{
    internal sealed class AssetIdDrawer : AbstractDrawer
    {
        private AssetRegistry _registry;

        private void EnsureRegistry()
        {
            if (_registry != null)
                return;
            var guids = AssetDatabase.FindAssets($"t:{nameof(AssetRegistry)}");
            if (guids.Length > 0)
                _registry = AssetDatabase.LoadAssetAtPath<AssetRegistry>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
        public override bool IsTypeSupported(Type type) =>
            type == typeof(AssetId) || type.IsSubclassOf(typeof(AssetId));

        public override void ShowDrawerGUI(object value, Type type, string name, IValueSetter setter, int _)
        {
            var typedValue = (AssetId)value;

            EnsureRegistry();

            Type interfaceType = GetInterfaceType(type);

            UnityEngine.Object currentObj = null;
            if (_registry != null && typedValue != null && typedValue.IsValid)
            {
                currentObj = _registry.GetAsset(typedValue);
            }

            EditorGUILayout.BeginHorizontal();

            Type filterType = (interfaceType != null && typeof(Component).IsAssignableFrom(interfaceType))
                ? interfaceType
                : typeof(UnityEngine.Object);

            var newObject = EditorGUILayout.ObjectField(new GUIContent(name), currentObj, filterType, false);

            if (newObject != currentObj)
            {
                if (newObject != null && interfaceType != null && !interfaceType.IsAssignableFrom(newObject.GetType()))
                {
                    if (newObject is GameObject go && go.GetComponent(interfaceType) == null)
                    {
                        Debug.LogWarning($"Selected object does not have component implementing {interfaceType.Name}");
                        return;
                    }
                }
                if (newObject == null)
                {
                    var assetId = Activator.CreateInstance(type, null) as AssetId;
                    setter.SetValue(assetId);
                }
                else if (_registry != null)
                {
                    string newGuid = _registry.Register(newObject);
                    var assetId = Activator.CreateInstance(type, newGuid) as AssetId;
                    setter.SetValue(assetId);
                }
            }

            if (currentObj != null && GUILayout.Button("*", GUILayout.Width(20)))
            {
                EditorGUIUtility.PingObject(currentObj);
            }

            EditorGUILayout.EndHorizontal();
        }

        private Type GetInterfaceType(Type fieldType)
        {
            while (fieldType != null && !fieldType.IsGenericType)
                fieldType = fieldType.BaseType;
            return fieldType?.GetGenericArguments()[0];
        }
    }
}
#endif