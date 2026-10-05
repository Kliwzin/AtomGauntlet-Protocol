using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerExecutionInteractor : MonoBehaviour
{
    [SerializeField] private Key executeKey = Key.E;
    [SerializeField] private int axeWeaponIndex = 2;
    [SerializeField] private float holdDuration = 0.8f;

    [Header("UI")]
    [SerializeField] private WorldExecutionIconUI executionUI;

    private WeaponSystem weaponSystem;
    private ExecutionTarget currentTarget;
    private float holdTimer = 0f;

    private void Start()
    {
        weaponSystem = GetComponent<WeaponSystem>();

        if (executionUI == null)
            executionUI = FindFirstObjectByType<WorldExecutionIconUI>();
    }

    private void Update()
    {
        if (!AxeUnlocked())
        {
            currentTarget = null;
            holdTimer = 0f;

            if (executionUI != null)
                executionUI.Hide();

            return;
        }

        FindClosestExecutableTarget();
        HandleExecution();
        UpdateUI();
    }

    private bool AxeUnlocked()
    {
        return GameSession.Instance != null &&
               GameSession.Instance.HasWeaponAddon(axeWeaponIndex);
    }

    private void FindClosestExecutableTarget()
    {
        currentTarget = null;

        ExecutionTarget[] targets = FindObjectsByType<ExecutionTarget>(FindObjectsSortMode.None);

        float closestDistance = float.MaxValue;

        foreach (ExecutionTarget target in targets)
        {
            if (target == null || !target.CanShowIcon())
                continue;

            float distance = Vector3.Distance(transform.position, target.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                currentTarget = target;
            }
        }
    }

    private void HandleExecution()
    {
        if (currentTarget == null || weaponSystem == null || Keyboard.current == null)
        {
            holdTimer = 0f;
            return;
        }

        bool hasAxeInHand = weaponSystem.CurrentWeaponIndex == axeWeaponIndex;
        bool inRange = currentTarget.CanExecute(transform);

        if (!hasAxeInHand || !inRange)
        {
            holdTimer = 0f;
            return;
        }

        if (Keyboard.current[executeKey].isPressed)
        {
            holdTimer += Time.deltaTime;

            if (holdTimer >= holdDuration)
            {
                currentTarget.Execute();
                holdTimer = 0f;
            }
        }
        else
        {
            holdTimer = 0f;
        }
    }

    private void UpdateUI()
    {
        if (executionUI == null)
            return;

        if (currentTarget == null)
        {
            executionUI.Hide();
            return;
        }

        bool hasAxeInHand = weaponSystem != null &&
                            weaponSystem.CurrentWeaponIndex == axeWeaponIndex;

        bool inRange = currentTarget.CanExecute(transform);
        bool ready = hasAxeInHand && inRange;

        float progress = ready ? holdTimer / holdDuration : 0f;

        executionUI.Show(currentTarget, ready, progress);
    }
}