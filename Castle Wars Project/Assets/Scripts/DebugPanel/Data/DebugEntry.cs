using System;

// -- Parameter descriptor

public enum DebugParamType { Int, Float, String, Bool, Enum }

// Describes one parameter slot: its type, default value, and optional display name.
// Created via static factories; the private constructor prevents unsupported types.
public sealed class DebugParam
{
    public readonly DebugParamType Type;
    public readonly object         Default;
    public readonly string         Name;
    public readonly Type           EnumType; // non-null only when Type == Enum

    private DebugParam(DebugParamType type, object defaultValue, string name, Type enumType = null)
    {
        Type     = type;
        Default  = defaultValue;
        Name     = name ?? string.Empty;
        EnumType = enumType;
    }

    public static DebugParam Int(int defaultValue = 0, string name = "")
        => new(DebugParamType.Int, defaultValue, name);

    public static DebugParam Float(float defaultValue = 0f, string name = "")
        => new(DebugParamType.Float, defaultValue, name);

    public static DebugParam String(string defaultValue = "", string name = "")
        => new(DebugParamType.String, defaultValue ?? string.Empty, name);

    public static DebugParam Bool(bool defaultValue = false, string name = "")
        => new(DebugParamType.Bool, defaultValue, name);

    public static DebugParam Enum(Type enumType, object defaultValue = null, string name = "")
    {
        var firstValue = defaultValue ?? System.Enum.GetValues(enumType).GetValue(0);
        return new(DebugParamType.Enum, firstValue, name, enumType);
    }
}

// -- Debug action
// Universal action entry: any callback arity, any supported param types.
// The typed Add overloads on DebugPanel wrap the original delegate in Action<object[]>
// so the UI can call it uniformly regardless of arity.
public sealed class DebugAction
{
    public readonly string       Label;
    public readonly int          Priority;
    public readonly DebugParam[] Params;

    // Preserved for identity-based deregistration via Remove(Delegate).
    public readonly Delegate OriginalCallback;

    internal readonly Action<object[]> Invoke;

    internal DebugAction(string label, int priority, DebugParam[] paramDefs,
                         Delegate originalCallback, Action<object[]> invoke)
    {
        Label            = label;
        Priority         = priority;
        Params           = paramDefs ?? Array.Empty<DebugParam>();
        OriginalCallback = originalCallback;
        Invoke           = invoke;
    }
}

// -- Debug variable
// Bidirectional binding to a single typed value via object-level getter/setter.
// The typed AddXxx overloads on DebugPanel box/unbox the concrete type internally.
public sealed class DebugVariable
{
    public readonly string     Label;
    public readonly int        Priority;
    public readonly DebugParam Param;

    // Preserved for identity-based deregistration via Remove(Delegate).
    public readonly Delegate OriginalGetter;

    internal readonly Func<object>   GetValue;
    internal readonly Action<object> SetValue;

    internal DebugVariable(string label, int priority, DebugParam param,
                           Delegate originalGetter,
                           Func<object> getValue, Action<object> setValue)
    {
        Label          = label;
        Priority       = priority;
        Param          = param;
        OriginalGetter = originalGetter;
        GetValue       = getValue;
        SetValue       = setValue;
    }
}
