using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyRangedChaser : MonoBehaviour
{
    [Header("Alvo")]
    [SerializeField] private string playerTag = "Player";

    [Header("Distâncias")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float loseTargetRange = 14f;
    [SerializeField] private float tooCloseDistance = 3.5f; // se o jogador entrar aqui, dá um recuo

    [Header("Recuo (kiting)")]
    [SerializeField] private float backstepSpeed = 4.5f;    // velocidade do recuo
    [SerializeField] private float backstepDuration = 0.4f; // quanto tempo recua (curto e fixo)
    [SerializeField] private float backstepCooldown = 0.8f; // pausa entre recuos para não recuar sem parar

    [Header("Rotação")]
    [SerializeField] private float turnSpeed = 10f;

    [Header("Tiro")]
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private float projectileDamage = 10f;
    [SerializeField] private float attackCooldown = 1.5f; // pausa depois de cada tiro
    [SerializeField] private float shootDuration = 1.3f;
    [SerializeField] private float shootClipLength = 3f;
    [SerializeField, Range(0f, 1f)] private float releasePoint = 0.811f;
    [SerializeField, Range(0f, 1f)] private float trackingPortion = 0.6f;
    [SerializeField] private float spitGravity = 12f;
    [SerializeField] private float aimHeight = 0.3f;

    [Header("Animação")]
    [SerializeField] private Animator animator;
    [SerializeField] private string shootTrigger = "Shoot"; // nome do Trigger no Animator
    [SerializeField] private string speedParam = "Speed";   // nome do float no Animator

    private Rigidbody rb;
    private Transform target;
    private bool isShooting = false;
    private bool isBackstepping = false;
    private float lastAttackTime = -999f;
    private float lastBackstepTime = -999f;
    private bool isTracking = false;
    private Coroutine shootCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        TryFindPlayer();
    }

    private void Update()
    {
        if (animator != null)
        {
            Vector3 horiz = rb.linearVelocity;
            horiz.y = 0f;
            animator.SetFloat(speedParam, horiz.magnitude);
        }
    }

    private void FixedUpdate()
    {
        // durante o recuo, o controlo é da coroutine; não decide mais nada
        if (isBackstepping)
            return;

        if (target == null)
        {
            TryFindPlayer();
            StopMovement();
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        // perdeu o alvo
        if (distance > loseTargetRange)
        {
            target = null;
            StopMovement();
            return;
        }

        // fora de alcance de deteção: fica quieto
        if (distance > detectionRange)
        {
            StopMovement();
            return;
        }

        if (!isShooting || isTracking)
        {
            FaceTarget();
        }
            
        StopMovement(); // o atirador é estacionário por defeito

        // jogador colou: dá um recuo curto (se já passou o cooldown)
        if (distance < tooCloseDistance && Time.time - lastBackstepTime >= backstepCooldown)
        {
            StartCoroutine(BackstepRoutine());
            return;
        }

        // caso contrário, dispara se puder
        if (CanShoot())
            ShootProjectile();
    }

    private IEnumerator BackstepRoutine()
    {
        isBackstepping = true;
        lastBackstepTime = Time.time;

        // cancela tiro a meio, se houver
        CancelShot();

        float t = 0f;
        while (t < backstepDuration)
        {
            // direção: oposta ao jogador, no plano horizontal
            Vector3 away = transform.position - target.position;
            away.y = 0f;

            if (away.sqrMagnitude > 0.001f)
            {
                away.Normalize();
                Vector3 vel = away * backstepSpeed;
                vel.y = rb.linearVelocity.y;
                rb.linearVelocity = vel;
            }

            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // para no fim do recuo
        Vector3 stop = Vector3.zero;
        stop.y = rb.linearVelocity.y;
        rb.linearVelocity = stop;

        isBackstepping = false;
    }

    private void StopMovement()
    {
        Vector3 stop = Vector3.zero;
        stop.y = rb.linearVelocity.y; // preserva gravidade
        rb.linearVelocity = stop;
    }

    private bool CanShoot()
    {
        if (isShooting) return false;
        if (Time.time - lastAttackTime < attackCooldown) return false;
        return true;
    }

    private void ShootProjectile()
    {
        shootCoroutine = StartCoroutine(ShootRoutine());
    }

    private IEnumerator ShootRoutine()
    {
        isShooting = true;
        isTracking = true;

        if (animator != null)
        {
            animator.SetFloat("AttackSpeed", shootClipLength / shootDuration);
            animator.SetTrigger(shootTrigger);
        }

        float windup = shootDuration * releasePoint;

        // Preparação, parte 1: mira acompanha o jogador.
        yield return new WaitForSeconds(windup * trackingPortion);

        // Preparação, parte 2: mira travada.
        isTracking = false;
        yield return new WaitForSeconds(windup * (1f - trackingPortion));

        // Disparo, para onde ele está olhando.
        if (target != null && projectilePrefab != null)
        {
            Vector3 spawnPos = projectileSpawnPoint != null
                ? projectileSpawnPoint.position
                : transform.position + transform.forward;

            Vector3 dir = transform.forward;
            dir.y = 0f;
            dir.Normalize();

            Vector3 toTarget = target.position - spawnPos;
            float heightDelta = (target.position.y + aimHeight) - spawnPos.y;
            toTarget.y = 0f;
            float horizontalDistance = toTarget.magnitude;

            EnemyProjectile proj = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(dir));
            proj.InitializeArc(dir, projectileDamage, horizontalDistance, heightDelta, spitGravity);
        }

        // Recuperação.
        yield return new WaitForSeconds(shootDuration * (1f - releasePoint));

        isShooting = false;
        shootCoroutine = null;
        lastAttackTime = Time.time;
    }

    private void CancelShot()
    {
        if (shootCoroutine != null)
        {
            StopCoroutine(shootCoroutine);
            shootCoroutine = null;
        }

        isShooting = false;
        isTracking = false;
        lastAttackTime = Time.time; // pausa antes de tentar de novo
    }

    private void OnDisable()
    {
        isShooting = false;
        isTracking = false;
        isBackstepping = false;
    }

    private void FaceTarget()
    {
        if (target == null) return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion look = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.fixedDeltaTime);
    }

    private void TryFindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null)
            target = p.transform;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, tooCloseDistance);

        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, loseTargetRange);
    }
}
