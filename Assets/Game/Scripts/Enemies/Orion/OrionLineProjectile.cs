using UnityEngine;

public class OrionLineProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifeTime = 4f;

    private Vector3 direction;
    private float damage;
    private OrionLineAttack sourceAttack;

    public void Initialize(Vector3 moveDirection, float finalDamage, OrionLineAttack attackSource)
    {
        direction = moveDirection.normalized;
        damage = finalDamage;
        sourceAttack = attackSource;

        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (sourceAttack != null && !sourceAttack.CanThisAttackDamagePlayer())
            return;

        if (other.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            damageable.TakeDamage(damage);

            if (sourceAttack != null)
                sourceAttack.RegisterPlayerHit();
        }

        // Não destrói. O raio continua passando.
    }
}