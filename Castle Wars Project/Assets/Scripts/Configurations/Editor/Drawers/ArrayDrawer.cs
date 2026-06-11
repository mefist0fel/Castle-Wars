#if UNITY_EDITOR
using Configurations.Model;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Configurations.Editor.Drawers
{
    internal sealed class ArrayDrawer : AbstractDrawer
    {
        public override bool IsTypeSupported(Type type) => type.IsArray;

        // ── Per-instance state ────────────────────────────────────────────────────

        private readonly List<ArraySetter>     _setters     = new();
        private readonly Dictionary<int, bool> _foldouts    = new();
        private readonly List<Rect>            _headerRects = new();

        // Drag-and-drop
        private int     _dragIndex  = -1;
        private bool    _isDragging;
        private Vector2 _dragOrigin;

        private const float DragThreshold = 6f;

        // ── Entry point ───────────────────────────────────────────────────────────

        public override void ShowDrawerGUI(object value, Type type, string name, IValueSetter setter, int id)
        {
            var elementType = type.GetElementType();
            var array       = (Array)value;

            if (array == null)
            {
                DrawNullHeader(elementType, type, name, setter, id);
                return;
            }

            int count = array.Length;

            // ── Array header ──────────────────────────────────────────────────────
            EditorGUILayout.BeginHorizontal();
            bool unfolded = DrawFoldout(id, $"{name} [{count}]");
            int  newCount = EditorGUILayout.IntField(count, GUILayout.Width(60));
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                setter.SetValue(null);
                return;
            }
            EditorGUILayout.EndHorizontal();

            if (newCount < 0)
                newCount = 0;

            if (!unfolded)
            {
                TryApplyCountChange(array, elementType, newCount, count, setter);
                return;
            }

            // ── Per-element rows ──────────────────────────────────────────────────
            while (_headerRects.Count < count)
                _headerRects.Add(Rect.zero);

            EditorGUILayout.BeginVertical("box");
            EditorGUI.indentLevel++;

            for (int i = 0; i < count; i++)
            {
                while (_setters.Count <= i)
                    _setters.Add(new ArraySetter());
                _setters[i].Set(array, i);
                var obj = array.GetValue(i);

                bool doMoveUp   = false;
                bool doMoveDown = false;
                bool doDelete   = false;

                // ── Control strip ─────────────────────────────────────────────────
                EditorGUILayout.BeginHorizontal();

                var handleRect = GUILayoutUtility.GetRect(
                    14, EditorGUIUtility.singleLineHeight,
                    GUILayout.Width(14), GUILayout.ExpandWidth(false));
                DrawDragHandle(handleRect, i);

                using (new EditorGUI.DisabledGroupScope(_isDragging))
                {
                    using (new EditorGUI.DisabledGroupScope(i == 0))
                        if (GUILayout.Button("↑", GUILayout.Width(22)))
                            doMoveUp = true;
                    using (new EditorGUI.DisabledGroupScope(i == count - 1))
                        if (GUILayout.Button("↓", GUILayout.Width(22)))
                            doMoveDown = true;
                    if (GUILayout.Button("-", GUILayout.Width(22)))
                        doDelete = true;
                }

                if (Event.current.type == EventType.Repaint)
                    _headerRects[i] = GUILayoutUtility.GetLastRect();

                HandleContextMenu(array, elementType, setter, i, count);

                // ── Element content (foldout or inline — handled by TryDraw) ──────
                ClassDrawer.TryDraw(obj, elementType, TryGetName(obj, i), _setters[i], ElementKey(id, i));

                EditorGUILayout.EndHorizontal();

                if (doDelete)
                {
                    _dragIndex = -1;
                    setter.SetValue(RemoveAt(array, elementType, i));
                    return;
                }

                if (doMoveUp)
                {
                    _dragIndex = -1;
                    Swap(array, i, i - 1);
                    setter.SetValue(array);
                    return;
                }

                if (doMoveDown)
                {
                    _dragIndex = -1;
                    Swap(array, i, i + 1);
                    setter.SetValue(array);
                    return;
                }
            }

            HandleDragEvents(array, elementType, setter, count);

            // ── Add button ────────────────────────────────────────────────────────
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("");
            if (GUILayout.Button("+", GUILayout.Width(20)))
                newCount++;
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            TryApplyCountChange(array, elementType, newCount, count, setter);
        }

        // ── Array-level foldout ───────────────────────────────────────────────────

        private bool DrawFoldout(int key, string label)
        {
            _foldouts.TryGetValue(key, out bool current);
            bool next = EditorGUILayout.Foldout(current, label, true);
            if (next != current)
                _foldouts[key] = next;
            return next;
        }

        public void Unfold(int id) => _foldouts[id] = true;

        private static int ElementKey(int arrayId, int index) =>
            arrayId * 31 + index + 1;

        // ── Drag and drop ─────────────────────────────────────────────────────────

        private void DrawDragHandle(Rect rect, int index)
        {
            var evt = Event.current;

            if (evt.type == EventType.Repaint)
            {
                var color = _dragIndex == index ? new Color(0.3f, 0.7f, 1f) : new Color(0.55f, 0.55f, 0.55f);
                var prev  = GUI.color;
                GUI.color = color;
                GUI.Label(rect, "≡");
                GUI.color = prev;
            }
            else if (evt.type == EventType.MouseDown && evt.button == 0 && rect.Contains(evt.mousePosition))
            {
                _dragIndex  = index;
                _isDragging = false;
                _dragOrigin = evt.mousePosition;
                evt.Use();
            }
        }

        private void HandleDragEvents(Array array, Type elementType, IValueSetter setter, int count)
        {
            if (_dragIndex < 0)
                return;

            var evt = Event.current;

            if (evt.type == EventType.MouseDrag)
            {
                if (!_isDragging && Vector2.Distance(evt.mousePosition, _dragOrigin) > DragThreshold)
                    _isDragging = true;

                if (_isDragging)
                {
                    evt.Use();
                    GUI.changed = true;
                }
            }
            else if (evt.type == EventType.MouseUp && evt.button == 0)
            {
                if (_isDragging)
                {
                    int dropIdx = DropIndex(evt.mousePosition.y, count);
                    if (dropIdx != _dragIndex && dropIdx != _dragIndex + 1)
                        setter.SetValue(MoveElement(array, elementType, _dragIndex, dropIdx));
                }

                _dragIndex  = -1;
                _isDragging = false;
                evt.Use();
                GUI.changed = true;
            }
            else if (evt.type == EventType.Repaint && _isDragging)
            {
                DrawDropLine(DropIndex(evt.mousePosition.y, count), count);
                EditorWindow.focusedWindow?.Repaint();
            }
        }

        private int DropIndex(float mouseY, int count)
        {
            for (int i = 0; i < count && i < _headerRects.Count; i++)
            {
                if (mouseY < _headerRects[i].center.y)
                    return i;
            }

            return count;
        }

        private void DrawDropLine(int dropIndex, int count)
        {
            if (count == 0 || _headerRects.Count == 0)
                return;

            float lineY;
            Rect  refRect;

            if (dropIndex <= 0)
            {
                refRect = _headerRects[0];
                lineY   = refRect.yMin - 1f;
            }
            else if (dropIndex >= count)
            {
                refRect = _headerRects[count - 1];
                lineY   = refRect.yMax;
            }
            else
            {
                refRect = _headerRects[dropIndex];
                lineY   = refRect.yMin - 1f;
            }

            EditorGUI.DrawRect(new Rect(refRect.x, lineY, refRect.width, 2f), new Color(0.25f, 0.6f, 1f));
        }

        // ── Context menu ──────────────────────────────────────────────────────────

        private void HandleContextMenu(Array array, Type elementType, IValueSetter setter, int index, int count)
        {
            if (Event.current.type != EventType.ContextClick)
                return;
            if (index >= _headerRects.Count || !_headerRects[index].Contains(Event.current.mousePosition))
                return;

            var capturedArray  = array;
            var capturedSetter = setter;
            var capturedType   = elementType;
            int cap            = index;

            var menu = new GenericMenu();

            if (index > 0)
                menu.AddItem(new GUIContent("Move Up"), false, () =>
                {
                    Swap(capturedArray, cap, cap - 1);
                    capturedSetter.SetValue(capturedArray);
                });
            else
                menu.AddDisabledItem(new GUIContent("Move Up"));

            if (index < count - 1)
                menu.AddItem(new GUIContent("Move Down"), false, () =>
                {
                    Swap(capturedArray, cap, cap + 1);
                    capturedSetter.SetValue(capturedArray);
                });
            else
                menu.AddDisabledItem(new GUIContent("Move Down"));

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Delete"), false, () =>
                capturedSetter.SetValue(RemoveAt(capturedArray, capturedType, cap)));

            menu.ShowAsContext();
            Event.current.Use();
        }

        // ── Array mutations ───────────────────────────────────────────────────────

        private static void Swap(Array array, int a, int b)
        {
            var tmp = array.GetValue(a);
            array.SetValue(array.GetValue(b), a);
            array.SetValue(tmp, b);
        }

        private static Array RemoveAt(Array array, Type elementType, int index)
        {
            var result = Array.CreateInstance(elementType, array.Length - 1);
            for (int i = 0, j = 0; i < array.Length; i++)
            {
                if (i != index)
                    result.SetValue(array.GetValue(i), j++);
            }

            return result;
        }

        private static Array MoveElement(Array array, Type elementType, int from, int to)
        {
            if (to == from || to == from + 1)
                return array;

            var list = new List<object>(array.Length);
            for (int i = 0; i < array.Length; i++)
                list.Add(array.GetValue(i));

            var item     = list[from];
            int insertAt = to > from ? to - 1 : to;
            list.RemoveAt(from);
            list.Insert(insertAt, item);

            var result = Array.CreateInstance(elementType, array.Length);
            for (int i = 0; i < list.Count; i++)
                result.SetValue(list[i], i);

            return result;
        }

        private static void TryApplyCountChange(Array array, Type elementType, int newCount, int oldCount, IValueSetter setter)
        {
            if (newCount == oldCount)
                return;

            var newArr = Array.CreateInstance(elementType, newCount);
            Array.Copy(array, newArr, Math.Min(array.Length, newArr.Length));
            setter.SetValue(newArr);
        }

        // ── Null header ───────────────────────────────────────────────────────────

        private void DrawNullHeader(Type elementType, Type arrayType, string name, IValueSetter setter, int id)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField($"{name} [{arrayType.Name}]", "[null]");
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("*", GUILayout.Width(20)))
            {
                setter.SetValue(Array.CreateInstance(elementType, 1));
                _foldouts[id] = true;
            }
            EditorGUILayout.EndHorizontal();
        }

        // ── Element naming ────────────────────────────────────────────────────────

        private string TryGetName(object obj, int index)
        {
            if (obj == null)
                return $"item [{index}]";

            var objType = obj.GetType();
            if (HasCustomToString(objType))
                return obj.ToString();

            var fields = objType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(string))
                {
                    var val = field.GetValue(obj) as string;
                    if (!string.IsNullOrEmpty(val))
                        return val;
                }

                if (typeof(GameConfiguration).IsAssignableFrom(field.FieldType))
                {
                    var val = field.GetValue(obj) as GameConfiguration;
                    if (val != null && !string.IsNullOrEmpty(val.name))
                        return val.name;
                }
            }

            return $"{objType.Name} [{index}]";
        }

        private static readonly Dictionary<Type, bool> _customToStringCache = new();

        private static bool HasCustomToString(Type type)
        {
            if (_customToStringCache.TryGetValue(type, out bool cached))
                return cached;

            var method = type.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance,
                null, Type.EmptyTypes, null);
            bool has = method != null
                && method.DeclaringType != typeof(object)
                && method.DeclaringType != typeof(ValueType);
            _customToStringCache[type] = has;

            return has;
        }
    }
}
#endif
