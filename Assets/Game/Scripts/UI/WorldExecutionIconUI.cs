using UnityEngine;
using UnityEngine.UI;

public class WorldExecutionIconUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private RectTransform iconRect;

    [Header("Visual States")]
    [SerializeField] private GameObject executableIcon;
    [SerializeField] private GameObject readyIcon;
    [SerializeField] private GameObject progressIcon;

    [Header("Progress")]
    [SerializeField] private Image progressFill;

    [Header("References")]
    [SerializeField] private Camera worldCamera;

    private ExecutionTarget currentTarget;

    private void Awake()
    {
        Hide();
    }

    public void Show(ExecutionTarget target, bool ready, float progress)
    {
        currentTarget = target;

        if (worldCamera == null)
            worldCamera = Camera.main;

        if (root != null)
            root.SetActive(true);

        bool isHolding = ready && progress > 0f;

        if (executableIcon != null)
            executableIcon.SetActive(!ready);

        // fundo continua ligado enquanto está pronto ou segurando E
        if (readyIcon != null)
            readyIcon.SetActive(ready);

        // preenchimento só aparece enquanto segura E
        if (progressIcon != null)
            progressIcon.SetActive(isHolding);

        if (progressFill != null)
            progressFill.fillAmount = Mathf.Clamp01(progress);
    }

    public void Hide()
    {
        currentTarget = null;

        if (root != null)
            root.SetActive(false);

        if (executableIcon != null)
            executableIcon.SetActive(false);

        if (readyIcon != null)
            readyIcon.SetActive(false);

        if (progressIcon != null)
            progressIcon.SetActive(false);

        if (progressFill != null)
            progressFill.fillAmount = 0f;
    }

    private void LateUpdate()
    {
        if (currentTarget == null || iconRect == null) return;

        if (worldCamera == null)
            worldCamera = Camera.main;

        if (worldCamera == null) return;

        WorldPromptTarget promptTarget = currentTarget.PromptTarget;
        if (promptTarget == null) return;

        Vector3 screenPos = worldCamera.WorldToScreenPoint(promptTarget.GetPromptWorldPosition());

        if (screenPos.z < 0f)
        {
            Hide();
            return;
        }

        iconRect.position = screenPos;
    }
}