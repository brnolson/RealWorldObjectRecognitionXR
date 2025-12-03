using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Android;
using Uralstech.UXR.QuestCamera;

public class VoiceCapture : MonoBehaviour
{
    [Header("References")]
    public AvatarToggle palm;
    public AvatarProcessing avatar;
    public QuestPassthroughManager QuestPassthroughManagerInstance;
    public AvatarBillboard billboard;

    [Header("Recording")]
    private const string MicPermission = Permission.Microphone;
    public AudioSource audioSource;
    public string microphoneDevice = null;
    public int sampleRate = 48000;

    [Header("Debug")]
    public bool debugLogging = true;

    private bool isListening = false;
    private AudioClip recordingClip;
    private bool lastPalmUp = false;

    void Log(string msg)
    {
        if (debugLogging)
            Debug.Log("[VoiceCapture] " + msg);
    }

    void LogWarning(string msg)
    {
        if (debugLogging)
            Debug.LogWarning("[VoiceCapture] " + msg);
    }

    void LogError(string msg)
    {
        Debug.LogError("[VoiceCapture] " + msg);
    }

    void Update()
    {
        if (palm == null)
        {
            LogWarning("No palm reference");
            return;
        }

        bool palmUp = palm.IsPalmup();
        if (palmUp != lastPalmUp)
        {
            Log("PalmUp changed: " + palmUp);
            lastPalmUp = palmUp;
        }

        if (!palmUp)
            return;
    }

    // Called by AvatarTouchPrompt
    public void StartListening()
    {
        Log("StartListening() called");
        billboard?.SetStatus("Listening...");
        StartCoroutine(StartListeningCoroutine());
    }

    private System.Collections.IEnumerator StartListeningCoroutine()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(MicPermission))
        {
            Log("Requesting microphone permission");
            Permission.RequestUserPermission(MicPermission);

            float timeout = 10f;
            float t = 0f;
            while (t < timeout &&
                   !Permission.HasUserAuthorizedPermission(MicPermission))
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            if (!Permission.HasUserAuthorizedPermission(MicPermission))
            {
                LogWarning("Microphone permission not granted after request");
                billboard?.SetStatus("Mic blocked");
                yield break;
            }

            Log("Microphone permission granted");
        }
#endif
        if (isListening)
        {
            Log("StartListening called but already listening");
            yield break;
        }

        if (recordingClip != null)
        {
            Microphone.End(null);
            recordingClip = null;
        }

        isListening = true;
        Log("Starting microphone recording (device=" + (microphoneDevice ?? "null") + ")");

        avatar?.SetState(AvatarState.Listening);
        billboard?.SetStatus("Listening...");

        recordingClip = Microphone.Start(null, false, 30, sampleRate);
        audioSource.clip = recordingClip;
        Log("Started recording");
    }

    public async Task StopListeningAndCaptureAsync()
    {
        Log("StopListeningAndCaptureAsync()");

        if (!isListening && recordingClip == null)
            LogWarning("StopListening called without active recording");

        isListening = false;

        if (Microphone.IsRecording(null))
        {
            Microphone.End(null);
            Log("Microphone ended");
        }

        // Small delay to allow buffer flush (helps avoid zero-byte audio)
        await Task.Delay(200);

        avatar?.SetState(AvatarState.Processing);
        billboard?.SetStatus("Processing...");

        try
        {
            // 1) Audio
            byte[] audioBytes = null;

            if (recordingClip != null && recordingClip.samples > 0)
            {
                Log("Converting AudioClip to WAV bytes");
                audioBytes = AudioClipToWavBytes(recordingClip);
                Log("Audio bytes length: " + (audioBytes?.Length ?? 0));
            }
            else
            {
                LogWarning("No valid recordingClip, skipping audio conversion");
            }

            // 2) Passthrough frame
            Log("Waiting for passthrough frame");
            RenderTexture rt = null;
            if (QuestPassthroughManagerInstance != null)
            {
                rt = await QuestPassthroughManagerInstance.GetFrameAsync();
            }

            if (rt == null)
            {
                LogError("No passthrough frame available");
                avatar?.SetState(AvatarState.Error);
                billboard?.SetStatus("No camera frame");
                return;
            }

            Log("Encoding frame to PNG");
            byte[] imageBytes = TextureUtils.RenderTextureToPNG(rt);
            Log("Image bytes length: " + (imageBytes?.Length ?? 0));

            // 3) STT first: /transcribe
            string transcript = await LlmClient.TranscribeOnly(audioBytes);
            Log("Transcript: " + transcript);

            if (!string.IsNullOrWhiteSpace(transcript))
            {
                billboard?.SetUserSpeech(transcript);
            }
            else
            {
                billboard?.SetStatus("Could not transcribe");
            }

            // 4) Then multimodal /llm
            Log("Sending to backend /llm");
            bool ok = await SendToBackendAsync(audioBytes, imageBytes);
            Log("Backend ok=" + ok);

            avatar?.SetState(ok ? AvatarState.Success : AvatarState.Error);
        }
        catch (Exception ex)
        {
            LogError("Exception in StopListeningAndCaptureAsync: " + ex);
            avatar?.SetState(AvatarState.Error);
            billboard?.SetStatus("Error");
        }
        finally
        {
            recordingClip = null;
        }

        Log("Waiting 1.5s before resetting state");
        await Task.Delay(1500);

        bool palmUp = palm != null && palm.IsPalmup();
        Log("After delay, palmUp=" + palmUp);

        avatar?.SetState(palmUp ? AvatarState.Idle : AvatarState.Hidden);
        Log("Final avatar state=" + avatar?.currentState);
    }

    byte[] AudioClipToWavBytes(AudioClip clip)
    {
        if (clip == null)
        {
            LogWarning("AudioClipToWavBytes: clip is null");
            return null;
        }

        int sampleCount = clip.samples * clip.channels;
        float[] samples = new float[sampleCount];
        clip.GetData(samples, 0);

        byte[] pcmBytes = new byte[sampleCount * 2];
        const float scale = 32767f;
        for (int i = 0; i < sampleCount; i++)
        {
            short s = (short)(Mathf.Clamp(samples[i], -1f, 1f) * scale);
            pcmBytes[i * 2] = (byte)(s & 0xFF);
            pcmBytes[i * 2 + 1] = (byte)((s >> 8) & 0xFF);
        }

        int sampleRate = clip.frequency;
        short channels = (short)clip.channels;
        int byteRate = sampleRate * channels * 2;
        int subchunk2Size = pcmBytes.Length;
        int chunkSize = 36 + subchunk2Size;

        byte[] wav = new byte[44 + pcmBytes.Length];

        System.Text.Encoding.ASCII.GetBytes("RIFF").CopyTo(wav, 0);
        BitConverter.GetBytes(chunkSize).CopyTo(wav, 4);
        System.Text.Encoding.ASCII.GetBytes("WAVE").CopyTo(wav, 8);

        System.Text.Encoding.ASCII.GetBytes("fmt ").CopyTo(wav, 12);
        BitConverter.GetBytes(16).CopyTo(wav, 16);
        BitConverter.GetBytes((short)1).CopyTo(wav, 20);
        BitConverter.GetBytes(channels).CopyTo(wav, 22);
        BitConverter.GetBytes(sampleRate).CopyTo(wav, 24);
        BitConverter.GetBytes(byteRate).CopyTo(wav, 28);
        BitConverter.GetBytes((short)(channels * 2)).CopyTo(wav, 32);
        BitConverter.GetBytes((short)16).CopyTo(wav, 34);

        System.Text.Encoding.ASCII.GetBytes("data").CopyTo(wav, 36);
        BitConverter.GetBytes(subchunk2Size).CopyTo(wav, 40);

        Buffer.BlockCopy(pcmBytes, 0, wav, 44, pcmBytes.Length);

        Log($"AudioClipToWavBytes: samples={sampleCount}, channels={channels}, sampleRate={sampleRate}, totalBytes={wav.Length}");
        return wav;
    }

    async Task<bool> SendToBackendAsync(byte[] audio, byte[] image)
    {
        Log("SendToBackendAsync: audio=" + (audio?.Length ?? 0) + " bytes, image=" + (image?.Length ?? 0) + " bytes");

        string json = await LlmClient.SendAsync(audio, image);
        Log("Backend raw JSON: " + json);

        if (string.IsNullOrEmpty(json))
        {
            LogWarning("Backend response empty");
            billboard?.SetStatus("AI error");
            return false;
        }

        AssistantResponse response = null;
        try
        {
            response = JsonUtility.FromJson<AssistantResponse>(json);
            Log("Parsed reply: " + response.reply);
        }
        catch (Exception ex)
        {
            LogWarning("Failed to parse assistant JSON: " + ex);
            billboard?.SetStatus("Bad AI response");
            return false;
        }

        if (response != null && billboard != null)
        {
            if (!string.IsNullOrWhiteSpace(response.transcript))
                billboard.SetUserSpeech(response.transcript);

            if (!string.IsNullOrWhiteSpace(response.reply))
                billboard.SetAiReply(response.reply);
        }

        bool valid = response != null && !string.IsNullOrEmpty(response.reply);
        Log("SendToBackendAsync complete, valid=" + valid);
        return valid;
    }
}
