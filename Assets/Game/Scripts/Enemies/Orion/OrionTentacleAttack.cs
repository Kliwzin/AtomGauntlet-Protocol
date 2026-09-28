using UnityEngine;

public class OrionTentacleAttack : MonoBehaviour
{
    [SerializeField] private OrionTentacleWarning warningPrefab;
    [SerializeField] private Transform player;

    [Header("Spawn")]
    [SerializeField] private float spawnGroundY = 0.03f;

    private OrionTentacleWarning activeWarning;

    private void Start()
    {
        FindPlayer();
    }

    public void Prepare()
    {
        FindPlayer();

        if (player == null || warningPrefab == null)
        {
            Debug.LogWarning("TentacleAttack sem player ou warningPrefab.");
            return;
        }

        Vector3 position = player.position;
        position.y = spawnGroundY;

        activeWarning = Instantiate(warningPrefab, position, Quaternion.identity);
        activeWarning.BeginTracking();

        Debug.Log("Orion iniciou mira do tentáculo.");
    }

    public void LockCurrentWarning()
    {
        if (activeWarning == null) return;

        activeWarning.LockAndStrikeSoon();
        activeWarning = null;
    }

    private void FindPlayer()
    {
        if (player != null) return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }
}