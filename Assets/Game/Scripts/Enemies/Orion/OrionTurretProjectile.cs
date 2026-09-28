using UnityEngine;

[RequireComponent(typeof(Collider))]
public class OrionTurretProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 7f;
    [SerializeField] private float homingDuration = 0.4f;
    [SerializeField] private float homingStrength = 5f;
    [SerializeField] private float lifeTime = 3f;

    private float damage;
    private Transform target;
    private Vector3 moveDirection;
    private float timer = 0f;

    public void Initialize(Transform targetTransform, float finalDamage)
    {
        target = targetTransform;
        damage = finalDamage;

        if (target != null)
            moveDirection = (target.position - transform.position).normalized;
        else
            moveDirection = transform.forward;

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (target != null && timer <= homingDuration)
        {
            Vector3 desiredDirection = (target.position - transform.position).normalized;

            moveDirection = Vector3.Lerp(
                moveDirection,
                desiredDirection,
                homingStrength * Time.deltaTime
            ).normalized;
        }

        transform.position += moveDirection * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (other.TryGetComponent<IDamageable>(out IDamageable damageable))
            damageable.TakeDamage(damage);

        Destroy(gameObject);
    }
}