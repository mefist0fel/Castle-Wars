#if UNITY_EDITOR
using Configurations.Editor.Drawers;
using Configurations.Model.Types;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Renders a <see cref="ColorData"/> field as a standard Unity color picker.
/// Auto-discovered by ClassDrawer via assembly scanning.
/// </summary>
internal sealed class ColorDataDrawer : AbstractDrawer<ColorData>
{
    public override void ShowDrawerGUI(ColorData value, string name, IValueSetter setter, int instanceId)
    {
        // Implicit cast ColorData → UnityEngine.Color (defined under UNITY_5_3_OR_NEWER)
        Color current = value;
        Color updated = EditorGUILayout.ColorField(name, current);

        if (updated != current)
            setter.SetValue((ColorData)updated); // explicit cast back
    }
}
#endif
