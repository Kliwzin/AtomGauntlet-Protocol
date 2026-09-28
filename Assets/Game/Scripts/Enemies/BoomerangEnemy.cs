using UnityEngine;

public class BoomerangEnemy : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Transform target;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 8f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float stoppingDistance = 5f;
    [SerializeField] private float fleeDistance = 3.5f;

    [Header("Attack")]
    [SerializeField] private BoomerangProjectile projectilePrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float attackCooldown = 1.2f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private Rigidbody rb;
    private bool projectileActive = false;
    private float lastAttackTime = -999f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX |
                             RigidbodyConstraints.FreezeRotationY |
                             RigidbodyConstraints.FreezeRotationZ;
        }

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        FindPlayer();
    }

    private void Update()
    {
        if (animator == null || rb == null) return;

        Vector3 horizontalVelocity = rb.linearVelocity;
        horizontalVelocity.y = 0f;

        animator.SetFloat("Speed", horizontalVelocity.magnitude);
    }

    private void FixedUpdate()
    {
        if (target == null)
        {
            FindPlayer();
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > detectionRange)
        {
            StopMoving();
            return;
        }

        FacePlayer();

        if (distance < fleeDistance)
        {
            MoveAwayFromPlayer();
        }
        else if (distance > stoppingDistance)
        {
            MoveTowardsPlayer();
        }
        else
        {
            StopMoving();
        }

        // pode atacar mesmo em movimento
        if (distance <= attackRange)
        {
            TryThrowProjectile();
        }
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player != null)
            target = player.transform;
    }

    private void MoveTowardsPlayer()
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;
        direction.Normalize();

        rb.linearVelocity = new Vector3(
            direction.x * moveSpeed,
            rb.linearVelocity.y,
            direction.z * moveSpeed
        );
    }

    private void StopMoving()
    {
        if (rb == null) return;

        rb.linearVelocity = new Vector3(
            0f,
            rb.linearVelocity.y,
            0f
        );
    }

    private void MoveAwayFromPlayer()
    {
        Vector3 direction = transform.position - target.position;
        direction.y = 0f;
        direction.Normalize();

        rb.linearVelocity = new Vector3(
            direction.x * moveSpeed,
            rb.linearVelocity.y,
            direction.z * moveSpeed
        );
    }

    private void FacePlayer()
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (Mathf.Abs(direction.x) < 0.01f) return;

        transform.forward = new Vector3(Mathf.Sign(direction.x), 0f, 0f);
    }

    private void TryThrowProjectile()
    {
        if (projectileActive) return;
        if (Time.time < lastAttackTime + attackCooldown) return;
        if (projectilePrefab == null || throwPoint == null) return;

        lastAttackTime = Time.time;
        projectileActive = true;

        // dispara a animação de arremesso
        if (animator != null)
            animator.SetTrigger("Throw");

        BoomerangProjectile projectile = Instantiate(
            projectilePrefab,
            throwPoint.position,
            Quaternion.identity
        );

        projectile.Initialize(
            owner: this,
            ownerTransform: transform,
            targetTransform: target
        );
    }

    public void OnProjectileReturned()
    {
        projectileActive = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }
}
