using UnityEngine;

public class OrionCardinalWarning : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private OrionLineProjectile projectilePrefab;
    [SerializeField] private float delayBeforeShot = 0.8f;

    [Header("Position")]
    [SerializeField] private float groundY = 0.03f;
    [SerializeField] private float projectileSpawnHeight = 0.5f;
    [SerializeField] private float distanceFromOwner = 0.5f;

    private Transform owner;
    private Vector3 shootDirection;
    private float projectileDamage;
    private OrionLineAttack sourceAttack;

    public void Initialize(
        Transform ownerTransform,
        Vector3 direction,
        float finalDamage,
        OrionLineAttack attackSource
    )
    {
        owner = ownerTransform;
        shootDirection = direction.normalized;
        projectileDamage = finalDamage;
        sourceAttack = attackSource;

        UpdatePosition();
        UpdateRotation();

        Invoke(nameof(Fire), delayBeforeShot);
    }

    private void Update()
    {
        if (owner == null) return;

        UpdatePosition();
        UpdateRotation();
    }

    private void UpdatePosition()
    {
        Vector3 pos = owner.position + shootDirection * distanceFromOwner;
        pos.y = groundY;
        transform.position = pos;
    }

    private void UpdateRotation()
    {
        if (shootDirection != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(shootDirection);
    }

    private void Fire()
    {
        if (projectilePrefab != null)
        {
            Vector3 spawnPos = owner != null
                ? owner.position + shootDirection * distanceFromOwner
                : transform.position;

            spawnPos.y += projectileSpawnHeight;

            OrionLineProjectile projectile = Instantiate(
                projectilePrefab,
                spawnPos,
                Quaternion.identity
            );

            projectile.Initialize(shootDirection, projectileDamage, sourceAttack);
        }

        Destroy(gameObject);
    }
}