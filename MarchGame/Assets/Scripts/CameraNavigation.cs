using Unity.Cinemachine;
using UnityEngine;

public class CameraNavigation : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float panSpeed = 20f;
    [SerializeField] private float rightClickPanSpeed = 25f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 15f;
    [SerializeField] private float smoothing = 5f;

    private Vector3 targetPosition;
    private float targetZoom;
    private Vector3 lastMousePosition;
    private CinemachineCamera mainCamera;
    private bool isRightClickHeld = false;
    public bool menuOpen = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = GetComponent<CinemachineCamera>();
        targetPosition = transform.position;
        targetZoom = mainCamera.Lens.OrthographicSize;
        panSpeed = PlayerPrefs.GetFloat("PanSpeed", panSpeed);
    }
    public void UpdatePanSpeed()
    {
        panSpeed = PlayerPrefs.GetFloat("PanSpeed", panSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        if(menuOpen)
        {
            return;
        }
        HandleKeyboardInput();
        HandleMouseInput();
        HandleZoom();
        SmoothCameraMovement();
    }

    private void HandleKeyboardInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Calculate movement vector based on keyboard input
        Vector3 movement = new Vector3(horizontal, vertical, 0) * panSpeed * Time.unscaledDeltaTime;
        
        targetPosition += movement;
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(2))
        {
            isRightClickHeld = true;
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(2))
        {
            isRightClickHeld = false;
        }

        if (isRightClickHeld)
        {
            Vector3 currentMousePosition = Input.mousePosition;
            Vector3 mouseDelta = currentMousePosition - lastMousePosition;
            
            Vector3 movement = new Vector3(-mouseDelta.x, -mouseDelta.y, 0) * rightClickPanSpeed * Time.unscaledDeltaTime;
            targetPosition += movement;
            
            lastMousePosition = currentMousePosition;
        }
    }

    private void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        
        targetZoom -= scrollInput * zoomSpeed;
        
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
    }

    private void SmoothCameraMovement()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.unscaledDeltaTime * smoothing);
        
        mainCamera.Lens.OrthographicSize = Mathf.Lerp(mainCamera.Lens.OrthographicSize, targetZoom, Time.unscaledDeltaTime * smoothing);
    }
}