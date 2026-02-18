using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// PlayerController — Isometric movement with responsive jumping and slope traversal.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 21f;
    public float rotationSpeed = 10f;
    public float acceleration = 95f;
    public float sprintMultiplier = 1.25f;

    [Header("Isometric")]
    public float isoAngle = 45f;

    [Header("Jumping")]
    public float jumpForce = 11.5f;
    public float holdJumpForce = 16f;
    public float maxJumpHoldTime = 0.2f;
    public float jumpBufferTime = 0.2f;
    public float coyoteTime = 0.16f;

    [Header("Gravity")]
    public float gravityMultiplier = 2.8f;
    public float fallMultiplier = 4.5f;

    [Header("Slope Handling")]
    public float maxSlopeAngle = 62f;
    public float slopeAssistForce = 42f;
    [Range(0.4f, 1f)] public float uphillSpeedRetention = 0.98f;
    public float uphillForwardAssist = 30f;

    [Header("Ground Check")]
    public float groundCheckRadius = 0.38f;
    public float groundCheckDistance = 0.5f;
    public LayerMask groundLayer = ~0;

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private Collider[] ownColliders;
    private Vector3 moveDirection;
    private Vector3 groundNormal = Vector3.up;
    private bool isSprinting;
    private bool isGrounded;
    private bool jumpHeld;
    private float jumpHoldTimer;
    private float jumpBufferTimer;
    private float coyoteTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        ownColliders = GetComponentsInChildren<Collider>();
    }

    void Start()
    {
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.drag = 0f;
        rb.angularDrag = 0.05f;
        rb.constraints &= ~RigidbodyConstraints.FreezePositionY;
        Debug.Log("🐚 PlayerController ready: WASD move, Shift sprint, Space jump");
    }

    void Update() => ReadInput();

    void FixedUpdate()
    {
        CheckGround();
        ApplyMovement();
        ApplySlope();
        ApplyJump();
        ApplyCustomGravity();
        ApplyRotation();

        jumpBufferTimer = Mathf.Max(0f, jumpBufferTimer - Time.fixedDeltaTime);
        coyoteTimer = Mathf.Max(0f, coyoteTimer - Time.fixedDeltaTime);
    }

    void ReadInput()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null)
        {
            moveDirection = Vector3.zero;
            return;
        }

        float h = 0f, v = 0f;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v += 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h += 1f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) h -= 1f;

        Vector3 rawMove = new Vector3(h, 0f, v);
        moveDirection = rawMove.sqrMagnitude > 0.01f
            ? (Quaternion.Euler(0f, isoAngle, 0f) * rawMove.normalized)
            : Vector3.zero;

        isSprinting = kb.leftShiftKey.isPressed;

        bool spacePressed = kb.spaceKey.wasPressedThisFrame;
        if (spacePressed)
        {
            jumpBufferTimer = jumpBufferTime;
        }

        jumpHeld = kb.spaceKey.isPressed;
    }

    void CheckGround()
    {
        float halfHeight = capsule != null
            ? Mathf.Max(0.5f, capsule.height * Mathf.Abs(transform.localScale.y) * 0.5f)
            : 1f;

        float skin = 0.05f;
        Vector3 origin = transform.position + Vector3.up * (halfHeight - groundCheckRadius - skin);
        float castDistance = groundCheckDistance + groundCheckRadius + skin;

        RaycastHit[] hits = Physics.SphereCastAll(origin, groundCheckRadius, Vector3.down, castDistance, groundLayer, QueryTriggerInteraction.Ignore);

        isGrounded = false;
        groundNormal = Vector3.up;

        float bestDistance = float.MaxValue;
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null || IsOwnCollider(hit.collider)) continue;

            if (hit.distance < bestDistance)
            {
                bestDistance = hit.distance;
                groundNormal = hit.normal;
                isGrounded = Vector3.Angle(hit.normal, Vector3.up) <= maxSlopeAngle + 10f;
            }
        }

        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
        }
    }

    bool IsOwnCollider(Collider col)
    {
        for (int i = 0; i < ownColliders.Length; i++)
        {
            if (ownColliders[i] == col) return true;
        }

        return false;
    }

    void ApplyMovement()
    {
        float speed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);
        Vector3 desired = Vector3.zero;

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            desired = isGrounded
                ? Vector3.ProjectOnPlane(moveDirection, groundNormal).normalized
                : moveDirection;
            desired *= speed;

            if (isGrounded)
            {
                float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);
                float climbFactor = Mathf.InverseLerp(4f, maxSlopeAngle, slopeAngle);
                desired *= Mathf.Lerp(1f, uphillSpeedRetention, climbFactor);
            }
        }

        Vector3 horizontal = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        Vector3 delta = desired - horizontal;
        delta = Vector3.ClampMagnitude(delta, acceleration * Time.fixedDeltaTime);
        horizontal += delta;

        rb.velocity = new Vector3(horizontal.x, rb.velocity.y, horizontal.z);
    }

    void ApplySlope()
    {
        if (!isGrounded || moveDirection.sqrMagnitude < 0.01f) return;

        float angle = Vector3.Angle(groundNormal, Vector3.up);
        if (angle > 1f && angle <= maxSlopeAngle)
        {
            Vector3 slopeDown = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;
            float uphill = Vector3.Dot(moveDirection.normalized, -slopeDown);
            if (uphill > 0.01f)
            {
                float angleFactor = Mathf.InverseLerp(3f, maxSlopeAngle, angle);
                rb.AddForce(Vector3.up * slopeAssistForce * uphill * angleFactor, ForceMode.Acceleration);

                Vector3 uphillDir = Vector3.ProjectOnPlane(moveDirection, groundNormal).normalized;
                rb.AddForce(uphillDir * uphillForwardAssist * uphill * angleFactor, ForceMode.Acceleration);
            }
        }

        if (angle > 1f && rb.velocity.y > -0.2f && rb.velocity.y < 0.8f)
        {
            rb.AddForce(-groundNormal * 12f, ForceMode.Acceleration);
        }
    }

    void ApplyJump()
    {
        bool canJump = coyoteTimer > 0f;
        if (jumpBufferTimer > 0f && canJump)
        {
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            jumpHoldTimer = 0f;

            Vector3 v = rb.velocity;
            v.y = Mathf.Max(v.y, 0f);
            v.y += jumpForce;
            rb.velocity = v;
            isGrounded = false;
        }

        if (jumpHeld && !isGrounded && jumpHoldTimer < maxJumpHoldTime && rb.velocity.y > 0f)
        {
            rb.AddForce(Vector3.up * holdJumpForce, ForceMode.Acceleration);
            jumpHoldTimer += Time.fixedDeltaTime;
        }

        if (!jumpHeld || isGrounded)
        {
            jumpHoldTimer = 0f;
        }
    }

    void ApplyCustomGravity()
    {
        if (isGrounded && rb.velocity.y <= 0.01f)
        {
            return;
        }

        float extra;
        if (rb.velocity.y < 0f)
            extra = Physics.gravity.y * (fallMultiplier - 1f);
        else if (!jumpHeld)
            extra = Physics.gravity.y * (fallMultiplier - 1f);
        else
            extra = Physics.gravity.y * (gravityMultiplier - 1f);

        rb.AddForce(new Vector3(0f, extra, 0f), ForceMode.Acceleration);
    }

    void ApplyRotation()
    {
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion target = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    public bool IsMoving() => moveDirection.magnitude > 0.1f;
    public bool IsSprinting() => isSprinting && IsMoving();
    public bool IsGrounded() => isGrounded;
    public bool IsJumping() => !isGrounded && rb.velocity.y > 0.5f;
    public bool IsFalling() => !isGrounded && rb.velocity.y < -0.5f;
    public float GetCurrentSpeed() => new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        float halfHeight = capsule != null
            ? Mathf.Max(0.5f, capsule.height * Mathf.Abs(transform.localScale.y) * 0.5f)
            : 1f;
        Vector3 origin = transform.position + Vector3.up * (halfHeight - groundCheckRadius - 0.05f);
        Vector3 pos = origin + Vector3.down * (groundCheckDistance + groundCheckRadius + 0.05f);
        Gizmos.DrawWireSphere(pos, groundCheckRadius);
    }
}
