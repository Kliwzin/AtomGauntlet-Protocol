using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.InputSystem;

public class CreditsVideoController : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;

    [Header("Cena do Menu")]
    public string nomeCenaMenu = "MainMenu";

    private bool jaVoltou = false;

    private void Start()
    {
        Debug.Log("Cena de créditos iniciou.");

        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (videoPlayer != null)
        {
            videoPlayer.isLooping = false;
            videoPlayer.loopPointReached += QuandoVideoAcabar;
            videoPlayer.Play();

            Debug.Log("Vídeo dos créditos começou.");
        }
        else
        {
            Debug.LogWarning("VideoPlayer não encontrado.");
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Debug.Log("ESC pressionado. A voltar para o menu.");
            VoltarParaMenu();
        }
    }

    private void QuandoVideoAcabar(VideoPlayer vp)
    {
        Debug.Log("Vídeo acabou. A voltar para o menu.");
        VoltarParaMenu();
    }

    private void VoltarParaMenu()
    {
        if (jaVoltou)
            return;

        jaVoltou = true;

        SceneManager.LoadScene(nomeCenaMenu);
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= QuandoVideoAcabar;
        }
    }
}
