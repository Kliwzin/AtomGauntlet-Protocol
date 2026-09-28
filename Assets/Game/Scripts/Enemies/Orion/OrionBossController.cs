using UnityEngine;

public class OrionBossController : MonoBehaviour
{
    public enum BossState
    {
        Dormant,
        Fighting,
        Stunned,
        Recovering,
        Dead
    }

    [Header("References")]
    [SerializeField] private OrionHealth health;
    [SerializeField] private OrionAttackManager attackManager;
    [SerializeField] private OrionAnimationController animationController;
    [SerializeField] private GameObject bossUI;

    [Header("Activation")]
    [SerializeField] private float activationRange = 8f;
    [SerializeField] private Transform player;

    [Header("Stun")]
    [SerializeField] private float stunDuration = 8f;
    [SerializeField] private int hitsToBreakStunAggressive = 5;
    [SerializeField] private float sealEnergyCost = 15f;
    [SerializeField] private float recoverDelay = 1f;

    [Header("Diálogo")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueLine[] falaInicioCombate;
    [SerializeField] private DialogueLine[] falaOrionCai;

    [Header("Vitória")]
    [SerializeField] private VictorySequence victorySequence;

    private BossState state = BossState.Dormant;
    private int sealLevel = 0;
    private int stunHitsTaken = 0;
    private float stunTimer = 0f;

    public BossState State => state;
    public int SealLevel => sealLevel;
    public bool IsStunned => state == BossState.Stunned;
    public int StunHitsTaken => stunHitsTaken;
    public int HitsToBreakStunAggressive => hitsToBreakStunAggressive;

    private void Start()
    {
        if (health == null)
            health = GetComponent<OrionHealth>();

        if (attackManager == null)
            attackManager = GetComponent<OrionAttackManager>();

        if (animationController == null)
            animationController = GetComponent<OrionAnimationController>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (bossUI != null)
            bossUI.SetActive(false);
    }

    private void Update()
    {
        if (state == BossState.Dormant)
            TryStartFight();

        if (state == BossState.Stunned)
            UpdateStun();
    }

    private void TryStartFight()
    {
        if (player == null) return;

        if (Vector3.Distance(transform.position, player.position) <= activationRange)
            StartFight();
    }

    private void StartFight()
    {
        state = BossState.Fighting;

        if (bossUI != null)
            bossUI.SetActive(true);

        if (attackManager != null)
            attackManager.StartAttacking();

        if (dialogueManager != null && falaInicioCombate != null && falaInicioCombate.Length > 0)
            dialogueManager.IniciarDialogo(falaInicioCombate);

        Debug.Log("Boss fight Orion começou.");
    }

    public void EnterStun()
    {
        if (state == BossState.Dead) return;

        state = BossState.Stunned;
        stunTimer = 0f;
        stunHitsTaken = 0;

        if (attackManager != null)
            attackManager.StopAttacking();

        if (animationController != null)
            animationController.PlayStun();

        Debug.Log("Orion entrou em stun.");
    }

    private void UpdateStun()
    {
        stunTimer += Time.deltaTime;

        if (stunTimer >= stunDuration)
        {
            if (health != null)
                health.RecoverCurrentSegment();

            ExitStun();
        }
    }

    public void RegisterStunHit()
    {
        if (state != BossState.Stunned) return;

        stunHitsTaken++;

        Debug.Log($"Hit no stun. Cristais restantes: {hitsToBreakStunAggressive - stunHitsTaken}");

        if (stunHitsTaken >= hitsToBreakStunAggressive)
        {
            ChangeSealLevel(-1);
            ExitStun();
        }
    }

    public void SealFromBackInteraction()
    {
        if (state != BossState.Stunned) return;

        ChangeSealLevel(+1);

        if (GameSession.Instance != null)
            GameSession.Instance.ConsumeEnergy(sealEnergyCost);

        ExitStun();
    }

    private void ChangeSealLevel(int amount)
    {
        sealLevel = Mathf.Clamp(sealLevel + amount, -2, 2);
        Debug.Log("Seal Level atual: " + sealLevel);
    }

    private void ExitStun()
    {
        if (health != null && health.IsDead) return;

        state = BossState.Recovering;
        stunTimer = 0f;

        if (animationController != null)
            animationController.PlayRecover();

        Invoke(nameof(ResumeFightAfterRecover), recoverDelay);

        Debug.Log("Orion saindo do stun.");
    }

    private void ResumeFightAfterRecover()
    {
        if (state == BossState.Dead) return;

        state = BossState.Fighting;

        if (attackManager != null)
            attackManager.StartAttacking();

        Debug.Log("Orion voltou para a luta.");
    }

    public float GetDamageMultiplier()
    {
        return sealLevel switch
        {
            -2 => 1.6f,
            -1 => 1.3f,
            0 => 1f,
            1 => 0.75f,
            2 => 0.5f,
            _ => 1f
        };
    }

    public void Die()
    {
        state = BossState.Dead;
        CancelInvoke();

        if (attackManager != null)
            attackManager.StopAttacking();

        if (bossUI != null)
            bossUI.SetActive(false);

        if (animationController != null)
            animationController.SetWalking(false);

        if (dialogueManager != null && falaOrionCai != null && falaOrionCai.Length > 0)
            dialogueManager.IniciarDialogo(falaOrionCai);

        if (victorySequence != null)
            victorySequence.StartVictory();

        Debug.Log("Orion morreu.");
    }
}
