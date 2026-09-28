using UnityEngine;

public class OrionShieldGroup : MonoBehaviour
{
    private int requiredWeaponIndex = 1;
    private int hitsToBreak = 3;
    private int currentHits;

    private OrionStoneDefense stoneDefense;

    public void Initialize(OrionStoneDefense owner, int requiredWeapon, int hits)
    {
        stoneDefense = owner;
        requiredWeaponIndex = requiredWeapon;
        hitsToBreak = hits;
        currentHits = hitsToBreak;
    }

    public void TakeWeaponHit(int weaponIndex)
    {
        if (weaponIndex != requiredWeaponIndex)
        {
            Debug.Log("Essa arma não consegue quebrar a defesa do Orion.");
            return;
        }

        currentHits--;

        Debug.Log($"Shield do Orion atingido. Faltam {currentHits} hits.");

        if (currentHits <= 0)
        {
            if (stoneDefense != null)
                stoneDefense.ClearDefense();
            else
                Destroy(gameObject);
        }
    }

    public bool ShouldShowResistanceIcon(int weaponIndex)
    {
        return weaponIndex != requiredWeaponIndex;
    }
}