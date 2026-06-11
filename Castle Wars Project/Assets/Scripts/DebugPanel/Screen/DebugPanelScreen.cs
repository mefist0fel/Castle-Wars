using DebugPanelExtention;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Root MonoBehaviour on the debug panel prefab.
// Instantiated and owned by DebugPanel (ScriptableObject).
// Actions are never executed immediately on click -- they are queued
// and dispatched in LateUpdate so the callback can safely modify registrations.
public sealed class DebugPanelScreen : MonoBehaviour
{
    [SerializeField] private Button             closeButton         = null;
    [SerializeField] private Transform          container           = null;
    [SerializeField] private DebugActionBlock   actionBlockPrefab   = null;
    [SerializeField] private DebugVariableBlock variableBlockPrefab = null;

    private DebugPanel              _panel;
    private Action                  _pendingAction;
    private readonly List<GameObject> _blocks = new();

    private void Awake()
    {
        closeButton.OnClickAddListenerSafe(Close);
        gameObject.SetActive(false);
    }

    public void Open(DebugPanel panel)
    {
        _panel = panel;
        gameObject.SetActive(true);
        Rebuild();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (_pendingAction == null)
            return;

        var action = _pendingAction;
        _pendingAction = null;
        action();

        if (gameObject.activeSelf && _panel != null)
            Rebuild();
    }

    // -- UI build

    private void Rebuild()
    {
        foreach (var block in _blocks)
        {
            if (block != null)
                Destroy(block);
        }

        _blocks.Clear();

        var allEntries = new List<(int priority, Action spawn)>();

        foreach (var action in _panel.Actions)
        {
            var captured = action;
            allEntries.Add((captured.Priority, () => SpawnAction(captured)));
        }

        foreach (var variable in _panel.Variables)
        {
            var captured = variable;
            allEntries.Add((captured.Priority, () => SpawnVariable(captured)));
        }

        allEntries.Sort((entryA, entryB) => entryB.priority.CompareTo(entryA.priority));

        foreach (var (priority, spawn) in allEntries)
            spawn();
    }

    private void SpawnAction(DebugAction action)
    {
        if (actionBlockPrefab == null)
            return;

        var block = Instantiate(actionBlockPrefab, container);
        block.Initialize(action, QueueAction);
        _blocks.Add(block.gameObject);
    }

    private void SpawnVariable(DebugVariable variable)
    {
        if (variableBlockPrefab == null)
            return;

        var block = Instantiate(variableBlockPrefab, container);
        block.Initialize(variable);
        _blocks.Add(block.gameObject);
    }

    private void QueueAction(Action action)
    {
        _pendingAction = action;
    }
}
