using UnityEngine;

public class AvatarToggle : MonoBehaviour
{
    [Header("References")]
    public AvatarProcessing avatar;

    [Header("Palm Detection")]
    [Range(-1f, 1f)]
    public float palmDotThreshold = 0.8f;       // how upright the palm must be
    public float minPalmTime = 0.15f;           // how long the palm must stay valid

    [Header("Debug")]
    public bool debugLogging = true;

    float timer;
    bool lastPalmUp = false;

    void Log(string msg)
    {
        if (debugLogging)
            Debug.Log("[AvatarToggle] " + msg);
    }

    // Public so others can query palm pose
    public bool IsPalmup()
    {
        float dot = Vector3.Dot(transform.up, Vector3.up);
        return dot >= palmDotThreshold;
    }

    void Update()
    {
        if (avatar == null)
        {
            Log("No Avatar reference assigned");
            return;
        }

        float dot = Vector3.Dot(transform.up, Vector3.up);
        bool palmUp = dot >= palmDotThreshold;

        // Log only when this value changes, not every frame
        if (palmUp != lastPalmUp)
        {
            Log("PalmUp changed: " + palmUp + " (dot=" + dot.ToString("F2") + ")");
            lastPalmUp = palmUp;
        }

        if (palmUp)
        {
            timer += Time.deltaTime;
            if (timer >= minPalmTime)
            {
                if (!avatar.gameObject.activeSelf)
                {
                    Log("PalmUp sustained -> Avatar IDLE");
                    avatar.SetState(AvatarState.Idle);
                }
            }
        }
        else
        {
            timer = 0f;

            // Do not hide if we're mid-task
            if (avatar.currentState == AvatarState.Idle)
            {
                Log("Palm lowered -> Avatar HIDDEN");
                avatar.SetState(AvatarState.Hidden);
            }
        }
    }
}
