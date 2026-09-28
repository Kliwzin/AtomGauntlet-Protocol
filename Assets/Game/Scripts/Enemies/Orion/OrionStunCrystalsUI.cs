using UnityEngine;
using UnityEngine.UI;

public class OrionStunCrystalsUI : MonoBehaviour
{
    [SerializeField] private OrionBossController bossController;

    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private Image crystalsOnFill;

    private void OnEnable()
    {
        FindBossIfNeeded();
        HideCrystals();
    }

    private void Update()
    {
        FindBossIfNeeded();

        if (bossController == null || root == null || crystalsOnFill == null)
            return;

        bool shouldShow = bossController.IsStunned;

        if (root.activeSelf != shouldShow)
            root.SetActive(shouldShow);

        if (!shouldShow)
            return;

        int totalHits = Mathf.Max(1, bossController.HitsToBreakStunAggressive);
        int hitsTaken = bossController.StunHitsTaken;

        float remainingPercent = 1f - ((float)hitsTaken / totalHits);
        crystalsOnFill.fillAmount = Mathf.Clamp01(remainingPercent);
    }

    private void FindBossIfNeeded()
    {
        if (bossController != null) return;
        bossController = FindFirstObjectByType<OrionBossController>();
    }

    private void HideCrystals()
    {
        if (root != null)
            root.SetActive(false);

        if (crystalsOnFill != null)
            crystalsOnFill.fillAmount = 1f;
    }
}