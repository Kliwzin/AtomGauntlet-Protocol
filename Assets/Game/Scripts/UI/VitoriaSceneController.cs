using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class VitoriaSceneController : MonoBehaviour
{
    [Header("Vídeo")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Cena seguinte")]
    [SerializeField] private string creditsSceneName = "Credits";

    [Header("Segurança")]
    [SerializeField] private bool permitirSkip = true;

    private bool jaAvancou = false;

    private void Start()
    {
        if (videoPlayer == null)
            videoPlayer = FindFirstObjectByType<VideoPlayer>();

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoTerminou;
            videoPlayer.Play();
        }
        else
        {
            IrParaCreditos();
        }
    }

    private void Update()
    {
        if (permitirSkip && !jaAvancou)
        {
            bool teclaPressionada = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
            bool cliquePressionado = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

            if (teclaPressionada || cliquePressionado)
                IrParaCreditos();
        }
    }

    private void OnVideoTerminou(VideoPlayer vp)
    {
        IrParaCreditos();
    }

    private void IrParaCreditos()
    {
        if (jaAvancou) return;
        jaAvancou = true;

        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoTerminou;

        SceneManager.LoadScene(creditsSceneName);
    }
}
