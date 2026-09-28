using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private DialogueManager dialogueManager;

    private bool isPaused = false;

    private void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) Continuar();
            else Pausar();
        }
    }

    public void Pausar()
    {
        isPaused = true;
        if (pausePanel != null) pausePanel.SetActive(true);
        if (dialogueManager != null) dialogueManager.SetPausado(true);
        Time.timeScale = 0f;
    }

    public void Continuar()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (dialogueManager != null) dialogueManager.SetPausado(false);
        Time.timeScale = 1f;
    }

    public void Guardar()
    {
        GameSession.EnsureExists();
        if (GameSession.Instance == null) return;

        if (!GameSession.Instance.HasCheckpointSaved)
        {
            Debug.Log("GUARDAR: HasCheckpointSaved = FALSE. Não há checkpoint, não guarda.");
            return;
        }

        Debug.Log("GUARDAR: TargetSpawnId = '" + GameSession.Instance.TargetSpawnId + "'");
        Debug.Log("GUARDAR: checkpointId guardado = '" +
            (GameSession.Instance.CheckpointData != null ? GameSession.Instance.CheckpointData.checkpointId : "NULL") + "'");

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            Debug.Log("GUARDAR: player encontrado, posição = " + player.transform.position);

            // Usa o checkpointId já guardado (não o TargetSpawnId, que pode estar vazio).
            string ultimoCheckpoint = GameSession.Instance.CheckpointData != null
                ? GameSession.Instance.CheckpointData.checkpointId
                : "";

            GameSession.Instance.SaveCheckpointComPosicao(
                ultimoCheckpoint,
                player.transform.position,
                player.transform.eulerAngles.y
            );

            Debug.Log("GUARDAR: gravado com posição livre. checkpointId = '" + ultimoCheckpoint + "'");
        }
        else
        {
            Debug.Log("GUARDAR: player NÃO encontrado (tag errada?), a usar checkpoint normal.");
            GameSession.Instance.SaveCheckpoint(GameSession.Instance.TargetSpawnId);
        }

        GameSession.Instance.SaveToDisk();
    }

    public void SairParaMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void GuardarESair()
    {
        Guardar();
        SairParaMenu();
    }

    public void SairDoJogo()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
