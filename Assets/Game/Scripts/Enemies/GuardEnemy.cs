using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GuardEnemy : MonoBehaviour
{
    public enum EnemyState { Idle, Chasing, Attacking }

    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private string playerTag = "Player";

    [Header("Detection")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float loseTargetRange = 12f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float stoppingDistance = 1.4f;
    [SerializeField] private float depthTrackingSpeed = 2.5f;
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private Vector3 modelRotationOffset = Vector3.zero;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.6f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private float attackDuration = 1.2f;
    [SerializeField] private float attackClipLength = 2.042f;
    [SerializeField, Range(0f, 1f)] private float impactPoint = 0.735f;
    [SerializeField, Range(0f, 1f)] private float trackingPortion = 0.5f;
    [SerializeField] private float hitArc = 100f;

    [Header("Natural Movement")]
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float deceleration = 10f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private Rigidbody rb;
    private EnemyState currentState = EnemyState.Idle;
    private float lastAttackTime = -999f;
    private bool isAttackInProgress = false;
    private bool isTracking = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationY |
                         RigidbodyConstraints.FreezeRotationZ;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        TryFindPlayer();
    }

    private void Update()
    {
        if (animator == null) return;

        Vector3 horizontalVelocity = rb.linearVelocity;
        horizontalVelocity.y = 0f;

        animator.SetFloat("Speed", horizontalVelocity.magnitude);
    }

    private void FixedUpdate()
    {
        if (target == null)
        {
            TryFindPlayer();
            ApplyIdleMovement();
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        switch (currentState)
        {
            case EnemyState.Idle:
                ApplyIdleMovement();

                if (distanceToTarget <= detectionRange)
                    currentState = EnemyState.Chasing;

                break;

            case EnemyState.Chasing:
                if (distanceToTarget > loseTargetRange)
                {
                    currentState = EnemyState.Idle;
                    return;
                }

                FaceTarget();

                if (distanceToTarget <= attackRange && CanAttack())
                {
                    currentState = EnemyState.Attacking;
                    StartCoroutine(AttackRoutine());
                    return;
                }

                MoveTowardsTarget();
                break;

            case EnemyState.Attacking:
                ApplyIdleMovement();
                if (isTracking)
                    FaceTarget();
                break;
        }
    }

    private void TryFindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player != null)
            target = player.transform;
    }

    private void MoveTowardsTarget()
    {
        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        Vector3 desiredVelocity = Vector3.zero;

        if (toTarget.magnitude > stoppingDistance)
            desiredVelocity = toTarget.normalized;

        desiredVelocity = new Vector3(
            desiredVelocity.x * moveSpeed,
            rb.linearVelocity.y,
            desiredVelocity.z * depthTrackingSpeed
        );

        Vector3 currentVelocity = rb.linearVelocity;

        currentVelocity.x = Mathf.MoveTowards(
            currentVelocity.x,
            desiredVelocity.x,
            acceleration * Time.fixedDeltaTime * moveSpeed
        );

        currentVelocity.z = Mathf.MoveTowards(
            currentVelocity.z,
            desiredVelocity.z,
            acceleration * Time.fixedDeltaTime * depthTrackingSpeed
        );

        rb.linearVelocity = currentVelocity;
    }

    private void ApplyIdleMovement()
    {
        Vector3 velocity = rb.linearVelocity;

        velocity.x = Mathf.MoveTowards(velocity.x, 0f, deceleration * Time.fixedDeltaTime);
        velocity.z = Mathf.MoveTowards(velocity.z, 0f, deceleration * Time.fixedDeltaTime);

        rb.linearVelocity = velocity;
    }

    private void FaceTarget()
    {
        if (target == null) return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction.normalized) * Quaternion.Euler(modelRotationOffset);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.fixedDeltaTime
        );
    }

    private bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown && !isAttackInProgress;
    }

    private System.Collections.IEnumerator AttackRoutine()
    {
        isAttackInProgress = true;
        isTracking = true;

        if (animator != null)
        {
            animator.SetFloat("AttackSpeed", attackClipLength / attackDuration);
            animator.SetTrigger("Attack");
        }

        float windup = attackDuration * impactPoint;

        yield return new WaitForSeconds(windup * trackingPortion);

        isTracking = false;
        yield return new WaitForSeconds(windup * (1f - trackingPortion));

        // Impacto.
        TryHitTarget();

        // Recuperação.
        yield return new WaitForSeconds(attackDuration * (1f - impactPoint));

        isAttackInProgress = false;
        lastAttackTime = Time.time;
        currentState = EnemyState.Chasing;
    }

    private void TryHitTarget()
    {
        if (target == null) return;

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.magnitude > attackRange + 0.2f) return;
        if (Vector3.Angle(GetFacingDirection(), toTarget) > hitArc * 0.5f) return;

        if (target.TryGetComponent<IDamageable>(out IDamageable damageable))
            damageable.TakeDamage(attackDamage);
    }

    private Vector3 GetFacingDirection()
    {
        // Desfaz o offset do modelo para achar a frente "real" do guarda.
        Vector3 facing = transform.rotation *
                         Quaternion.Inverse(Quaternion.Euler(modelRotationOffset)) *
                         Vector3.forward;
        facing.y = 0f;
        return facing.normalized;
    }

    private void OnDisable()
    {
        isAttackInProgress = false;
        isTracking = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, loseTargetRange);
    }
}
