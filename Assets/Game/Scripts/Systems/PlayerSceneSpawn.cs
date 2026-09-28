using UnityEngine;

public class PlayerSceneSpawn : MonoBehaviour
{
    private void Start()
    {
        GameSession.EnsureExists();
        PlacePlayer();
    }

    private void PlacePlayer()
    {
        if (GameSession.Instance.UsarPosicaoLivre)
        {
            transform.position = GameSession.Instance.PosicaoLivre;
            transform.rotation = Quaternion.Euler(0f, GameSession.Instance.RotacaoLivreY, 0f);

            Debug.Log("SPAWN: a colocar o player na posição livre = " + GameSession.Instance.PosicaoLivre);

            GameSession.Instance.LimparPosicaoLivre();
            GameSession.Instance.ClearTargetSpawn();
            return;
        }

        string spawnIdToUse = GameSession.Instance.TargetSpawnId;

        if (string.IsNullOrEmpty(spawnIdToUse))
        {
            Debug.Log("SPAWN: spawnId vazio, player fica onde está.");
            return;
        }

        SpawnPoint[] spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);

        foreach (SpawnPoint spawnPoint in spawnPoints)
        {
            if (spawnPoint.SpawnId == spawnIdToUse)
            {
                transform.position = spawnPoint.transform.position;
                transform.rotation = spawnPoint.transform.rotation;
                GameSession.Instance.ClearTargetSpawn();
                return;
            }
        }

        Debug.LogWarning("SpawnPoint não encontrado para id: " + spawnIdToUse);
    }
}
