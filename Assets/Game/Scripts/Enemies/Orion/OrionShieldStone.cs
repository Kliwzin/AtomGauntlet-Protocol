using UnityEngine;

public class OrionShieldStone : MonoBehaviour
{
    private OrionShieldGroup shieldGroup;

    private void Awake()
    {
        shieldGroup = GetComponentInParent<OrionShieldGroup>();
    }

    public void TakeWeaponHit(int weaponIndex)
    {
        if (shieldGroup != null)
            shieldGroup.TakeWeaponHit(weaponIndex);
    }
}