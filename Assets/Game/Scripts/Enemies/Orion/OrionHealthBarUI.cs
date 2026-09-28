using UnityEngine;
using UnityEngine.UI;

public class OrionHealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private OrionHealth orionHealth;
    [SerializeField] private OrionBossController bossController;

    [Header("Health Fill")]
    [SerializeField] private Image fillImage;

    [Header("Seal Level Sprites")]
    [SerializeField] private Sprite sealMinus2Sprite;
    [SerializeField] private Sprite sealMinus1Sprite;
    [SerializeField] private Sprite sealZeroSprite;
    [SerializeField] private Sprite sealPlus1Sprite;
    [SerializeField] private Sprite sealPlus2Sprite;

    private void Update()
    {
        if (orionHealth == null)
            orionHealth = FindFirstObjectByType<OrionHealth>();

        if (bossController == null)
            bossController = FindFirstObjectByType<OrionBossController>();

        if (orionHealth == null || bossController == null || fillImage == null)
            return;

        fillImage.fillAmount = orionHealth.HealthPercent;
        fillImage.sprite = GetSpriteForSealLevel(bossController.SealLevel);
    }

    private Sprite GetSpriteForSealLevel(int sealLevel)
    {
        return sealLevel switch
        {
            -2 => sealMinus2Sprite,
            -1 => sealMinus1Sprite,
            0 => sealZeroSprite,
            1 => sealPlus1Sprite,
            2 => sealPlus2Sprite,
            _ => sealZeroSprite
        };
    }
}