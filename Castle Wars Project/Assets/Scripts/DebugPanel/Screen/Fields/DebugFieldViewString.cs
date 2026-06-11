using DebugPanelExtention;
using TMPro;
using UnityEngine;

public sealed class DebugFieldViewString : DebugFieldView
{
    [SerializeField] private TMP_InputField _input     = null;
    [SerializeField] private TMP_Text       _nameLabel = null;

    public override void Initialize(DebugParam param)
    {
        _nameLabel.SetTextSafe(param.Name);

        if (_input == null)
            return;

        _input.contentType = TMP_InputField.ContentType.Standard;
        _input.text        = (string)param.Default ?? string.Empty;
        _input.onEndEdit.AddListener(OnEndEdit);
    }

    public override object GetValue() => _input != null ? _input.text : string.Empty;

    public override void SetDisplayValue(object value)
    {
        if (_input == null || _input.isFocused)
            return;

        _input.text = value as string ?? string.Empty;
    }

    private void OnEndEdit(string text)
    {
        OnValueCommitted?.Invoke(text);
    }
}
