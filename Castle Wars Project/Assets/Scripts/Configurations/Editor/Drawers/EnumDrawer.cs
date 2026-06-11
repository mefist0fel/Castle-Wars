#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;

namespace Configurations.Editor.Drawers
{
    internal sealed class EnumDrawer : AbstractDrawer
    {
        public override bool IsTypeSupported(Type type) =>
            type.IsEnum;

        public override void ShowDrawerGUI(object value, Type type, string name, IValueSetter setter, int _)
        {
            Enum newEnum;
            if (type.IsDefined(typeof(FlagsAttribute), false))
            {
                newEnum = EditorGUILayout.EnumFlagsField(name, (Enum)value);
            }
            else
            {
                newEnum = EditorGUILayout.EnumPopup(name, (Enum)value);
            }
            if (!Equals(value, newEnum))
            {
                setter.SetValue(newEnum);
            }
        }
    }
}
#endif