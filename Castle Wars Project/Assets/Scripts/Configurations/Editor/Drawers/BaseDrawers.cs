#if UNITY_EDITOR
using Configurations.Model.Id;
using UnityEditor;
using UnityEngine;

namespace Configurations.Editor.Drawers
{
    internal class IntDrawer : AbstractDrawer<int>
    {
        public override void ShowDrawerGUI(int value, string name, IValueSetter setter, int _)
        {
            var newValue = EditorGUILayout.IntField(new GUIContent(name), value);
            if (newValue != value)
            {
                setter.SetValue(newValue);
            }
        }
    }

    internal class UintDrawer : AbstractDrawer<uint>
    {
        public override void ShowDrawerGUI(uint value, string name, IValueSetter setter, int _)
        {
            var newValue = (uint)EditorGUILayout.IntField(new GUIContent(name), (int)value);
            if (newValue != value)
            {
                setter.SetValue(newValue);
            }
        }
    }

    internal class ByteDrawer : AbstractDrawer<byte>
    {
        public override void ShowDrawerGUI(byte value, string name, IValueSetter setter, int _)
        {
            var newValue = (byte)EditorGUILayout.IntField(new GUIContent(name), (int)value);
            if (newValue != value)
            {
                setter.SetValue(newValue);
            }
        }
    }

    internal class SbyteDrawer : AbstractDrawer<sbyte>
    {
        public override void ShowDrawerGUI(sbyte value, string name, IValueSetter setter, int _)
        {
            var newValue = (sbyte)EditorGUILayout.IntField(new GUIContent(name), (int)value);
            if (newValue != value)
            {
                setter.SetValue(newValue);
            }
        }
    }

    internal class LongDrawer : AbstractDrawer<long>
    {
        public override void ShowDrawerGUI(long value, string name, IValueSetter setter, int _)
        {
            var newValue = (long)EditorGUILayout.IntField(new GUIContent(name), (int)value);
            if (newValue != value)
            {
                setter.SetValue(newValue);
            }
        }
    }

    internal class UlongDrawer : AbstractDrawer<ulong>
    {
        public override void ShowDrawerGUI(ulong value, string name, IValueSetter setter, int _)
        {
            var newValue = (ulong)EditorGUILayout.IntField(new GUIContent(name), (int)value);
            if (newValue != value)
            {
                setter.SetValue(newValue);
            }
        }
    }

    internal class FloatDrawer : AbstractDrawer<float>
    {
        public override void ShowDrawerGUI(float value, string name, IValueSetter setter, int _)
        {
            var newValue = EditorGUILayout.FloatField(new GUIContent(name), value);
            if (newValue != value)
            {
                setter.SetValue(newValue);
            }
        }
    }
    internal class StringDrawer : AbstractDrawer<string>
    {
        public override void ShowDrawerGUI(string value, string name, IValueSetter setter, int _)
        {
            var newValue = EditorGUILayout.TextField(new GUIContent(name), value);
            if (newValue != value)
            {
                setter.SetValue(newValue);
            }
        }
    }
    internal class ConfigurationIdDrawer : AbstractDrawer<ConfigurationId>
    {
        public override void ShowDrawerGUI(ConfigurationId value, string name, IValueSetter setter, int _)
        {
            var stringValue = value.ToString();
            var newValue = EditorGUILayout.TextField(new GUIContent(name), value.ToString());
            if (stringValue != newValue)
            {
                setter.SetValue(new ConfigurationId(newValue));
            }
        }
    }
    internal class BoolDrawer : AbstractDrawer<bool>
    {
        public override void ShowDrawerGUI(bool value, string name, IValueSetter setter, int _)
        {
            var newValue = EditorGUILayout.Toggle(new GUIContent(name), value);
            if (newValue != value)
            {
                setter.SetValue(newValue);
            }
        }
    }
}
#endif