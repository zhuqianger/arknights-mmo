using UnityEngine.Events;
using UnityEngine.UI;

public class UIButton : Button
{
    public void SetClick(UnityAction action)
    {
        onClick.RemoveAllListeners();
        if (action != null)
            onClick.AddListener(action);
    }
}
