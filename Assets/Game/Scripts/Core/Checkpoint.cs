using UnityEngine;

public class Checkpoint : InteractableBase
{
    [SerializeField] private string checkpointId;
    [SerializeField] private float holdDuration = 1.5f;

    [Header("Save / Estado do Mundo")]
    [SerializeField] private string eventoId = "";

    private bool jaUsado = false;

    private void Start()
    {
        // Se este checkpoint já foi usado num save anterior, fica bloqueado.
        if (!string.IsNullOrEmpty(eventoId) &&
            GameSession.Instance != null &&
            GameSession.Instance.EventoJaConcluido(eventoId))
        {
            jaUsado = true;
        }
    }

    public override bool CanInteract()
    {
        return !jaUsado;
    }

    public override float GetHoldDuration()
    {
        return holdDuration;
    }

    public override string GetInteractionText()
    {
        return "Recarregar";
    }

    public override void Interact()
    {
        if (jaUsado) return;

        GameSession.EnsureExists();

        if (GameSession.Instance == null) return;

        GameSession.Instance.RestoreFullEnergy();
        GameSession.Instance.RestoreFullHealth();

        if (!string.IsNullOrEmpty(eventoId))
            GameSession.Instance.MarcarEventoConcluido(eventoId);

        GameSession.Instance.SaveCheckpoint(checkpointId);

        GameSession.Instance.SaveToDisk();

        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.HealFull();

        jaUsado = true;

        Debug.Log("Checkpoint ativado e guardado: " + checkpointId);
    }
}
