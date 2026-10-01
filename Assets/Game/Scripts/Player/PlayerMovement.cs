using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float depthSpeed = 4.5f;

    [Header("Depth Limit")]
    [SerializeField] private bool useDepthClamp = false;
    [SerializeField] private float depthLimit = 20f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 10f;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckOffset = 0.05f;

    [Header("Camera Reference")]
    [SerializeField] private CameraFollow cameraFollow;

    [Header("Ladder")]
    [SerializeField] private float ladderSpeed = 4f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Combat")]
    [SerializeField, Range(0f, 1f)] private float attackMoveMultiplier = 0.3f;
    [SerializeField] private bool lockRotationWhileAttacking = true;

    private WeaponSystem weaponSystem;

    private bool isOnLadder = false;
    private bool inLadderZone = false;
    private Ladder currentLadder;

    private Vector2 input;
    private Rigidbody rb;
    private CapsuleCollider col;
    private bool grounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        weaponSystem = GetComponent<WeaponSystem>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationY |
                         RigidbodyConstraints.FreezeRotationZ;

        rb.useGravity = true;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        if (cameraFollow == null && Camera.main != null)
            cameraFollow = Camera.main.GetComponent<CameraFollow>();
    }

    private void Update()
    {
        grounded = CheckGrounded();

        if (inLadderZone && !isOnLadder && Mathf.Abs(input.y) > 0.1f)
        {
            EnterLadder(currentLadder);
        }

        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (isOnLadder)
        {
            Vector3 ladderVelocity = rb.linearVelocity;
            ladderVelocity.x = 0f;
            ladderVelocity.z = 0f;
            ladderVelocity.y = input.y * ladderSpeed;
            rb.linearVelocity = ladderVelocity;
            return;
        }

        bool attacking = weaponSystem != null && weaponSystem.IsAttacking;
        float speedMultiplier = attacking ? attackMoveMultiplier : 1f;

        Vector3 velocity = rb.linearVelocity;
        velocity.x = input.x * moveSpeed * speedMultiplier;
        velocity.z = input.y * depthSpeed * speedMultiplier;
        rb.linearVelocity = velocity;

        if (useDepthClamp)
        {
            Vector3 clampedPosition = rb.position;
            clampedPosition.z = Mathf.Clamp(clampedPosition.z, -depthLimit, depthLimit);
            rb.MovePosition(clampedPosition);
        }

        bool canRotate = !(attacking && lockRotationWhileAttacking);
        Vector3 moveDirection = new Vector3(input.x, 0f, input.y);

        if (canRotate && moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection.normalized);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                12f * Time.fixedDeltaTime
            );
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        if (isOnLadder)
        {
            // Na escada: a "velocidade" da animação Escalar segue o input vertical.
            // Speed a 0 para não disparar Walking; OnLadder controla o estado.
            animator.SetFloat("Speed", 0f);
            animator.SetBool("OnLadder", true);
        }
        else
        {
            Vector3 horizontalVelocity = rb.linearVelocity;
            horizontalVelocity.y = 0f;

            animator.SetFloat("Speed", horizontalVelocity.magnitude);
            animator.SetBool("OnLadder", false);
        }
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        input = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        if (isOnLadder)
        {
            ExitLadder();
            Jump();
            return;
        }

        if (!grounded) return;

        Jump();
    }

    private void Jump()
    {
        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0f;
        rb.linearVelocity = velocity;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        if (animator != null)
            animator.SetTrigger("Jump");

        if (cameraFollow != null)
            cameraFollow.OnPlayerJump();
    }

    public void SetLadderZone(bool inZone, Ladder ladder)
    {
        if (inZone)
        {
            inLadderZone = true;
            currentLadder = ladder;
        }
        else
        {
            inLadderZone = false;

            if (isOnLadder && ladder == currentLadder)
                ExitLadder();

            currentLadder = null;
        }
    }

    public void EnterLadder(Ladder ladder)
    {
        isOnLadder = true;
        currentLadder = ladder;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
    }

    public void ExitLadder()
    {
        isOnLadder = false;
        rb.useGravity = true;
    }

    public void ExitLadderIfNeeded()
    {
        if (isOnLadder)
            ExitLadder();
    }

    private bool CheckGrounded()
    {
        float radius = col.radius * 0.95f;
        Vector3 centerWorld = transform.TransformPoint(col.center);
        float bottom = centerWorld.y - (col.height * 0.5f) + col.radius;

        Vector3 checkPosition = new Vector3(
            centerWorld.x,
            bottom - groundCheckOffset,
            centerWorld.z
        );

        return Physics.CheckSphere(checkPosition, radius, groundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule == null) return;

        float radius = capsule.radius * 0.95f;
        Vector3 centerWorld = transform.TransformPoint(capsule.center);
        float bottom = centerWorld.y - (capsule.height * 0.5f) + capsule.radius;

        Vector3 checkPosition = new Vector3(
            centerWorld.x,
            bottom - groundCheckOffset,
            centerWorld.z
        );

        Gizmos.color = grounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(checkPosition, radius);
    }
}
