using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifeTime = 4f;

    private Vector3 velocity;
    private float gravity = 0f;

    // Tiro reto
    public void Initialize(Vector3 shootDirection, float projectileDamage)
    {
        velocity = shootDirection.normalized * speed;
        damage = projectileDamage;
        gravity = 0f;

        Destroy(gameObject, lifeTime);
    }

    // Tiro em arco
    public void InitializeArc(Vector3 flatDirection, float projectileDamage,
                              float horizontalDistance, float heightDelta, float arcGravity)
    {
        flatDirection.y = 0f;
        flatDirection.Normalize();

        damage = projectileDamage;
        gravity = arcGravity;

        float flightTime = Mathf.Max(0.1f, horizontalDistance / speed);
        float verticalSpeed = (heightDelta + 0.5f * gravity * flightTime * flightTime) / flightTime;

        velocity = flatDirection * speed + Vector3.up * verticalSpeed;

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        velocity.y -= gravity * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;

        if (velocity.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(velocity);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy")) return;

        if (other.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            damageable.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (!other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}