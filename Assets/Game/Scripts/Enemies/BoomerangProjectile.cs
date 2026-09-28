using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BoomerangProjectile : MonoBehaviour
{
    private enum ProjectileState
    {
        GoingToPlayer,
        Returning
    }

    [Header("Movement")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float returnSpeed = 9f;
    [SerializeField] private float homingStrength = 4f;
    [SerializeField] private float maxTravelTime = 1.2f;
    [SerializeField] private float returnDistance = 0.6f;

    [Header("Damage")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private bool destroyAfterHitPlayer = false;

    private BoomerangEnemy owner;
    private Transform ownerTransform;
    private Transform targetTransform;

    private ProjectileState state = ProjectileState.GoingToPlayer;
    private Vector3 moveDirection;
    private float travelTimer = 0f;
    private bool alreadyHitPlayer = false;

    public void Initialize(BoomerangEnemy owner, Transform ownerTransform, Transform targetTransform)
    {
        this.owner = owner;
        this.ownerTransform = ownerTransform;
        this.targetTransform = targetTransform;

        if (targetTransform != null)
        {
            moveDirection = (targetTransform.position - transform.position).normalized;
        }
        else
        {
            moveDirection = transform.forward;
        }
    }

    private void Update()
    {
        if (ownerTransform == null)
        {
            Destroy(gameObject);
            return;
        }

        switch (state)
        {
            case ProjectileState.GoingToPlayer:
                MoveTowardsPlayer();
                break;

            case ProjectileState.Returning:
                ReturnToOwner();
                break;
        }
    }

    private void MoveTowardsPlayer()
    {
        travelTimer += Time.deltaTime;

        if (targetTransform != null)
        {
            Vector3 desiredDirection = (targetTransform.position - transform.position).normalized;

            moveDirection = Vector3.Lerp(
                moveDirection,
                desiredDirection,
                homingStrength * Time.deltaTime
            ).normalized;
        }

        transform.position += moveDirection * speed * Time.deltaTime;

        if (travelTimer >= maxTravelTime)
        {
            state = ProjectileState.Returning;
        }
    }

    private void ReturnToOwner()
    {
        Vector3 directionToOwner = (ownerTransform.position - transform.position).normalized;
        transform.position += directionToOwner * returnSpeed * Time.deltaTime;

        float distance = Vector3.Distance(transform.position, ownerTransform.position);

        if (distance <= returnDistance)
        {
            owner.OnProjectileReturned();
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (alreadyHitPlayer) return;

        if (other.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            damageable.TakeDamage(damage);
            alreadyHitPlayer = true;

            if (destroyAfterHitPlayer)
            {
                owner.OnProjectileReturned();
                Destroy(gameObject);
                return;
            }

            state = ProjectileState.Returning;
        }
    }
}
