using TMPro;

public class UIText : TextMeshProUGUI
{
    public void SetContent(string value)
    {
        text = value ?? string.Empty;
    }
}
