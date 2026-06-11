using DebugPanelExtention;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class DebugFieldViewBool : DebugFieldView
{
    [SerializeField] private Toggle   _toggle    = null;
    [SerializeField] private TMP_Text _nameLabel = null;

    // Guards against SetDisplayValue triggering OnValueCommitted via the toggle callback.
    private bool _suppressCommit;

    public override void Initialize(DebugParam param)
    {
        _nameLabel.SetTextSafe(param.Name);

        if (_toggle == null)
            return;

        _toggle.isOn = (bool)param.Default;
        _toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    public override object GetValue() => _toggle != null && _toggle.isOn;

    public override void SetDisplayValue(object value)
    {
        if (_toggle == null)
            return;

        bool target = (bool)value;
        if (_toggle.isOn == target)
            return;

        _suppressCommit = true;
        _toggle.isOn    = target;
        _suppressCommit = false;
    }

    private void OnToggleChanged(bool value)
    {
        if (!_suppressCommit)
            OnValueCommitted?.Invoke(value);
    }
}
