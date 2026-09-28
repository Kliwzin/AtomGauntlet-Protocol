using UnityEngine;

public class ExecutionIconDisplay : MonoBehaviour
{
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private WeaponSystem weaponSystem;

    [Header("Execution")]
    [SerializeField] private float executeThreshold = 0.2f;
    [SerializeField] private int executionWeaponIndex = 2;

    [Header("Icons")]
    [SerializeField] private GameObject executableIcon;
    [SerializeField] private GameObject readyExecutableIcon;

    private void Start()
    {
        if (enemyHealth == null)
            enemyHealth = GetComponentInParent<EnemyHealth>();

        if (weaponSystem == null)
            weaponSystem = FindFirstObjectByType<WeaponSystem>();
    }

    private void Update()
    {
        if (enemyHealth == null || weaponSystem == null)
            return;

        bool canExecute = enemyHealth.HealthPercent <= executeThreshold;
        bool hasCorrectWeapon = weaponSystem.CurrentWeaponIndex == executionWeaponIndex;

        if (executableIcon != null)
            executableIcon.SetActive(canExecute && !hasCorrectWeapon);

        if (readyExecutableIcon != null)
            readyExecutableIcon.SetActive(canExecute && hasCorrectWeapon);
    }
}