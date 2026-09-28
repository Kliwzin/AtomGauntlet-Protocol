using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Start()
    {
        Hide();
    }

    public void Show()
    {
        if (root != null)
            root.SetActive(true);
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);

        Time.timeScale = 1f;
    }

    public void OnReplayButtonClicked()
    {
        Debug.Log("Botão Replay clicado!");

        Time.timeScale = 1f;

        GameSession.EnsureExists();

        if (GameSession.Instance == null) return;

        if (!GameSession.Instance.HasCheckpointSaved)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        // Caminho de MORTE: volta à foto do checkpoint físico (inimigos ressuscitam).
        GameSession.Instance.RestoreFromCheckpointDeath();

        // Respawn por morte usa o checkpoint, não a posição onde guardaste.
        GameSession.Instance.LimparPosicaoLivre();

        GameSession.Instance.RestoreFullHealth();
        GameSession.Instance.RestoreFullEnergy();

        SceneManager.LoadScene(GameSession.Instance.GetCheckpointSceneName());
    }

    public void OnMenuButtonClicked()
    {
        Debug.Log("Botão Voltar ao Menu clicado!");

        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OnQuitButtonClicked()
    {
        Debug.Log("Botão Sair do Jogo clicado!");

        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
