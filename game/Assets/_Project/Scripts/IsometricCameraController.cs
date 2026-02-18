using UnityEngine;

namespace CoralCritter
{
    /// <summary>
    /// CC-002: Locked-angle isometric camera with constrained zoom and optional occlusion support.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class IsometricCameraController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private bool autoFindPlayerTarget = true;
        [SerializeField] private Vector3 offset = new(0f, 18f, -16f);
        [SerializeField] private float followSmooth = 8f;
        [SerializeField] private float minZoom = 35f;
        [SerializeField] private float maxZoom = 55f;
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private Vector3 eulerViewAngle = new(35f, 45f, 0f);

        private Camera _camera;

        public Transform Target => target;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            transform.rotation = Quaternion.Euler(eulerViewAngle);
            TryAutoBindTarget();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                TryAutoBindTarget();
                if (target == null)
                {
                    return;
                }
            }

            var desiredPosition = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, followSmooth * Time.deltaTime);

            var scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > Mathf.Epsilon)
            {
                _camera.fieldOfView = Mathf.Clamp(_camera.fieldOfView - scroll * zoomSpeed, minZoom, maxZoom);
            }
        }

        public void SetTarget(Transform nextTarget)
        {
            target = nextTarget;
        }

        private void TryAutoBindTarget()
        {
            if (!autoFindPlayerTarget || target != null)
            {
                return;
            }

            var playerByName = GameObject.Find("Player");
            if (playerByName != null)
            {
                target = playerByName.transform;
                return;
            }

            var playerByTag = GameObject.FindGameObjectWithTag("Player");
            if (playerByTag != null)
            {
                target = playerByTag.transform;
            }
        }
    }
}
