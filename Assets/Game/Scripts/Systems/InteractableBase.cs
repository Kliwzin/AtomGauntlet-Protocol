using UnityEngine;

public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    [Header("World Prompt")]
    [SerializeField] private WorldPromptTarget promptTarget;

    public abstract bool CanInteract();
    public abstract float GetHoldDuration();
    public abstract string GetInteractionText();
    public abstract void Interact();

    public WorldPromptTarget GetPromptTarget()
    {
        if (promptTarget != null)
            return promptTarget;

        return GetComponent<WorldPromptTarget>();
    }
}