using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CoralCritter
{
    /// <summary>
    /// CC-001: Basic 3D movement controller with sprint support.
    /// Attach to player root with a CharacterController component.
    /// Supports both legacy and new Unity input systems.
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
            var input = ReadMoveInput();
            input = Vector2.ClampMagnitude(input, 1f);

            var moveDirection = new Vector3(input.x, 0f, input.y);
            var isSprinting = ReadSprintInput();
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

        private static Vector2 ReadMoveInput()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                return ReadKeyboardVector(
                    Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed,
                    Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed,
                    Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed,
                    Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed);
            }
#endif
            return ReadKeyboardVector(
                Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow),
                Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow),
                Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow),
                Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow));
        }

        private static bool ReadSprintInput()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                return Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
            }
#endif
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        private static Vector2 ReadKeyboardVector(bool left, bool right, bool down, bool up)
        {
            var x = 0f;
            var y = 0f;

            if (left) x -= 1f;
            if (right) x += 1f;
            if (down) y -= 1f;
            if (up) y += 1f;

            return new Vector2(x, y);
        }
    }
}
