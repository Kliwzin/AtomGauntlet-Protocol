/*using UnityEngine;

public class ToggleLiftButton : MonoBehaviour, IInteractable
{
    [SerializeField] private MovingPlatform targetPlatform;
    [SerializeField] private float holdDuration = 0.5f;

    public bool CanInteract()
    {
        return targetPlatform != null;
    }

    public float GetHoldDuration()
    {
        return holdDuration;
    }

    public string GetInteractionText()
    {
        return "Ativar";
    }

    public void Interact()
    {
        targetPlatform.Toggle();
    }
}*/

using UnityEngine;

public class ToggleLiftButton : InteractableBase
{
    [SerializeField] private MovingPlatform targetPlatform;
    [SerializeField] private float holdDuration = 0.5f;

    public override bool CanInteract()
    {
        return targetPlatform != null;
    }

    public override float GetHoldDuration()
    {
        return holdDuration;
    }

    public override string GetInteractionText()
    {
        return "Ativar";
    }

    public override void Interact()
    {
        if (targetPlatform != null)
            targetPlatform.Toggle();
    }
}