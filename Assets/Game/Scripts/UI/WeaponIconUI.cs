using UnityEngine;
using UnityEngine.UI;

public class WeaponIconUI : MonoBehaviour
{
    [SerializeField] private WeaponSystem weaponSystem;
    [SerializeField] private Image weaponIcon;

    [Header("Icons")]
    [SerializeField] private Sprite swordIcon;
    [SerializeField] private Sprite hammerIcon;
    [SerializeField] private Sprite axeIcon;
    [SerializeField] private Sprite fistsIcon;

    private void Update()
    {
        if (weaponSystem == null)
            weaponSystem = FindFirstObjectByType<WeaponSystem>();

        if (weaponSystem == null || weaponIcon == null)
            return;

        weaponIcon.sprite = GetCurrentSprite();
    }

    private Sprite GetCurrentSprite()
    {
        if (weaponSystem.IsUsingFists)
            return fistsIcon;

        return weaponSystem.CurrentWeaponIndex switch
        {
            0 => swordIcon,
            1 => hammerIcon,
            2 => axeIcon,
            _ => fistsIcon
        };
    }
}