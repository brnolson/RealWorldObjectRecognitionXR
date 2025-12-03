using UnityEngine;
using UnityEngine.UI;

public enum AvatarState
{
    Hidden,
    Idle,
    Listening,
    Processing,
    Success,
    Error
}

public class AvatarProcessing : MonoBehaviour
{
    [Header("Visual Target")]
    public Renderer avatarRenderer;
    public Image uiImage;

    [Header("Colors")]
    public Color idleColor = Color.white;
    public Color listeningColor = Color.cyan;
    public Color processingColor = Color.yellow;
    public Color successColor = Color.green;
    public Color errorColor = Color.red;

    public AvatarState currentState = AvatarState.Hidden;

    void Awake()
    {
        ApplyState();
    }

    public void SetState(AvatarState state)
    {
        currentState = state;
        ApplyState();
    }

    void ApplyState()
    {
        bool visible = currentState != AvatarState.Hidden;
        gameObject.SetActive(visible);

        Color c = idleColor;
        switch (currentState)
        {
            case AvatarState.Idle:       c = idleColor;       break;
            case AvatarState.Listening:  c = listeningColor;  break;
            case AvatarState.Processing: c = processingColor; break;
            case AvatarState.Success:    c = successColor;    break;
            case AvatarState.Error:      c = errorColor;      break;
        }

        if (avatarRenderer != null)
        {
#if UNITY_EDITOR
            // In edit/build time, do NOT instantiate materials
            if (!Application.isPlaying)
                avatarRenderer.sharedMaterial.color = c;
            else
                avatarRenderer.material.color = c;
#else
            avatarRenderer.material.color = c;
#endif
        }

        if (uiImage != null)
            uiImage.color = c;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        ApplyState();
    }
#endif
}
