using UnityEngine;
using UnityEngine.UI;

public class WorldInteractionPrompt : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private RectTransform promptRect;
    [SerializeField] private Image progressFill;
    [SerializeField] private Camera worldCamera;

    private WorldPromptTarget currentTarget;

    private void Awake()
    {
        Hide();
    }

    private void LateUpdate()
    {
        if (currentTarget == null || worldCamera == null || promptRect == null)
            return;

        Vector3 screenPosition = worldCamera.WorldToScreenPoint(
            currentTarget.GetPromptWorldPosition()
        );

        if (screenPosition.z < 0f)
        {
            Hide();
            return;
        }

        promptRect.position = screenPosition;
    }

    public void Show(WorldPromptTarget target)
    {
        currentTarget = target;

        if (worldCamera == null)
            worldCamera = Camera.main;

        if (root != null)
            root.SetActive(true);

        SetProgress(0f);
    }

    public void Hide()
    {
        currentTarget = null;

        if (root != null)
            root.SetActive(false);

        SetProgress(0f);
    }

    public void SetProgress(float value)
    {
        if (progressFill != null)
            progressFill.fillAmount = Mathf.Clamp01(value);
    }
}