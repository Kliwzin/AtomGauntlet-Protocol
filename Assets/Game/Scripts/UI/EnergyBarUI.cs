using UnityEngine;
using UnityEngine.UI;

public class EnergyBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    private WeaponSystem weaponSystem;

    private void Update()
    {
        if (weaponSystem == null)
        {
            weaponSystem = FindFirstObjectByType<WeaponSystem>();
            if (weaponSystem == null) return;
        }

        if (fillImage == null) return;

        float current = weaponSystem.GetCurrentEnergy();
        float max = weaponSystem.GetMaxEnergy();

        if (max <= 0f) return;
        fillImage.fillAmount = current / max;
    }
}