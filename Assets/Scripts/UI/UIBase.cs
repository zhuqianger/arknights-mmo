using UnityEngine;

public class UIBase : MonoBehaviour
{
    public bool IsOpen => gameObject.activeSelf;

    public virtual void Open()
    {
        gameObject.SetActive(true);
        OnOpen();
    }

    public virtual void Close()
    {
        OnClose();
        gameObject.SetActive(false);
    }

    protected virtual void OnOpen()
    {
    }

    protected virtual void OnClose()
    {
    }
}
