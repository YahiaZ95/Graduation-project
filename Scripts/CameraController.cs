using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public enum CameraMode
    {
        FreeLook,
        TopDown,
        BlenderOrbit
    }

    [Header("General")]
    public CameraMode mode = CameraMode.FreeLook;
    public Transform target;
    public KeyCode freeLookKey = KeyCode.Alpha1;
    public KeyCode topDownKey = KeyCode.Alpha2;
    public KeyCode blenderKey = KeyCode.Alpha3;

    [Header("Free Look")]
    public float freeMoveSpeed = 10f;
    public float freeBoostSpeed = 30f;
    public float freeRotationSpeed = 3f;
    public float freeVerticalSpeed = 5f;
    public bool freeLookRequiresMouseButton = true;

    [Header("Top Down")]
    public float topDownHeight = 25f;
    public float topDownDistance = 0f;
    public float topDownPanSpeed = 10f;
    public float topDownMousePanSpeed = 1.0f;
    public float topDownZoomSpeed = 10f;
    public float topDownMinOrthoSize = 2f;
    public float topDownMaxOrthoSize = 100f;

    [Header("Blender Orbit")]
    public float orbitDistance = 10f;
    public float orbitMinDistance = 2f;
    public float orbitMaxDistance = 50f;
    public float orbitSpeed = 150f;
    public float orbitZoomSpeed = 10f;
    public float orbitPanSpeed = 0.5f;
    public float orbitMinY = 5f;
    public float orbitMaxY = 85f;

    private float currentOrbitDistance;
    private Vector2 orbitAngles = new Vector2(45f, 45f);
    private Vector3 topDownPosition;
    private Camera cam;
    private Vector3 topDownOffset = Vector3.zero;

    void Start()
    {
        cam = GetComponent<Camera>();
        currentOrbitDistance = orbitDistance;
        orbitAngles = new Vector2(transform.eulerAngles.y, transform.eulerAngles.x);
        if (target == null)
        {
            Debug.LogWarning("CameraController: Target is not assigned. TopDown and BlenderOrbit modes will use the camera position only.");
        }
        ApplyModeSettings(mode);
    }

    void Update()
    {
        HandleModeSwitch();

        switch (mode)
        {
            case CameraMode.FreeLook:
                UpdateFreeLook();
                break;
            case CameraMode.TopDown:
                UpdateTopDown();
                break;
            case CameraMode.BlenderOrbit:
                UpdateBlenderOrbit();
                break;
        }
    }

    private void HandleModeSwitch()
    {
        if (Input.GetKeyDown(freeLookKey))
        {
            mode = CameraMode.FreeLook;
            ApplyModeSettings(mode);
        }
        else if (Input.GetKeyDown(topDownKey))
        {
            mode = CameraMode.TopDown;
            ApplyModeSettings(mode);
        }
        else if (Input.GetKeyDown(blenderKey))
        {
            mode = CameraMode.BlenderOrbit;
            ApplyModeSettings(mode);
        }
    }

    private void ApplyModeSettings(CameraMode newMode)
    {
        if (cam == null) cam = GetComponent<Camera>();
        switch (newMode)
        {
            case CameraMode.TopDown:
                cam.orthographic = true;
                // keep orthographic size proportional to height if not set
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize > 0f ? cam.orthographicSize : topDownHeight * 0.5f, topDownMinOrthoSize, topDownMaxOrthoSize);
                // initialize offset so camera doesn't snap back when switching modes
                {
                    Vector3 basePosition = target != null ? target.position : transform.position;
                    Vector3 desiredPosition = basePosition + Vector3.up * topDownHeight + Vector3.forward * topDownDistance;
                    topDownOffset = transform.position - desiredPosition;
                }
                break;
            default:
                cam.orthographic = false;
                break;
        }
    }

    private void UpdateFreeLook()
    {
        float speed = freeMoveSpeed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            speed = freeBoostSpeed;
        }

        Vector3 move = Vector3.zero;
        move += transform.forward * Input.GetAxis("Vertical");
        move += transform.right * Input.GetAxis("Horizontal");
        move += transform.up * ((Input.GetKey(KeyCode.E) ? 1f : 0f) - (Input.GetKey(KeyCode.Q) ? 1f : 0f));
        transform.position += move * speed * Time.deltaTime;

        if (!freeLookRequiresMouseButton || Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * freeRotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * freeRotationSpeed;
            Vector3 euler = transform.eulerAngles;
            euler.x -= mouseY;
            euler.y += mouseX;
            transform.eulerAngles = euler;
        }
    }

    private void UpdateTopDown()
    {
        Vector3 basePosition = target != null ? target.position : transform.position;
        Vector3 desiredPosition = basePosition + Vector3.up * topDownHeight + Vector3.forward * topDownDistance;

        // Apply persistent offset to keep camera where user moved it
        Vector3 targetPosition = desiredPosition + topDownOffset;
        // snap immediately to avoid returning to original place
        transform.position = targetPosition;
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // Keyboard pan (world X/Z)
        // Keyboard pan adjusts the persistent offset (world X/Z)
        Vector3 pan = Vector3.zero;
        pan += Vector3.right * Input.GetAxis("Horizontal");
        pan += Vector3.forward * Input.GetAxis("Vertical");
        topDownOffset += pan * topDownPanSpeed * Time.deltaTime;

        // Mouse wheel zoom (orthographic size) or adjust height in perspective
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            if (cam != null && cam.orthographic)
            {
                cam.orthographicSize -= scroll * topDownZoomSpeed;
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, topDownMinOrthoSize, topDownMaxOrthoSize);
            }
            else
            {
                topDownHeight -= scroll * topDownZoomSpeed;
                topDownHeight = Mathf.Max(1f, topDownHeight);
            }
        }

        // Mouse-drag panning (middle mouse button)
        if (Input.GetMouseButton(2))
        {
            float mx = Input.GetAxis("Mouse X");
            float my = Input.GetAxis("Mouse Y");
            // invert to match drag direction
            Vector3 mousePan = new Vector3(-mx, 0f, -my);
            float factor = topDownMousePanSpeed * (cam != null && cam.orthographic ? cam.orthographicSize * 0.1f : 1f);
            topDownOffset += mousePan * factor;
        }
    }

    private void UpdateBlenderOrbit()
    {
        Vector3 orbitTarget = target != null ? target.position : transform.position + transform.forward * orbitDistance;

        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * orbitSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * orbitSpeed * Time.deltaTime;
            orbitAngles.x += mouseX;
            orbitAngles.y -= mouseY;
            orbitAngles.y = Mathf.Clamp(orbitAngles.y, orbitMinY, orbitMaxY);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            currentOrbitDistance -= scroll * orbitZoomSpeed;
            currentOrbitDistance = Mathf.Clamp(currentOrbitDistance, orbitMinDistance, orbitMaxDistance);
        }

        if (Input.GetMouseButton(2))
        {
            Vector3 pan = new Vector3(-Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"), 0f);
            orbitTarget += transform.TransformDirection(pan) * orbitPanSpeed;
        }

        Quaternion rotation = Quaternion.Euler(orbitAngles.y, orbitAngles.x, 0f);
        Vector3 direction = rotation * Vector3.back;
        transform.position = orbitTarget + direction * currentOrbitDistance;
        transform.rotation = rotation;
    }
}
