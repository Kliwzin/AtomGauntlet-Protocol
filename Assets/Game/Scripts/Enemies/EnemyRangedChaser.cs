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
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float shootWindup = 0.3f;

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

        FaceTarget();
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
        isShooting = false;

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
        lastAttackTime = Time.time;
        StartCoroutine(ShootRoutine());
    }

    private IEnumerator ShootRoutine()
    {
        isShooting = true;

        // toca a animação de atirar no início do windup
        if (animator != null)
            animator.SetTrigger(shootTrigger);

        yield return new WaitForSeconds(shootWindup);

        // se perdeu o alvo ou começou a recuar durante o windup, cancela
        if (target == null || isBackstepping)
        {
            isShooting = false;
            yield break;
        }

        Vector3 spawnPos = projectileSpawnPoint != null
            ? projectileSpawnPoint.position
            : transform.position + transform.forward;

        Vector3 dir = target.position - spawnPos;
        dir.y = 0f;
        dir.Normalize();

        if (projectilePrefab != null)
        {
            EnemyProjectile proj = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(dir));
            proj.Initialize(dir, projectileDamage);
        }

        isShooting = false;
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
