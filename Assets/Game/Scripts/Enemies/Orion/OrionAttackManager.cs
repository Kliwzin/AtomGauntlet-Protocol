using UnityEngine;

public class OrionAttackManager : MonoBehaviour
{
    [SerializeField] private OrionBossController boss;
    [SerializeField] private Transform player;

    [Header("Special Attack Timing")]
    [SerializeField] private float baseSpecialAttackInterval = 3.5f;
    [SerializeField] private float maxBusySafetyTime = 5f;
    [SerializeField] private float[] segmentIntervalFactors = { 1f, 0.85f, 0.7f };
    [SerializeField] private float meleeReactionTime = 0.8f;

    [Header("Seal Damage Multipliers")]
    [SerializeField] private float sealMinus2DamageMultiplier = 1.6f;
    [SerializeField] private float sealMinus1DamageMultiplier = 1.3f;
    [SerializeField] private float sealZeroDamageMultiplier = 1f;
    [SerializeField] private float sealPlus1DamageMultiplier = 0.75f;
    [SerializeField] private float sealPlus2DamageMultiplier = 0.5f;

    [Header("Attacks")]
    [SerializeField] private OrionMeleeAttack meleeAttack;
    [SerializeField] private OrionLineAttack lineAttack;
    [SerializeField] private OrionTentacleAttack tentacleAttack;
    [SerializeField] private OrionStoneDefense stoneDefense;
    [SerializeField] private OrionTurretSpawner turretSpawner;
    [SerializeField] private OrionMovement movement;

    [Header("Animation")]
    [SerializeField] private OrionAnimationController animationController;

    private bool isActive = false;
    private bool isBusy = false;
    private float specialTimer = 0f;
    private float closeTimer = 0f;

    private void Start()
    {
        if (boss == null) boss = GetComponent<OrionBossController>();
        if (movement == null) movement = GetComponent<OrionMovement>();
        if (animationController == null) animationController = GetComponent<OrionAnimationController>();

        FindPlayer();
    }

    private void Update()
    {
        if (!isActive || boss == null) return;

        FindPlayer();

        if (!isBusy && movement != null)
            movement.Move();

        if (animationController != null && movement != null)
            animationController.SetWalking(movement.IsMoving && !isBusy);

        if (!isBusy)
            TryMeleeIfPlayerIsClose();

        if (!isBusy)
            specialTimer += Time.deltaTime;

        if (!isBusy && specialTimer >= GetCurrentSpecialAttackInterval())
        {
            specialTimer = 0f;
            ChooseSpecialAttack();
        }
    }

    public void StartAttacking()
    {
        isActive = true;
        isBusy = false;
        specialTimer = 0f;
    }

    public void StopAttacking()
    {
        isActive = false;
        isBusy = false;
        CancelInvoke();

        movement?.Stop();
        animationController?.SetWalking(false);
        stoneDefense?.ClearDefense();
        turretSpawner?.ClearTurrets();
    }

    private void TryMeleeIfPlayerIsClose()
    {
        if (player == null || meleeAttack == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= meleeAttack.Range)
            closeTimer += Time.deltaTime;
        else
            closeTimer = 0f;

        if (closeTimer >= meleeReactionTime && meleeAttack.CanExecute())
        {
            StartBusyState();

            animationController?.PlayMelee();
            meleeAttack.PrepareAttack();
        }
    }

    private void ChooseSpecialAttack()
    {
        if (isBusy) return;

        int roll = Random.Range(0, 100);

        if (roll < 20 && lineAttack != null)
        {
            StartBusyState();
            lineAttack.PrepareCardinal();
            animationController?.PlayCardinals();
            return;
        }

        if (roll < 40 && tentacleAttack != null)
        {
            StartBusyState();
            tentacleAttack.Prepare();
            animationController?.PlayTentaclePrepare();
            return;
        }

        if (roll < 55 && stoneDefense != null && stoneDefense.CanUseShield())
        {
            StartBusyState();
            stoneDefense.PrepareShield();
            animationController?.PlayShield();
            return;
        }

        if (roll < 70 && lineAttack != null)
        {
            StartBusyState();
            lineAttack.PrepareDiagonal();
            animationController?.PlayCardinals();
            return;
        }

        if (roll < 85 && turretSpawner != null && !turretSpawner.HasActiveTurrets)
        {
            StartBusyState();
            turretSpawner.PrepareTurrets();
            animationController?.PlayTurrets();
            return;
        }

        if (lineAttack != null)
        {
            StartBusyState();
            lineAttack.PrepareUniversal();
            animationController?.PlayCardinals();
        }
    }

    private void StartBusyState()
    {
        isBusy = true;
        closeTimer = 0f;
        CancelInvoke(nameof(ForceEndBusyState));

        movement?.Stop();
        animationController?.SetWalking(false);

        Invoke(nameof(ForceEndBusyState), maxBusySafetyTime);
    }

    public void EndBusyFromAnimationEvent()
    {
        isBusy = false;
        CancelInvoke(nameof(ForceEndBusyState));
    }

    private void ForceEndBusyState()
    {
        isBusy = false;
    }

    private void FindPlayer()
    {
        if (player != null) return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private float GetCurrentSpecialAttackInterval()
    {
        float sealFactor = boss.SealLevel switch
        {
            -2 => 0.7f,
            -1 => 0.85f,
            0 => 1f,
            1 => 1.2f,
            2 => 1.4f,
            _ => 1f
        };

        float segmentFactor = 1f;

        if (segmentIntervalFactors != null && segmentIntervalFactors.Length > 0)
        {
            int index = Mathf.Clamp(boss.SegmentsBroken, 0, segmentIntervalFactors.Length - 1);
            segmentFactor = segmentIntervalFactors[index];
        }

        return baseSpecialAttackInterval * segmentFactor * sealFactor;
    }

    public float GetAttackDamage(float baseDamage)
    {
        float multiplier = boss.SealLevel switch
        {
            -2 => sealMinus2DamageMultiplier,
            -1 => sealMinus1DamageMultiplier,
            0 => sealZeroDamageMultiplier,
            1 => sealPlus1DamageMultiplier,
            2 => sealPlus2DamageMultiplier,
            _ => 1f
        };

        return baseDamage * multiplier;
    }
}