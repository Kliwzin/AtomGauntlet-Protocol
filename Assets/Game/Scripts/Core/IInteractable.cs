public interface IInteractable
{
    bool CanInteract();
    float GetHoldDuration();
    string GetInteractionText();
    void Interact();

    WorldPromptTarget GetPromptTarget();
}