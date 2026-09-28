using UnityEngine;

public class WorldResistanceIconUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private RectTransform iconRect;
    [SerializeField] private Camera worldCamera;

    private Transform player;
    private WeaponSystem weaponSystem;
    private WorldPromptTarget currentPromptTarget;

    private void Awake()
    {
        Hide();
    }

    private void Start()
    {
        if (worldCamera == null)
            worldCamera = Camera.main;

        PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement != null)
            player = playerMovement.transform;

        weaponSystem = FindFirstObjectByType<WeaponSystem>();
    }

    private void LateUpdate()
    {
        if (player == null || weaponSystem == null)
        {
            Hide();
            return;
        }

        FindBestTarget();

        if (currentPromptTarget == null)
        {
            Hide();
            return;
        }

        Vector3 screenPos = worldCamera.WorldToScreenPoint(
            currentPromptTarget.GetPromptWorldPosition()
        );

        if (screenPos.z < 0f)
        {
            Hide();
            return;
        }

        Show();
        iconRect.position = screenPos;
    }

    private void FindBestTarget()
    {
        currentPromptTarget = null;
        float closestDistance = float.MaxValue;

        ResistanceTarget[] normalTargets =
            FindObjectsByType<ResistanceTarget>(FindObjectsSortMode.None);

        foreach (ResistanceTarget target in normalTargets)
        {
            if (target == null) continue;

            if (!target.ShouldShowResistance(player, weaponSystem.CurrentWeaponIndex))
                continue;

            WorldPromptTarget promptTarget = target.PromptTarget;
            if (promptTarget == null) continue;

            float distance = Vector3.Distance(player.position, target.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                currentPromptTarget = promptTarget;
            }
        }

        OrionShieldResistanceTarget[] shieldTargets =
            FindObjectsByType<OrionShieldResistanceTarget>(FindObjectsSortMode.None);

        foreach (OrionShieldResistanceTarget target in shieldTargets)
        {
            if (target == null) continue;

            if (!target.ShouldShowResistance(player, weaponSystem.CurrentWeaponIndex))
                continue;

            WorldPromptTarget promptTarget = target.PromptTarget;
            if (promptTarget == null) continue;

            float distance = Vector3.Distance(player.position, target.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                currentPromptTarget = promptTarget;
            }
        }
    }

    private void Show()
    {
        if (root != null && !root.activeSelf)
            root.SetActive(true);
    }

    private void Hide()
    {
        currentPromptTarget = null;

        if (root != null)
            root.SetActive(false);
    }
}