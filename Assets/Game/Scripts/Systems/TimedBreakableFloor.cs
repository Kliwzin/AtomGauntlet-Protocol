using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TimedBreakableFloor : MonoBehaviour
{
    [SerializeField] private float timeToBreak = 2f;
    [SerializeField] private GameObject brokenVersion;

    [Header("Save / Estado do Mundo")]
    [Tooltip("ID único deste chão. Ex.: manutencao_chao_01")]
    [SerializeField] private string eventoId = "";

    private float timer = 0f;
    private bool playerOnTop = false;
    private bool isBroken = false;

    private void Awake()
    {
        // Se este chão já partiu num save anterior, instancia já a versão partida
        // e destrói este antes de o jogador o ver intacto.
        if (GameSession.Instance != null &&
            !string.IsNullOrEmpty(eventoId) &&
            GameSession.Instance.EventoJaConcluido(eventoId))
        {
            if (brokenVersion != null)
                Instantiate(brokenVersion, transform.position, transform.rotation);

            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!playerOnTop || isBroken) return;

        timer += Time.deltaTime;

        if (timer >= timeToBreak)
            Break();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Player"))
            playerOnTop = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            playerOnTop = false;
            timer = 0f;
        }
    }

    private void Break()
    {
        isBroken = true;

        if (GameSession.Instance != null && !string.IsNullOrEmpty(eventoId))
            GameSession.Instance.MarcarEventoConcluido(eventoId);

        if (brokenVersion != null)
            Instantiate(brokenVersion, transform.position, transform.rotation);

        Destroy(gameObject);
    }
}
