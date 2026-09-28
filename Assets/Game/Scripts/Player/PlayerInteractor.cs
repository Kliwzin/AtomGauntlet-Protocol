using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Transform interactionPoint;

    [Header("Input")]
    [SerializeField] private Key interactKey = Key.E;

    [Header("UI")]
    [SerializeField] private WorldInteractionPrompt worldPromptUI;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private IInteractable currentInteractable;
    private IInteractable lastInteractable;
    private float holdTimer = 0f;

    private bool isInteractingAnim = false;

    private void Start()
    {
        if (interactionPoint == null)
            interactionPoint = transform;

        if (worldPromptUI == null)
            worldPromptUI = FindFirstObjectByType<WorldInteractionPrompt>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        FindInteractable();
        HandleFocusChange();
        HandleInteraction();
    }

    private void FindInteractable()
    {
        currentInteractable = null;

        Vector3 center = interactionPoint.position;
        Collider[] hits = Physics.OverlapSphere(center, interactionRadius, interactableLayer);

        float closestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();

            if (interactable == null || !interactable.CanInteract())
                continue;

            float distance = Vector3.Distance(transform.position, hit.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                currentInteractable = interactable;
            }
        }
    }

    private void HandleFocusChange()
    {
        if (currentInteractable == lastInteractable)
            return;

        holdTimer = 0f;

        if (worldPromptUI != null)
        {
            if (currentInteractable != null)
            {
                WorldPromptTarget target = currentInteractable.GetPromptTarget();

                if (target != null)
                    worldPromptUI.Show(target);
            }
            else
            {
                worldPromptUI.Hide();
            }
        }

        lastInteractable = currentInteractable;
    }

    private void HandleInteraction()
    {
        if (currentInteractable == null || Keyboard.current == null)
        {
            SetInteractingAnim(false);
            return;
        }

        float requiredHold = currentInteractable.GetHoldDuration();

        if (Keyboard.current[interactKey].isPressed)
        {
            holdTimer += Time.deltaTime;
            float progress = holdTimer / requiredHold;

            SetInteractingAnim(true);

            if (worldPromptUI != null)
                worldPromptUI.SetProgress(progress);

            if (holdTimer >= requiredHold)
            {
                currentInteractable.Interact();

                holdTimer = 0f;

                SetInteractingAnim(false);

                if (worldPromptUI != null)
                    worldPromptUI.SetProgress(0f);
            }
        }
        else
        {
            holdTimer = 0f;

            SetInteractingAnim(false);

            if (worldPromptUI != null)
                worldPromptUI.SetProgress(0f);
        }
    }

    private void SetInteractingAnim(bool value)
    {
        if (isInteractingAnim == value) return; 
        isInteractingAnim = value;

        if (animator != null)
            animator.SetBool("IsInteracting", value);
    }

    private void OnDisable()
    {
        if (worldPromptUI != null)
            worldPromptUI.Hide();

        SetInteractingAnim(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Vector3 center = interactionPoint != null ? interactionPoint.position : transform.position;
        Gizmos.DrawWireSphere(center, interactionRadius);
    }
}
