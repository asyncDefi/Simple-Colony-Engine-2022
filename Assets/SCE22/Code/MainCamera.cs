using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MainCamera : SingletonMonoBehaviour<MainCamera>
{
    [field: SerializeField] public Camera Camera { get; private set; }

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float edgeScrollBorderThickness = 10f;
    [SerializeField] private Vector2 cameraBoundsMin = new Vector2(-50f, -50f);
    [SerializeField] private Vector2 cameraBoundsMax = new Vector2(50f, 50f);
    [SerializeField] private bool _edgeMove = false;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 100f;
    [SerializeField] private float minZoom = 10f;
    [SerializeField] private float maxZoom = 30f;

    private Vector3 _targetPosition;
    private float _targetZoom;
    private Vector3 _lastMousePosition;

    public bool Frezed;

    private void Awake()
    {
        _targetZoom = transform.position.y;
        _targetPosition = transform.position;
    }


    protected void OnValidate()
    {
        if (Camera == null)
            Camera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (Frezed) return;

        HandleMovement();
        HandleZoom();
        HandleMousePan(); // Call the new mouse pan method

        // Smoothly move the camera to the target position
        transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.fixedDeltaTime * moveSpeed);

        // Clamp camera position within bounds (only X and Z)
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, cameraBoundsMin.x, cameraBoundsMax.x),
            Mathf.Clamp(transform.position.y, minZoom, maxZoom), // Clamp the Y position based on zoom settings
            Mathf.Clamp(transform.position.z, cameraBoundsMin.y, cameraBoundsMax.y)
        );
    }

    private void HandleMovement()
    {
        Vector3 moveDirection = Vector3.zero;

        /*
        // Keyboard movement (WASD/Arrow Keys)
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            moveDirection += Vector3.forward;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            moveDirection += Vector3.back;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            moveDirection += Vector3.left;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            moveDirection += Vector3.right;
        */

        if (_edgeMove)
        {
            // Mouse edge scrolling
            if (Input.mousePosition.y >= Screen.height - edgeScrollBorderThickness)
                moveDirection += Vector3.forward;
            if (Input.mousePosition.y <= edgeScrollBorderThickness)
                moveDirection += Vector3.back;
            if (Input.mousePosition.x >= Screen.width - edgeScrollBorderThickness)
                moveDirection += Vector3.right;
            if (Input.mousePosition.x <= edgeScrollBorderThickness)
                moveDirection += Vector3.left;
        }


        // Apply movement
        if (moveDirection.magnitude > 0)
        {
            // Normalize to prevent faster diagonal movement
            moveDirection.Normalize();
            _targetPosition += moveDirection * moveSpeed * Time.fixedDeltaTime;
        }
    }

    private void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            _targetZoom -= scrollInput * zoomSpeed;
            _targetZoom = Mathf.Clamp(_targetZoom, minZoom, maxZoom);
            _targetPosition.y = _targetZoom;
        }
    }

    private void HandleMousePan()
    {
        if (Input.GetMouseButtonDown(2))
        {
            // When the middle mouse button is first pressed, store the current position
            _lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(2))
        {
            // Calculate the delta mouse position since the last frame
            Vector3 deltaMousePosition = Input.mousePosition - _lastMousePosition;
            _lastMousePosition = Input.mousePosition;

            // Invert the delta to make the camera move in the direction of the mouse
            Vector3 panDirection = new Vector3(-deltaMousePosition.x, 0, -deltaMousePosition.y);

            // Apply the pan movement to the target position
            // The value is scaled by the camera's current Y position to account for zoom level
            _targetPosition += panDirection * (transform.position.y / 100f);
        }
    }


    public Vector3? TryHitGround()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            return hitInfo.point;
        }


        return null;
    }

    /// <summary>
    /// Draws debug gizmos in the editor to visualize camera bounds and zoom levels.
    /// </summary>
    private void OnDrawGizmos()
    {
        // Visualize Movement Bounds
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3(
            (cameraBoundsMin.x + cameraBoundsMax.x) / 2f,
            transform.position.y,
            (cameraBoundsMin.y + cameraBoundsMax.y) / 2f
        );
        Vector3 size = new Vector3(
            cameraBoundsMax.x - cameraBoundsMin.x,
            0.1f,
            cameraBoundsMax.y - cameraBoundsMin.y
        );
        Gizmos.DrawWireCube(center, size);

        // Visualize Zoom Levels (Horizontal Planes)
        Gizmos.color = Color.green;
        Vector3 minZoomCenter = new Vector3(center.x, minZoom, center.z);
        Vector3 planeSize = new Vector3(size.x, 0.1f, size.z);
        Gizmos.DrawWireCube(minZoomCenter, planeSize);

        Gizmos.color = Color.yellow;
        Vector3 maxZoomCenter = new Vector3(center.x, maxZoom, center.z);
        Gizmos.DrawWireCube(maxZoomCenter, planeSize);
    }
}