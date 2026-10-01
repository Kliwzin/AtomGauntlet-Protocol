using System.Collections;
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
    [SerializeField] private float turnSpeed = 10f;

    [Header("Attack")]
    [SerializeField] private BoomerangProjectile projectilePrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float attackCooldown = 1.2f; // pausa depois de pegar o bumerangue de volta
    [SerializeField] private float throwDuration = 1.4f;
    [SerializeField] private float throwClipLength = 2.167f;
    [SerializeField, Range(0f, 1f)] private float releasePoint = 0.673f;
    [SerializeField, Range(0f, 1f)] private float trackingPortion = 0.5f;
    [SerializeField] private GameObject heldBoomerangModel;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private Rigidbody rb;
    private bool projectileActive = false;
    private bool isThrowing = false;
    private bool isTracking = false;
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

        if (!isThrowing || isTracking)
        {
            FacePlayer();
        }
            
        if (isThrowing)
        {
            StopMoving();
            return;
        }

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

        if (direction.sqrMagnitude < 0.01f) return;

        Quaternion look = Quaternion.LookRotation(direction.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.fixedDeltaTime);
    }

    private void TryThrowProjectile()
    {
        if (isThrowing || projectileActive) return;
        if (Time.time < lastAttackTime + attackCooldown) return;
        if (projectilePrefab == null || throwPoint == null) return;

        StartCoroutine(ThrowRoutine());
    }

    private IEnumerator ThrowRoutine()
    {
        isThrowing = true;
        StopMoving();

        if (animator != null)
        {
            animator.SetFloat("AttackSpeed", throwClipLength / throwDuration);
            animator.SetTrigger("Throw");
        }

        float windup = throwDuration * releasePoint;
        isTracking = true;

        // Preparação, parte 1: ainda vira para o jogador.
        yield return new WaitForSeconds(windup * trackingPortion);

        // Preparação, parte 2: direção travada.
        isTracking = false;
        yield return new WaitForSeconds(windup * (1f - trackingPortion));

        // Soltura.
        if (target != null)
        {
            projectileActive = true;

            if (heldBoomerangModel != null)
                heldBoomerangModel.SetActive(false);

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

        // Recuperação.
        yield return new WaitForSeconds(throwDuration * (1f - releasePoint));

        isThrowing = false;
    }

    public void OnProjectileReturned()
    {
        projectileActive = false;
        lastAttackTime = Time.time; // a pausa começa quando ele pega de volta

        if (heldBoomerangModel != null)
            heldBoomerangModel.SetActive(true);
    }

    private void OnDisable()
    {
        isThrowing = false;
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
