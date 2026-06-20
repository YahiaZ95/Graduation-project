using UnityEngine;

public class CameraFlags : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera targetCamera;
    public Color solidColor;
    public bool enableSunModeAtStart = true;

    [Header("Sun Settings")]
    public Light sunLight;
    public Material skyboxMaterial;

    private CameraClearFlags originalClearFlags;
    private Color originalBackgroundColor;
    private Material originalSkybox;
    private bool originalSunEnabled;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera != null)
        {
            originalClearFlags = targetCamera.clearFlags;
            originalBackgroundColor = targetCamera.backgroundColor;
        }

        if (skyboxMaterial == null)
            originalSkybox = RenderSettings.skybox;
        else
            originalSkybox = RenderSettings.skybox;

        if (sunLight != null)
            originalSunEnabled = sunLight.enabled;
    }

    private void Start()
    {
        if (enableSunModeAtStart)
            SetSunMode();
        else
            SetSolidColorMode();
    }

    public void SetSunMode()
    {
        if (targetCamera == null)
            return;

        targetCamera.clearFlags = CameraClearFlags.Skybox;
        if (skyboxMaterial != null)
            RenderSettings.skybox = skyboxMaterial;

        if (sunLight != null)
            sunLight.enabled = true;
    }

    public void SetSolidColorMode()
    {
        if (targetCamera == null)
            return;

        targetCamera.clearFlags = CameraClearFlags.SolidColor;
        targetCamera.backgroundColor = solidColor;

        if (sunLight != null)
            sunLight.enabled = false;
    }

    public void ResetCameraMode()
    {
        if (targetCamera == null)
            return;

        targetCamera.clearFlags = originalClearFlags;
        targetCamera.backgroundColor = originalBackgroundColor;
        RenderSettings.skybox = originalSkybox;

        if (sunLight != null)
            sunLight.enabled = originalSunEnabled;
    }
}
