using UnityEngine;

public class ResistanceTarget : MonoBehaviour
{
    [SerializeField] private int idealWeaponIndex = 1;
    [SerializeField] private float detectionRange = 3f;
    [SerializeField] private WorldPromptTarget promptTarget;

    public int IdealWeaponIndex => idealWeaponIndex;
    public float DetectionRange => detectionRange;

    public WorldPromptTarget PromptTarget
    {
        get
        {
            if (promptTarget != null) return promptTarget;
            return GetComponent<WorldPromptTarget>();
        }
    }

    public bool ShouldShowResistance(Transform player, int currentWeaponIndex)
    {
        if (player == null) return false;

        float distance = Vector3.Distance(transform.position, player.position);

        return distance <= detectionRange && currentWeaponIndex != idealWeaponIndex;
    }
}