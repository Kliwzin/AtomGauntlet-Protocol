using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    private PlayerHealth playerHealth;

    private void Update()
    {
        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth == null) return;
        }

        if (fillImage == null) return;
        fillImage.fillAmount = playerHealth.HealthPercent;
    }
}