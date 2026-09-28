using UnityEngine;

public static class SceneTransitionManager
{
    public static string TargetSpawnId { get; private set; }

    public static void SetTargetSpawn(string spawnId)
    {
        TargetSpawnId = spawnId;
    }

    public static void ClearTargetSpawn()
    {
        TargetSpawnId = string.Empty;
    }
}