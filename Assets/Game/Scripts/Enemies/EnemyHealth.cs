using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 30f;
    [SerializeField] private float currentHealth = 30f;

    [Header("Save / Estado do Mundo")]
    [SerializeField] private string eventoId = "";

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float HealthPercent => maxHealth <= 0f ? 0f : currentHealth / maxHealth;
    public bool IsDead => currentHealth <= 0f;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        if (!string.IsNullOrEmpty(eventoId) &&
            GameSession.Instance != null &&
            GameSession.Instance.InimigoEstaMorto(eventoId))
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        if (currentHealth <= 0f)
            Die();
    }

    public bool CanBeExecuted(float threshold)
    {
        return !IsDead && HealthPercent <= threshold;
    }

    public void Execute()
    {
        if (IsDead) return;

        Debug.Log($"{name} foi executado.");
        Die();
    }

    private void Die()
    {
        if (!string.IsNullOrEmpty(eventoId) && GameSession.Instance != null)
            GameSession.Instance.MarcarInimigoMorto(eventoId);

        Destroy(gameObject);
    }

    public float GetHealthPercent()
    {
        if (maxHealth <= 0f) return 0f;
        return Mathf.Clamp01(currentHealth / maxHealth);
    }
}
