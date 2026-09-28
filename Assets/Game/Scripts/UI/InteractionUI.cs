using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Image progressFill;
    [SerializeField] private TMP_Text interactionText;

    private float currentDuration = 1f;

    private void Start()
    {
        Hide();
    }

    public void Show(string text, float holdDuration)
    {
        if (root != null) root.SetActive(true);
        currentDuration = Mathf.Max(holdDuration, 0.01f);

        if (interactionText != null)
            interactionText.text = text;
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
        ResetProgress();
    }

    public void SetProgress(float normalized)
    {
        if (progressFill != null)
            progressFill.fillAmount = Mathf.Clamp01(normalized);
    }

    public void ResetProgress()
    {
        if (progressFill != null)
            progressFill.fillAmount = 0f;
    }
}