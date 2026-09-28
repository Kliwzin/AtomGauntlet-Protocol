using System.Collections;
using UnityEngine;

public class GuardaFugitivo : MonoBehaviour
{
    [Header("Alvo e Deteção")]
    public Transform jogador;
    public float distanciaDeteccao = 6f;

    [Header("Fuga")]
    public Transform pontoDeFuga;
    public float velocidadeFuga = 5f;
    public float distanciaChegada = 0.5f;

    [Header("Diálogo")]
    public DialogueManager dialogueManager;
    public DialogueLine[] falaAoVer;

    [Header("Desaparecer")]
    public float atrasoAntesDeDesaparecer = 0f;

    [Header("Save / Estado do Mundo")]
    public string eventoId = "";

    [Header("Animação")]
    public Animator animator;

    private bool aFugir;
    private bool jaViu;

    void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // DEBUG: confirma se encontrou um Animator
        if (animator == null)
            Debug.LogError("GuardaFugitivo: ANIMATOR É NULL! Não há Animator no objeto nem nos filhos.");
        else
            Debug.Log("GuardaFugitivo: Animator encontrado em '" + animator.gameObject.name + "'. Controller = " +
                      (animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name : "NENHUM (Controller vazio!)"));
    }

    void Start()
    {
        if (!string.IsNullOrEmpty(eventoId) &&
            GameSession.Instance != null &&
            GameSession.Instance.EventoJaConcluido(eventoId))
        {
            Debug.Log("GuardaFugitivo: evento '" + eventoId + "' já concluído. A desativar.");
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!jaViu)
            VerificarDistancia();
        else if (aFugir)
            Fugir();

        // diz ao Animator se está a correr ou parado
        if (animator != null)
        {
            float velocidadeAnim = aFugir ? velocidadeFuga : 0f;
            animator.SetFloat("Speed", velocidadeAnim);

            // DEBUG: mostra o que está a ser enviado (só quando foge, para não encher a consola)
            if (aFugir)
                Debug.Log("GuardaFugitivo: aFugir=TRUE | Speed enviado ao Animator = " + velocidadeAnim);
        }
    }

    void VerificarDistancia()
    {
        if (jogador == null)
        {
            // DEBUG: o campo jogador não está preenchido
            Debug.LogWarning("GuardaFugitivo: campo 'jogador' está NULL. Arrasta o player no Inspector.");
            return;
        }

        float distancia = Vector3.Distance(transform.position, jogador.position);

        if (distancia <= distanciaDeteccao)
        {
            jaViu = true;
            aFugir = true;

            Debug.Log("GuardaFugitivo: viu o jogador! A começar a fugir.");

            if (!string.IsNullOrEmpty(eventoId) && GameSession.Instance != null)
                GameSession.Instance.MarcarEventoConcluido(eventoId);

            if (dialogueManager != null && falaAoVer != null && falaAoVer.Length > 0)
                dialogueManager.IniciarDialogo(falaAoVer);
        }
    }

    void Fugir()
    {
        if (pontoDeFuga == null)
        {
            Debug.LogWarning("GuardaFugitivo: 'pontoDeFuga' está NULL. Vai desaparecer em vez de correr.");
            aFugir = false;
            StartCoroutine(Desaparecer());
            return;
        }

        Vector3 direcao = pontoDeFuga.position - transform.position;
        direcao.y = 0f;

        if (direcao.magnitude <= distanciaChegada)
        {
            Debug.Log("GuardaFugitivo: chegou ao ponto de fuga. A desaparecer.");
            aFugir = false;
            StartCoroutine(Desaparecer());
            return;
        }

        transform.position += direcao.normalized * velocidadeFuga * Time.deltaTime;

        if (direcao.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direcao),
                10f * Time.deltaTime);
        }
    }

    IEnumerator Desaparecer()
    {
        if (atrasoAntesDeDesaparecer > 0f)
            yield return new WaitForSeconds(atrasoAntesDeDesaparecer);

        gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaDeteccao);
    }
}
