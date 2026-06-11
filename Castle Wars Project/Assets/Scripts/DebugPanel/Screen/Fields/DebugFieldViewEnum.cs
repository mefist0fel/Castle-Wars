using DebugPanelExtention;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class DebugFieldViewEnum : DebugFieldView
{
    [SerializeField] private TMP_Dropdown _dropdown  = null;
    [SerializeField] private TMP_Text     _nameLabel = null;

    private Array _enumValues;

    public override void Initialize(DebugParam param)
    {
        _nameLabel.SetTextSafe(param.Name);

        if (_dropdown == null || param.EnumType == null)
            return;

        _enumValues = Enum.GetValues(param.EnumType);

        _dropdown.ClearOptions();

        var options = new List<string>(_enumValues.Length);
        foreach (var value in _enumValues)
            options.Add(value.ToString());

        _dropdown.AddOptions(options);

        int selectedIndex = Array.IndexOf(_enumValues, param.Default);
        _dropdown.value = selectedIndex >= 0 ? selectedIndex : 0;

        _dropdown.onValueChanged.AddListener(OnDropdownChanged);
    }

    public override object GetValue()
    {
        if (_dropdown == null || _enumValues == null)
            return null;

        return _enumValues.GetValue(_dropdown.value);
    }

    public override void SetDisplayValue(object value)
    {
        if (_dropdown == null || _enumValues == null)
            return;

        int index = Array.IndexOf(_enumValues, value);
        if (index >= 0 && _dropdown.value != index)
            _dropdown.value = index;
    }

    private void OnDropdownChanged(int index)
    {
        if (_enumValues != null)
            OnValueCommitted?.Invoke(_enumValues.GetValue(index));
    }
}
