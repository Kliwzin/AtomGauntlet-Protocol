/*using UnityEngine;

public class WeaponAddonPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private int weaponIndex;
    [SerializeField] private float holdDuration = 1.5f;
    [SerializeField] private bool requiresPowerDisabled = false;

    private bool alreadyCollected = false;

    public bool CanInteract()
    {
        return !alreadyCollected;
    }

    public float GetHoldDuration()
    {
        return holdDuration;
    }

    public string GetInteractionText()
    {
        if (IsLocked())
            return "Trancado";

        return "Coletar módulo";
    }

    public void Interact()
    {
        if (alreadyCollected) return;

        if (IsLocked())
        {
            Debug.Log("Trancado. Desligue a energia primeiro.");
            return;
        }

        GameSession.EnsureExists();

        if (GameSession.Instance == null) return;

        GameSession.Instance.UnlockWeaponAddon(weaponIndex);
        alreadyCollected = true;

        Debug.Log("Addon coletado: " + weaponIndex);

        Destroy(gameObject);
    }

    private bool IsLocked()
    {
        return requiresPowerDisabled &&
               (LevelPowerState.Instance == null || !LevelPowerState.Instance.PowerDisabled);
    }
}*/

using UnityEngine;

public class WeaponAddonPickup : InteractableBase
{
    [SerializeField] private int weaponIndex;
    [SerializeField] private float holdDuration = 1.5f;
    [SerializeField] private bool requiresPowerDisabled = false;

    [Header("Diálogo ao coletar")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueLine[] falaAoColetar;

    private bool alreadyCollected = false;

    public override bool CanInteract()
    {
        if (alreadyCollected) return false;

        if (requiresPowerDisabled)
        {
            if (LevelPowerState.Instance == null) return false;
            return LevelPowerState.Instance.PowerDisabled;
        }

        return true;
    }

    public override float GetHoldDuration()
    {
        return holdDuration;
    }

    public override string GetInteractionText()
    {
        return "Coletar módulo";
    }

    public override void Interact()
    {
        if (!CanInteract()) return;

        GameSession.EnsureExists();

        if (GameSession.Instance == null) return;

        GameSession.Instance.UnlockWeaponAddon(weaponIndex);
        alreadyCollected = true;

        Debug.Log("Addon coletado: " + weaponIndex);

        if (dialogueManager != null && falaAoColetar != null && falaAoColetar.Length > 0)
            dialogueManager.IniciarDialogo(falaAoColetar);

        Destroy(gameObject);
    }
}
