using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class HttpResult
{
    public bool Success;
    public long Status;
    public string Text;
    public string Error;
}

public static class HttpApi
{
    public static IEnumerator Get(string url, Action<HttpResult> onDone)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            onDone?.Invoke(ToResult(request));
        }
    }

    public static IEnumerator Post(string url, string json, Action<HttpResult> onDone)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            byte[] body = Encoding.UTF8.GetBytes(json ?? "");
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            onDone?.Invoke(ToResult(request));
        }
    }

    static HttpResult ToResult(UnityWebRequest request)
    {
        bool success = request.result == UnityWebRequest.Result.Success;
        return new HttpResult
        {
            Success = success,
            Status = request.responseCode,
            Text = request.downloadHandler != null ? request.downloadHandler.text : null,
            Error = success ? null : request.error
        };
    }
}
