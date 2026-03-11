using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// IsometricCameraController — A smooth-following isometric camera.
/// 
/// HOW IT WORKS:
/// The camera sits at a fixed angle looking down at the player, like a classic 
/// isometric/strategy game. It smoothly follows the player as they move, and 
/// supports zooming in/out with the scroll wheel.
///
/// SETUP:
/// 1. Create an empty GameObject called "CameraRig"
/// 2. Attach this script to CameraRig
/// 3. Make the Main Camera a CHILD of CameraRig
/// 4. Set Main Camera's Transform to position (0,0,0) and rotation (0,0,0)
///    — this script handles all positioning
/// 5. Set Main Camera to Orthographic projection, size ~8
/// 6. Drag your Player into the "Target" slot in the Inspector
/// 7. Drag the Main Camera into the "Camera Transform" slot
///
/// WHY A CAMERA RIG?
/// Instead of moving the camera directly, we move a parent "rig" object.
/// This makes it easy to rotate the camera angle independently from the 
/// follow behavior, and is a very common pattern in game dev.
/// </summary>
public class IsometricCameraController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The object the camera follows (usually the Player)")]
    public Transform target;

    [Tooltip("The actual camera object (should be a child of this CameraRig)")]
    public Transform cameraTransform;

    [Header("Isometric Angle")]
    [Tooltip("Camera pitch (X rotation) — 30° is classic isometric, 45° is more top-down")]
    [Range(20f, 60f)]
    public float pitch = 35f;

    [Tooltip("Camera yaw (Y rotation) — 45° is standard isometric diamond view")]
    [Range(0f, 360f)]
    public float yaw = 45f;

    [Header("Follow Settings")]
    [Tooltip("How far the camera sits from the target")]
    public float distance = 20f;

    [Tooltip("How smoothly the camera follows (lower = more lag, higher = tighter)")]
    [Range(1f, 20f)]
    public float followSmoothness = 8f;

    [Header("Zoom Settings")]
    [Tooltip("Minimum orthographic size (most zoomed in)")]
    public float minZoom = 4f;

    [Tooltip("Maximum orthographic size (most zoomed out)")]
    public float maxZoom = 15f;

    [Tooltip("How fast scroll wheel zooms")]
    public float zoomSpeed = 2f;

    [Tooltip("How smoothly zoom transitions")]
    public float zoomSmoothness = 8f;

    [Header("Screen Edge Offset (Optional)")]
    [Tooltip("Slight offset so the player isn't dead-center — gives room to see ahead")]
    public Vector3 targetOffset = Vector3.zero;

    // Private state
    private Camera cam;
    private float currentZoom;
    private float targetZoom;

    // ───────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ───────────────────────────────────────────────

    void Start()
    {
        // Validate setup
        if (target == null)
        {
            Debug.LogError("IsometricCamera: No target assigned! Drag your Player into the Target slot.");
            return;
        }

        if (cameraTransform == null)
        {
            // Try to find the camera as a child
            cam = GetComponentInChildren<Camera>();
            if (cam != null)
            {
                cameraTransform = cam.transform;
                Debug.Log("IsometricCamera: Auto-found camera as child object.");
            }
            else
            {
                Debug.LogError("IsometricCamera: No Camera Transform assigned and no Camera found in children!");
                return;
            }
        }
        else
        {
            cam = cameraTransform.GetComponent<Camera>();
        }

        // Initialize zoom
        if (cam != null && cam.orthographic)
        {
            currentZoom = cam.orthographicSize;
            targetZoom = currentZoom;
        }
        else if (cam != null)
        {
            // If perspective camera, we'll adjust distance instead
            currentZoom = distance;
            targetZoom = distance;
        }

        // Set initial camera position
        ApplyCameraAngle();
        SnapToTarget(); // No smoothing on first frame
    }

    void LateUpdate()
    {
        // LateUpdate runs after all Update() calls, which means the player
        // has already moved this frame. This prevents camera jitter.

        if (target == null) return;

        HandleZoomInput();
        SmoothFollowTarget();
        SmoothZoom();
    }

    // ───────────────────────────────────────────────
    // CAMERA POSITIONING
    // ───────────────────────────────────────────────

    /// <summary>
    /// Sets the camera's local position and rotation based on pitch, yaw, and distance.
    /// This defines the "isometric angle" of the view.
    /// </summary>
    void ApplyCameraAngle()
    {
        if (cameraTransform == null) return;

        // Calculate the camera's offset from the rig center using spherical coordinates
        // This is like placing the camera on an invisible arm extending from the rig
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rotation * new Vector3(0f, 0f, -distance);

        cameraTransform.localPosition = offset;
        cameraTransform.localRotation = rotation;
    }

    /// <summary>
    /// Smoothly moves the camera rig toward the target position.
    /// </summary>
    void SmoothFollowTarget()
    {
        Vector3 desiredPosition = target.position + targetOffset;

        // Lerp = Linear Interpolation — smoothly blends between current and target position
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSmoothness * Time.deltaTime
        );
    }

    /// <summary>
    /// Immediately snaps camera to target (used on startup).
    /// </summary>
    void SnapToTarget()
    {
        if (target != null)
        {
            transform.position = target.position + targetOffset;
        }
    }

    // ───────────────────────────────────────────────
    // ZOOM
    // ───────────────────────────────────────────────

    /// <summary>
    /// Reads scroll wheel input to adjust zoom level.
    /// </summary>
    void HandleZoomInput()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        float scrollInput = mouse.scroll.ReadValue().y;

        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            // scroll.y gives larger values than the old system, so scale it down
            targetZoom -= (scrollInput / 120f) * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }
    }

    /// <summary>
    /// Smoothly applies the zoom change to the camera.
    /// </summary>
    void SmoothZoom()
    {
        if (cam == null) return;

        if (cam.orthographic)
        {
            // For orthographic camera, we adjust the "size" property
            currentZoom = Mathf.Lerp(currentZoom, targetZoom, zoomSmoothness * Time.deltaTime);
            cam.orthographicSize = currentZoom;
        }
        else
        {
            // For perspective camera, we adjust the distance
            currentZoom = Mathf.Lerp(currentZoom, targetZoom, zoomSmoothness * Time.deltaTime);
            distance = currentZoom;
            ApplyCameraAngle();
        }
    }

    // ───────────────────────────────────────────────
    // PUBLIC METHODS
    // ───────────────────────────────────────────────

    /// <summary>
    /// Change the camera's target at runtime (e.g., during cutscenes or creature focus).
    /// </summary>
    public void SetTarget(Transform newTarget, bool snapImmediate = false)
    {
        target = newTarget;
        if (snapImmediate)
        {
            SnapToTarget();
        }
    }

    /// <summary>
    /// Smoothly shifts the camera offset (e.g., to look ahead of the player during a chase).
    /// </summary>
    public void SetOffset(Vector3 newOffset)
    {
        targetOffset = newOffset;
    }

    /// <summary>
    /// Returns the current yaw — used by PlayerController to align movement to camera.
    /// </summary>
    public float GetYaw()
    {
        return yaw;
    }

    /// <summary>
    /// Shake the camera briefly (for impacts, captures, etc.).
    /// Call via StartCoroutine(cameraController.Shake(0.3f, 0.2f))
    /// </summary>
    public System.Collections.IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalOffset = targetOffset;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float z = Random.Range(-1f, 1f) * magnitude;

            targetOffset = originalOffset + new Vector3(x, 0f, z);

            elapsed += Time.deltaTime;

            // Reduce magnitude over time for a natural fade-out
            magnitude = Mathf.Lerp(magnitude, 0f, elapsed / duration);

            yield return null; // Wait one frame
        }

        targetOffset = originalOffset;
    }
}
