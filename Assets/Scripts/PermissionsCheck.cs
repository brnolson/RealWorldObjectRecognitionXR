#if UNITY_ANDROID
using UnityEngine.Android;
#endif
using UnityEngine;
using UnityEngine.Events;

public class PermissionsCheck : MonoBehaviour
{
    // Meta Scene permission for Space Setup data:
    private const string PermissionId = "com.oculus.permission.USE_SCENE";

    [Header("Events")]
    [Tooltip("Invoked if the user denies the permission")]
    public UnityEvent<string> OnPermissionDenied;

    [Tooltip("Invoked once the permission is granted")]
    public UnityEvent<string> OnPermissionGranted;

#if UNITY_ANDROID
    void Start()
    {
        if (Permission.HasUserAuthorizedPermission(PermissionId))
        {
            HandleGranted(PermissionId);
        }
        else
        {
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionDenied += HandleDenied;
            callbacks.PermissionGranted += HandleGranted;

            Debug.Log($"Requesting permission: {PermissionId}");
            Permission.RequestUserPermission(PermissionId, callbacks);
        }
    }

    void HandleDenied(string p)
    {
        Debug.LogWarning($"User denied permission: {p}");
        OnPermissionDenied?.Invoke(p);
    }

    void HandleGranted(string p)
    {
        Debug.Log($"User granted permission: {p}");
        OnPermissionGranted?.Invoke(p);
    }
#endif
}
