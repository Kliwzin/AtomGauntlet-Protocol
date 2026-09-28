using UnityEngine;

public class OrionTurret : MonoBehaviour, IDamageable
{
    [Header("Projectile")]
    [SerializeField] private OrionTurretProjectile projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Combat")]
    [SerializeField] private float baseDamage = 5f;
    [SerializeField] private float fireCooldown = 1.8f;
    [SerializeField] private float firstShotDelay = 0.5f;

    [Header("Look")]
    [SerializeField] private bool lookAtPlayer = true;
    [SerializeField] private float turnSpeed = 12f;
    [SerializeField] private Vector3 rotationOffset = Vector3.zero;

    [Header("Lifetime")]
    [SerializeField] private float lifeTime = 6f;

    [Header("Health")]
    [SerializeField] private float maxHealth = 20f;

    private float currentHealth;
    private Transform player;
    private float timer = 0f;
    private float aliveTimer = 0f;
    private OrionAttackManager attackManager;

    public void Initialize(OrionAttackManager manager)
    {
        attackManager = manager;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        timer = -firstShotDelay;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void Update()
    {
        aliveTimer += Time.deltaTime;

        if (aliveTimer >= lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        if (player == null) return;

        if (lookAtPlayer)
            FacePlayer();

        timer += Time.deltaTime;

        if (timer >= fireCooldown)
        {
            timer = 0f;
            Shoot();
        }
    }

    private void FacePlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction) * Quaternion.Euler(rotationOffset);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime
        );
    }

    private void Shoot()
    {
        if (projectilePrefab == null) return;

        OrionTurretProjectile projectile = Instantiate(
            projectilePrefab,
            firePoint != null ? firePoint.position : transform.position,
            Quaternion.identity
        );

        float finalDamage = attackManager != null
            ? attackManager.GetAttackDamage(baseDamage)
            : baseDamage;

        projectile.Initialize(player, finalDamage);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0f)
            Destroy(gameObject);
    }
}