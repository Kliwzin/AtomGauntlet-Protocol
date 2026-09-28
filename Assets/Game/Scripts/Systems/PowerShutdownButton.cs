/*using UnityEngine;

public class PowerShutdownButton : MonoBehaviour, IInteractable
{
    [SerializeField] private float holdDuration = 1f;
    [SerializeField] private MovingPlatform platformToDrop;
    [SerializeField] private Light[] lightsToDisable;
    [SerializeField] private GameObject[] objectsToDisable;
    [SerializeField] private GameObject[] objectsToEnable;

    private bool alreadyUsed = false;

    public bool CanInteract()
    {
        return !alreadyUsed;
    }

    public float GetHoldDuration()
    {
        return holdDuration;
    }

    public string GetInteractionText()
    {
        return "Desligar energia";
    }

    public void Interact()
    {
        if (alreadyUsed) return;

        alreadyUsed = true;

        if (LevelPowerState.Instance != null)
            LevelPowerState.Instance.DisablePower();

        if (platformToDrop != null)
            platformToDrop.ForceLower();

        foreach (Light light in lightsToDisable)
        {
            if (light != null)
                light.enabled = false;
        }

        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        Debug.Log("Botão de energia ativado.");
    }
}*/

using UnityEngine;

public class PowerShutdownButton : InteractableBase
{
    [SerializeField] private float holdDuration = 1f;
    [SerializeField] private MovingPlatform platformToDrop;
    [SerializeField] private Light[] lightsToDisable;
    [SerializeField] private GameObject[] objectsToDisable;
    [SerializeField] private GameObject[] objectsToEnable;

    private bool alreadyUsed = false;

    public override bool CanInteract()
    {
        return !alreadyUsed;
    }

    public override float GetHoldDuration()
    {
        return holdDuration;
    }

    public override string GetInteractionText()
    {
        return "Desligar energia";
    }

    public override void Interact()
    {
        if (alreadyUsed) return;

        alreadyUsed = true;

        if (LevelPowerState.Instance != null)
            LevelPowerState.Instance.DisablePower();

        if (platformToDrop != null)
            platformToDrop.ForceLower();

        foreach (Light light in lightsToDisable)
        {
            if (light != null)
                light.enabled = false;
        }

        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        Debug.Log("Energia desligada.");
    }
}