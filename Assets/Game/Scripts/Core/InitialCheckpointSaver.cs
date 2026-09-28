using UnityEngine;

public class InitialCheckpointSaver : MonoBehaviour
{
    [SerializeField] private string initialCheckpointId = "InitialCheckpoint";

    private void Start()
    {
        GameSession.EnsureExists();

        if (GameSession.Instance == null) return;

        if (!GameSession.Instance.HasCheckpointSaved)
        {
            GameSession.Instance.RestoreFullHealth();
            GameSession.Instance.RestoreFullEnergy();
            GameSession.Instance.SaveCheckpoint(initialCheckpointId);

            Debug.Log("Checkpoint inicial salvo.");
        }
    }
}