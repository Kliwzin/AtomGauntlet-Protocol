using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyChaser : MonoBehaviour
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

    [Header("Combo")]
    [SerializeField] private float attackClipLength = 3.167f;
    [SerializeField, Range(0f, 1f)] private float firstImpact = 0.095f;
    [SerializeField, Range(0f, 1f)] private float secondImpact = 0.558f;
    [SerializeField] private float jabTime = 0.25f;         // do início até o 1º soco
    [SerializeField] private float crossWindupTime = 1.2f;  // do 1º ao 2º soco: o aviso
    [SerializeField] private float recoveryTime = 0.8f;     // do 2º soco ao fim: a janela
    [SerializeField] private float jabDamage = 4f;          // o 2º soco usa o Attack Damage
    [SerializeField] private float comboMoveSpeed = 1.2f;
    [SerializeField] private float comboTurnSpeed = 3f;
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
                FaceTarget(comboTurnSpeed);
                AdvanceDuringCombo();
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
        FaceTarget(turnSpeed);
    }

    private void FaceTarget(float speed)
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
            speed * Time.fixedDeltaTime
        );
    }

    private bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown && !isAttackInProgress;
    }

    private System.Collections.IEnumerator AttackRoutine()
    {
        isAttackInProgress = true;

        // Quanto tempo cada trecho dura no clip original.
        float clipToFirst = attackClipLength * firstImpact;
        float clipToSecond = attackClipLength * (secondImpact - firstImpact);
        float clipToEnd = attackClipLength * (1f - secondImpact);

        // Trecho 1: jab rápido.
        SetAttackSpeed(clipToFirst / jabTime);
        if (animator != null)
            animator.SetTrigger("Attack");

        yield return new WaitForSeconds(jabTime);
        TryHitTarget(jabDamage);

        // Trecho 2: preparação do soco forte (o aviso).
        SetAttackSpeed(clipToSecond / crossWindupTime);
        yield return new WaitForSeconds(crossWindupTime);
        TryHitTarget(attackDamage);

        // Trecho 3: recuperação (a janela do jogador).
        SetAttackSpeed(clipToEnd / recoveryTime);
        yield return new WaitForSeconds(recoveryTime);

        isAttackInProgress = false;
        lastAttackTime = Time.time;
        currentState = EnemyState.Chasing;
    }

    private void SetAttackSpeed(float speed)
    {
        if (animator != null)
            animator.SetFloat("AttackSpeed", speed);
    }

    private void AdvanceDuringCombo()
    {
        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.magnitude <= stoppingDistance)
        {
            ApplyIdleMovement();
            return;
        }

        Vector3 step = toTarget.normalized * comboMoveSpeed;
        Vector3 velocity = rb.linearVelocity;
        velocity.x = step.x;
        velocity.z = step.z;
        rb.linearVelocity = velocity;
    }

    private void TryHitTarget(float damage)
    {
        if (target == null) return;

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.magnitude > attackRange + 0.2f) return;

        Vector3 facing = transform.rotation *
                         Quaternion.Inverse(Quaternion.Euler(modelRotationOffset)) *
                         Vector3.forward;
        facing.y = 0f;

        if (Vector3.Angle(facing, toTarget) > hitArc * 0.5f) return;

        if (target.TryGetComponent<IDamageable>(out IDamageable damageable))
            damageable.TakeDamage(damage);
    }

    private void OnDisable()
    {
        isAttackInProgress = false;
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