using TMPro;

namespace DebugPanelExtention
{
    public static class TMProExtension
    {
        public static void SetTextSafe(this TMP_Text text, string value)
        {
            if (text == null)
                return;
            text.text = value;
        }
    }
}