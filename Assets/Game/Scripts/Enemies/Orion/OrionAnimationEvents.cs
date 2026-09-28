using UnityEngine;

public class OrionAnimationEvents : MonoBehaviour
{
    [SerializeField] private OrionMeleeAttack meleeAttack;
    [SerializeField] private OrionLineAttack lineAttack;
    [SerializeField] private OrionStoneDefense stoneDefense;
    [SerializeField] private OrionTurretSpawner turretSpawner;
    [SerializeField] private OrionTentacleAttack tentacleAttack;
    [SerializeField] private OrionAttackManager attackManager;
    [SerializeField] private OrionAnimationController animationController;

    private void Awake()
    {
        if (meleeAttack == null) meleeAttack = GetComponentInParent<OrionMeleeAttack>();
        if (lineAttack == null) lineAttack = GetComponentInParent<OrionLineAttack>();
        if (stoneDefense == null) stoneDefense = GetComponentInParent<OrionStoneDefense>();
        if (turretSpawner == null) turretSpawner = GetComponentInParent<OrionTurretSpawner>();
        if (tentacleAttack == null) tentacleAttack = GetComponentInParent<OrionTentacleAttack>();
        if (attackManager == null) attackManager = GetComponentInParent<OrionAttackManager>();
        if (animationController == null) animationController = GetComponentInParent<OrionAnimationController>();
    }

    public void AnimEvent_MeleeHit()
    {
        meleeAttack?.ApplyDamage();
    }

    public void AnimEvent_SpawnCardinalProjectiles()
    {
        lineAttack?.FirePreparedAttack();
    }

    public void AnimEvent_SpawnShield()
    {
        stoneDefense?.Execute();
    }

    public void AnimEvent_SpawnTurrets()
    {
        turretSpawner?.Execute();
    }

    public void AnimEvent_LockTentacleTarget()
    {
        tentacleAttack?.LockCurrentWarning();
        animationController?.PlayTentacleSlam();
    }

    public void AnimEvent_EndSpecial()
    {
        attackManager?.EndBusyFromAnimationEvent();
    }
}