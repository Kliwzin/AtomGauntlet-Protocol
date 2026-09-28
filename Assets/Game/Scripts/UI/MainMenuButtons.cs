using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuButtons : MonoBehaviour
{
    [Header("Cenas")]
    public string nomeCenaJogo = "Game";
    public string nomeCenaCutscene = "Cutscene"; 
    public string nomeCenaCreditos = "Credits";

    [Header("Botões")]
    public Button botaoContinuar;

    private void Start()
    {
        bool temSave = PlayerPrefs.GetInt("TemSave", 0) == 1;

        if (botaoContinuar != null)
        {
            botaoContinuar.interactable = temSave;
        }
    }

    public void NovoJogo()
    {
        Debug.Log("Novo Jogo");

        GameSession.EnsureExists();


        GameSession.Instance.ResetForNewGame();


        SceneManager.LoadScene(nomeCenaCutscene);
    }

    public void Continuar()
    {
        Debug.Log("Continuar");

        GameSession.EnsureExists();

        bool carregou = GameSession.Instance.LoadFromDisk();

        if (carregou)
        {
            string cena = GameSession.Instance.GetCheckpointSceneName();
            if (string.IsNullOrEmpty(cena)) cena = nomeCenaJogo;
            SceneManager.LoadScene(cena);
        }
        else
        {
            Debug.Log("Não existe jogo salvo.");
        }
    }

    public void AbrirCreditos()
    {
        Debug.Log("Abrir Créditos");

        SceneManager.LoadScene("Credits");
    }

    public void Sair()
    {
        Debug.Log("Sair do jogo");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
