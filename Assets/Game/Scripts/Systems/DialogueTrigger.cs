using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Referência ao Manager")]
    public DialogueManager dialogueManager;

    [Header("Falas deste trigger")]
    public DialogueLine[] falas;

    [Header("Definições")]
    public bool dispararUmaVez = true;
    public string tagJogador = "Player";

    [Tooltip("Dispara automaticamente no início da cena, sem precisar de colisão.")]
    public bool dispararNoInicio = false;

    [Header("Save / Estado do Mundo")]
    public string eventoId = "";
    public bool naoRepetirEmSave = false;

    private bool jaDisparou;

    void Start()
    {
        if (!string.IsNullOrEmpty(eventoId) &&
            GameSession.Instance != null &&
            GameSession.Instance.EventoJaConcluido(eventoId))
        {
            jaDisparou = true;
            return;
        }

        if (dispararNoInicio)
        {
            if (naoRepetirEmSave &&
                GameSession.Instance != null &&
                GameSession.Instance.HasCheckpointSaved)
            {
                jaDisparou = true;
                return;
            }

            Disparar();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (jaDisparou && dispararUmaVez) return;

        if (other.CompareTag(tagJogador))
            Disparar();
    }

    void Disparar()
    {
        if (jaDisparou && dispararUmaVez) return;

        if (dialogueManager != null)
        {
            dialogueManager.IniciarDialogo(falas);
            jaDisparou = true;

            if (!string.IsNullOrEmpty(eventoId) && GameSession.Instance != null)
                GameSession.Instance.MarcarEventoConcluido(eventoId);
        }
        else
        {
            Debug.LogWarning("DialogueTrigger: falta ligar o DialogueManager no Inspector.");
        }
    }
}
