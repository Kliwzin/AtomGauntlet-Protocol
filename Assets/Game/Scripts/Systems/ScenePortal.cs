using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ScenePortal : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private string targetSpawnId;
    [SerializeField] private float requiredContactTime = 2f;
    [SerializeField] private string playerTag = "Player";

    [SerializeField] private bool requiresPowerDisabled = false;

    [Header("Transição")]
    [SerializeField, TextArea(2, 3)] private string fraseTransicao = "";

    private float timer = 0f;
    private bool playerInside = false;
    private bool isLoading = false;

    private void Reset()
    {
        BoxCollider col = GetComponent<BoxCollider>();
        col.isTrigger = true;
    }

    private void Update()
    {
        if (!playerInside || isLoading) return;

        timer += Time.deltaTime;

        if (timer >= requiredContactTime)
            LoadScene();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = true;
        timer = 0f;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = false;
        timer = 0f;
    }

    private void LoadScene()
    {
        if (requiresPowerDisabled)
        {
            if (LevelPowerState.Instance == null || !LevelPowerState.Instance.PowerDisabled)
            {
                Debug.Log("Porta trancada. A energia ainda está ligada.");
                return;
            }
        }

        if (GameSession.Instance == null)
        {
            Debug.LogWarning("GameSession não encontrada.");
            return;
        }

        isLoading = true;

        Debug.Log("SceneTransition.Instance é null? " + (SceneTransition.Instance == null));

        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.LoadScene(targetSceneName, fraseTransicao, () =>
            {
                GameSession.Instance.SetTargetSpawn(targetSpawnId);
            });
        }
        else
        {
            GameSession.Instance.SetTargetSpawn(targetSpawnId);
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
        }
    }
}
