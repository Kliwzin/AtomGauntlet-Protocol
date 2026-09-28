using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float fadeDuration = 0.6f;

    [Header("Frase de Loading")]
    [SerializeField] private TMP_Text fraseText;
    [SerializeField] private float tempoFraseVisivel = 1.2f;

    private bool isTransitioning = false;
    public bool IsTransitioning => isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeGroup != null)
        {
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
        }

        if (fraseText != null)
            fraseText.alpha = 0f;
    }

    public void LoadScene(string sceneName, string frase = "", System.Action beforeLoad = null)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionRoutine(sceneName, frase, beforeLoad));
    }

    private IEnumerator TransitionRoutine(string sceneName, string frase, System.Action beforeLoad)
    {
        isTransitioning = true;

        if (fadeGroup != null)
            fadeGroup.blocksRaycasts = true;

        yield return Fade(0f, 1f);

        beforeLoad?.Invoke();
        Time.timeScale = 1f;

        bool temFrase = fraseText != null && !string.IsNullOrEmpty(frase);
        if (temFrase)
        {
            fraseText.text = frase;
            yield return FadeTexto(0f, 1f);
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        if (temFrase)
            yield return new WaitForSecondsRealtime(tempoFraseVisivel);

        op.allowSceneActivation = true;
        while (!op.isDone)
            yield return null;

        yield return new WaitForSecondsRealtime(0.1f);

        if (temFrase)
            yield return FadeTexto(1f, 0f);

        yield return Fade(1f, 0f);

        if (fadeGroup != null)
            fadeGroup.blocksRaycasts = false;

        isTransitioning = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        if (fadeGroup == null) yield break;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            fadeGroup.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }
        fadeGroup.alpha = to;
    }

    private IEnumerator FadeTexto(float from, float to)
    {
        if (fraseText == null) yield break;

        float t = 0f;
        float dur = 0.5f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            fraseText.alpha = Mathf.Lerp(from, to, t / dur);
            yield return null;
        }
        fraseText.alpha = to;
    }
}
