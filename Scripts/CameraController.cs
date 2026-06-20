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

    void Start()
    {
        currentOrbitDistance = orbitDistance;
        orbitAngles = new Vector2(transform.eulerAngles.y, transform.eulerAngles.x);
        if (target == null)
        {
            Debug.LogWarning("CameraController: Target is not assigned. TopDown and BlenderOrbit modes will use the camera position only.");
        }
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
        }
        else if (Input.GetKeyDown(topDownKey))
        {
            mode = CameraMode.TopDown;
        }
        else if (Input.GetKeyDown(blenderKey))
        {
            mode = CameraMode.BlenderOrbit;
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

        topDownPosition = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10f);
        transform.position = topDownPosition;
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        Vector3 pan = Vector3.zero;
        pan += transform.right * Input.GetAxis("Horizontal");
        pan += transform.forward * Input.GetAxis("Vertical");
        transform.position += pan * topDownPanSpeed * Time.deltaTime;
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
