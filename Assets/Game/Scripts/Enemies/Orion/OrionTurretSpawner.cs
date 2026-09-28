using UnityEngine;

public class OrionTurretSpawner : MonoBehaviour
{
    [SerializeField] private OrionTurret turretPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private OrionAttackManager attackManager;

    private readonly System.Collections.Generic.List<OrionTurret> activeTurrets = new();

    public bool HasActiveTurrets
    {
        get
        {
            CleanupNullTurrets();
            return activeTurrets.Count > 0;
        }
    }

    public void PrepareTurrets()
    {
        Debug.Log("Orion preparou torretas.");
    }

    public void Execute()
    {
        CleanupNullTurrets();

        if (HasActiveTurrets)
        {
            Debug.Log("Torretas ainda ativas. Orion não invocou novas.");
            return;
        }

        if (turretPrefab == null || spawnPoints == null) return;

        foreach (Transform point in spawnPoints)
        {
            if (point == null) continue;

            OrionTurret turret = Instantiate(turretPrefab, point.position, point.rotation);
            turret.Initialize(attackManager);
            activeTurrets.Add(turret);
        }

        Debug.Log("Orion invocou torretas.");
    }

    private void CleanupNullTurrets()
    {
        activeTurrets.RemoveAll(turret => turret == null);
    }

    public void ClearTurrets()
    {
        CleanupNullTurrets();

        foreach (OrionTurret turret in activeTurrets)
        {
            if (turret != null)
                Destroy(turret.gameObject);
        }

        activeTurrets.Clear();
    }
}