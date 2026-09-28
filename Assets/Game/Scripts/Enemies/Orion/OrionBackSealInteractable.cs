/*using UnityEngine;

public class OrionBackSealInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private OrionBossController boss;
    [SerializeField] private float holdDuration = 1.5f;

    public bool CanInteract()
    {
        return boss != null && boss.IsStunned;
    }

    public float GetHoldDuration()
    {
        return holdDuration;
    }

    public string GetInteractionText()
    {
        return "Selar Orion";
    }

    public void Interact()
    {
        if (!CanInteract()) return;

        boss.SealFromBackInteraction();
    }
}*/

using UnityEngine;

public class OrionBackSealInteractable : InteractableBase
{
    [SerializeField] private OrionBossController boss;
    [SerializeField] private float holdDuration = 1.5f;

    public override bool CanInteract()
    {
        return boss != null && boss.IsStunned;
    }

    public override float GetHoldDuration()
    {
        return holdDuration;
    }

    public override string GetInteractionText()
    {
        return "Selar Orion";
    }

    public override void Interact()
    {
        if (!CanInteract()) return;

        boss.SealFromBackInteraction();
    }
}