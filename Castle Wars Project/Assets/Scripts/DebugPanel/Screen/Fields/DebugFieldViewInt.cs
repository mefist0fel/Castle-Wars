using DebugPanelExtention;
using TMPro;
using UnityEngine;

public sealed class DebugFieldViewInt : DebugFieldView
{
    [SerializeField] private TMP_InputField _input     = null;
    [SerializeField] private TMP_Text       _nameLabel = null;

    public override void Initialize(DebugParam param)
    {
        _nameLabel.SetTextSafe(param.Name);

        if (_input == null)
            return;

        _input.contentType = TMP_InputField.ContentType.IntegerNumber;
        _input.text        = ((int)param.Default).ToString();
        _input.onEndEdit.AddListener(OnEndEdit);
    }

    public override object GetValue()
    {
        if (_input == null || !int.TryParse(_input.text, out int value))
            return 0;

        return value;
    }

    public override void SetDisplayValue(object value)
    {
        if (_input == null || _input.isFocused)
            return;

        _input.text = ((int)value).ToString();
    }

    private void OnEndEdit(string text)
    {
        if (int.TryParse(text, out int value))
            OnValueCommitted?.Invoke(value);
    }
}
