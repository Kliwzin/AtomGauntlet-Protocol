using UnityEngine;

public class OrionStoneDefense : MonoBehaviour
{
    [Header("Shield Setup")]
    [SerializeField] private GameObject stonePrefab;
    [SerializeField] private float radius = 1.5f;
    [SerializeField] private int stoneCount = 6;
    [SerializeField] private float heightOffset = 0f;
    [SerializeField] private Transform shieldCenter;

    [Header("Stone Rotation")]
    [SerializeField] private bool makeStonesFaceOutward = false;
    [SerializeField] private Vector3 stoneLocalEulerOffset = Vector3.zero;

    [Header("Break Settings")]
    [SerializeField] private int requiredWeaponIndex = 1;
    [SerializeField] private int hitsToBreak = 3;

    [Header("Cooldown")]
    [SerializeField] private float cooldownAfterBreak = 6f;

    [Header("Damage Reduction")]
    [SerializeField] private float damageReductionMultiplier = 0.25f;

    private GameObject shieldGroupObject;
    private bool shieldActive = false;
    private float nextAllowedShieldTime = 0f;

    public bool HasActiveDefense => shieldActive;

    public bool CanUseShield()
    {
        return !shieldActive && Time.time >= nextAllowedShieldTime;
    }

    public void PrepareShield()
    {
        Debug.Log("Orion preparou shield.");
    }

    public void Execute()
    {
        if (!CanUseShield())
        {
            Debug.Log("Orion tentou usar shield, mas ainda não pode.");
            return;
        }

        shieldActive = true;

        shieldGroupObject = new GameObject("OrionShieldGroup");
        Transform center = shieldCenter != null ? shieldCenter : transform;
        shieldGroupObject.transform.SetParent(center);  
        shieldGroupObject.transform.localPosition = Vector3.zero;
        shieldGroupObject.transform.localRotation = Quaternion.identity;
        shieldGroupObject.transform.localScale = Vector3.one;

        OrionShieldGroup shieldGroup = shieldGroupObject.AddComponent<OrionShieldGroup>();
        shieldGroup.Initialize(this, requiredWeaponIndex, hitsToBreak);

        for (int i = 0; i < stoneCount; i++)
        {
            float angle = i * Mathf.PI * 2f / stoneCount;

            Vector3 localPos = new Vector3(
                Mathf.Cos(angle) * radius,
                heightOffset,
                Mathf.Sin(angle) * radius
            );

            GameObject stone = Instantiate(stonePrefab, shieldGroupObject.transform);
            stone.transform.localPosition = localPos;

            if (makeStonesFaceOutward)
            {
                Vector3 outward = new Vector3(localPos.x, 0f, localPos.z).normalized;
                stone.transform.localRotation =
                    Quaternion.LookRotation(outward) * Quaternion.Euler(stoneLocalEulerOffset);
            }
            else
            {
                stone.transform.localRotation = Quaternion.Euler(stoneLocalEulerOffset);
            }
        }

        Debug.Log("Orion criou defesa de pedra.");
    }

    public void ClearDefense()
    {
        if (!shieldActive && shieldGroupObject == null)
            return;

        shieldActive = false;
        nextAllowedShieldTime = Time.time + cooldownAfterBreak;

        if (shieldGroupObject != null)
            Destroy(shieldGroupObject);

        shieldGroupObject = null;

        Debug.Log("Shield do Orion destruído.");
    }

    public float GetDamageReductionMultiplier()
    {
        return shieldActive ? damageReductionMultiplier : 1f;
    }
}