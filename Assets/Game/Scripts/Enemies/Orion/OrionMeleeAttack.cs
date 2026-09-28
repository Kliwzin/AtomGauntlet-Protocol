using UnityEngine;

public class OrionMeleeAttack : MonoBehaviour
{
    [SerializeField] private OrionAttackManager attackManager;
    [SerializeField] private Transform attackPoint;

    [Header("Melee Settings")]
    [SerializeField] private float range = 2f;
    [SerializeField] private float baseDamage = 8f;
    [SerializeField] private float cooldown = 1.4f;
    [SerializeField] private LayerMask playerLayer;

    private float lastAttackTime = -999f;

    public float Range => range;

    public bool CanExecute()
    {
        return Time.time >= lastAttackTime + cooldown;
    }

    public void PrepareAttack()
    {
        if (!CanExecute()) return;

        lastAttackTime = Time.time;
        Debug.Log("Orion preparou melee.");
    }

    public void ApplyDamage()
    {
        Vector3 position = attackPoint != null ? attackPoint.position : transform.position;

        Collider[] hits = Physics.OverlapSphere(position, range, playerLayer);

        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                float damage = attackManager != null
                    ? attackManager.GetAttackDamage(baseDamage)
                    : baseDamage;

                damageable.TakeDamage(damage);
            }
        }

        Debug.Log("Orion aplicou dano melee.");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 position = attackPoint != null ? attackPoint.position : transform.position;
        Gizmos.DrawWireSphere(position, range);
    }
}