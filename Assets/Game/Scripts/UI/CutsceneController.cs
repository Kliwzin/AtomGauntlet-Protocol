using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

[RequireComponent(typeof(VideoPlayer))]
public class CutsceneController : MonoBehaviour
{
    [Header("Cena a carregar depois da cutscene")]
    [SerializeField] private string proximaCena = "Fase1_C1";

    private VideoPlayer videoPlayer;
    private bool jaAvancou = false;
    private bool videoComecou = false;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
    }

    private void Start()
    {
        videoPlayer.isLooping = false; 
        videoPlayer.loopPointReached += OnVideoTerminou;

        videoPlayer.started += (vp) => videoComecou = true;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            AvancarParaJogo();
            return;
        }

        if (videoComecou && !videoPlayer.isPlaying && !jaAvancou)
        {
            if (videoPlayer.frameCount > 0 &&
                videoPlayer.frame >= (long)videoPlayer.frameCount - 1)
            {
                AvancarParaJogo();
            }
        }
    }

    private void OnVideoTerminou(VideoPlayer vp)
    {
        AvancarParaJogo();
    }

    private void AvancarParaJogo()
    {
        if (jaAvancou) return;
        jaAvancou = true;

        Debug.Log("Cutscene: a carregar a cena '" + proximaCena + "'");
        SceneManager.LoadScene(proximaCena);
    }
}
