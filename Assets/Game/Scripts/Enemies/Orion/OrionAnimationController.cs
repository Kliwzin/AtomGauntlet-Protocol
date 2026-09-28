using UnityEngine;

public class OrionAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int Melee = Animator.StringToHash("Melee");
    private static readonly int Cardinals = Animator.StringToHash("Cardinals");
    private static readonly int Shield = Animator.StringToHash("Shield");
    private static readonly int Turrets = Animator.StringToHash("Turrets");
    private static readonly int TentaclePrepare = Animator.StringToHash("TentaclePrepare");
    private static readonly int TentacleSlam = Animator.StringToHash("TentacleSlam");
    private static readonly int Stun = Animator.StringToHash("Stun");
    private static readonly int Recover = Animator.StringToHash("Recover");

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    public void SetWalking(bool value)
    {
        if (animator != null)
            animator.SetBool(IsWalking, value);
    }

    public void PlayMelee() => Trigger(Melee);
    public void PlayCardinals() => Trigger(Cardinals);
    public void PlayShield() => Trigger(Shield);
    public void PlayTurrets() => Trigger(Turrets);
    public void PlayTentaclePrepare() => Trigger(TentaclePrepare);
    public void PlayTentacleSlam() => Trigger(TentacleSlam);
    public void PlayStun() => Trigger(Stun);
    public void PlayRecover() => Trigger(Recover);

    private void Trigger(int triggerHash)
    {
        if (animator != null)
            animator.SetTrigger(triggerHash);
    }
}