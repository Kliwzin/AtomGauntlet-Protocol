using UnityEngine;

public class OrionHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 900f;
    [SerializeField] private OrionBossController boss;
    [SerializeField] private OrionStoneDefense stoneDefense;

    [Header("Diálogo")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueLine[] falaNovaFase;

    private float currentHealth;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float HealthPercent => maxHealth <= 0f ? 0f : currentHealth / maxHealth;
    public bool IsDead => currentHealth <= 0f;

    private float firstThreshold;
    private float secondThreshold;

    private bool firstThresholdArmed = true;
    private bool secondThresholdArmed = true;

    private void Awake()
    {
        currentHealth = maxHealth;

        firstThreshold = maxHealth * (2f / 3f);
        secondThreshold = maxHealth * (1f / 3f);

        if (boss == null)
            boss = GetComponent<OrionBossController>();

        if (stoneDefense == null)
            stoneDefense = GetComponent<OrionStoneDefense>();
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        if (boss != null && boss.IsStunned)
        {
            boss.RegisterStunHit();
            return;
        }

        float finalDamage = damage;

        if (stoneDefense != null)
            finalDamage *= stoneDefense.GetDamageReductionMultiplier();

        currentHealth = Mathf.Max(currentHealth - finalDamage, 0f);

        Debug.Log("Orion vida: " + currentHealth);

        if (currentHealth <= 0f)
        {
            Die();
            return;
        }

        CheckStunThreshold();
    }

    public void TakeDamageFromAttacker(float damage, Transform attacker)
    {
        TakeDamage(damage);
    }

    public bool CanBeExecuted(float threshold)
    {
        return !IsDead && HealthPercent <= threshold;
    }

    public void Execute()
    {
        if (IsDead) return;

        Debug.Log("Orion foi executado pelo machado.");

        Die();
    }

    private void Die()
    {
        currentHealth = 0f;

        if (boss != null)
            boss.Die();

        gameObject.SetActive(false);
    }

    public void RecoverCurrentSegment()
    {
        float recoverAmount = maxHealth / 3f;
        currentHealth = Mathf.Min(currentHealth + recoverAmount, maxHealth);

        RearmThresholdsAfterHeal();

        Debug.Log("Orion recuperou vida por falta de ação.");
    }

    private void CheckStunThreshold()
    {
        if (firstThresholdArmed && currentHealth <= firstThreshold)
        {
            firstThresholdArmed = false;


            if (dialogueManager != null && falaNovaFase != null && falaNovaFase.Length > 0)
                dialogueManager.IniciarDialogo(falaNovaFase);

            if (boss != null)
                boss.EnterStun();

            return;
        }

        if (secondThresholdArmed && currentHealth <= secondThreshold)
        {
            secondThresholdArmed = false;

            if (dialogueManager != null && falaNovaFase != null && falaNovaFase.Length > 0)
                dialogueManager.IniciarDialogo(falaNovaFase);

            if (boss != null)
                boss.EnterStun();
        }
    }

    private void RearmThresholdsAfterHeal()
    {
        if (currentHealth > firstThreshold)
            firstThresholdArmed = true;

        if (currentHealth > secondThreshold)
            secondThresholdArmed = true;
    }
}
