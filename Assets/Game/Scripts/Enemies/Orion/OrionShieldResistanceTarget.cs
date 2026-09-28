using UnityEngine;

public class OrionShieldResistanceTarget : MonoBehaviour
{
    [SerializeField] private OrionStoneDefense stoneDefense;
    [SerializeField] private int idealWeaponIndex = 1;
    [SerializeField] private float detectionRange = 4f;
    [SerializeField] private WorldPromptTarget promptTarget;

    private void Awake()
    {
        FindReferences();
    }

    private void Update()
    {
        FindReferences();
    }

    private void FindReferences()
    {
        if (stoneDefense == null)
            stoneDefense = GetComponentInParent<OrionStoneDefense>();

        if (stoneDefense == null)
            stoneDefense = FindFirstObjectByType<OrionStoneDefense>();

        if (promptTarget == null)
            promptTarget = GetComponent<WorldPromptTarget>();
    }

    public WorldPromptTarget PromptTarget => promptTarget;

    public bool ShouldShowResistance(Transform player, int currentWeaponIndex)
    {
        if (stoneDefense == null) return false;
        if (player == null) return false;
        if (!stoneDefense.HasActiveDefense) return false;
        if (currentWeaponIndex == idealWeaponIndex) return false;

        float distance = Vector3.Distance(transform.position, player.position);
        return distance <= detectionRange;
    }
}