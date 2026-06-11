#if UNITY_EDITOR
using Configurations.Model;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Configurations.Editor.Drawers
{
    internal sealed class ClassDrawer : AbstractDrawer
    {
        private static readonly AbstractDrawer[] drawers = BuildDrawers();

        public override bool IsTypeSupported(Type type) =>
            type.IsClass || (type.IsValueType && !type.IsPrimitive && !type.IsEnum);

        public override void ShowDrawerGUI(object value, Type type, string name, IValueSetter setter, int id)
        {
            if (value == null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField($"{name} [{type.Name}]", "null");
                EditorGUI.EndDisabledGroup();
                if (GUILayout.Button("*", GUILayout.Width(20)))
                {
                    setter.SetValue(Activator.CreateInstance(type));
                    Unfold(id);
                }
                EditorGUILayout.EndHorizontal();
                return;
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            var unfolded = DrawUnFolded(id, name);

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                setter.SetValue(null);
            }
            EditorGUILayout.EndHorizontal();

            if (unfolded)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUI.indentLevel++;
                DrawComplexTypeEditor(value, type, setter, name);
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }

        private static readonly ClassFieldSetter cachedSetter = new();
        private static readonly StructFieldSetter cachedStructSetter = new();

        public static void DrawComplexTypeEditor(object obj, Type objType, IValueSetter setter, string name)
        {
            FieldInfo[] fields = objType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var contextMenuMethods = CollectMethods<ConfigContextMenuAttribute>(
                objType, (attr, _) => attr.Label);

            foreach (var field in fields)
            {
                if (IsExcluded(field, objType))
                    continue;

                var header = field.GetCustomAttribute<ConfigHeaderAttribute>();
                if (header != null)
                {
                    EditorGUILayout.LabelField(header.Label, EditorStyles.boldLabel);
                }

                IValueSetter fieldSetter;
                if (objType.IsValueType)
                {
                    cachedStructSetter.Set(obj, field, setter);
                    fieldSetter = cachedStructSetter;
                }
                else
                {
                    cachedSetter.Set(obj, field);
                    fieldSetter = cachedSetter;
                }

                var fieldValue = field.GetValue(obj);
                var hash = obj.GetHashCode() + field.GetHashCode();

                // Wrap in a vertical group to capture the full rect for context-menu detection
                EditorGUILayout.BeginVertical();
                bool drawn = TryDraw(fieldValue, field.FieldType, field.Name, fieldSetter, hash);
                if (!drawn)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.TextField($"{field.Name} [{field.FieldType.Name}]", "Not supported");
                    EditorGUI.EndDisabledGroup();
                }
                EditorGUILayout.EndVertical();

                // Right-click context menu: Copy / Paste + [ConfigContextMenuItem] methods
                var fieldRect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.ContextClick
                    && fieldRect.Contains(Event.current.mousePosition))
                {
                    // Clone setter NOW, before the lambda executes asynchronously
                    var capturedSetter = fieldSetter.Clone();
                    var capturedValue = fieldValue;
                    var capturedType = field.FieldType;

                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Copy value"), false, () =>
                        ConfigurationClipboard.Copy(capturedValue, capturedType));

                    if (ConfigurationClipboard.CanPaste(capturedType))
                        menu.AddItem(new GUIContent($"Paste value ({ConfigurationClipboard.StoredTypeName})"), false, () =>
                        {
                            if (ConfigurationClipboard.TryPaste(capturedType, out var pasted))
                                capturedSetter.SetValue(pasted);
                        });
                    else
                        menu.AddDisabledItem(new GUIContent("Paste value (incompatible)"));

                    if (contextMenuMethods.Length > 0)
                    {
                        menu.AddSeparator("");
                        var capturedObj = obj;
                        foreach (var (method, label) in contextMenuMethods)
                        {
                            var capturedMethod = method;
                            menu.AddItem(new GUIContent(label), false, () => capturedMethod.Invoke(capturedObj, null));
                        }
                    }

                    menu.ShowAsContext();
                    Event.current.Use();
                }
            }

            DrawButtonMethods(obj, objType);
        }

        private static void DrawButtonMethods(object obj, Type objType)
        {
            foreach (var (method, label) in CollectMethods<ConfigButtonAttribute>(
                objType, (attr, m) => attr.Label ?? ObjectNames.NicifyVariableName(m.Name)))
            {
                if (GUILayout.Button(label))
                {
                    method.Invoke(obj, null);
                }
            }
        }

        private static (MethodInfo method, string label)[] CollectMethods<TAttr>(
            Type type, Func<TAttr, MethodInfo, string> getLabel) where TAttr : Attribute
        {
            var result = new List<(MethodInfo, string)>();
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (method.GetParameters().Length > 0 || method.ReturnType != typeof(void))
                    continue;

                var attr = method.GetCustomAttribute<TAttr>();
                if (attr == null)
                    continue;

                result.Add((method, getLabel(attr, method)));
            }

            return result.ToArray();
        }

        private static bool IsExcluded(FieldInfo field, Type objType) =>
            objType.IsSubclassOf(typeof(GameConfiguration)) &&
            //field.Name == nameof(BaseConfiguration.id) ||
            field.Name == nameof(GameConfiguration.name);


        private readonly Dictionary<int, bool> internalFoldouts = new();
        private bool DrawUnFolded(int key, string name)
        {
            if (!internalFoldouts.TryGetValue(key, out var foldout))
            {
                foldout = false;
            }
            var newFoldout = EditorGUILayout.Foldout(foldout, name, true);
            if (newFoldout != foldout)
            {
                internalFoldouts[key] = newFoldout;
            }
            return newFoldout;
        }

        public void Unfold(int id)
        {
            internalFoldouts[id] = true;
        }

public static bool TryDraw(object obj, Type type, string name, IValueSetter setter, int id)
        {
            foreach (var drawer in drawers)
            {
                if (drawer.IsTypeSupported(type))
                {
                    drawer.ShowDrawerGUI(obj, type, name, setter, id);
                    return true;
                }
            }
            return false;
        }

        #region Search drawers via reflection
        private static AbstractDrawer[] BuildDrawers()
        {
            var baseType = typeof(AbstractDrawer);
            var classDrawerType = typeof(ClassDrawer);
            var attrType = typeof(DrawerOrderAttribute);
            var discovered = new List<(int order, AbstractDrawer drawer)>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch
                {
                    continue;
                }

                foreach (var type in types)
                {
                    if (type.IsAbstract || type == classDrawerType || !baseType.IsAssignableFrom(type))
                        continue;

                    var orderAttr = (DrawerOrderAttribute)type.GetCustomAttribute(attrType);
                    int order = orderAttr?.Order ?? 500;

                    try
                    {
                        discovered.Add((order, (AbstractDrawer)Activator.CreateInstance(type)));
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[DrawerRegistry] Could not instantiate {type.FullName}: {ex.Message}");
                    }
                }
            }

            discovered.Sort((a, b) => a.order.CompareTo(b.order));

            var result = new AbstractDrawer[discovered.Count + 1];
            for (int i = 0; i < discovered.Count; i++)
            {
                result[i] = discovered[i].drawer;
            }
            result[discovered.Count] = new ClassDrawer();

            return result;
        }
        #endregion

    }
}
#endif