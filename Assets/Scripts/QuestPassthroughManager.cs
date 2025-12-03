using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Android;
using Uralstech.UXR.QuestCamera;
using UnityEngine.UI;

public class QuestPassthroughManager : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool debugLogging = true;

    [Header("Optional debug UI")]
    [SerializeField] private RawImage debugRawImage;

    private CameraDevice _cameraDevice;
    private CapturePipeline<ContinuousCaptureSession> _sessionPipeline;

    // The current passthrough frame
    public RenderTexture CurrentFrame =>
        _sessionPipeline != null ? _sessionPipeline.TextureConverter.FrameRenderTexture : null;

    void Log(string msg)
    {
        if (debugLogging)
            Debug.Log("[QuestPassthroughManager] " + msg);
    }

    void LogError(string msg) => Debug.LogError("[QuestPassthroughManager] " + msg);

    private void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        StartCoroutine(InitPassthroughCoroutine());
#else
        Log("Skipping passthrough init (not running on Quest device build).");
#endif
    }

    /// <summary>
    /// Async helper to wait until we actually have a valid frame.
    /// </summary>
    public async Task<RenderTexture> GetFrameAsync(int timeoutMs = 1000)
    {
        float start = Time.realtimeSinceStartup;
        while ((CurrentFrame == null || CurrentFrame.width == 0 || CurrentFrame.height == 0) &&
               (Time.realtimeSinceStartup - start) * 1000f < timeoutMs)
        {
            await Task.Yield();
        }
        return CurrentFrame;
    }

    private System.Collections.IEnumerator InitPassthroughCoroutine()
    {
        Log("InitPassthroughCoroutine starting");

        // 1) Device support
        if (!CameraSupport.IsSupported)
        {
            LogError("Device does not support the Passthrough Camera API!");
            yield break;
        }

        // 2) Permission
        if (!Permission.HasUserAuthorizedPermission(UCameraManager.HeadsetCameraPermission))
        {
            Log("Requesting headset camera permission");
            Permission.RequestUserPermission(UCameraManager.HeadsetCameraPermission);
            yield break;
        }

        // 3) Get camera info (left eye)
        CameraInfo currentCamera = UCameraManager.Instance.GetCamera(CameraInfo.CameraEye.Left);
        if (currentCamera == null)
        {
            LogError("No camera available!");
            yield break;
        }

        // 4) Pick highest resolution
        Resolution highestResolution = default;
        foreach (var r in currentCamera.SupportedResolutions)
        {
            if (r.width * r.height > highestResolution.width * highestResolution.height)
                highestResolution = r;
        }
        Log($"Using resolution {highestResolution.width}x{highestResolution.height}");

        // 5) Open camera
        _cameraDevice = UCameraManager.Instance.OpenCamera(currentCamera);
        if (_cameraDevice == null)
        {
            LogError("Could not open camera!");
            yield break;
        }

        Log("Waiting for camera initialization...");
        yield return _cameraDevice.WaitForInitialization();

        if (_cameraDevice.CurrentState != NativeWrapperState.Opened)
        {
            LogError("Camera failed to open. Disposing...");
            yield return _cameraDevice.DisposeAsync().Yield();
            _cameraDevice = null;
            yield break;
        }

        // 6) Create continuous capture session
        _sessionPipeline = _cameraDevice.CreateContinuousCaptureSession(highestResolution);
        if (_sessionPipeline == null)
        {
            LogError("Could not create capture session!");
            yield return _cameraDevice.DisposeAsync().Yield();
            _cameraDevice = null;
            yield break;
        }

        Log("Waiting for capture session initialization...");
        yield return _sessionPipeline.CaptureSession.WaitForInitialization();

        if (_sessionPipeline.CaptureSession.CurrentState != NativeWrapperState.Opened)
        {
            LogError("Capture session failed to open. Disposing...");
            yield return _sessionPipeline.DisposeAsync().Yield();
            yield return _cameraDevice.DisposeAsync().Yield();
            _sessionPipeline = null;
            _cameraDevice = null;
            yield break;
        }

        Log("Passthrough camera is running.");

        // 7) Hook the live frame texture into any debug RawImage (world-space or screen-space)
        if (debugRawImage != null)
        {
            debugRawImage.texture = _sessionPipeline.TextureConverter.FrameRenderTexture;
        }
    }

    private async void OnDestroy()
    {
        if (_sessionPipeline != null || _cameraDevice != null)
        {
            Log("OnDestroy: closing camera and session asynchronously");
            await Task.WhenAll(
                _sessionPipeline != null ? _sessionPipeline.DisposeAsync().AsTask() : Task.CompletedTask,
                _cameraDevice != null ? _cameraDevice.DisposeAsync().AsTask() : Task.CompletedTask
            );
        }
    }
}
