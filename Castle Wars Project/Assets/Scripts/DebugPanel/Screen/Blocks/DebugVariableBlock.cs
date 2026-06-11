using DebugPanelExtention;
using TMPro;
using UnityEngine;

// Variable block: a label plus one dynamically spawned field that stays
// in sync with the variable's getter/setter each LateUpdate.
// Prefab layout (suggested):
//   root  -- DebugVariableBlock + HorizontalLayoutGroup
//     label  -- TMP_Text
//     field  -- empty Transform for the field container (fieldContainer)
//       [one DebugFieldView prefab spawned here at runtime]
public sealed class DebugVariableBlock : MonoBehaviour
{
    [SerializeField] private TMP_Text  label          = null;
    [SerializeField] private Transform fieldContainer = null;

    [Header("Field prefabs")]
    [SerializeField] private DebugFieldViewInt    intFieldPrefab    = null;
    [SerializeField] private DebugFieldViewFloat  floatFieldPrefab  = null;
    [SerializeField] private DebugFieldViewString stringFieldPrefab = null;
    [SerializeField] private DebugFieldViewBool   boolFieldPrefab   = null;
    [SerializeField] private DebugFieldViewEnum   enumFieldPrefab   = null;

    private DebugVariable  _variable;
    private DebugFieldView _field;

    public void Initialize(DebugVariable variable)
    {
        _variable = variable;
        label.SetTextSafe(variable.Label);

        var container = fieldContainer != null ? fieldContainer : transform;
        _field = DebugFieldSpawner.Spawn(variable.Param, container,
            intFieldPrefab, floatFieldPrefab, stringFieldPrefab, boolFieldPrefab, enumFieldPrefab);

        if (_field != null)
            _field.OnValueCommitted = value => variable.SetValue(value);
    }

    private void LateUpdate()
    {
        if (_variable == null || _field == null)
            return;

        _field.SetDisplayValue(_variable.GetValue());
    }
}
