using UnityEngine;

namespace CoralCritter
{
    /// <summary>
    /// CC-001: Basic 3D movement controller with sprint support.
    /// Attach to player root with a CharacterController component.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovementController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float acceleration = 14f;
        [SerializeField] private float rotationSpeed = 14f;

        [Header("Grounding")]
        [SerializeField] private float gravity = -25f;
        [SerializeField] private float groundedStickForce = -2f;

        private CharacterController _controller;
        private Vector3 _currentHorizontalVelocity;
        private float _verticalVelocity;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            input = Vector2.ClampMagnitude(input, 1f);

            var moveDirection = new Vector3(input.x, 0f, input.y);
            var isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            var targetSpeed = isSprinting ? sprintSpeed : walkSpeed;

            var targetHorizontalVelocity = moveDirection * targetSpeed;
            _currentHorizontalVelocity = Vector3.MoveTowards(
                _currentHorizontalVelocity,
                targetHorizontalVelocity,
                acceleration * Time.deltaTime);

            if (_controller.isGrounded)
            {
                _verticalVelocity = groundedStickForce;
            }
            else
            {
                _verticalVelocity += gravity * Time.deltaTime;
            }

            var velocity = _currentHorizontalVelocity + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);

            if (moveDirection.sqrMagnitude > 0.001f)
            {
                var targetRotation = Quaternion.LookRotation(moveDirection.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
}
