using UnityEngine;

public class LevelPowerState : MonoBehaviour
{
    public static LevelPowerState Instance { get; private set; }

    [SerializeField] private bool powerDisabled = false;

    public bool PowerDisabled => powerDisabled;

    private void Awake()
    {
        Instance = this;
    }

    public void DisablePower()
    {
        powerDisabled = true;
        Debug.Log("Energia da fase desligada.");
    }
}