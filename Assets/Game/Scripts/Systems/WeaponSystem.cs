using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSystem : MonoBehaviour
{
    [System.Serializable]
    public class Weapon
    {
        public string weaponName;
        public GameObject weaponModel;
        public float damage = 10f;
        public float attackRange = 1.5f;
        public float knockback = 5f;
        public float attackCooldown = 0.5f;
        public float energyCostPerAttack = 2f;
        public float energyCostToEquip = 1f;
        public bool requiresEnergy = true;
    }

    [Header("Main Weapons")]
    [SerializeField] private Weapon[] weapons = new Weapon[3];
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Fists")]
    [SerializeField] private GameObject fistsModel;
    [SerializeField] private float fistsDamage = 1f;
    [SerializeField] private float fistsAttackRange = 1.1f;
    [SerializeField] private float fistsKnockback = 1f;
    [SerializeField] private float fistsAttackCooldown = 0.35f;

    private int currentWeaponIndex = 0;
    private int lastValidWeaponIndex = 0;
    private bool isUsingFists = false;

    private float lastAttackTime;
    private Animator animator;
    private CameraFollow cameraFollow;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        cameraFollow = Camera.main != null ? Camera.main.GetComponent<CameraFollow>() : null;
    }

    private void Start()
    {
        GameSession.EnsureExists();

        if (GameSession.Instance != null)
        {
            currentWeaponIndex = GameSession.Instance.CurrentWeaponIndex;
            lastValidWeaponIndex = currentWeaponIndex;
        }

        UpdateForcedFistsState();
        UpdateWeaponVisuals();
    }

    private void Update()
    {
        UpdateForcedFistsState();
    }

    public void OnAttack(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        TryAttack();
    }

    public void OnSelectWeapon1(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        TrySelectWeapon(0);
    }

    public void OnSelectWeapon2(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        TrySelectWeapon(1);
    }

    public void OnSelectWeapon3(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        TrySelectWeapon(2);
    }

    private Weapon CreateFistsWeapon()
    {
        return new Weapon
        {
            weaponName = "Fists",
            weaponModel = fistsModel,
            damage = fistsDamage,
            attackRange = fistsAttackRange,
            knockback = fistsKnockback,
            attackCooldown = fistsAttackCooldown,
            energyCostPerAttack = 0f,
            energyCostToEquip = 0f,
            requiresEnergy = false
        };
    }

    private void UpdateForcedFistsState()
    {
        if (GameSession.Instance == null) return;
        if (weapons == null || weapons.Length == 0) return;
        if (currentWeaponIndex < 0 || currentWeaponIndex >= weapons.Length) return;

        Weapon selectedWeapon = weapons[currentWeaponIndex];
        if (selectedWeapon == null) return;

        float requiredEnergy = selectedWeapon.energyCostPerAttack;

        if (GameSession.Instance.CurrentEnergy < requiredEnergy)
        {
            if (!isUsingFists)
            {
                isUsingFists = true;
                lastValidWeaponIndex = currentWeaponIndex;
                UpdateWeaponVisuals();
                Debug.Log("Energia insuficiente. Modo punhos ativado.");
            }

            return;
        }

        if (isUsingFists)
        {
            isUsingFists = false;
            currentWeaponIndex = lastValidWeaponIndex;
            GameSession.Instance.CurrentWeaponIndex = currentWeaponIndex;
            UpdateWeaponVisuals();
            Debug.Log("Energia restaurada. Armas reativadas.");
        }
    }

    private void TrySelectWeapon(int index)
    {
        if (GameSession.Instance == null) return;

        if (isUsingFists)
        {
            Debug.Log("Sem energia. Não é possível trocar de arma.");
            return;
        }

        if (weapons == null || weapons.Length == 0) return;
        if (index < 0 || index >= weapons.Length) return;
        if (index == currentWeaponIndex) return;

        if (!GameSession.Instance.HasWeaponAddon(index))
        {
            Debug.Log("Addon dessa arma ainda não foi desbloqueado.");
            return;
        }

        float equipCost = weapons[index].energyCostToEquip;

        if (!GameSession.Instance.HasEnoughEnergy(equipCost))
        {
            Debug.Log("Energia insuficiente para trocar de arma.");
            return;
        }

        GameSession.Instance.ConsumeEnergy(equipCost);

        currentWeaponIndex = index;
        lastValidWeaponIndex = index;
        GameSession.Instance.CurrentWeaponIndex = currentWeaponIndex;

        UpdateForcedFistsState();
        UpdateWeaponVisuals();

        Debug.Log("Arma equipada: " + weapons[currentWeaponIndex].weaponName);
    }

    private void TryAttack()
    {
        if (GameSession.Instance == null) return;
        if (weapons == null || weapons.Length == 0) return;
        if (currentWeaponIndex < 0 || currentWeaponIndex >= weapons.Length) return;

        UpdateForcedFistsState();

        Weapon weaponToUse = isUsingFists ? CreateFistsWeapon() : weapons[currentWeaponIndex];
        int weaponIndexToUse = isUsingFists ? -1 : currentWeaponIndex;

        if (weaponToUse == null) return;

        bool cooldownReady = Time.time - lastAttackTime >= weaponToUse.attackCooldown;
        if (!cooldownReady) return;

        if (!isUsingFists && weaponToUse.requiresEnergy)
        {
            if (!GameSession.Instance.HasEnoughEnergy(weaponToUse.energyCostPerAttack))
            {
                UpdateForcedFistsState();
                weaponToUse = CreateFistsWeapon();
                weaponIndexToUse = -1;
            }
            else
            {
                GameSession.Instance.ConsumeEnergy(weaponToUse.energyCostPerAttack);
            }
        }

        lastAttackTime = Time.time;

        // Animação de ataque única (serve para todas as armas).
        if (animator != null)
            animator.SetTrigger("Attack");

        bool hitSomeone = DetectAndHitTargets(weaponToUse, weaponIndexToUse);

        if (hitSomeone && cameraFollow != null)
            cameraFollow.Shake();

        Debug.Log(isUsingFists
            ? "Atacou com os punhos."
            : "Atacou com " + weapons[currentWeaponIndex].weaponName);

        UpdateForcedFistsState();
    }

    private bool DetectAndHitTargets(Weapon currentWeapon, int weaponIndex)
    {
        Vector3 pos = attackPoint != null ? attackPoint.position : transform.position + transform.forward;

        Collider[] hitTargets = Physics.OverlapSphere(pos, currentWeapon.attackRange, enemyLayer);

        bool hitSomeone = false;

        foreach (Collider col in hitTargets)
        {
            OrionHealth orion = col.GetComponent<OrionHealth>();

            if (orion != null)
            {
                hitSomeone = true;

                float finalDamage = CalculateWeaponDamage(
                    weaponIndex,
                    currentWeapon.damage,
                    col.gameObject
                );

                orion.TakeDamageFromAttacker(finalDamage, transform);

                ApplyKnockback(col, currentWeapon.knockback);
                continue;
            }

            if (col.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                hitSomeone = true;

                float finalDamage = CalculateWeaponDamage(
                    weaponIndex,
                    currentWeapon.damage,
                    col.gameObject
                );

                damageable.TakeDamage(finalDamage);

                ApplyKnockback(col, currentWeapon.knockback);
            }

            OrionShieldStone shieldStone = col.GetComponent<OrionShieldStone>();
            if (shieldStone != null)
            {
                shieldStone.TakeWeaponHit(weaponIndex);
                hitSomeone = true;
                continue;
            }

            BreakableWall wall = col.GetComponent<BreakableWall>();
            if (wall != null)
            {
                wall.TakeWeaponHit(weaponIndex);
                hitSomeone = true;
            }
        }

        return hitSomeone;
    }

    private void ApplyKnockback(Collider col, float knockback)
    {
        Rigidbody targetRb = col.attachedRigidbody;
        if (targetRb == null) return;

        Vector3 knockbackDir = (col.transform.position - transform.position).normalized;
        knockbackDir.y = 0f;

        targetRb.AddForce(knockbackDir * knockback, ForceMode.Impulse);
    }

    private float CalculateWeaponDamage(int weaponIndex, float baseDamage, GameObject target)
    {
        if (weaponIndex == -1)
            return baseDamage;

        if (weaponIndex == 2)
        {
            EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();

            if (enemyHealth != null)
            {
                float missingHealthFactor = 1f - enemyHealth.GetHealthPercent();
                return baseDamage + (baseDamage * missingHealthFactor);
            }
        }

        return baseDamage;
    }

    private void UpdateWeaponVisuals()
    {
        if (weapons != null)
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] != null && weapons[i].weaponModel != null)
                    weapons[i].weaponModel.SetActive(!isUsingFists && i == currentWeaponIndex);
            }
        }

        if (fistsModel != null)
            fistsModel.SetActive(isUsingFists);
    }

    public float GetCurrentEnergy()
    {
        return GameSession.Instance != null ? GameSession.Instance.CurrentEnergy : 0f;
    }

    public float GetMaxEnergy()
    {
        return GameSession.Instance != null ? GameSession.Instance.MaxEnergy : 100f;
    }

    private void OnDrawGizmosSelected()
    {
        Weapon previewWeapon = null;

        if (isUsingFists)
        {
            previewWeapon = CreateFistsWeapon();
        }
        else if (weapons != null &&
                 weapons.Length > 0 &&
                 currentWeaponIndex >= 0 &&
                 currentWeaponIndex < weapons.Length)
        {
            previewWeapon = weapons[currentWeaponIndex];
        }

        if (previewWeapon == null) return;

        Gizmos.color = Color.red;

        Vector3 pos = attackPoint != null ? attackPoint.position : transform.position + transform.forward;
        Gizmos.DrawWireSphere(pos, previewWeapon.attackRange);
    }

    public int CurrentWeaponIndex => isUsingFists ? -1 : currentWeaponIndex;
    public bool IsUsingFists => isUsingFists;
}
