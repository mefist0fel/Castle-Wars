using System;
using System.Collections.Generic;
using UnityEngine;

// ScriptableObject that owns the debug panel instance and entry registry.
// Any owner of this SO can register actions/variables and call Show()/Hide().
// The panel prefab is self-hosted: instantiated on first Show(), kept DontDestroyOnLoad.
[CreateAssetMenu(fileName = "DebugPanel", menuName = "Debug/DebugPanel")]
public sealed class DebugPanel : ScriptableObject
{
    [SerializeField] private GameObject _prefab = null;

    private readonly List<DebugAction>   _actions = new();
    private readonly List<DebugVariable> _variables = new();

    private DebugPanelScreen _screen;

    public IReadOnlyList<DebugAction>   Actions   => _actions;
    public IReadOnlyList<DebugVariable> Variables => _variables;

    // -- Registration: actions
    //
    // Generic overloads (A..D) accept any combination of: int, float, string, bool, any Enum.
    // Type validation happens at registration time -- unsupported types throw ArgumentException.
    // Default values (a, b, c, d) follow priority and use the type's default when omitted.

    public void Add(string label, Action callback, int priority = 0)
    {
        _actions.Add(new DebugAction(label, priority,
            Array.Empty<DebugParam>(),
            callback,
            _ => callback()));
    }

    public void Add<A>(string label, Action<A> callback, int priority = 0, A a = default)
    {
        var paramA = ParamFor(a);
        _actions.Add(new DebugAction(label, priority,
            new[] { paramA },
            callback,
            args => callback((A)args[0])));
    }

    public void Add<A, B>(string label, Action<A, B> callback, int priority = 0, A a = default, B b = default)
    {
        var paramA = ParamFor(a);
        var paramB = ParamFor(b);
        _actions.Add(new DebugAction(label, priority,
            new[] { paramA, paramB },
            callback,
            args => callback((A)args[0], (B)args[1])));
    }

    public void Add<A, B, C>(string label, Action<A, B, C> callback, int priority = 0,
                              A a = default, B b = default, C c = default)
    {
        var paramA = ParamFor(a);
        var paramB = ParamFor(b);
        var paramC = ParamFor(c);
        _actions.Add(new DebugAction(label, priority,
            new[] { paramA, paramB, paramC },
            callback,
            args => callback((A)args[0], (B)args[1], (C)args[2])));
    }

    public void Add<A, B, C, D>(string label, Action<A, B, C, D> callback, int priority = 0,
                                 A a = default, B b = default, C c = default, D d = default)
    {
        var paramA = ParamFor(a);
        var paramB = ParamFor(b);
        var paramC = ParamFor(c);
        var paramD = ParamFor(d);
        _actions.Add(new DebugAction(label, priority,
            new[] { paramA, paramB, paramC, paramD },
            callback,
            args => callback((A)args[0], (B)args[1], (C)args[2], (D)args[3])));
    }

    // Escape hatch for cases not covered by the generic overloads.
    // Caller supplies DebugParam descriptors explicitly and receives object[] on invoke.
    public void Add(string label, Action<object[]> callback, int priority = 0, params DebugParam[] paramDefs)
    {
        _actions.Add(new DebugAction(label, priority,
            paramDefs,
            callback,
            args => callback(args)));
    }

    // -- Registration: variables

    public void AddBool(string label, Func<bool> get, Action<bool> set, int priority = 0)
    {
        _variables.Add(new DebugVariable(label, priority, DebugParam.Bool(),
            get, () => get(), value => set((bool)value)));
    }

    public void AddInt(string label, Func<int> get, Action<int> set, int priority = 0)
    {
        _variables.Add(new DebugVariable(label, priority, DebugParam.Int(),
            get, () => get(), value => set((int)value)));
    }

    public void AddFloat(string label, Func<float> get, Action<float> set, int priority = 0)
    {
        _variables.Add(new DebugVariable(label, priority, DebugParam.Float(),
            get, () => get(), value => set((float)value)));
    }

    public void AddString(string label, Func<string> get, Action<string> set, int priority = 0)
    {
        _variables.Add(new DebugVariable(label, priority, DebugParam.String(),
            get, () => get(), value => set((string)value)));
    }

    public void AddEnum<T>(string label, Func<T> get, Action<T> set, int priority = 0) where T : Enum
    {
        _variables.Add(new DebugVariable(label, priority, DebugParam.Enum(typeof(T), get()),
            get, () => get(), value => set((T)value)));
    }

    // -- Deregistration

    public void Remove(string label)
    {
        _actions.RemoveAll(entry => entry.Label == label);
        _variables.RemoveAll(entry => entry.Label == label);
    }

    // Pass the same delegate reference that was used in Add() or AddXxx().
    // For actions: the Action / Action<A> / ... reference.
    // For variables: the Func<T> getter reference.
    public void Remove(Delegate callback)
    {
        _actions.RemoveAll(entry => entry.OriginalCallback == callback);
        _variables.RemoveAll(entry => entry.OriginalGetter == callback);
    }

    // -- Visibility

    public void Show()
    {
        EnsureScreen();
        _screen.Open(this);
    }

    public void Hide()
    {
        _screen?.Close();
    }

    public bool IsVisible => _screen != null && _screen.gameObject.activeSelf;

    // -- Internal

    private void EnsureScreen()
    {
        if (_screen != null)
            return;

        var go = Instantiate(_prefab);
        DontDestroyOnLoad(go);
        _screen = go.GetComponent<DebugPanelScreen>();
    }

    private void OnDisable()
    {
        _actions.Clear();
        _variables.Clear();

        if (_screen == null)
            return;

        if (Application.isPlaying)
            Destroy(_screen.gameObject);

        _screen = null;
    }

    // -- Type resolution
    //
    // Maps a typed default value to the corresponding DebugParam.
    // Throws ArgumentException at registration time if the type is not supported.
    // Supported: int, float, string, bool, any Enum.
    private static DebugParam ParamFor<T>(T defaultValue)
    {
        var type = typeof(T);

        if (type == typeof(int))    return DebugParam.Int((int)(object)defaultValue);
        if (type == typeof(float))  return DebugParam.Float((float)(object)defaultValue);
        if (type == typeof(string)) return DebugParam.String((string)(object)defaultValue);
        if (type == typeof(bool))   return DebugParam.Bool((bool)(object)defaultValue);
        if (type.IsEnum)            return DebugParam.Enum(type, defaultValue);

        throw new ArgumentException(
            $"[DebugPanel] Unsupported parameter type '{type.Name}'. Supported: int, float, string, bool, any Enum.");
    }
}
