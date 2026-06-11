using UnityEngine;

// Shared utility used by DebugActionBlock and DebugVariableBlock to
// instantiate the correct field-view prefab for a given DebugParam.
internal static class DebugFieldSpawner
{
    internal static DebugFieldView Spawn(
        DebugParam param, Transform parent,
        DebugFieldViewInt    intPrefab,
        DebugFieldViewFloat  floatPrefab,
        DebugFieldViewString stringPrefab,
        DebugFieldViewBool   boolPrefab,
        DebugFieldViewEnum   enumPrefab)
    {
        DebugFieldView prefab;

        switch (param.Type)
        {
            case DebugParamType.Int:    prefab = intPrefab;    break;
            case DebugParamType.Float:  prefab = floatPrefab;  break;
            case DebugParamType.String: prefab = stringPrefab; break;
            case DebugParamType.Bool:   prefab = boolPrefab;   break;
            case DebugParamType.Enum:   prefab = enumPrefab;   break;
            default:                    prefab = null;         break;
        }

        if (prefab == null)
        {
            Debug.LogWarning($"[DebugPanel] No field prefab assigned for param type {param.Type} (name: \"{param.Name}\"). Field skipped.");
            return null;
        }

        var field = Object.Instantiate(prefab, parent);
        field.Initialize(param);
        return field;
    }
}
