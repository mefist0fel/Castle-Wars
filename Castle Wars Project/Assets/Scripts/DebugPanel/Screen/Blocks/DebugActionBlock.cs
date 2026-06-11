using DebugPanelExtention;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// "Function starter" prefab: a labeled button plus dynamically spawned parameter fields.
// Prefab layout (suggested, all optional except button):
//   root  -- DebugActionBlock + HorizontalLayoutGroup (or Vertical)
//     button   -- Button with TMP_Text label child
//     fields   -- empty Transform used as field container (fieldContainer)
//       [DebugFieldView prefabs spawned here at runtime]
public sealed class DebugActionBlock : MonoBehaviour
{
    [SerializeField] private Button    button         = null;
    [SerializeField] private TMP_Text  label          = null;
    [SerializeField] private Transform fieldContainer = null;

    [Header("Field prefabs")]
    [SerializeField] private DebugFieldViewInt    intFieldPrefab    = null;
    [SerializeField] private DebugFieldViewFloat  floatFieldPrefab  = null;
    [SerializeField] private DebugFieldViewString stringFieldPrefab = null;
    [SerializeField] private DebugFieldViewBool   boolFieldPrefab   = null;
    [SerializeField] private DebugFieldViewEnum   enumFieldPrefab   = null;

    private DebugFieldView[] _fields;

    public void Initialize(DebugAction action, Action<Action> queueAction)
    {
        label.SetTextSafe(action.Label);

        var container = fieldContainer != null ? fieldContainer : transform;

        _fields = new DebugFieldView[action.Params.Length];
        for (int i = 0; i < action.Params.Length; i++)
        {
            _fields[i] = DebugFieldSpawner.Spawn(action.Params[i], container,
                intFieldPrefab, floatFieldPrefab, stringFieldPrefab, boolFieldPrefab, enumFieldPrefab);
        }

        button.OnClickAddListenerSafe(() =>
        {
            var args = CollectArgs();
            queueAction(() => action.Invoke(args));
        });
    }

    private object[] CollectArgs()
    {
        var args = new object[_fields.Length];
        for (int i = 0; i < _fields.Length; i++)
            args[i] = _fields[i] != null ? _fields[i].GetValue() : null;

        return args;
    }
}
