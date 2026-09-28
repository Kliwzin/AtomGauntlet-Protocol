using UnityEngine;

public class ResistanceIconDisplay : MonoBehaviour
{
    [SerializeField] private MonoBehaviour targetBehaviour;
    [SerializeField] private GameObject resistanceIcon;

    private IWeaponResistanceHint resistanceHint;
    private WeaponSystem weaponSystem;

    private void Awake()
    {
        resistanceHint = targetBehaviour as IWeaponResistanceHint;
    }

    private void Start()
    {
        weaponSystem = FindFirstObjectByType<WeaponSystem>();

        if (resistanceIcon != null)
            resistanceIcon.SetActive(false);
    }

    private void Update()
    {
        if (resistanceHint == null || weaponSystem == null || resistanceIcon == null)
            return;

        int weaponIndex = weaponSystem.CurrentWeaponIndex;

        resistanceIcon.SetActive(resistanceHint.ShouldShowResistanceIcon(weaponIndex));
    }
}