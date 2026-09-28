using UnityEngine;

public class ExecutionTarget : MonoBehaviour
{
    [Header("Health References")]
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private OrionHealth orionHealth;

    [Header("UI Target")]
    [SerializeField] private WorldPromptTarget promptTarget;

    [Header("Execution")]
    [SerializeField] private float executeThreshold = 0.2f;
    [SerializeField] private float executeRange = 2f;

    public float ExecuteRange => executeRange;

    public WorldPromptTarget PromptTarget
    {
        get
        {
            if (promptTarget != null) return promptTarget;
            return GetComponent<WorldPromptTarget>();
        }
    }

    private void Awake()
    {
        if (enemyHealth == null)
            enemyHealth = GetComponent<EnemyHealth>();

        if (orionHealth == null)
            orionHealth = GetComponent<OrionHealth>();
    }

    public bool CanShowIcon()
    {
        if (enemyHealth != null)
            return enemyHealth.CanBeExecuted(executeThreshold);

        if (orionHealth != null)
            return orionHealth.CanBeExecuted(executeThreshold);

        return false;
    }

    public bool CanExecute(Transform player)
    {
        if (!CanShowIcon() || player == null) return false;

        float distance = Vector3.Distance(transform.position, player.position);
        return distance <= executeRange;
    }

    public void Execute()
    {
        if (enemyHealth != null)
        {
            enemyHealth.Execute();
            return;
        }

        if (orionHealth != null)
        {
            orionHealth.Execute();
            return;
        }
    }
}