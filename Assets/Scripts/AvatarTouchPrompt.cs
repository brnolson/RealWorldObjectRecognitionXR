using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AvatarTouchPrompt : MonoBehaviour
{
    [Header("References")]
    public VoiceCapture voiceCapture;
    public AvatarProcessing avatar;

    [Header("Detection")]
    [Tooltip("Only colliders with this tag will trigger the prompt")]
    public string handFingerTag = "HandFinger";

    [Header("Debug")]
    public bool debugLogging = true;

    bool listening = false;
    bool fingerInside = false;

    void Log(string msg)
    {
        if (debugLogging)
            Debug.Log("[AvatarTouchPrompt] " + msg);
    }

    void LogWarning(string msg)
    {
        if (debugLogging)
            Debug.LogWarning("[AvatarTouchPrompt] " + msg);
    }

    void OnTriggerEnter(Collider other)
    {
        Log("OnTriggerEnter by " + other.gameObject.name + " (tag=" + other.tag + ")");

        if (!other.CompareTag(handFingerTag))
        {
            Log("Ignoring collider; tag mismatch (expected '" + handFingerTag + "')");
            return;
        }

        if (fingerInside)
        {
            Log("Finger already inside, ignoring extra OnTriggerEnter");
            return;
        }
        fingerInside = true;

        if (voiceCapture == null || avatar == null)
        {
            LogWarning("Missing reference(s): " +
                       (voiceCapture == null ? "VoiceCapture " : "") +
                       (avatar == null ? "AvatarProcessing" : ""));
            return;
        }

        if (!listening)
        {
            listening = true;
            Log("Touch -> ENTER LISTENING state");
            avatar.SetState(AvatarState.Listening);
            voiceCapture.StartListening();
        }
        else
        {
            listening = false;
            Log("Touch -> ENTER PROCESSING state");
            avatar.SetState(AvatarState.Processing);
            _ = voiceCapture.StopListeningAndCaptureAsync();
        }
    }

    void OnTriggerExit(Collider other)
    {
        Log("OnTriggerExit by " + other.gameObject.name + " (tag=" + other.tag + ")");

        if (!other.CompareTag(handFingerTag))
            return;

        fingerInside = false;
    }
}
