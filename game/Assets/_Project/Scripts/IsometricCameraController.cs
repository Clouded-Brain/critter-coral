using UnityEngine;

namespace CoralCritter
{
    /// <summary>
    /// Locked-angle isometric camera with constrained zoom for early prototype readability.
    /// </summary>
    public class IsometricCameraController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new(0f, 18f, -16f);
        [SerializeField] private float followSmooth = 8f;
        [SerializeField] private float minZoom = 35f;
        [SerializeField] private float maxZoom = 55f;
        [SerializeField] private float zoomSpeed = 5f;

        private Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            transform.rotation = Quaternion.Euler(35f, 45f, 0f);
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            var desired = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desired, followSmooth * Time.deltaTime);

            if (_camera == null)
            {
                return;
            }

            var scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0f)
            {
                _camera.fieldOfView = Mathf.Clamp(_camera.fieldOfView - scroll * zoomSpeed, minZoom, maxZoom);
            }
        }
    }
}
