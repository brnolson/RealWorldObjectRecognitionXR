using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class LlmClient
{
    private const string BaseUrl = "https://shayne-rosaceous-francina.ngrok-free.dev";

    [Serializable]
    private class TranscribeResponse
    {
        public string text;
    }

    /// <summary>
    /// Call /transcribe to get text from audio.
    /// Returns just the "text" field, not raw JSON.
    /// </summary>
    public static async Task<string> TranscribeOnly(byte[] audio)
    {
        if (audio == null || audio.Length == 0)
        {
            Debug.LogWarning("[LlmClient] TranscribeOnly called with empty audio");
            return null;
        }

        var form = new WWWForm();
        form.AddBinaryData("audio", audio, "audio.wav", "audio/wav");

        using var request = UnityWebRequest.Post($"{BaseUrl}/transcribe", form);

        var op = request.SendWebRequest();
        while (!op.isDone)
            await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("[LlmClient] TranscribeOnly failed: " + request.responseCode + " " + request.error);
            return null;
        }

        string raw = request.downloadHandler.text;
        Debug.Log("[LlmClient] /transcribe raw: " + raw);

        try
        {
            var resp = JsonUtility.FromJson<TranscribeResponse>(raw);
            return resp != null ? resp.text : raw;
        }
        catch
        {
            // If JSON parse fails, just return raw response
            return raw;
        }
    }

    /// <summary>
    /// Call /llm with audio + image.
    /// Returns raw JSON string (AssistantResponse) from backend.
    /// </summary>
    public static async Task<string> SendAsync(byte[] audio, byte[] image)
    {
        var form = new WWWForm();
        if (audio != null && audio.Length > 0)
            form.AddBinaryData("audio", audio, "audio.wav", "audio/wav");
        if (image != null && image.Length > 0)
            form.AddBinaryData("image", image, "image.png", "image/png");

        using var request = UnityWebRequest.Post($"{BaseUrl}/llm", form);

        var op = request.SendWebRequest();
        while (!op.isDone)
            await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("[LlmClient] LLM request failed: " + request.responseCode + " " + request.error);
            return null;
        }

        return request.downloadHandler.text;
    }
}
