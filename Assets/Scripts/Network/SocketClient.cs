using System;
using UnityWebSocket;

public class SocketClient
{
    readonly WebSocket socket;

    public bool IsOpen => socket.ReadyState == WebSocketState.Open;

    public event Action OnOpen;
    public event Action<string> OnText;
    public event Action<byte[]> OnBinary;
    public event Action<string> OnError;
    public event Action<ushort, string> OnClose;

    public SocketClient(string url)
    {
        socket = new WebSocket(url);
        socket.OnOpen += HandleOpen;
        socket.OnMessage += HandleMessage;
        socket.OnError += HandleError;
        socket.OnClose += HandleClose;
    }

    public void Connect()
    {
        socket.ConnectAsync();
    }

    public void Send(string text)
    {
        if (!IsOpen)
        {
            OnError?.Invoke("socket is not open");
            return;
        }

        socket.SendAsync(text);
    }

    public void Send(byte[] data)
    {
        if (!IsOpen)
        {
            OnError?.Invoke("socket is not open");
            return;
        }

        socket.SendAsync(data);
    }

    public void Close()
    {
        socket.CloseAsync();
    }

    void HandleOpen(object sender, OpenEventArgs e)
    {
        OnOpen?.Invoke();
    }

    void HandleMessage(object sender, MessageEventArgs e)
    {
        if (e.IsBinary)
            OnBinary?.Invoke(e.RawData);
        else
            OnText?.Invoke(e.Data);
    }

    void HandleError(object sender, ErrorEventArgs e)
    {
        OnError?.Invoke(e.Message);
    }

    void HandleClose(object sender, CloseEventArgs e)
    {
        OnClose?.Invoke(e.Code, e.Reason);
    }
}
