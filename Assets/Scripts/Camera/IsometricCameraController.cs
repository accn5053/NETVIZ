using UnityEngine;
using SimpleFileBrowser;

public class IsometricCameraController : MonoBehaviour
{
    [Header("Zoom")]
    public float zoomSpeed = 5f;
    public float minZoom = 5f;
    public float maxZoom = 40f;

    [Header("Pan")]
    public float panSpeed = 0.3f;

    // Fixed isometric angle — never changes
    private const float PITCH = 45f;
    private const float YAW = 45f;

    // Internal state
    private float _currentZoom = 22f;
    private Vector3 _targetPoint = Vector3.zero;  // world point camera orbits around
    private Vector3 _lastMousePos;
    private bool _isPanning;


    void Start()
    {
        transform.rotation = Quaternion.Euler(PITCH, YAW, 0f);
        SnapToTarget();
    }

    void Update()
    {
        if (SimpleFileBrowser.FileBrowser.IsOpen) return;
        HandleZoom();
        HandlePan();
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) < 0.001f) return;

        _currentZoom -= scroll * zoomSpeed * _currentZoom;
        _currentZoom = Mathf.Clamp(_currentZoom, minZoom, maxZoom);
        SnapToTarget();
    }

    private void HandlePan()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PanelResizer resizer = FindAnyObjectByType<PanelResizer>();
            if (resizer != null && resizer.IsDragging()) return;

            _lastMousePos = Input.mousePosition;
            _isPanning = true;
        }
        if (Input.GetMouseButtonUp(0))
            _isPanning = false;

        if (!_isPanning) return;

        PanelResizer resizerCheck = FindAnyObjectByType<PanelResizer>();
        if (resizerCheck != null && resizerCheck.IsDragging()) return;

        Vector3 delta = Input.mousePosition - _lastMousePos;
        _lastMousePos = Input.mousePosition;

        float unitsPerPixel = (_currentZoom * 2f) / Screen.height;

        Vector3 right = transform.right;
        Vector3 forward = Vector3.Cross(right, Vector3.up).normalized;

        _targetPoint += right * (-delta.x * unitsPerPixel);
        _targetPoint += forward * (-delta.y * unitsPerPixel);

        SnapToTarget();
    }

    // Positions camera behind and above _targetPoint at the fixed angle
    private void SnapToTarget()
    {
        // Back-calculate camera position from target + zoom + fixed rotation
        Vector3 direction = transform.rotation * Vector3.back;
        transform.position = _targetPoint + direction * _currentZoom;
    }
}