using DebugPanelExtention;
using System.Globalization;
using TMPro;
using UnityEngine;

public sealed class DebugFieldViewFloat : DebugFieldView
{
    [SerializeField] private TMP_InputField _input     = null;
    [SerializeField] private TMP_Text       _nameLabel = null;

    public override void Initialize(DebugParam param)
    {
        _nameLabel.SetTextSafe(param.Name);

        if (_input == null)
            return;

        _input.contentType = TMP_InputField.ContentType.DecimalNumber;
        _input.text        = ((float)param.Default).ToString(CultureInfo.InvariantCulture);
        _input.onEndEdit.AddListener(OnEndEdit);
    }

    public override object GetValue()
    {
        if (_input == null)
            return 0f;

        return float.TryParse(_input.text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value)
            ? value
            : 0f;
    }

    public override void SetDisplayValue(object value)
    {
        if (_input == null || _input.isFocused)
            return;

        _input.text = ((float)value).ToString(CultureInfo.InvariantCulture);
    }

    private void OnEndEdit(string text)
    {
        if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
            OnValueCommitted?.Invoke(value);
    }
}
