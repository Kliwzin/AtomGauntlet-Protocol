using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;

    private float currentHealth;
    private CameraFollow cameraFollow;
    private PlayerMovement playerMovement;
    private Animator animator;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float HealthPercent => maxHealth <= 0f ? 0f : currentHealth / maxHealth;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        cameraFollow = Camera.main != null ? Camera.main.GetComponent<CameraFollow>() : null;

        GameSession.EnsureExists();

        if (GameSession.Instance != null)
        {
            maxHealth = GameSession.Instance.MaxHealth;
            currentHealth = GameSession.Instance.CurrentHealth;

            if (currentHealth <= 0f)
            {
                currentHealth = maxHealth;
                GameSession.Instance.RestoreFullHealth();
            }
        }
        else
        {
            currentHealth = maxHealth;
        }
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f) return;

        currentHealth = Mathf.Max(currentHealth - damage, 0f);

        if (GameSession.Instance != null)
        {
            GameSession.Instance.SetHealth(currentHealth);
        }

        playerMovement.ExitLadderIfNeeded();

        if (cameraFollow != null)
        {
            cameraFollow.Shake();
        }

        Debug.Log($"Player levou {damage} de dano. Vida atual: {currentHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void HealFull()
    {
        currentHealth = maxHealth;

        if (GameSession.Instance != null)
        {
            GameSession.Instance.RestoreFullHealth();
        }
    }

    private void Die()
    {
        Debug.Log("Player morreu.");

        // Dispara a animação de morte.
        if (animator != null)
            animator.SetTrigger("Die");

        StartCoroutine(DeathRoutine());
    }

    private System.Collections.IEnumerator DeathRoutine()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
            playerInput.enabled = false;

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
            movement.enabled = false;

        WeaponSystem weaponSystem = GetComponent<WeaponSystem>();
        if (weaponSystem != null)
            weaponSystem.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = Vector3.zero;

        CameraFollow cameraFollow = Camera.main != null ? Camera.main.GetComponent<CameraFollow>() : null;
        if (cameraFollow != null)
            cameraFollow.FreezeCamera();

        // Tempo para a animação "Died" tocar (clip dura ~24 frames).
        yield return new WaitForSecondsRealtime(1.5f);

        DeathScreenUI deathScreen = FindFirstObjectByType<DeathScreenUI>();

        if (deathScreen != null)
        {
            deathScreen.Show();
        }
        else
        {
            Debug.LogWarning("DeathScreenUI não encontrada.");
        }

        Time.timeScale = 0f;
    }
}
