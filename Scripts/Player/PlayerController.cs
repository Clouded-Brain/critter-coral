using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// PlayerController — Isometric movement with jump, slope climbing, and custom gravity.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 12f;
    public float rotationSpeed = 10f;
    public float acceleration = 60f;
    public float sprintMultiplier = 1.8f;

    [Header("Isometric")]
    public float isoAngle = 45f;

    [Header("Jumping")]
    [Tooltip("Initial jump force (tap spacebar)")]
    public float jumpForce = 10f;
    [Tooltip("Extra upward force while holding spacebar")]
    public float holdJumpForce = 18f;
    [Tooltip("Max time you can hold jump for extra height (seconds)")]
    public float maxJumpHoldTime = 0.25f;

    [Tooltip("How long jump input is remembered before landing")]
    public float jumpBufferTime = 0.15f;

    [Tooltip("How long after leaving ground you can still jump")]
    public float coyoteTime = 0.12f;

    [Header("Gravity")]
    [Tooltip("Gravity multiplier while rising with jump held")]
    public float gravityMultiplier = 3f;
    [Tooltip("Extra gravity when falling — makes jumps feel snappy")]
    public float fallMultiplier = 4f;

    [Header("Slope Handling")]
    [Tooltip("Max slope angle the player can walk up (degrees)")]
    public float maxSlopeAngle = 55f;
    [Tooltip("Extra push force applied when going uphill")]
    public float slopeAssistForce = 15f;

    [Header("Ground Check")]
    public float groundCheckRadius = 0.4f;
    public float groundCheckDistance = 0.3f;
    public LayerMask groundLayer = ~0;

    // Private state
    private Rigidbody rb;
    private Vector3 moveDirection;
    private bool isSprinting;
    private bool isGrounded;
    private bool jumpHeld;
    private float jumpHoldTimer;
    private float jumpBufferTimer;
    private float coyoteTimer;
    private Vector3 groundNormal = Vector3.up;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("PlayerController: No Rigidbody found!");
            return;
        }
        rb.useGravity = true;
        Debug.Log("🐚 PlayerController: WASD=Move, Shift=Sprint, Space=Jump (hold for higher)");
    }

    void Update()
    {
        ReadInput();
    }

    void FixedUpdate()
    {
        CheckGround();
        ApplyMovement();
        ApplySlope();
        ApplyJump();
        ApplyCustomGravity();
        ApplyRotation();
    }

    // ─── INPUT ───────────────────────────────────

    void ReadInput()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) { moveDirection = Vector3.zero; return; }

        float h = 0f, v = 0f;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    v += 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed)   v -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed)  h += 1f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)   h -= 1f;

        moveDirection = Quaternion.Euler(0f, isoAngle, 0f) * new Vector3(h, 0f, v).normalized;
        isSprinting = kb.leftShiftKey.isPressed;

        if (kb.spaceKey.wasPressedThisFrame)
        {
            jumpBufferTimer = jumpBufferTime;
        }
        jumpHeld = kb.spaceKey.isPressed;
    }

    // ─── GROUND CHECK ────────────────────────────

    void CheckGround()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        if (Physics.SphereCast(origin, groundCheckRadius, Vector3.down,
            out RaycastHit hit, groundCheckDistance + 0.1f, groundLayer))
        {
            isGrounded = true;
            coyoteTimer = coyoteTime;
            groundNormal = hit.normal;
        }
        else
        {
            isGrounded = false;
            coyoteTimer -= Time.fixedDeltaTime;
            groundNormal = Vector3.up;
        }
    }

    // ─── MOVEMENT ────────────────────────────────

    void ApplyMovement()
    {
        if (moveDirection.magnitude > 0.1f)
        {
            float speed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);

            // Project move direction onto slope so we glide along hills
            Vector3 dir = isGrounded
                ? Vector3.ProjectOnPlane(moveDirection, groundNormal).normalized
                : moveDirection;

            Vector3 target = dir * speed;
            target.y = rb.linearVelocity.y;

            Vector3 delta = target - rb.linearVelocity;
            delta.y = 0;
            delta = Vector3.ClampMagnitude(delta, acceleration * Time.fixedDeltaTime);
            rb.linearVelocity += delta;
        }
        else
        {
            Vector3 v = rb.linearVelocity;
            v.x = Mathf.Lerp(v.x, 0f, 12f * Time.fixedDeltaTime);
            v.z = Mathf.Lerp(v.z, 0f, 12f * Time.fixedDeltaTime);
            rb.linearVelocity = v;
        }
    }

    // ─── SLOPE ASSIST ────────────────────────────

    void ApplySlope()
    {
        if (!isGrounded || moveDirection.magnitude < 0.1f) return;

        float angle = Vector3.Angle(groundNormal, Vector3.up);
        if (angle > 2f && angle <= maxSlopeAngle)
        {
            // Check if going uphill
            Vector3 slopeDown = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;
            float uphill = Vector3.Dot(moveDirection.normalized, -slopeDown);

            if (uphill > 0.1f)
            {
                float assist = slopeAssistForce * uphill * (angle / maxSlopeAngle);
                rb.AddForce(Vector3.up * assist, ForceMode.Acceleration);
            }
        }

        // Stick to ground going downhill (prevents bouncing off terrain)
        if (angle > 2f && rb.linearVelocity.y > -0.1f && rb.linearVelocity.y < 1f)
        {
            rb.AddForce(-groundNormal * 5f, ForceMode.Acceleration);
        }
    }

    // ─── JUMP (variable height) ──────────────────

    void ApplyJump()
    {
        jumpBufferTimer -= Time.fixedDeltaTime;

        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            jumpHoldTimer = 0f;

            Vector3 v = rb.linearVelocity;
            v.y = jumpForce;
            rb.linearVelocity = v;
        }

        // Hold spacebar = keep pushing up for a bit longer
        if (jumpHeld && !isGrounded && jumpHoldTimer < maxJumpHoldTime && rb.linearVelocity.y > 0)
        {
            rb.AddForce(Vector3.up * holdJumpForce, ForceMode.Acceleration);
            jumpHoldTimer += Time.fixedDeltaTime;
        }
    }

    // ─── CUSTOM GRAVITY (fast falling) ───────────

    void ApplyCustomGravity()
    {
        if (isGrounded) return;

        float extra;
        if (rb.linearVelocity.y < 0)
            extra = Physics.gravity.y * (fallMultiplier - 1f);      // Falling — heavy
        else if (!jumpHeld)
            extra = Physics.gravity.y * (fallMultiplier - 1f);      // Rising, released jump — cut short
        else
            extra = Physics.gravity.y * (gravityMultiplier - 1f);   // Rising, holding jump — moderate

        rb.AddForce(new Vector3(0f, extra, 0f), ForceMode.Acceleration);
    }

    // ─── ROTATION ────────────────────────────────

    void ApplyRotation()
    {
        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion target = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    // ─── PUBLIC QUERIES ──────────────────────────

    public bool IsMoving() => moveDirection.magnitude > 0.1f;
    public bool IsSprinting() => isSprinting && IsMoving();
    public bool IsGrounded() => isGrounded;
    public bool IsJumping() => !isGrounded && rb.linearVelocity.y > 0.5f;
    public bool IsFalling() => !isGrounded && rb.linearVelocity.y < -0.5f;
    public float GetCurrentSpeed() => new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;

    // ─── DEBUG GIZMOS ────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 pos = transform.position + Vector3.up * 0.1f + Vector3.down * groundCheckDistance;
        Gizmos.DrawWireSphere(pos, groundCheckRadius);
    }
}
